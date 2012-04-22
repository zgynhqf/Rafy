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
using OEA.MetaModel.View;

namespace OEA.MetaModel
{
    /// <summary>
    /// 为元数据扩展的配置 API
    /// </summary>
    public static class MetaExtensionConfig
    {
        /// <summary>
        /// 默认按照某个属性进行排序
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="property"></param>
        /// <param name="ascending"></param>
        /// <returns></returns>
        public static EntityMeta DataOrderBy(this EntityMeta meta, IManagedProperty property, bool ascending = true)
        {
            if (meta.IsTreeEntity) { throw new NotSupportedException("树型实体不支持修改默认排序规则！"); }

            meta.DefaultOrderBy = meta.Property(property);
            meta.DefaultOrderByAscending = ascending;

            return meta;
        }

        /// <summary>
        /// 指定所有属性全部映射数据库字段
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        public static EntityMeta MapAllPropertiesToTable(this EntityMeta meta)
        {
            var properties = meta.ManagedProperties.GetCompiledProperties()
                .Where(p => !p.IsReadOnly
                    && !(p is IListProperty)
                    && p.Name != DBConvention.FieldName_TreeCode
                    && p.Name != DBConvention.FieldName_TreePId
                ).ToArray();
            return meta.HasColumns(properties);
        }

        /// <summary>
        /// 指定某个实体的所有属性全部映射数据库字段
        /// 同时排除指定的属性列表。
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="exceptProperties">
        /// 这些属性不需要映射数据库
        /// </param>
        /// <returns></returns>
        public static EntityMeta MapAllPropertiesToTableExcept(this EntityMeta meta, params IManagedProperty[] exceptProperties)
        {
            var properties = meta.ManagedProperties.GetCompiledProperties()
                .Where(p => !p.IsReadOnly
                    && !(p is IListProperty)
                    && p.Name != DBConvention.FieldName_TreeCode
                    && p.Name != DBConvention.FieldName_TreePId
                ).Except(exceptProperties)
                .ToArray();

            return meta.HasColumns(properties);
        }

        /// <summary>
        /// 指定该实体类型中的某些属性直接映射数据库字段
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static EntityMeta HasColumns(this EntityMeta meta, params IManagedProperty[] properties)
        {
            foreach (var p in properties)
            {
                var ep = meta.Property(p);
                ep.MapColumn(true);
            }

            return meta;
        }

        /// <summary>
        /// 打开指定实体类型的树型功能
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        public static EntityMeta SupportTree(this EntityMeta meta)
        {
            meta.IsTreeEntity = true;

            EnsureTreeColumns(meta);

            return meta;
        }

        /// <summary>
        /// 指定某实体映射某个表。
        /// 并表明该表是否支持数据库迁移。
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="supprtMigrating"></param>
        /// <returns></returns>
        public static EntityMeta MapTable(this EntityMeta meta, bool supprtMigrating = true)
        {
            meta.TableMeta = new TableMeta(meta.EntityType.Name);
            meta.TableMeta.SupportMigrating = supprtMigrating;

            EnsureTreeColumns(meta);

            return meta;
        }

        /// <summary>
        /// 指定某实体映射某个表，指定表名。
        /// 并表明该表是否支持数据库迁移。
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="tableName"></param>
        /// <param name="supprtMigrating"></param>
        /// <returns></returns>
        public static EntityMeta MapTable(this EntityMeta meta, string tableName, bool supprtMigrating = true)
        {
            meta.TableMeta = new TableMeta(tableName);
            meta.TableMeta.SupportMigrating = supprtMigrating;

            EnsureTreeColumns(meta);

            return meta;
        }

        private static void EnsureTreeColumns(EntityMeta meta)
        {
            if (meta.IsTreeEntity && meta.TableMeta != null)
            {
                var p = meta.Property(DBConvention.FieldName_TreeCode);
                if (p != null)
                {
                    p.MapColumn();
                    meta.DefaultOrderBy = p;
                    meta.DefaultOrderByAscending = true;
                }

                p = meta.Property(DBConvention.FieldName_TreePId);
                if (p != null) p.MapColumn();
            }
        }

        /// <summary>
        /// 指定某个属性是否需要直接映射字段
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="isMappingColumn"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 开始指定某个属性直接映射字段的详细信息
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        public static ColumnMeta MapColumn(this EntityPropertyMeta meta)
        {
            if (meta.ColumnMeta == null)
            {
                meta.ColumnMeta = new ColumnMeta();
            }

            return meta.ColumnMeta;
        }

        /// <summary>
        /// 指定某个属性映射字段时的列名
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static ColumnMeta HasColumnName(this ColumnMeta meta, string columnName)
        {
            meta.ColumnName = columnName;
            return meta;
        }

        /// <summary>
        /// 指定某个属性映射字段时的是否为必需的
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="isRequired"></param>
        /// <returns></returns>
        public static ColumnMeta IsRequired(this ColumnMeta meta, bool isRequired)
        {
            meta.IsRequired = isRequired;
            return meta;
        }
    }
}