/*******************************************************
 * 
 * 作者：刘雷
 * 创建日期：20170116
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 刘雷 20170116 14:36
 * 
*******************************************************/

using MySql.Data.MySqlClient;
using Rafy.Data;
using Rafy.DbMigration;
using Rafy.DbMigration.MySql;
using Rafy.Domain.ORM.MySql;
using Rafy.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Domain.ORM.BatchSubmit.MySql
{
    /// <summary>
    /// MySql 数据库的实体批量导入器
    /// </summary>
    [Serializable]
    public sealed class MySqlBatchImporter : BatchImporter
    {
        /// <summary>
        /// 默认无参构造函数，初始化批处理的容量和MySql语句生成器
        /// </summary>
        public MySqlBatchImporter()
        {
            this.BatchSize = 100000;
            this.SqlGenerator = new MySqlSqlGenerator();
        }

        /// <summary>
        /// 每次导入时，一批最多同时导入多少条数据。
        /// </summary>
        /// <exception cref="System.InvalidOperationException">MySql 中每批次最多只能导入 100000 行数据。</exception>
        public override int BatchSize
        {
            get { return base.BatchSize; }
            set
            {
                if (value > 100000) throw new InvalidOperationException("MySql 中每批次最多只能导入 100000 行数据。");
                base.BatchSize = value;
            }
        }

        #region ImportInsert

        /// <summary>
        /// 批量导入指定的实体或列表
        /// </summary>
        /// <param name="batch">批量导入数据的对象</param>
        protected override void ImportInsert(EntityBatch batch)
        {
            var entities = batch.InsertBatch;

            if (batch.Table.IdentityColumn != null)
            {
                this.GenerateId(batch, entities);
            }

            foreach (var section in this.EnumerateAllBatches(entities))
            {
                this.ImportBatch(batch, section);
            }
        }

        /// <summary>
        /// 执行批插入的 核心方法
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="entities"></param>
        private void ImportBatch(EntityBatch meta, IList<Entity> entities)
        {
            var sql = this.GenerateBatchInsertStatement(meta,entities);
            meta.DBA.RawAccesser.ExecuteText(sql);
        }

        /// <summary>
        /// 生成批量插入的Sql语句
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="entities">需要插入的实体类型集合</param>
        /// <returns>返回拼接完成的、批量插入的Sql语句</returns>
        private string GenerateBatchInsertStatement(EntityBatch meta, IList<Entity> entities)
        {
            //其它方案：https://www.cnblogs.com/yinzhou/p/11093033.html

            var dba = meta.DBA.RawAccesser;
            var table = meta.Table;

            var sql = new StringWriter();
            sql.Write("INSERT INTO ");
            sql.AppendQuote(table, table.Name).Write("(");

            var columns = table.Columns;
            for (int i = 0, c = columns.Count; i < c; i++)
            {
                if (i != 0) { sql.Write(','); }

                sql.AppendQuote(table, columns[i].Name);
            }
            sql.Write(")VALUES");

            this.GenerateValuesSql(sql, entities, table);

            return sql.ToString();
        }

        #endregion

        #region ImportUpdate

        /// <summary>
        /// 子类重写此方法实现批量更新逻辑。
        /// </summary>
        /// <param name="batch"></param>
        protected override void ImportUpdate(EntityBatch batch)
        {
            //分批插入实体，每批最多十万条数据
            foreach (var section in this.EnumerateAllBatches(batch.UpdateBatch))
            {
                this.ImportUpdate(batch, section);
            }
        }

        /// <summary>
        /// 执行批量更新的操作
        /// </summary>
        /// <param name="meta">数据库操作对象</param>
        /// <param name="entities">需要更新的实例集合</param>
        private void ImportUpdate(EntityBatch meta, IList<Entity> entities)
        {
            var sql = this.GenerateBatchUpdateStatement(meta, entities);
            meta.DBA.RawAccesser.ExecuteText(sql);
        }

        /// <summary>
        /// 生成批量更新的Sql语句
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="entities">需要插入的实体类型集合</param>
        /// <returns>返回拼接完成的、批量插入的Sql语句</returns>
        private string GenerateBatchUpdateStatement(EntityBatch meta, IList<Entity> entities)
        {
            var dba = meta.DBA.RawAccesser;
            var table = meta.Table;

            var sql = new StringWriter();
            sql.Write("REPLACE INTO ");
            sql.AppendQuote(table, table.Name).Write("(");

            var columns = table.Columns;
            for (int i = 0, c = columns.Count; i < c; i++)
            {
                if (i != 0) { sql.Write(','); }

                sql.AppendQuote(table, columns[i].Name);
            }
            sql.Write(")VALUES");

            this.GenerateValuesSql(sql, entities, table);

            return sql.ToString();
        }

        #endregion

        #region 公用方法

        private void GenerateValuesSql(StringWriter sql, IList<Entity> entities, RdbTable table)
        {
            var columns = table.Columns;

            for (int e = 0, ec = entities.Count; e < ec; e++)
            {
                var entity = entities[e];

                if (e > 0) { sql.Write(","); }

                sql.Write("(");

                for (int i = 0, c = columns.Count; i < c; i++)
                {
                    var column = columns[i];
                    if (i != 0) { sql.Write(','); }

                    //获取数据类型及值。
                    var dbType = column.Info.DbType;
                    var value = column.ReadDbParameterValue(entity);

                    //处理空值
                    if (value == DBNull.Value || value == null)
                    {
                        sql.Write("null");
                        continue;
                    }

                    //按类型写入值。
                    if (dbType == DbType.Binary)
                    {
                        var bytes = value as byte[];
                        string newValue = Convert.ToBase64String(bytes);
                        sql.Write("'" + newValue + "'");
                    }
                    else if (dbType == DbType.String || dbType == DbType.DateTime)
                    {
                        sql.Write("'" + value + "'");
                    }
                    else
                    {
                        sql.Write(value);
                    }
                }

                sql.Write(")");
            }
        }

        #endregion

        #region 自动生成主键 ID

        private static object _identityLock = new object();

        /// <summary>
        /// 为每个实体类的主键生成具体的数值
        /// </summary>
        /// <param name="meta">数据操作对象</param>
        /// <param name="entities">需要插入数据库的实体类列表</param>
        internal void GenerateId(EntityBatch meta, IList<Entity> entities)
        {
            var startId = GetBatchIDs(meta.DBA, meta.Table);

            for (int i = 0, c = entities.Count; i < c; i++)
            {
                var item = entities[i];
                if (!((IEntityWithId)item).IdProvider.IsAvailable(item.Id))
                {
                    item.Id = ++startId;
                }
            }
        }

        /// <summary>
        /// 获取指定大小批量的连续的 Id 号。返回第一个 Id 号的值。
        /// </summary>
        /// <param name="dba">数据操作对象</param>
        /// <param name="table">数据表对象</param>
        /// <returns></returns>
        private static long GetBatchIDs(IDbAccesser dba, RdbTable table)
        {
            var tableName = table.Name;
            lock (_identityLock)
            {
                var currentValue = Convert.ToInt64(dba.QueryValue(string.Format("SELECT IFNULL(MAX(id),0) FROM {0}", tableName)));
                return currentValue;
            }

            throw new InvalidOperationException("生成 Id 时，发生未知错误。");
        }

        #endregion
    }
}