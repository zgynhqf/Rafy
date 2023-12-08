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
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel.Attributes;
using Rafy.Reflection;

namespace Rafy.MetaModel
{
    /// <summary>
    /// 为元数据扩展的配置 API
    /// </summary>
    public static class MetaExtension
    {
        /// <summary>
        /// 指定所有属性全部映射数据库字段
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        public static EntityMeta MapAllProperties(this EntityMeta meta)
        {
            return MapAllPropertiesExcept(meta, null);
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
        public static EntityMeta MapAllPropertiesExcept(this EntityMeta meta, params IManagedProperty[] exceptProperties)
        {
            IEnumerable<IManagedProperty> properties = meta.ManagedProperties.GetNonReadOnlyCompiledProperties()
                .Where(p => !(p is IListProperty) && !(p is IRefProperty)
                    && p != EntityConvention.Property_TreeIndex
                    && p != EntityConvention.Property_TreePId
                    && p != EntityConvention.Property_IsPhantom
                );
            if (exceptProperties != null)
            {
                properties = properties.Except(exceptProperties);
            }

            return meta.MapProperties(properties.ToArray());
        }

        /// <summary>
        /// 指定该实体类型中的某些属性直接映射数据库字段
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static EntityMeta MapProperties(this EntityMeta meta, params IManagedProperty[] properties)
        {
            foreach (var p in properties)
            {
                var ep = meta.Property(p);
                ep.MapColumn();
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

            MapDefaultColumns(meta);

            return meta;
        }

        /// <summary>
        /// 指定某实体映射某个表。
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        public static EntityMeta MapTable(this EntityMeta meta)
        {
            return MapTable(meta, meta.EntityType.Name);
        }

        /// <summary>
        /// 指定某实体映射某个表，指定表名。
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static EntityMeta MapTable(this EntityMeta meta, string tableName)
        {
            if (tableName == null) throw new ArgumentNullException(nameof(tableName));

            if (meta.TableMeta == null)
            {
                meta.TableMeta = new TableMeta();
            }

            meta.TableMeta.TableName = tableName;

            MapDefaultColumns(meta);

            return meta;
        }

        /// <summary>
        /// 指定某实体映射某个虚拟的视图。
        /// 当映射视图时，不会生成数据库表，仓库中也需要在所有的查询中都编写自定义查询。
        /// </summary>
        /// <param name="meta">The meta.</param>
        /// <param name="mapDefaultProperties">设置为 true，则会自动映射 Id、TreeId、TreePId 以及有 ColumnAttribute 标签的属性。</param>
        /// <returns></returns>
        ///// <param name="viewSql">
        ///// 可以是一个数据库视图，也可以是一个能查询出数据的 Sql 语句。
        ///// 
        ///// 如果不指定此参数，则需要在所有的查询中都编写自定义查询。
        ///// </param>
        public static EntityMeta MapView(this EntityMeta meta, bool mapDefaultProperties = true)//, string viewSql = null)
        {
            if (meta.TableMeta == null)
            {
                meta.TableMeta = new TableMeta(meta.EntityType.Name);
            }
            meta.TableMeta.IsMappingView = true;
            //meta.TableMeta.ViewSql = viewSql;

            if (mapDefaultProperties)
            {
                MapDefaultColumns(meta);
            }

            return meta;
        }

        /// <summary>
        /// 映射 Id、TreePId、TreeIndex 以及标记了 <see cref="ColumnAttribute"/> 的属性。
        /// </summary>
        /// <param name="meta"></param>
        private static void MapDefaultColumns(EntityMeta meta)
        {
            if (meta.TableMeta != null)
            {
                foreach (var ep in meta.EntityProperties)
                {
                    if (ep.ColumnMeta != null) continue;

                    var mp = ep.ManagedProperty;
                    //Id 属性，默认的元数据中，即是主键，也是自增长列。应用层可以考虑再做配置。
                    if (mp == EntityConvention.Property_Id)
                    {
                        ep.ColumnMeta = new ColumnMeta
                        {
                            ColumnName = mp.Name,
                            IsPrimaryKey = true,
                        };
                        if (meta.IdType == typeof(int) || meta.IdType == typeof(long))
                        {
                            ep.ColumnMeta.IsIdentity = true;
                        }
                    }
                    //其它标记了 ColumnAttribute 的属性
                    else if (mp != EntityConvention.Property_TreeIndex && mp != EntityConvention.Property_TreePId)
                    {
                        var clrProperty = ep.CLRProperty;
                        if (clrProperty != null)
                        {
                            var columnAttri = clrProperty.GetSingleAttribute<ColumnAttribute>();
                            if (columnAttri != null)
                            {
                                var columnMeta = ep.MapColumn();
                                var name = columnAttri.ColumnName;
                                if (!string.IsNullOrWhiteSpace(name))
                                {
                                    columnMeta.HasColumnName(name);
                                }
                            }
                        }
                    }
                }

                //树的两个属性
                if (meta.IsTreeEntity)
                {
                    var p = meta.Property(EntityConvention.Property_TreeIndex);
                    if (p != null && p.ColumnMeta == null) { p.MapColumn(); }

                    p = meta.Property(EntityConvention.Property_TreePId);
                    if (p != null && p.ColumnMeta == null) { p.MapColumn(); }
                }
            }
        }

        /// <summary>
        /// 指定某个属性是否需要直接映射字段
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        public static EntityPropertyMeta DontMapColumn(this EntityPropertyMeta meta)
        {
            meta.ColumnMeta = null;

            return meta;
        }

        /// <summary>
        /// 开始指定某个属性直接映射字段的详细信息
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        public static ColumnMeta MapColumn(this EntityPropertyMeta meta)
        {
            if (meta.ManagedProperty is IRefProperty)
            {
                throw new InvalidOperationException(string.Format(
                    "引用实体属性 {0} 不能映射数据库，请使用相应的 id 属性。", meta.Name
                    ));
            }

            if (meta.ColumnMeta == null)
            {
                meta.ColumnMeta = new ColumnMeta
                {
                    ColumnName = meta.Name
                };

                if (RefPropertyHelper.IsRefKeyProperty(meta.ManagedProperty, out var refP) && refP.KeyPropertyOfRefEntity == Entity.IdProperty)
                {
                    meta.ColumnMeta.HasFKConstraint = true;
                }
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
        /// 声明实体指定的一个属性的值是通过指定的引用关系来获取。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="meta"></param>
        /// <param name="property">当前实体中的指定值属性。</param>
        /// <param name="refValuePath">通过引用关系到达值属性的路径的表达式。</param>
        /// <param name="dataMode">关系数据获取的方式</param>
        public static void MapRefValue<TEntity>(this EntityMeta meta, Expression<Func<TEntity, object>> property, Expression<Func<TEntity, object>> refValuePath, ReferenceValueDataMode dataMode = ReferenceValueDataMode.ReadJoinTable)
        {
            var propertyName = Reflect.GetProperty(property).Name;
            var propertyMeta = meta.Property(propertyName);
            MapRefValue(propertyMeta, refValuePath, dataMode);
        }

        /// <summary>
        /// 声明一个属性的值是通过指定的引用关系来获取。
        /// </summary>
        /// <param name="meta">当前实体中的指定值属性。</param>
        /// <param name="refValuePath">通过引用关系到达值属性的路径的表达式。</param>
        /// <param name="dataMode">关系数据获取的方式</param>
        public static void MapRefValue<TEntity>(this EntityPropertyMeta meta, Expression<Func<TEntity, object>> refValuePath, ReferenceValueDataMode dataMode = ReferenceValueDataMode.ReadJoinTable)
        {
            var properties = new List<PropertyInfo>();
            var memberExp = Reflect.GetMemberExpression(refValuePath);
            while (true)
            {
                var property = memberExp.Member as PropertyInfo;
                if (property == null) break;
                properties.Insert(0, property);
                memberExp = memberExp.Expression as MemberExpression;
                if (memberExp == null) break;
            }

            var mpPath = new List<object>();
            var propertyOwnerType = meta.Owner.EntityType;
            foreach (var property in properties)
            {
                var mpList = ManagedPropertyRepository.Instance.GetTypePropertiesContainer(propertyOwnerType).GetNonReadOnlyCompiledProperties();
                var mp = mpList.Find(property.Name);
                if (mp.OwnerType == propertyOwnerType)
                {
                    mpPath.Add(mp);
                }
                else
                {
                    mpPath.Add(new ConcreteProperty(mp, propertyOwnerType));
                }

                if (mp is IRefProperty refP)
                {
                    propertyOwnerType = refP.RefEntityType;
                }
            }

            var rvPath = new ReferenceValuePath(mpPath.ToArray());
            MapRefValue(meta, rvPath, dataMode);
        }

        private static void MapRefValue(this EntityPropertyMeta meta, ReferenceValuePath path, ReferenceValueDataMode dataMode = ReferenceValueDataMode.ReadJoinTable)
        {
            var cm = meta.MapColumn();
            if (dataMode == ReferenceValueDataMode.Redundancy)
            {
                meta.ManagedProperty.CastTo<IPropertyInternal>().AsRedundantOf(path);
            }
            else
            {
                cm.RefValuePath = path;
            }
            cm.RefValueDataMode = dataMode;
        }

        /// <summary>
        /// 指定某个属性映射字段时的数据列类型
        /// </summary>
        /// <param name="meta">The meta.</param>
        /// <param name="dataType">Type of the data.</param>
        /// <returns></returns>
        public static ColumnMeta HasDbType(this ColumnMeta meta, DbType dataType)
        {
            meta.DbType = dataType;
            return meta;
        }

        /// <summary>
        /// 指定某个属性映射字段时的列的长度、精度等信息。
        /// 注意，这个属性的变化，不会自动同步到数据库上。
        /// </summary>
        /// <param name="meta">The meta.</param>
        /// <param name="length">
        /// 映射数据库中的字段的长度。
        /// 可以是数字，也可以是 MAX 等字符串。
        /// 如果是空，则表示使用默认的长度。
        /// </param>
        /// <returns></returns>
        public static ColumnMeta HasLength(this ColumnMeta meta, string length)
        {
            meta.DbTypeLength = length;
            return meta;
        }

        /// <summary>
        /// 指定某个属性映射字段时的是否为必需的
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        public static ColumnMeta IsRequired(this ColumnMeta meta)
        {
            meta.IsRequired = true;
            return meta;
        }

        /// <summary>
        /// 指定某个属性映射字段时的是否为必需的
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        public static ColumnMeta IsNullable(this ColumnMeta meta)
        {
            meta.IsRequired = false;
            return meta;
        }

        /// <summary>
        /// 指定某个属性映射字段时是否为主键。
        /// 
        /// 一般情况下，直接使用 Id 为主键。
        /// 但是在映射一些旧数据库的表时，可以保留原来的主键。而只让 Id 映射的字段保持自增长即可。
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ColumnMeta IsPrimaryKey(this ColumnMeta meta, bool value = true)
        {
            meta.IsPrimaryKey = value;
            return meta;
        }

        /// <summary>
        /// 指定某个属性映射字段时是否为自增列。
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ColumnMeta IsIdentity(this ColumnMeta meta, bool value = true)
        {
            meta.IsIdentity = value;
            return meta;
        }

        /// <summary>
        /// 指定某个属性映射字段时是否拥有索引。
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ColumnMeta HasIndex(this ColumnMeta meta, bool value = true)
        {
            meta.HasIndex = value;
            return meta;
        }

        /// <summary>
        /// 设置某个列是否需要映射外键。
        /// 如果出现循环引用的外键，则可以使用此方法来忽略某个列的外键，使得数据库生成时不生成该外键引用。
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ColumnMeta IsForeignKey(this ColumnMeta meta, bool value = true)
        {
            meta.HasFKConstraint = value;
            return meta;
        }
    }
}