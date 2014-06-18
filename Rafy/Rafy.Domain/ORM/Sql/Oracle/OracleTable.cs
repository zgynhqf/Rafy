/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120429
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120429
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Rafy.Data;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.Utils;
using Rafy.ManagedProperty;
using System.IO;
using Rafy.Reflection;

namespace Rafy.Domain.ORM.Oracle
{
    internal class OracleTable : SqlOraTable
    {
        public OracleTable(IRepositoryInternal repository) : base(repository) { }

        internal override DbColumn CreateColumn(string columnName, PropertyMeta property)
        {
            return new OracleColumn(this, columnName, property);
        }

        private string _selectSEQSql;

        public override int Insert(IDbAccesser dba, Entity item)
        {
            var idColumn = this.IdentityColumn;
            if (idColumn != null)
            {
                if (_selectSEQSql == null)
                {
                    var seqName = new StringWriter();
                    seqName.Write("SEQ_");
                    this.AppendPrepare(seqName, this.Name);
                    seqName.Write('_');
                    this.AppendPrepare(seqName, idColumn.Name);
                    var seqNameValue = Rafy.DbMigration.Oracle.OracleMigrationProvider.LimitOracleIdentifier(seqName.ToString());

                    //此序列是由 DbMigration 中自动生成的。
                    _selectSEQSql = string.Format(@"SELECT {0} .nextval from dual", seqNameValue);
                }
                //由于默认可能不是 int 类型，所以需要类型转换。
                var value = dba.RawAccesser.QueryValue(_selectSEQSql);
                value = TypeHelper.CoerceValue(item.KeyProvider.KeyType, value);
                idColumn.LoadValue(item, value);

                //如果实体的 Id 是在插入的过程中生成的，
                //那么需要在插入组合子对象前，先把新生成的父对象 Id 都同步到子列表中。
                item.SyncIdToChildren();
            }

            var result = base.Insert(dba, item);

            return result;
        }

        protected override bool CanInsert(DbColumn column)
        {
            return true;//&& !column.IsPKID
        }

        internal override void AppendQuote(TextWriter sql, string identifier)
        {
            sql.Write("\"");
            this.AppendPrepare(sql, identifier);
            sql.Write("\"");
        }

        internal override void AppendPrepare(TextWriter sql, string identifier)
        {
            sql.Write(identifier.ToUpper());
        }

        public override SqlGenerator CreateSqlGenerator()
        {
            return new OracleSqlGenerator();
        }
    }
}