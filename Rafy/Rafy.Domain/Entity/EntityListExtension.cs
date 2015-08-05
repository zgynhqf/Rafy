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
        public static DataTable ToDataTable(this EntityList list)
        {
            DataTable table = new DataTable();

            //找到属性容器
            var container = list.GetRepository().EntityMeta.ManagedProperties;
            var properties = container.GetCompiledProperties();

            foreach (var property in properties)
            {
                //table.Columns.Add(property.Name, property.PropertyType);
                table.Columns.Add(property.Name, TypeHelper.IgnoreNullable(property.PropertyType));
            }

            list.EachNode(item =>
            {
                var row = table.NewRow();

                for (int j = 0, c2 = properties.Count; j < c2; j++)
                {
                    var property = properties[j];
                    var value = item.GetProperty(property);
                    row[j] = value ?? DBNull.Value;
                }

                table.Rows.Add(row);

                return false;
            });

            return table;
        }
    }
}
