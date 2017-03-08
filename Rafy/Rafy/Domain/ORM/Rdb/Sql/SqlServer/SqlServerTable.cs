/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150729
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150729 11:22
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Data;
using Rafy.Reflection;

namespace Rafy.Domain.ORM.SqlServer
{
    class SqlServerTable : SqlTable
    {
        private string _insertSQL;

        // 缓存  identity 手动、自动赋值 两种sql 
        private readonly Dictionary<bool, string> _insertSqlDic = new Dictionary<bool, string>(2);

        public SqlServerTable(IRepositoryInternal repository) : base(repository) { }

        public override void Insert(IDbAccesser dba, Entity item)
        {
            //如果有 Id 列，那么需要在执行 Insert 的同时，执行 SELECT @@IDENTITY。
            //在为 SQL Server 插入数据时，执行 Insert 的同时，必须同时执行 SELECT @@IDENTITY。否则会有多线程问题。
            var idColumn = this.IdentityColumn;
            if (idColumn != null)
            {
                // identity 是 int 或者long 型
                var isIdnetityHasValue = Convert.ToInt64(idColumn.ReadParameterValue(item)) > 0;

                if (!_insertSqlDic.TryGetValue(isIdnetityHasValue, out _insertSQL))
                {
                    _insertSQL = string.Empty;
                    if (isIdnetityHasValue)
                    {
                        _insertSQL = $" SET IDENTITY_INSERT {this.Name} ON ;";
                    }
                    _insertSQL += this.GenerateInsertSQL(isIdnetityHasValue);
                    _insertSQL += Environment.NewLine;
                    if (isIdnetityHasValue)
                    {
                        _insertSQL += $" SET IDENTITY_INSERT {this.Name} OFF ;";
                    }
                    else
                    {
                        _insertSQL += "SELECT @@IDENTITY;";
                    }
                    _insertSqlDic[isIdnetityHasValue] = _insertSQL;
                }
                var parameters = new List<object>();
                foreach (RdbColumn column in this.Columns)
                {
                    if (column.CanInsert || isIdnetityHasValue)
                    {
                        var value = column.ReadParameterValue(item);
                        parameters.Add(value);
                    }
                }

                //由于默认是 decimal 类型，所以需要类型转换。
                var idValue = dba.QueryValue(this._insertSQL, parameters.ToArray());
                if (!isIdnetityHasValue)
                {
                    idValue = TypeHelper.CoerceValue((item as IEntityWithId).IdProvider.KeyType, idValue);
                    idColumn.LoadValue(item, idValue);
                }

                //如果实体的 Id 是在插入的过程中生成的，
                //那么需要在插入组合子对象前，先把新生成的父对象 Id 都同步到子列表中。
                item.SyncIdToChildren();
            }
            else
            {
                base.Insert(dba, item);
            }
        }
    }
}
