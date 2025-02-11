﻿/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101101
 * 说明：实体仓库工厂类
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101101
 * 添加OverrideRepository方法 胡庆访 20101122
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using Rafy;
using Rafy.Reflection;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.Utils;
using Castle.DynamicProxy;
using Rafy.Domain.ORM;
using Rafy.DataPortal;

namespace Rafy.Domain
{
    /// <summary>
    /// 实体仓库工厂类
    /// 
    /// 用于创建指定实体的仓库。
    /// 
    /// Repository 全部使用单例模式
    /// </summary>
    public class DictionaryRepositoryFactory : IRepositoryFactory, IDataPortalTargetFactory
    {
        /// <summary>
        /// 使用 EntityType 作为查询键的字典。
        /// </summary>
        private IDictionary<Type, EntityRepository> _repoByEntityType = new SortedDictionary<Type, EntityRepository>(TypeNameComparer.Instance);

        /// <summary>
        /// 使用 RepositoryType 作为查询键的字典。
        /// </summary>
        private IDictionary<Type, EntityRepository> _repoByType = new SortedDictionary<Type, EntityRepository>(TypeNameComparer.Instance);

        /// <summary>
        /// 使用这个字段高速缓存最后一次使用的Repository。
        /// </summary>
        private EntityRepository _lastRepository;

        /// <summary>
        /// 通过仓库类型查找指定的仓库。
        /// </summary>
        /// <param name="repositoryType"></param>
        /// <param name="throwIfNotfound"></param>
        /// <returns></returns>
        internal IRepositoryInternal Find(Type repositoryType, bool throwIfNotfound)
        {
            var last = _lastRepository;
            if (last != null && last.GetType() == repositoryType) return last;

            EntityRepository result = null;

            if (!_repoByType.TryGetValue(repositoryType, out result))
            {
                lock (_repoByType)
                {
                    if (!_repoByType.TryGetValue(repositoryType, out result))
                    {
                        var matrix = EntityMatrix.FindByRepository(repositoryType);
                        var entityType = matrix.EntityType;
                        result = this.FindByEntity(entityType, throwIfNotfound) as EntityRepository;

                        _repoByType.Add(repositoryType, result);
                    }
                }
            }

            _lastRepository = result;

            return result;
        }

        /// <summary>
        /// 用于查找指定实体的仓库。
        /// 如果还没有创建，则直接创建一个。
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="throwIfNotfound"></param>
        /// <returns></returns>
        internal IRepositoryInternal FindByEntity(Type entityType, bool throwIfNotfound)
        {
            var last = _lastRepository;
            if (last != null && last.EntityType == entityType) return last;

            EntityRepository result = null;

            if (!_repoByEntityType.TryGetValue(entityType, out result))
            {
                //只有标记了这些标签的实体才会有元数据，这样才能正确生成对应的仓库类型。
                if (entityType.HasMarked<RootEntityAttribute>() ||
                    entityType.HasMarked<ChildEntityAttribute>())
                {
                    lock (_repoByEntityType)
                    {
                        if (!_repoByEntityType.TryGetValue(entityType, out result))
                        {
                            result = this.CreateWithExtensions(entityType);

                            _repoByEntityType.Add(entityType, result);
                        }
                    }
                }
                else
                {
                    if (throwIfNotfound)
                    {
                        throw new InvalidOperationException($"没有找到实体 {entityType.FullName} 对应的仓库，可能是因为该实体没有添加相应的实体标签。");
                    }
                }
            }

            _lastRepository = result;

            return result;
        }

        private EntityRepository CreateWithExtensions(Type entityType)
        {
            var result = this.DoCreate(entityType);

            //仓库完成后，需要把仓库扩展都加入到仓库中。
            this.AddExtensions(result);

            return result;
        }

        /// <summary>
        /// 创建一个实体类型的仓库。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        private EntityRepository DoCreate(Type entityType)
        {
            EntityRepository repo = null;

            //先尝试在约定中寻找实体类型自己定义的仓库类型。
            var metrix = EntityMatrix.FindByEntity(entityType);
            var repoType = metrix.RepositoryType;
            if (repoType == null)
            {
                //如果上面在约定中没有找到，则直接生成一个默认的实体仓库。
                repoType = this.GenerateDefaultRepositoryType(entityType);
            }

            if (repoType.IsAbstract) throw new InvalidProgramException(repoType.FullName + " 仓库类型是抽象的，无法创建。");

            repo = this.CreateInstanceProxy(repoType) as EntityRepository;
            //EntityRepository repo;
            //if (RafyEnvironment.Location.ConnectDataDirectly || repoType.IsSubclassOf(typeof(MemoryEntityRepository)))
            //{
            //    repo = Activator.CreateInstance(repoType, true) as EntityRepository;

            //    if (repo == null)
            //    {
            //        throw new InvalidProgramException("{0} 类型必须继承自 EntityRepository 类型。".FormatArgs(repoType));
            //    }
            //}
            //else
            //{
            //    repo = _proxyGenerator.CreateClassProxy(repoType, RepositoryInterceptor.Instance) as EntityRepository;
            //}

            var repoDataProvider = DataProviderComposer.CreateDataProvider(repoType);

            repo.InitDataProvider(repoDataProvider);
            repoDataProvider.InitRepository(repo);

            return repo;
        }

