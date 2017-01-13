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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Rafy.DbMigration.Oracle;
using Rafy.MetaModel;
using Rafy.Reflection;

namespace Rafy.Domain.ORM.Oracle
{
    class OracleColumn : RdbColumn
    {
        internal OracleColumn(RdbTable table, IPersistanceColumnInfo columnInfo) : base(table, columnInfo) { }

        public override bool CanInsert
        {
            get
            {
                return true;//&& !this.IsPKID
            }
        }

        public override object ConvertToParameterValue(object value)
        {
            value = base.ConvertToParameterValue(value);

            value = OracleSqlGenerator.PrepareConstraintValueInternal(value);

            return value;
        }

        public override void Write(Entity entity, object value)
        {
            var info = this.Info;
            if (value != null && info.IsBooleanType)
            {
                value = OracleDbTypeHelper.ToCLRBoolean(value);
            }
            else if (value == null && info.IsStringType)//null 转换为空字符串
            {
                value = string.Empty;
            }

            base.Write(entity, value);
        }
    }
}