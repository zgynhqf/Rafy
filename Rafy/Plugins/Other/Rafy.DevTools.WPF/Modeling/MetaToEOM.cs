/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130410 10:44
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130410 10:44
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Reflection;
using Rafy.Domain;
using Rafy.EntityObjectModel;
using Rafy.ManagedProperty;
using Rafy.MetaModel;

namespace Rafy.DevTools.Modeling
{
    /// <summary>
    /// 把 EntityMeta 转换为 EOMGroup
    /// </summary>
    class MetaToEOM
    {
        private IEnumerable<EntityMeta> _metaList;

        private EOMGroup _result;

        public MetaToEOM(IEnumerable<EntityMeta> metaList)
        {
            _metaList = metaList;
        }

        public EOMGroup Read()
        {
            Reset();

            foreach (var meta in _metaList)
            {
                AddEntity(meta);
            }

            AddRelations();

            return _result;
        }

        private void Reset()
        {
            _result = new EOMGroup();
        }

        #region AddEntity

        private void AddEntity(EntityMeta meta)
        {
            var allProperties = meta.ManagedProperties.GetCompiledProperties();

            //添加这个类型以及它的基类型
            var types = TypeHelper.GetHierarchy(meta.EntityType, typeof(Entity));
            foreach (var type in types)
            {
                //如果还没有添加了这个类型，则尝试添加到结果中。
                var exist = _result.EntityTypes.Find(type.FullName);
                if (exist == null)
                {
                    var properties = allProperties.Where(mp => mp.DeclareType == type).ToArray();
                    AddEntity(type, properties);
                }
            }
        }

        private EntityType AddEntity(Type entityType, IList<IManagedProperty> properties)
        {
            var type = new EntityType();
            type.Name = entityType.Name;
            type.FullName = entityType.FullName;
            if (entityType.BaseType != null)
            {
                type[BaseTypeFullNameProperty] = entityType.BaseType.FullName;
            }

            foreach (var property in properties)
            {
                if (property is IRefIdProperty)
                {
                    var refProperty = property as IRefProperty;

                    var reference = new Reference();
                    reference.ReferenceType = (Rafy.EntityObjectModel.ReferenceType)refProperty.ReferenceType;//两个枚举的值是相等的。
                    reference.IdProperty = property.Name;
                    reference.Nullable = refProperty.Nullable;
                    if (refProperty.RefEntityProperty != null)
                    {
                        reference.EntityProperty = refProperty.RefEntityProperty.Name;
                    }
                    reference[MPProperty] = refProperty;

                    type.References.Add(reference);
                    continue;
                }
                if (property is IRefEntityProperty) continue;

                if (property is IListProperty)
                {
                    var listProperty = property as IListProperty;
                    if (listProperty.HasManyType == HasManyType.Composition)
                    {
                        var child = new Child();
                        child.Name = listProperty.Name;
                        child.ListTypeFullName = listProperty.PropertyType.FullName;
                        child[MPProperty] = listProperty;

                        type.Children.Add(child);
                        continue;
                    }
                }

                var valueProperty = ConvertToValueProperty(property);
                type.ValueProperties.Add(valueProperty);
            }

            _result.EntityTypes.Add(type);

            return type;
        }

        private ValueProperty ConvertToValueProperty(IManagedProperty property)
        {
            var valueProperty = new ValueProperty();
            valueProperty.Name = property.Name;

            var propertyType = property.PropertyType;

            //处理可空类型
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                propertyType = propertyType.GetGenericArguments()[0];
                valueProperty.Nullable = true;
            }

            if (propertyType == typeof(string))
            {
                valueProperty.PropertyType = ValuePropertyType.String;
            }
            else if (propertyType == typeof(bool))
            {
                valueProperty.PropertyType = ValuePropertyType.Boolean;
            }
            else if (propertyType == typeof(int))
            {
                valueProperty.PropertyType = ValuePropertyType.Int;
            }
            else if (propertyType == typeof(double))
            {
                valueProperty.PropertyType = ValuePropertyType.Double;
            }
            else if (propertyType == typeof(DateTime))
            {
                valueProperty.PropertyType = ValuePropertyType.DateTime;
            }
            else if (propertyType == typeof(byte[]))
            {
                valueProperty.PropertyType = ValuePropertyType.Bytes;
            }
            else if (propertyType.IsEnum)
            {
                valueProperty.PropertyType = ValuePropertyType.Enum;
                valueProperty.EnumType = AddEnumIf(propertyType);
            }
            else
            {
                valueProperty.PropertyType = ValuePropertyType.Unknown;
            }

            return valueProperty;
        }

        #endregion

        private EnumType AddEnumIf(Type enumType)
        {
            var res = _result.EnumTypes.FirstOrDefault(t => t.TypeFullName == enumType.FullName);
            if (res == null)
            {
                res = new EnumType();

                res.Name = enumType.Name;
                res.TypeFullName = enumType.FullName;

                var names = Enum.GetNames(enumType);
                foreach (var name in names)
                {
                    res.Items.Add(new EnumItem
                    {
                        Name = name
                    });
                }

                _result.EnumTypes.Add(res);
            }

            return res;
        }

        #region AddRelations

        private const string BaseTypeFullNameProperty = "BaseTypeFullNameProperty";

        private const string MPProperty = "RefEntityTypeProperty";

        private void AddRelations()
        {
            var types = _result.EntityTypes;
            foreach (var type in types)
            {
                //处理基类关系
                var baseTypeFullName = type[BaseTypeFullNameProperty] as string;
                if (baseTypeFullName != null)
                {
                    type[BaseTypeFullNameProperty] = null;

                    var baseType = types.Find(baseTypeFullName);
                    if (baseType != null) { type.BaseType = baseType; }
                }

                //处理引用关系
                foreach (var reference in type.References)
                {
                    var refProperty = reference[MPProperty] as IRefProperty;
                    reference[MPProperty] = null;

                    var refType = types.Find(refProperty.RefEntityType.FullName);
                    if (refType != null) { reference.RefEntityType = refType; }
                }

                //处理子类型关系
                foreach (var child in type.Children)
                {
                    var listProperty = child[MPProperty] as IListProperty;
                    child[MPProperty] = null;

                    var childType = types.Find(listProperty.ListEntityType.FullName);
                    if (childType != null) { child.ChildEntityType = childType; }
                }
            }
        }

        #endregion
    }
}