//using System;
//using System.Reflection;
//using OEA.Utils;
//using OEA.ManagedProperty;
//using System.Collections.Generic;
//using OEA.MetaModel.Attributes;

//namespace OEA.ORM.sqlserver
//{
//    public class SqlTableFactory
//    {
//        public static readonly SqlTableFactory Instance = new SqlTableFactory();

//        private SqlTableFactory() { }

//        private Dictionary<string, SqlTable> cache = new Dictionary<string, SqlTable>(50);

//        public SqlTable Build(Type type)
//        {
//            SqlTable table = null;

//            if (!cache.TryGetValue(type.FullName, out table))
//            {
//                lock (cache)
//                {
//                    if (!cache.TryGetValue(type.FullName, out table))
//                    {
//                        table = BuildTable(type);
//                        cache.Add(type.FullName, table);
//                    }
//                }
//            }

//            return table;
//        }

//        private SqlTable BuildTable(Type type)
//        {
//            SqlTable table = null;
//            if (!type.IsDefined(typeof(TableAttribute), false))
//            {
//                if (!type.IsDefined(typeof(SPResultAttribute), false))
//                    throw new LightException("no TableAttribute or SPResultAttribute found on " + type.FullName);
//                else
//                    table = new SqlTable(type, null, null);
//            }
//            else
//            {
//                TableAttribute tableAttr = (TableAttribute)type.GetCustomAttributes(typeof(TableAttribute), false)[0];
//                string name = tableAttr.Name;
//                string schema = tableAttr.Schema;
//                if (name == null || name.Length == 0)
//                    name = type.Name;
//                table = new SqlTable(type, name, schema);
//            }

//            ProcessProperties(type, table);

//            return table;
//        }

//        private void ProcessProperties(Type type, SqlTable table)
//        {
//            PropertyInfo[] fields = type.GetProperties(
//                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
//            foreach (PropertyInfo field in fields)
//            {
//                if (!field.IsDefined(typeof(ColumnAttribute), false))
//                    continue;
//                ColumnAttribute colAttr = (ColumnAttribute)
//                    field.GetCustomAttributes(typeof(ColumnAttribute), false)[0];
//                //edit by zhoujg
//                IDataBridge data;
//                FieldInfo cslaPropertyInfo = GetCSLAField(type, field);
//                if (null != cslaPropertyInfo)
//                    data = new ManagedPropertyBridge((IManagedProperty)(cslaPropertyInfo.GetValue(null)));
//                else
//                    data = new PropertyBridge(field);

//                if (colAttr.ColumnName == null || colAttr.ColumnName.Length == 0)
//                    colAttr.ColumnName = field.Name;
//                SqlColumn column = new SqlColumn(table, colAttr.ColumnName, data);
//                column.PropertyName = field.Name;
//                if (field.IsDefined(typeof(IDAttribute), false))
//                    column.IsID = true;
//                if (field.IsDefined(typeof(PKAttribute), false))
//                    column.IsPK = true;
//                table.Add(column);
//            }
//        }

//        /// <summary>
//        /// 查询CSLA静态属性时，应该从整个类型树找，而不只是当前类型。
//        /// </summary>
//        /// <param name="type"></param>
//        /// <param name="field"></param>
//        /// <returns></returns>
//        private static FieldInfo GetCSLAField(Type type, PropertyInfo field)
//        {
//            while (type != null)
//            {
//                FieldInfo cslaPropertyInfo = type.GetField(field.Name + "Property", BindingFlags.Static | BindingFlags.NonPublic);
//                if (cslaPropertyInfo != null)
//                {
//                    return cslaPropertyInfo;
//                }
//                type = type.BaseType;
//            }

//            return null;
//        }
//    }
//}
