using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.MetaModel
{
    public class DBConvention
    {
        /// <summary>
        /// 约定属性名
        /// 如果某个类有这个属性，则表示它是自关联外键
        /// </summary>
        public const string FieldName_PId = "PId";

        public const string FieldName_TreePId = "TreePId";

        public const string FieldName_TreeCode = "TreeCode";

        public const string FieldName_Id = "Id";

        public static string TableName(Type entityType)
        {
            return entityType.Name;
        }

        public static string ColumnName(string propertyName)
        {
            return propertyName;
        }
    }
}