/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150607
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150607 21:53
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.ManagedProperty;
using Rafy.Reflection;

namespace Rafy.Domain
{
    public static class EntityListExtension
    {
        /// <summary>
        /// 把指定的实体列表中的数据完全转换到一个 DataTable 中。
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static DataTable ToDataTable(this IList<Entity> list)
        {
            DataTable table = new DataTable();

            //找到属性容器
            List<IManagedProperty> availableProperties = null;

            var enumerator = CompositionEnumerator.Create(list, includesChildren: false, includesTreeChildren: true);

            foreach (var item in enumerator)
            {
                if (availableProperties == null)
                {
                    availableProperties = new List<IManagedProperty>();

                    foreach (var property in item.GetRepository().EntityMeta.ManagedProperties.GetCompiledProperties())
                    {
                        if (item.IsDisabled(property)) continue;
                        table.Columns.Add(property.Name, TypeHelper.IgnoreNullable(property.PropertyType));
                        availableProperties.Add(property);
                    }
                }

                var row = table.NewRow();

                for (int j = 0, c2 = availableProperties.Count; j < c2; j++)
                {
                    var property = availableProperties[j];
                    var value = item.GetProperty(property);
                    row[j] = value ?? DBNull.Value;
                }

                table.Rows.Add(row);
            }

            return table;
        }
    }
}