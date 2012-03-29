using System;
using System.Data;
using OEA.Library;

namespace OEA.ORM
{
    public interface ITable
    {
        Type Class { get; }
        string Name { get; }
        string Schema { get; }
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
    }
}
