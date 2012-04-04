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
using System.Linq.Expressions;
using OEA.MetaModel;
using OEA.ORM;
using OEA.MetaModel.Attributes;
using OEA.Reflection;

namespace OEA.Library
{
    /// <summary>
    /// Property Register
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public static class P<TEntity>
        where TEntity : Entity
    {
        #region Register

        public static Property<TProperty> Register<TProperty>(Expression<Func<TEntity, TProperty>> propertyExp)
        {
            return RegisterCore<TProperty>(GetPropertyName(propertyExp), new PropertyMetadata<TProperty>());
        }

        public static Property<TProperty> Register<TProperty>(Expression<Func<TEntity, TProperty>> propertyExp, TProperty defaultValue)
        {
            return RegisterCore<TProperty>(GetPropertyName(propertyExp), new PropertyMetadata<TProperty>()
            {
                DefaultValue = defaultValue
            });
        }

        public static Property<TProperty> Register<TProperty>(Expression<Func<TEntity, TProperty>> propertyExp, ManagedPropertyChangedCallBack<TProperty> propertyChangedCallBack)
        {
            return RegisterCore<TProperty>(GetPropertyName(propertyExp), new PropertyMetadata<TProperty>()
            {
                PropertyChangedCallBack = propertyChangedCallBack
            });
        }

        public static Property<TProperty> Register<TProperty>(Expression<Func<TEntity, TProperty>> propertyExp, ManagedPropertyChangedCallBack<TProperty> propertyChangedCallBack, TProperty defaultValue)
        {
            return RegisterCore<TProperty>(GetPropertyName(propertyExp), new PropertyMetadata<TProperty>()
            {
                PropertyChangedCallBack = propertyChangedCallBack,
                DefaultValue = defaultValue
            });
        }

        public static Property<TProperty> Register<TProperty>(Expression<Func<TEntity, TProperty>> propertyExp, PropertyMetadata<TProperty> defaultMeta)
        {
            return RegisterCore<TProperty>(GetPropertyName(propertyExp), defaultMeta);
        }

        private static Property<TProperty> RegisterCore<TProperty>(string propertyName, ManagedPropertyChangedCallBack<TProperty> propertyChangedCallBack, TProperty defaultValue, bool isChild)
        {
            return RegisterCore(propertyName, new PropertyMetadata<TProperty>
            {
                DefaultValue = defaultValue,
                IsChild = isChild,
                PropertyChangedCallBack = propertyChangedCallBack
            });
        }

        private static Property<TProperty> RegisterCore<TProperty>(string propertyName, PropertyMetadata<TProperty> defaultMeta)
        {
            var mp = new Property<TProperty>(typeof(TEntity), propertyName, defaultMeta);

            ManagedPropertyRepository.Instance.RegisterProperty(mp);

            return mp;
        }

        #endregion

        #region RegisterReadOnly

        public static Property<TProperty> RegisterReadOnly<TProperty>(Expression<Func<TEntity, TProperty>> propertyExp, Func<TEntity, TProperty> readOnlyValueProvider, ManagedPropertyChangedCallBack<TProperty> propertyChangedCallBack, params IManagedProperty[] dependencies)
        {
            return RegisterReadOnlyCore(GetPropertyName(propertyExp), readOnlyValueProvider, new PropertyMetadata<TProperty>
            {
                PropertyChangedCallBack = propertyChangedCallBack
            }, dependencies);
        }

        private static Property<TProperty> RegisterReadOnlyCore<TProperty>(string property, Func<TEntity, TProperty> readOnlyValueProvider, PropertyMetadata<TProperty> defaultMeta, params IManagedProperty[] dependencies)
        {
            var mp = new Property<TProperty>(typeof(TEntity), property, defaultMeta);
            mp.AsReadOnly(mpo => readOnlyValueProvider(mpo as TEntity), dependencies);

            ManagedPropertyRepository.Instance.RegisterProperty(mp);

            return mp;
        }

        #endregion

        #region RegisterExtension

        public static Property<TProperty> RegisterExtension<TProperty>(string propertyName)
        {
            return RegisterExtension<TProperty>(propertyName, new PropertyMetadata<TProperty>());
        }

        public static Property<TProperty> RegisterExtension<TProperty>(string propertyName, TProperty defaultValue)
        {
            return RegisterExtension<TProperty>(propertyName, new PropertyMetadata<TProperty>
            {
                DefaultValue = defaultValue
            });
        }

        public static Property<TProperty> RegisterExtension<TProperty>(string propertyName, ManagedPropertyChangedCallBack<TProperty> propertyChangedCallBack)
        {
            return RegisterExtension<TProperty>(propertyName, new PropertyMetadata<TProperty>
            {
                PropertyChangedCallBack = propertyChangedCallBack
            });
        }

        public static Property<TProperty> RegisterExtension<TProperty>(string propertyName, ManagedPropertyChangedCallBack<TProperty> propertyChangedCallBack, TProperty defaultValue)
        {
            return RegisterExtension<TProperty>(propertyName, new PropertyMetadata<TProperty>
            {
                PropertyChangedCallBack = propertyChangedCallBack,
                DefaultValue = defaultValue
            });
        }

        public static Property<TProperty> RegisterExtension<TProperty>(string propertyName, PropertyMetadata<TProperty> defaultMeta)
        {
            var mp = RegisterCore(propertyName, defaultMeta);

            mp.IsExtension = true;

            return mp;
        }

        #endregion

        #region RegisterExtensionReadOnly

        public static Property<TProperty> RegisterExtensionReadOnly<TProperty>(string property, Func<TEntity, TProperty> readOnlyValueProvider, ManagedPropertyChangedCallBack<TProperty> propertyChangedCallBack, params IManagedProperty[] dependencies)
        {
            var mp = RegisterReadOnlyCore(property, readOnlyValueProvider, new PropertyMetadata<TProperty>
            {
                PropertyChangedCallBack = propertyChangedCallBack
            }, dependencies);

            mp.IsExtension = true;

            return mp;
        }

        #endregion

        #region RegisterRef

        public static RefProperty<TRefEntity> RegisterRef<TRefEntity>(
            Expression<Func<TEntity, TRefEntity>> refEntityPropertyExp,
            ReferenceType referenceType
            )
            where TRefEntity : Entity
        {
            var propertyName = GetPropertyName(refEntityPropertyExp);
            var idProperty = propertyName + DBConvention.FieldName_Id;

            return RegisterRefCore<TRefEntity>(new RefPropertyMeta()
            {
                IdProperty = idProperty,
                RefEntityProperty = propertyName,
                ReferenceType = referenceType
            });
        }

        public static RefProperty<TRefEntity> RegisterRef<TRefEntity>(
            Expression<Func<TEntity, object>> idPropertyExp,
            Expression<Func<TEntity, TRefEntity>> refEntityPropertyExp,
            ReferenceType referenceType
            )
            where TRefEntity : Entity
        {
            var propertyName = GetPropertyName(refEntityPropertyExp);
            var idProperty = GetPropertyName(idPropertyExp);

            return RegisterRefCore<TRefEntity>(new RefPropertyMeta()
            {
                IdProperty = idProperty,
                RefEntityProperty = propertyName,
                ReferenceType = referenceType
            });
        }

        public static RefProperty<TRefEntity> RegisterRef<TRefEntity>(
            Expression<Func<TEntity, TRefEntity>> refEntityPropertyExp,
            RefPropertyMeta meta
            )
            where TRefEntity : Entity
        {
            meta.RefEntityProperty = GetPropertyName(refEntityPropertyExp);
            if (string.IsNullOrWhiteSpace(meta.IdProperty))
            {
                meta.IdProperty = meta.RefEntityProperty + DBConvention.FieldName_Id;
            }

            return RegisterRefCore<TRefEntity>(meta);
        }

        public static RefProperty<TRefEntity> RegisterRef<TRefEntity>(
            Expression<Func<TEntity, object>> idPropertyExp,
            Expression<Func<TEntity, TRefEntity>> refEntityPropertyExp,
            RefPropertyMeta meta
            )
            where TRefEntity : Entity
        {
            meta.RefEntityProperty = GetPropertyName(refEntityPropertyExp);
            meta.IdProperty = GetPropertyName(idPropertyExp);

            return RegisterRefCore<TRefEntity>(meta);
        }

        private static RefProperty<TRefEntity> RegisterRefCore<TRefEntity>(RefPropertyMeta core) where TRefEntity : Entity
        {
            var meta = new RefPropertyMetadata<TRefEntity>(core, IsNullable(core.IdProperty));

            var managedPropertyName = LazyEntityRefPropertyInfo.CorrespondingManagedProperty(core.RefEntityProperty);

            var property = new RefProperty<TRefEntity>(typeof(TEntity), managedPropertyName, meta);

            ManagedPropertyRepository.Instance.RegisterProperty(property);

            return property;
        }

        public static RefProperty<TRefEntity> OverrideRefMeta<TRefEntity>(RefProperty<TRefEntity> property, RefPropertyMeta core)
            where TRefEntity : Entity
        {
            //以下两个属性无法从外界设置，所以这里需要进行拷贝。
            var defaultMeta = property.GetMeta(typeof(TEntity)) as IRefPropertyMetadata;
            if (string.IsNullOrWhiteSpace(core.IdProperty)) core.IdProperty = defaultMeta.IdProperty;
            if (string.IsNullOrWhiteSpace(core.RefEntityProperty)) core.RefEntityProperty = defaultMeta.RefEntityProperty;

            var meta = new RefPropertyMetadata<TRefEntity>(core, IsNullable(core.IdProperty));
            property.OverrideMeta(typeof(TEntity), meta);

            return property;
        }

        private static bool IsNullable(string refIdProperty)
        {
            var property = typeof(TEntity).GetProperty(refIdProperty);
            var refIdPropertyType = property.PropertyType;
            return refIdPropertyType.IsGenericType && refIdPropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        #endregion

        public static void UnRegisterAllRuntimeProperties()
        {
            ManagedPropertyRepository.Instance.UnRegisterAllRuntimeProperties(typeof(TEntity));
        }

        #region private static methods

        private static string GetPropertyName<TProperty>(Expression<Func<TEntity, TProperty>> propertyExp)
        {
            return Reflect<TEntity>.GetProperty(propertyExp).Name;
        }

        #endregion
    }

    public static class P
    {
        public static void UnRegister(params IManagedProperty[] properties)
        {
            ManagedPropertyRepository.Instance.UnRegister(properties);
        }

        public static void UnRegister(IEnumerable<IManagedProperty> properties)
        {
            ManagedPropertyRepository.Instance.UnRegister(properties.ToArray());
        }
    }

    ///// <summary>
    ///// OEA Used Only
    ///// </summary>
    //public static class ExtensionPropertyManager
    //{
    //    public static void AddModelForExtentions()
    //    {
    //        foreach (var kv in ManagedPropertyRepository.Instance)
    //        {
    //            var type = kv.Key;
    //            var properties = kv.Value.ConsolidatedContainer.GetCompiledProperties();

    //            if (OEAEnvironment.IsRootType(type))
    //            {
    //                var evm = AppModel.Entities.FindViewMeta(type);
    //                foreach (var mp in properties)
    //                {
    //                    if (mp.IsExtension)
    //                    {
    //                        AppModel.Entities.CreateExtensionPropertyViewMeta(evm, mp);
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}
}