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
            var entity = CreateEntity(table.Class);

            foreach (var column in table.Columns)
            {
                object val = row[column.Name];
                column.SetValue(entity, val);
            }

            return entity;
        }

        internal static Entity CreateEntity(Type entityType)
        {
            var entity = Activator.CreateInstance(entityType, true) as Entity;
            if (entity == null) throw new NotSupportedException("只支持实体类型");

            entity.NotifyLoaded(null);
            return entity;
        }
    }
}
