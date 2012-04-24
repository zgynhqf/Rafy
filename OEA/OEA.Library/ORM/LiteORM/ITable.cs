using System;
using System.Data;
using OEA.Library;

namespace OEA.ORM
{
    public interface ITable
    {
        Type Class { get; }
        string Name { get; }
        IColumn[] Columns { get; }
    }

    public static class ITableExtension
    {
        public static object Translate(this ITable table, DataRow row)
        {
            var entity = Entity.New(table.Class);
            entity.Status = PersistenceStatus.Unchanged;

            foreach (var column in table.Columns)
            {
                object val = row[column.Name];
                column.SetValue(entity, val);
            }

            return entity;
        }

        /// <summary>
        /// 直接把data中的数据读取到entity中。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="data"></param>
        public static void SetValues(this ITable table, object entity, IResultSet data)
        {
            foreach (var column in table.Columns)
            {
                object val = data[column.Name];
                column.SetValue(entity, val);
            }
        }
    }
}