        #region RepositoryExtension

        /// <summary>
        /// 是否所有的仓库扩展已经加载完成。
        /// </summary>
        private bool _extLoaded = false;

        /// <summary>
        /// 所有的仓库扩展类。
        /// 
        /// Key: 仓库类型。
        /// Value: 一个仓库扩展类型的列表。
        /// </summary>
        private IDictionary<Type, List<Type>> _extByType = new SortedDictionary<Type, List<Type>>(TypeNameComparer.Instance);

        /// <summary>
        /// 为指定的仓库加载所有的仓库扩展类。
        /// </summary>
        /// <param name="repository"></param>
        private void AddExtensions(EntityRepository repository)
        {
            var repoExtensions = new List<IRepositoryExt>();

            this.LoadAllExtensionTypes();

            //把对该类及它的基类的扩展都加入进来。
            var baseTypes = TypeHelper.GetHierarchy(repository.GetType(), typeof(EntityRepositoryQueryBase)).Reverse().ToArray();
            foreach (var baseType in baseTypes)
            {
                List<Type> extList = null;
                if (_extByType.TryGetValue(baseType, out extList))
                {
                    foreach (var extType in extList)
                    {
                        var ext = this.CreateInstanceProxy(extType) as IRepositoryExt;
                        repoExtensions.Add(ext);

                        (ext as IRepositoryExtInternal).BindRepository(repository);
                    }
                }
            }

            repository.Extensions = new ReadOnlyCollection<IRepositoryExt>(repoExtensions);
        }

        /// <summary>
        /// 加载所有插件中的实体仓库扩展类
        /// </summary>
        private void LoadAllExtensionTypes()
        {
            if (!_extLoaded)
            {
                lock (_extByType)
                {
                    if (!_extLoaded)
                    {
                        //对于所有插件中的实体仓库扩展类，都需要检测仓库扩展类。
                        var baseClass = typeof(EntityRepositoryExt);
                        foreach (var plugin in RafyEnvironment.Plugins)
                        {
                            var types = plugin.Assembly.GetTypes();
                            foreach (var type in types)
                            {
                                //所有继承自 baseClass 都是仓库扩展类。
                                if (type.IsSubclassOf(baseClass) && !type.IsAbstract && !type.IsGenericTypeDefinition)
                                {
                                    var instance = Activator.CreateInstance(type, true) as IRepositoryExt;

                                    List<Type> list = null;
                                    if (!_extByType.TryGetValue(instance.RepositoryType, out list))
                                    {
                                        list = new List<Type>();
                                        _extByType.Add(instance.RepositoryType, list);
                                    }
                                    list.Add(type);
                                }
                            }
                        }
                    }
                }

                _extLoaded = true;
            }
        }

        #endregion

        #region DataPortal 相关、生成代理、方法拦截

        private ProxyGenerator _proxyGenerator = new ProxyGenerator();

        /// <summary>
        /// 生成仓库、仓库扩展的代理类型。
        /// </summary>
        /// <param name="repoOrExtType"></param>
        /// <returns></returns>
        private object CreateInstanceProxy(Type repoOrExtType)
        {
            var options = new ProxyGenerationOptions(DataPortalCallMethodHook.Instance);
            options.Selector = new InterceptorSelector();

            var instance = _proxyGenerator.CreateClassProxy(repoOrExtType, options);
            return instance;
        }

        private class InterceptorSelector : IInterceptorSelector
        {
            public IInterceptor[] SelectInterceptors(Type type, MethodInfo method, IInterceptor[] interceptors)
            {
                //RepositoryInterceptor 需要拦截 RepositoryQueryAttribute 标记的方法。
                //DataPortalCallInterceptor 需要拦截 RepositoryQueryAttribute 或 DataPortalCallAttribute 标记的方法。
                if (method.HasMarked<RepositoryQueryAttribute>())
                {
                    return new IInterceptor[] { RepositoryQueryInterceptor.Instance, DataPortalCallInterceptor.Instance };
                }
                if (method.HasMarked<DataPortalCallAttribute>())
                {
                    return new IInterceptor[] { DataPortalCallInterceptor.Instance };
                }
                return null;
            }
        }

