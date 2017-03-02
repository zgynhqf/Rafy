﻿/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150313
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150313 23:20
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rafy.Data;
using Rafy.ManagedProperty;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 本类是为关系型数据库设计的数据提供器。
    /// IRepositoryDataProvider 则是更通用的接口。
    /// </summary>
    public class RdbDataProvider : RepositoryDataProvider, IDbConnector
    {
        private static readonly AppContextItem<Tuple<string, string>> ShareDbSettingContextItem = new AppContextItem<Tuple<string, string>>("RdbDataProvider.ShareDbSetting");

        public RdbDataProvider()
        {
            this.DataSaver = new RdbDataSaver();
            this.DataQueryer = new RdbDataQueryer();
        }

        /// <summary>
        /// 数据的保存器。
        /// </summary>
        public new RdbDataSaver DataSaver
        {
            get { return base.DataSaver as RdbDataSaver; }
            set { base.DataSaver = value; }
        }

        /// <summary>
        /// 数据的查询器。
        /// </summary>
        public new RdbDataQueryer DataQueryer
        {
            get { return base.DataQueryer as RdbDataQueryer; }
            set { base.DataQueryer = value; }
        }

        #region 数据库配置

        private DbSetting _dbSetting;
        /// <summary>
        /// 这个字段用于存储运行时解析出来的 ORM 信息。
        /// </summary>
        private RdbTable _ormTable;

        /// <summary>
        /// 创建数据库操作对象
        /// </summary>
        /// <returns></returns>
        public IDbAccesser CreateDbAccesser()
        {
            return DbAccesserFactory.Create(this.DbSetting);
        }

        /// <summary>
        /// 数据库配置（每个库有一个唯一的配置名）
        /// </summary>
        public DbSetting DbSetting
        {
            get
            {
                string conSetting = string.Empty;
                //先取线程上下文切换的数据源
                if (string.IsNullOrEmpty(conSetting))
                {
                    var tupleDbSettingName = ShareDbSettingContextItem.Value;
                    if (tupleDbSettingName != null)
                    {
                        if (this.ConnectionStringSettingName == tupleDbSettingName.Item1)
                        {
                            conSetting = tupleDbSettingName.Item2;
                        }
                    }
                }
                //在取DataProvider设置的
                if (string.IsNullOrEmpty(conSetting))
                {
                    conSetting = ConnectionStringSettingName;
                }
                if (conSetting == null) throw new InvalidProgramException("数据库配置属性重写有误，不能返回 null。");
                this._dbSetting = DbSetting.FindOrCreate(conSetting);
                return this._dbSetting;
            }
        }

        /// <summary>
        /// 数据库配置名称（每个库有一个唯一的配置名）
        /// 
        /// 默认使用 ConnectionStringNames.RafyPlugins 中配置的数据库。
        /// </summary>
        internal protected virtual string ConnectionStringSettingName
        {
            get { return DbSettingNames.RafyPlugins; }
        }

        ///// <summary>
        ///// 获取该实体对应的 ORM 运行时对象。
        ///// 
        ///// 如果该实体没有对应的实体元数据或者该实体没有被配置为映射数据库，
        ///// 则本方法则无法创建对应的 ORM 运行时，此时会返回 null。
        ///// </summary>
        ///// <returns></returns>
        //public ITable GetDbTable()
        //{
        //    return this.DbTable;
        //}

        internal RdbTable DbTable
        {
            get
            {
                if (_ormTable == null && this.Repository.EntityMeta != null)
                {
                    _ormTable = RdbTableFactory.CreateORMTable(this.Repository);
                }

                return _ormTable;
            }
        }

        #endregion

        #region 查询

        /// <summary>
        /// 子类重写此方法，来实现自己的 GetEntityValue 方法的数据层代码。
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public override LiteDataTable GetEntityValue(object entityId, string property)
        {
            var table = this.DbTable;
            var idColumn = table.PKColumn.Name;
            var column = table.Translate(property);

            var sql = new StringWriter();
            sql.Write("SELECT ");
            sql.AppendQuote(table, column);
            sql.Write(" FROM ");
            sql.AppendQuoteName(table);
            sql.Write(" WHERE ");
            sql.AppendQuote(table, idColumn);
            sql.Write(" = {0}");

            return this.QueryTable(new TableQueryArgs
            {
                FormattedSql = sql.ToString(),
                Parameters = new object[] { entityId },
            });
        }

        /// <summary>
        /// QueryTable 方法完成后调用。
        /// 
        /// 子类可重写此方法来实现查询完成后的数据修整工具。
        /// </summary>
        /// <param name="args"></param>
        internal protected virtual void OnTableQueryed(TableQueryArgs args)
        {
            this.DataQueryer.OnTableQueryed(args);
        }

        #endregion

        #region 提供给子类的查询接口

        /// <summary>
        /// 从持久层中查询数据。
        /// 本方法只能由仓库中的方法来调用。本方法的返回值的类型将与仓库中方法的返回值保持一致。
        /// 支持的返回值：EntityList、Entity、int、LiteDataTable。
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paging"></param>
        /// <param name="eagerLoad"></param>
        /// <returns></returns>
        public object QueryData(FormattedSql sql, PagingInfo paging = null, EagerLoadOptions eagerLoad = null)
        {
            return this.DataQueryer.QueryData(sql, paging, eagerLoad);
        }

        /// <summary>
        /// 从持久层中查询数据。
        /// 本方法只能由仓库中的方法来调用。本方法的返回值的类型将与仓库中方法的返回值保持一致。
        /// 支持的返回值：EntityList、Entity、int、LiteDataTable。
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object QueryData(SqlQueryArgs args)
        {
            return this.DataQueryer.QueryData(args);
        }

        /// <summary>
        /// 使用 sql 语句来查询数据表。
        /// </summary>
        /// <param name="sql">Sql 语句.</param>
        /// <param name="paging">分页信息。</param>
        /// <returns></returns>
        protected LiteDataTable QueryTable(FormattedSql sql, PagingInfo paging = null)
        {
            return this.DataQueryer.QueryTable(sql, paging);
        }

        /// <summary>
        /// 使用 sql 语句查询数据表。
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        protected LiteDataTable QueryTable(TableQueryArgs args)
        {
            return this.DataQueryer.QueryTable(args);
        }

        #endregion

        #region 其它

        private SQLColumnsGenerator _sqlColumnsGenerator;

        internal SQLColumnsGenerator SQLColumnsGenerator
        {
            get
            {
                if (_sqlColumnsGenerator == null)
                {
                    _sqlColumnsGenerator = new SQLColumnsGenerator(this.Repository);
                }
                return _sqlColumnsGenerator;
            }
        }

        /// <summary>
        /// 强制转换指定仓库的数据提供程序为关系数据库的数据提供程。
        /// 如果该仓库的 <see cref="IRepository.DataProvider"/> 不是此类型的子类，则会抛出异常。
        /// </summary>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static RdbDataProvider Get(IRepository repository)
        {
            var dp = repository.DataProvider as RdbDataProvider;
            if (dp == null)
            {
                throw new InvalidProgramException(string.Format(
                    "{0} 仓库类型使用的数据提供程序类型是 {1}，该类型不能转换为关系数据库的数据提供程序。",
                    repository.GetType(),
                    repository.DataProvider.GetType()
                    ));
            }
            return dp;
        }

        /// <summary>
        /// 切换实体仓储的数据源
        /// 在当前上下文中原仓储的数据源都会切换为新的数据源
        /// </summary>
        /// <param name="oldDbSettingName">实体仓储旧的数据源</param>
        /// <param name="newDbSettingName">实体仓储新的数据源</param>
        /// <returns></returns>
        public static IDisposable RedirectDbSetting(string oldDbSettingName, string newDbSettingName)
        {
            return ShareDbSettingContextItem.UseScopeValue(new Tuple<string, string>(oldDbSettingName, newDbSettingName));
        }
        #endregion
    }
}