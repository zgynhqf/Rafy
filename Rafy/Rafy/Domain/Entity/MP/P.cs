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
using Rafy.ManagedProperty;
using System.Linq.Expressions;
using Rafy.MetaModel;
using Rafy.Domain.ORM;
using Rafy.MetaModel.Attributes;
using Rafy.Reflection;

namespace Rafy.Domain
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

        public static Property<TProperty> Register<TProperty>(Expression<Func<TEntity, TProperty>> propertyExp, ManagedPropertyChangedCallBack propertyChangedCallBack)
        {
            return RegisterCore<TProperty>(GetPropertyName(propertyExp), new PropertyMetadata<TProperty>()
            {
                PropertyChangedCallBack = propertyChangedCallBack
            });
        }

        public static Property<TProperty> Register<TProperty>(Expression<Func<TEntity, TProperty>> propertyExp, ManagedPropertyChangedCallBack propertyChangedCallBack, TProperty defaultValue)
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

        private static Property<TProperty> RegisterCore<TProperty>(string propertyName, PropertyMetadata<TProperty> defaultMeta)
        {
            var mp = new Property<TProperty>(typeof(TEntity), propertyName, defaultMeta);

            ManagedPropertyRepository.Instance.RegisterProperty(mp);

            return mp;
        }

        #endregion

        #region RegisterLOB

        public static LOBProperty<string> RegisterLOB(Expression<Func<TEntity, string>> propertyExp)
        {
            return RegisterLOBCore<string>(GetPropertyName(propertyExp), new PropertyMetadata<string>());
        }

        public static LOBProperty<byte[]> RegisterLOB(Expression<Func<TEntity, byte[]>> propertyExp)
        {
            return RegisterLOBCore<byte[]>(GetPropertyName(propertyExp), new PropertyMetadata<byte[]>());
        }

        public static LOBProperty<string> RegisterLOB(Expression<Func<TEntity, string>> propertyExp, PropertyMetadata<string> defaultMeta)
        {
            return RegisterLOBCore(GetPropertyName(propertyExp), defaultMeta);
        }

        public static LOBProperty<byte[]> RegisterLOB(Expression<Func<TEntity, byte[]>> propertyExp, PropertyMetadata<byte[]> defaultMeta)
        {
            return RegisterLOBCore(GetPropertyName(propertyExp), defaultMeta);
        }

        private static LOBProperty<TProperty> RegisterLOBCore<TProperty>(string propertyName, PropertyMetadata<TProperty> defaultMeta)
            where TProperty : class
        {
            var mp = new LOBProperty<TProperty>(typeof(TEntity), propertyName, defaultMeta);

            var propertyType = typeof(TProperty);
            if (propertyType == typeof(string))
            {
                mp.LOBType = LOBType.String;
                defaultMeta.DefaultValue = (TProperty)(object)PropertyDefailtValues.DefaultLOBString;
            }
            else
            {
                mp.LOBType = LOBType.Binary;
                defaultMeta.DefaultValue = (TProperty)(object)PropertyDefailtValues.DefaultLOBBinary;
            }

            ManagedPropertyRepository.Instance.RegisterProperty(mp);

            return mp;
        }

        #endregion

        #region RegisterReadOnly

        public static Property<TProperty> RegisterReadOnly<TProperty>(Expression<Func<TEntity, TProperty>> propertyExp, Func<TEntity, TProperty> readOnlyValueProvider, params IManagedProperty[] dependencies)
        {
            return RegisterReadOnlyCore(GetPropertyName(propertyExp), readOnlyValueProvider, new PropertyMetadata<TProperty>(), dependencies);
        }

        public static Property<TProperty> RegisterReadOnly<TProperty>(Expression<Func<TEntity, TProperty>> propertyExp, Func<TEntity, TProperty> readOnlyValueProvider, PropertyMetadata<TProperty> defaultMeta, params IManagedProperty[] dependencies)
        {
            return RegisterReadOnlyCore(GetPropertyName(propertyExp), readOnlyValueProvider, defaultMeta, dependencies);
        }

        private static Property<TProperty> RegisterReadOnlyCore<TProperty>(string property, Func<TEntity, TProperty> readOnlyValueProvider, PropertyMetadata<TProperty> defaultMeta, params IManagedProperty[] dependencies)
        {
            var mp = new Property<TProperty>(typeof(TEntity), property, defaultMeta);
            mp.AsReadOnly(mpo => readOnlyValueProvider(mpo as TEntity), dependencies);

            ManagedPropertyRepository.Instance.RegisterProperty(mp);

            return mp;
        }

        #endregion

        #region RegisterRedundancy

        /// <summary>
        /// 注册一个冗余属性
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propertyExp"></param>
        /// <param name="path">属性冗余的路径</param>
        /// <returns></returns>
        /// 不使用 lambda 表达式来注册冗余路径，这是因为可能会与属性生命周期冲突，同时也没有这个必要。
        public static Property<TProperty> RegisterRedundancy<TProperty>(Expression<Func<TEntity, TProperty>> propertyExp, RedundantPath path)
        {
            var property = GetPropertyName(propertyExp);

            var mp = new Property<TProperty>(typeof(TEntity), property, new PropertyMetadata<TProperty>());
            mp.AsRedundantOf(path);

            ManagedPropertyRepository.Instance.RegisterProperty(mp);

            return mp;
        }

        /// <summary>
        /// 注册一个冗余扩展属性
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="declareType">Type of the declare.</param>
        /// <param name="path">属性冗余的路径</param>
        /// <returns></returns>
        public static Property<TProperty> RegisterRedundancyExtension<TProperty>(string propertyName, Type declareType, RedundantPath path)
        {
            var mp = new Property<TProperty>(typeof(TEntity), declareType, propertyName, new PropertyMetadata<TProperty>());
            mp.AsRedundantOf(path);

            ManagedPropertyRepository.Instance.RegisterProperty(mp);

            return mp;
        }

        #endregion

        #region RegisterExtension

        public static Property<TProperty> RegisterExtension<TProperty>(string propertyName, Type declareType)
        {
            return RegisterExtension<TProperty>(propertyName, declareType, new PropertyMetadata<TProperty>());
        }

        public static Property<TProperty> RegisterExtension<TProperty>(string propertyName, Type declareType, TProperty defaultValue)
        {
            return RegisterExtension<TProperty>(propertyName, declareType, new PropertyMetadata<TProperty>
            {
                DefaultValue = defaultValue
            });
        }

        public static Property<TProperty> RegisterExtension<TProperty>(string propertyName, Type declareType, ManagedPropertyChangedCallBack propertyChangedCallBack)
        {
            return RegisterExtension<TProperty>(propertyName, declareType, new PropertyMetadata<TProperty>
            {
                PropertyChangedCallBack = propertyChangedCallBack
            });
        }

        public static Property<TProperty> RegisterExtension<TProperty>(string propertyName, Type declareType, ManagedPropertyChangedCallBack propertyChangedCallBack, TProperty defaultValue)
        {
            return RegisterExtension<TProperty>(propertyName, declareType, new PropertyMetadata<TProperty>
            {
                PropertyChangedCallBack = propertyChangedCallBack,
                DefaultValue = defaultValue
            });
        }

        public static Property<TProperty> RegisterExtension<TProperty>(string propertyName, Type declareType, PropertyMetadata<TProperty> defaultMeta)
        {
            return RegisterExtensionCore(propertyName, declareType, defaultMeta);
        }

        private static Property<TProperty> RegisterExtensionCore<TProperty>(string propertyName, Type declareType, PropertyMetadata<TProperty> defaultMeta)
        {
            var mp = new Property<TProperty>(typeof(TEntity), declareType, propertyName, defaultMeta);

            ManagedPropertyRepository.Instance.RegisterProperty(mp);

            return mp;
        }

        #endregion

        #region RegisterExtensionReadOnly

        public static Property<TProperty> RegisterExtensionReadOnly<TProperty>(string property, Type declareType, Func<TEntity, TProperty> readOnlyValueProvider, params IManagedProperty[] dependencies)
        {
            return RegisterExtensionReadOnlyCore(property, declareType, readOnlyValueProvider, new PropertyMetadata<TProperty>(), dependencies);
        }

        public static Property<TProperty> RegisterExtensionReadOnly<TProperty>(string property, Type declareType, Func<TEntity, TProperty> readOnlyValueProvider, ManagedPropertyChangedCallBack propertyChangedCallBack, params IManagedProperty[] dependencies)
        {
            var mp = RegisterExtensionReadOnlyCore(property, declareType, readOnlyValueProvider, new PropertyMetadata<TProperty>
            {
                PropertyChangedCallBack = propertyChangedCallBack
            }, dependencies);

            return mp;
        }

        private static Property<TProperty> RegisterExtensionReadOnlyCore<TProperty>(string property, Type declareType, Func<TEntity, TProperty> readOnlyValueProvider, PropertyMetadata<TProperty> defaultMeta, params IManagedProperty[] dependencies)
        {
            var mp = new Property<TProperty>(typeof(TEntity), declareType, property, defaultMeta);
            mp.AsReadOnly(mpo => readOnlyValueProvider(mpo as TEntity), dependencies);

            ManagedPropertyRepository.Instance.RegisterProperty(mp);

            return mp;
        }

        #endregion

        #region RegisterRef

        /// <summary>
        /// 声明一个引用 Id 属性
        /// </summary>
        /// <param name="propertyExp">指向相应 CLR 的表达式。</param>
        /// <param name="referenceType">引用的类型</param>
        /// <returns></returns>
        public static IRefIdProperty RegisterRefId<TKey>(Expression<Func<TEntity, TKey?>> propertyExp, ReferenceType referenceType)
            where TKey : struct
        {
            var propertyInfo = Reflect<TEntity>.GetProperty(propertyExp);
            return RegisterRefIdCore(propertyInfo.Name, referenceType, propertyInfo.PropertyType, new RegisterRefIdArgs<TKey>());
        }

        /// <summary>
        /// 声明一个引用 Id 属性
        /// </summary>
        /// <param name="propertyExp"></param>
        /// <param name="args">一系列相关的参数。</param>
        /// <returns></returns>
        public static IRefIdProperty RegisterRefId<TKey>(Expression<Func<TEntity, TKey?>> propertyExp, RegisterRefIdArgs<TKey> args)
            where TKey : struct
        {
            var propertyInfo = Reflect<TEntity>.GetProperty(propertyExp);

            //简单地，args 直接作为 defaultMeta 传入。
            return RegisterRefIdCore(propertyInfo.Name, args.ReferenceType, propertyInfo.PropertyType, args);
        }

        /// <summary>
        /// 声明一个引用 Id 属性
        /// </summary>
        /// <param name="propertyExp">指向相应 CLR 的表达式。</param>
        /// <param name="referenceType">引用的类型</param>
        /// <returns></returns>
        public static IRefIdProperty RegisterRefId<TKey>(Expression<Func<TEntity, TKey>> propertyExp, ReferenceType referenceType)
        {
            var propertyInfo = Reflect<TEntity>.GetProperty(propertyExp);
            return RegisterRefIdCore(propertyInfo.Name, referenceType, propertyInfo.PropertyType, new RegisterRefIdArgs<TKey>());
        }

        /// <summary>
        /// 声明一个引用 Id 属性
        /// </summary>
        /// <param name="propertyExp"></param>
        /// <param name="args">一系列相关的参数。</param>
        /// <returns></returns>
        public static IRefIdProperty RegisterRefId<TKey>(Expression<Func<TEntity, TKey>> propertyExp, RegisterRefIdArgs<TKey> args)
        {
            var propertyInfo = Reflect<TEntity>.GetProperty(propertyExp);

            //简单地，args 直接作为 defaultMeta 传入。
            return RegisterRefIdCore(propertyInfo.Name, args.ReferenceType, propertyInfo.PropertyType, args);
        }

        private static IRefIdProperty RegisterRefIdCore<TKey>(string propertyName, ReferenceType referenceType, Type propertyType, PropertyMetadata<TKey> defaultMeta)
        {
            var property = new RefIdProperty<TKey>(typeof(TEntity), propertyName, defaultMeta)
            {
                ReferenceType = referenceType,
                Nullable = propertyType.IsClass || TypeHelper.IsNullable(propertyType)
            };

            ManagedPropertyRepository.Instance.RegisterProperty(property);

            return property;
        }

        /// <summary>
        /// 声明一个引用实体属性。
        /// </summary>
        /// <typeparam name="TRefEntity"></typeparam>
        /// <param name="propertyExp">指向引用实体属性的表达式。</param>
        /// <param name="refIdProperty">对应的引用 Id 属性，将为其建立关联。</param>
        /// <returns></returns>
        public static RefEntityProperty<TRefEntity> RegisterRef<TRefEntity>(Expression<Func<TEntity, TRefEntity>> propertyExp, IRefIdProperty refIdProperty)
            where TRefEntity : Entity
        {
            if (refIdProperty == null) throw new ArgumentNullException("refIdProperty", "必须指定引用 Id 属性，将为其建立关联。");

            var defaultMeta = new PropertyMetadata<Entity>();

            var property = new RefEntityProperty<TRefEntity>(typeof(TEntity), GetPropertyName(propertyExp), defaultMeta)
            {
                RefIdProperty = refIdProperty
            };

            //默认只从服务端序列化到客户端。
            defaultMeta.Serializable = RafyEnvironment.IsOnServer();

            ManagedPropertyRepository.Instance.RegisterProperty(property);

            return property;
        }

        /// <summary>
        /// 声明一个引用实体属性。
        /// </summary>
        /// <typeparam name="TRefEntity"></typeparam>
        /// <param name="propertyExp">指向引用实体属性的表达式。</param>
        /// <param name="args">引用实体属性相应的参数对象。</param>
        /// <returns></returns>
        public static RefEntityProperty<TRefEntity> RegisterRef<TRefEntity>(Expression<Func<TEntity, TRefEntity>> propertyExp, RegisterRefArgs args)
            where TRefEntity : Entity
        {
            if (args == null) throw new ArgumentNullException("args");
            if (args.RefIdProperty == null) throw new ArgumentNullException("args.RefIdProperty", "必须指定引用 Id 属性，将为其建立关联。");

            //简单地，直接把 Args 作为 defaultMeta
            var defaultMeta = args;

            var property = new RefEntityProperty<TRefEntity>(typeof(TEntity), GetPropertyName(propertyExp), defaultMeta)
            {
                RefIdProperty = args.RefIdProperty,
                Loader = args.Loader
            };

            //默认只从服务端序列化到客户端。
            (defaultMeta as PropertyMetadata<Entity>).Serializable = args.Serializable.GetValueOrDefault(RafyEnvironment.IsOnServer());

            ManagedPropertyRepository.Instance.RegisterProperty(property);

            return property;
        }

        /// <summary>
        /// 为某个实体类型重写指定引用属性对应的元数据。
        /// </summary>
        /// <typeparam name="TRefEntity">The type of the reference entity.</typeparam>
        /// <typeparam name="TMeta">The type of the meta.</typeparam>
        /// <param name="property">The property.</param>
        /// <param name="args">The arguments.</param>
        /// <param name="overrideValues">The override values.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">args</exception>
        /// <exception cref="System.InvalidOperationException">不支持重写 Loader 属性。</exception>
        public static RefEntityProperty<TRefEntity> OverrideRefMeta<TRefEntity, TMeta>(
            RefEntityProperty<TRefEntity> property, TMeta args, Action<TMeta> overrideValues
            )
            where TRefEntity : Entity
            where TMeta : RegisterRefArgs
        {
            if (property == null) throw new ArgumentNullException("property");
            if (args == null) throw new ArgumentNullException("args");
            if (overrideValues == null) throw new ArgumentNullException("overrideValues");
            if (args.Loader != null) { throw new InvalidOperationException("不支持重写 Loader 属性。"); }

            //简单地，直接把 Args 作为 meta
            property.OverrideMeta(typeof(TEntity), args, m =>
            {
                //默认只从服务端序列化到客户端。
                (m as PropertyMetadata<Entity>).Serializable = m.Serializable.GetValueOrDefault(RafyEnvironment.IsOnServer());

                overrideValues(m);
            });

            return property;
        }

        #endregion

        #region RegisterRefExtension

        /// <summary>
        /// 扩展一个引用属性
        /// </summary>
        /// <typeparam name="TKey">The type of the entity list.</typeparam>
        /// <param name="propertyName">属性名称。</param>
        /// <param name="declareType">声明此属性的类型。</param>
        /// <param name="isNullable">是否为一个可空的引用属性。</param>
        /// <returns></returns>
        public static IRefIdProperty RegisterRefIdExtension<TKey>(string propertyName, Type declareType, bool isNullable = false)
            where TKey : struct
        {
            var property = new RefIdProperty<TKey>(typeof(TEntity), declareType, propertyName, new RegisterRefIdArgs<TKey>())
            {
                ReferenceType = ReferenceType.Normal,
                Nullable = isNullable
            };

            ManagedPropertyRepository.Instance.RegisterProperty(property);

            return property;
        }

        /// <summary>
        /// 扩展一个引用实体属性。
        /// </summary>
        /// <typeparam name="TRefEntity"></typeparam>
        /// <param name="propertyName">实体属性的名称。</param>
        /// <param name="declareType">声明此属性的类型。</param>
        /// <param name="refIdProperty">对应的引用 Id 属性，将为其建立关联。</param>
        /// <returns></returns>
        public static RefEntityProperty<TRefEntity> RegisterRefExtension<TRefEntity>(string propertyName, Type declareType, IRefIdProperty refIdProperty)
            where TRefEntity : Entity
        {
            if (refIdProperty == null) throw new ArgumentNullException("refIdProperty", "必须指定引用 Id 属性，将为其建立关联。");

            var defaultMeta = new PropertyMetadata<Entity>();

            var property = new RefEntityProperty<TRefEntity>(typeof(TEntity), declareType, propertyName, defaultMeta)
            {
                RefIdProperty = refIdProperty
            };

            //默认只从服务端序列化到客户端。
            defaultMeta.Serializable = RafyEnvironment.IsOnServer();

            ManagedPropertyRepository.Instance.RegisterProperty(property);

            return property;
        }

        #endregion

        #region RegisterList

        /// <summary>
        /// 注册一个列表属性
        /// </summary>
        /// <typeparam name="TEntityList">The type of the entity list.</typeparam>
        /// <param name="propertyExp">The property exp.</param>
        /// <returns></returns>
        public static ListProperty<TEntityList> RegisterList<TEntityList>(Expression<Func<TEntity, TEntityList>> propertyExp)
            where TEntityList : EntityList
        {
            return RegisterListCore<TEntityList>(GetPropertyName(propertyExp), new ListPropertyMeta());
        }

        /// <summary>
        /// 注册一个列表属性
        /// </summary>
        /// <typeparam name="TEntityList"></typeparam>
        /// <param name="propertyExp"></param>
        /// <param name="meta"></param>
        /// <returns></returns>
        public static ListProperty<TEntityList> RegisterList<TEntityList>(Expression<Func<TEntity, TEntityList>> propertyExp, ListPropertyMeta meta)
            where TEntityList : EntityList
        {
            return RegisterListCore<TEntityList>(GetPropertyName(propertyExp), meta);
        }

        private static ListProperty<TEntityList> RegisterListCore<TEntityList>(string propertyName, ListPropertyMeta args)
            where TEntityList : EntityList
        {
            var meta = new ListPropertyMetadata<TEntityList>(args.DataProvider);

            var property = new ListProperty<TEntityList>(typeof(TEntity), propertyName, meta);

            property._hasManyType = args.HasManyType;

            ManagedPropertyRepository.Instance.RegisterProperty(property);

            return property;
        }

        #endregion

        #region RegisterListExtension

        /// <summary>
        /// 扩展一个列表属性
        /// </summary>
        /// <typeparam name="TEntityList">The type of the entity list.</typeparam>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="declareType">Type of the declare.</param>
        /// <returns></returns>
        public static ListProperty<TEntityList> RegisterListExtension<TEntityList>(string propertyName, Type declareType)
            where TEntityList : EntityList
        {
            return RegisterListExtensionCore<TEntityList>(propertyName, declareType, new ListPropertyMeta());
        }

        /// <summary>
        /// 扩展一个列表属性
        /// </summary>
        /// <typeparam name="TEntityList">The type of the entity list.</typeparam>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="declareType">Type of the declare.</param>
        /// <param name="meta">The meta.</param>
        /// <returns></returns>
        public static ListProperty<TEntityList> RegisterListExtension<TEntityList>(string propertyName, Type declareType, ListPropertyMeta meta)
            where TEntityList : EntityList
        {
            return RegisterListExtensionCore<TEntityList>(propertyName, declareType, meta);
        }

        private static ListProperty<TEntityList> RegisterListExtensionCore<TEntityList>(string propertyName, Type declareType, ListPropertyMeta args)
            where TEntityList : EntityList
        {
            var meta = new ListPropertyMetadata<TEntityList>(args.DataProvider);

            var property = new ListProperty<TEntityList>(typeof(TEntity), declareType, propertyName, meta);

            property._hasManyType = args.HasManyType;

            ManagedPropertyRepository.Instance.RegisterProperty(property);

            return property;
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

        /// <summary>
        /// 为指定的实体类型扩展指定名称的属性。
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="entityType"></param>
        /// <param name="propertyName"></param>
        /// <param name="declareType"></param>
        /// <returns></returns>
        public static Property<TProperty> RegisterExtension<TProperty>(Type entityType, string propertyName, Type declareType)
        {
            var mp = new Property<TProperty>(entityType, declareType, propertyName, new PropertyMetadata<TProperty>());

            ManagedPropertyRepository.Instance.RegisterProperty(mp);

            return mp;
        }
    }

    ///// <summary>
    ///// Rafy Used Only
    ///// </summary>
    //public static class ExtensionPropertyManager
    //{
    //    public static void AddModelForExtentions()
    //    {
    //        foreach (var kv in ManagedPropertyRepository.Instance)
    //        {
    //            var type = kv.Key;
    //            var properties = kv.Value.ConsolidatedContainer.GetCompiledProperties();

    //            if (RafyEnvironment.IsRootType(type))
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