        string IDataPortalTargetFactory.Name => RepositoryFactoryHost.DataPortalTargetFactoryName;

        IDataPortalTarget IDataPortalTargetFactory.GetTarget(DataPortalTargetFactoryInfo info)
        {
            var repositoryType = TypeSerializer.Deserialize(info.TargetInfo as string);
            var repository = this.Find(repositoryType, true);

            if (info is RepoExtInfo)
            {
                var extType = TypeSerializer.Deserialize((info as RepoExtInfo).ExtType);
                return repository.Extensions.First(e => extType.IsInstanceOfType(e)) as IDataPortalTarget;
            }

            return repository as IDataPortalTarget;
        }

        //private void CheckVirtualMethods(Type type)
        //{
        //    var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public );
        //    foreach (var m in methods)
        //    {
        //        if (m.IsPublic && !m.IsVirtual)
        //        {
        //            if (m.DeclaringType != typeof(EntityRepository) &&
        //                m.DeclaringType != typeof(EntityRepositoryQueryBase) &&
        //                m.DeclaringType != typeof(object))
        //            {
        //                throw new InvalidProgramException("仓库{0}方法{1}必须是virtual".FormatArgs(type.FullName, m.Name));
        //            }
        //        }
        //    }
        //}

        #endregion

        #region 生成默认Repository类

        /// <summary>
        /// 是否为没有定义仓库类的实体，自动生成仓库类型。
        /// </summary>
        public bool AutoGenerateDefaultRepository { get; set; } = true;

        /// <summary>
        /// 如果上面在约定中没有找到（开发者没有编写仓库类型），则直接生成一个默认的实体仓库。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        private Type GenerateDefaultRepositoryType(Type entityType)
        {
            if (!this.AutoGenerateDefaultRepository)
            {
                //实体类必须编写对应的 EntityRepository 类型，不再支持生成默认类型。
                throw new InvalidProgramException(entityType.FullName + " 类型没有对应的仓库，创建仓库失败！");
            }

            var repositoryType = this.GenerateDefaultRepositoryTypeCore(entityType);

            var listType = EntityMatrix.Convention_ListForEntity(entityType);
            EntityMatrix.Set(new EntityMatrix(entityType, listType, repositoryType));

            //var result = Activator.CreateInstance(dynamicType) as EntityRepository;
            //result.RealEntityType = entityType;
            return repositoryType;
        }

        /// <summary>
        /// 为实体类型生成一个默认的实体类。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        private Type GenerateDefaultRepositoryTypeCore(Type entityType)
        {
            var baseType = entityType.BaseType;

            //找到继承链条上，最近的一个非泛型父类的仓库类型
            Type baseRepositoryType = null;
            while (baseType != typeof(Entity))
            {
                if (baseType == null) throw new InvalidProgramException("此类并没有继承 Entity 类，不能创建 Repository。");
                if (!baseType.IsGenericType)
                {
                    //不需要为基类创建仓库，这是因为基类声明的仓库类型可能是抽象类型。
                    //var repository = this.FindWithoutLock(baseType);
                    //baseRepositoryType = repository.GetType();

                    var convention = EntityMatrix.FindByEntity(baseType);
                    baseRepositoryType = convention.RepositoryType;
                    break;
                }

                baseType = baseType.BaseType;
            }

            //如果没有，则直接从DefaultEntityRepository上继承。
            if (baseRepositoryType == null) { baseRepositoryType = typeof(EntityRepository); }

            //查找这个类型
            var module = EmitContext.Instance.GetDynamicModule();
            var className = EntityMatrix.RepositoryFullName(entityType);

            //尝试查找这个类型
            var exsitType = module.GetType(className, false);
            if (exsitType != null) return exsitType;

            //构造一个新的类型。
            var newType = module.DefineType(
                className,
                TypeAttributes.Public | TypeAttributes.Class,
                baseRepositoryType
                );
            return newType.CreateTypeInfo();
        }

        #endregion

        IRepository IRepositoryFactory.FindByEntity(Type entityType, bool throwIfNotfound)
        {
            return this.FindByEntity(entityType, throwIfNotfound);
        }
        IRepository IRepositoryFactory.Find(Type repositoryType, bool throwIfNotfound)
        {
            return this.Find(repositoryType, throwIfNotfound);
        }
    }
}
