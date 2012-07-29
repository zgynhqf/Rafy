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
using hxy.Common.Data;
using OEA.Library;
using OEA.MetaModel;
using OEA.Utils;
using OEA.ManagedProperty;

namespace OEA.ORM.Oracle
{
    internal class OracleTable : DbTable
    {
        public OracleTable(EntityMeta meta) : base(meta) { }

        public override int Insert(IDb db, IEntity item)
        {
            if (this.PKID != null)
            {
                var sql = string.Format(@"SELECT SQ_{0}_{1} .nextval from dual",
                    this.Prepare(this.Name), this.Prepare(this.PKID.Name));

                var value = db.DBA.QueryValueNormal(sql);
                this.PKID.LoadValue(item, value);
            }

            var result = base.Insert(db, item);

            return result;
        }

        protected override bool CanInsert(DbColumn column)
        {
            return column.IsReadable;//&& !column.IsPKID
        }

        internal override string Quote(string identifier)
        {
            //return this.Prepare(identifier);
            return "\"" + this.Prepare(identifier) + "\"";
        }

        internal override string Prepare(string identifier)
        {
            return identifier.ToUpper();
        }
    }
}