/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111110
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111110
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.ManagedProperty;

namespace OEA.MetaModel
{
    /// <summary>
    /// 为元数据扩展的配置 API
    /// </summary>
    public static class MetaExtensionConfig
    {
        public static EntityMeta HasColumns(this EntityMeta meta, params IManagedProperty[] properties)
        {
            foreach (var p in properties)
            {
                var ep = meta.Property(p);
                ep.MapColumn(true);
            }

            return meta;
        }

        public static EntityMeta DefaultOrderBy(this EntityMeta meta, IManagedProperty property, bool ascending = true)
        {
            meta.DefaultOrderBy = meta.Property(property);
            meta.DefaultOrderByAscending = ascending;

            return meta;
        }

        public static EntityMeta MapTable(this EntityMeta meta, bool supprtMigrating = true)
        {
            meta.TableMeta = new TableMeta(meta.EntityType.Name);
            meta.TableMeta.SupportMigrating = supprtMigrating;

            return meta;
        }

        public static EntityMeta MapTable(this EntityMeta meta, string tableName, bool supprtMigrating = true)
        {
            meta.TableMeta = new TableMeta(tableName);
            meta.TableMeta.SupportMigrating = supprtMigrating;

            return meta;
        }

        public static EntityPropertyMeta MapColumn(this EntityPropertyMeta meta, bool isMappingColumn)
        {
            if (isMappingColumn)
            {
                if (meta.ColumnMeta == null)
                {
                    meta.ColumnMeta = new ColumnMeta();
                }
            }
            else
            {
                meta.ColumnMeta = null;
            }

            return meta;
        }

        public static ColumnMeta MapColumn(this EntityPropertyMeta meta)
        {
            if (meta.ColumnMeta == null)
            {
                meta.ColumnMeta = new ColumnMeta();
            }

            return meta.ColumnMeta;
        }

        public static ColumnMeta HasColumnName(this ColumnMeta meta, string columnName)
        {
            meta.ColumnName = columnName;
            return meta;
        }

        public static ColumnMeta IsRequired(this ColumnMeta meta, bool isRequired)
        {
            meta.IsRequired = isRequired;
            return meta;
        }
    }
}