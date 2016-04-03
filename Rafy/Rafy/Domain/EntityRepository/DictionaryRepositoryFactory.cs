/*******************************************************
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

namespace Rafy.Domain
{
    /// <summary>
    /// 实体仓库工厂类
    /// 
    /// 用于创建指定实体的仓库。
    /// 
    /// Repository 全部使用单例模式
    /// </summary>
    internal class DictionaryRepositoryFactory : IRepositoryFactory
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
        /// <returns></returns>
        public IRepositoryInternal Find(Type repositoryType)
        {
            var last = this._lastRepository;
            if (last != null && last.GetType() == repositoryType) return last;

            EntityRepository result = null;

            if (!this._repoByType.TryGetValue(repositoryType, out result))
            {
                lock (this._repoByType)
                {
                    if (!this._repoByType.TryGetValue(repositoryType, out result))
                    {
                        var matrix = EntityMatrix.FindByRepository(repositoryType);
                        var entityType = matrix.EntityType;
                        result = this.FindByEntity(entityType) as EntityRepository;

                        this._repoByType.Add(repositoryType, result);
                    }
                }
            }

            this._lastRepository = result;

            return result;
        }

        /// <summary>
        /// 用于查找指定实体的仓库。
        /// 如果还没有创建，则直接创建一个。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public IRepositoryInternal FindByEntity(Type entityType)
        {
            var last = this._lastRepository;
            if (last != null && last.EntityType == entityType) return last;

            EntityRepository result = null;

            if (!this._repoByEntityType.TryGetValue(entityType, out result))
            {
                //只有实体才能有对应的仓库类型。
                if (entityType.HasMarked<RootEntityAttribute>() ||
                    entityType.HasMarked<ChildEntityAttribute>())
                {
                    lock (this._repoByEntityType)
                    {
                        result = this.FindOrCreateWithoutLock(entityType);
                    }
                }
            }

            this._lastRepository = result;

            return result;
        }

        private EntityRepository FindOrCreateWithoutLock(Type entityType)
        {
            EntityRepository result = null;

            if (!this._repoByEntityType.TryGetValue(entityType, out result))
            {
                result = this.DoCreate(entityType);

                //仓库完成后，需要把仓库扩展都加入到仓库中。
                this.AddExtensions(result);

                this._repoByEntityType.Add(entityType, result);
            }

            return result;
        }

        /// <summary>
        /// 创建一个实体类型的仓库。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        private EntityRepository DoCreate(Type entityType)
        {
            //先尝试在约定中寻找实体类型自己定义的仓库类型。
            var metrix = EntityMatrix.FindByEntity(entityType);
            var repoType = metrix.RepositoryType;
            if (repoType != null)
            {
                if (repoType.IsAbstract) throw new InvalidProgramException(repoType.FullName + " 仓库类型是抽象的，无法创建。");

                var repo = this.CreateInstanceProxy(repoType) as EntityRepository;
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

            throw new InvalidProgramException(entityType.FullName + " 类型没有对应的仓库，创建仓库失败！");

            //实体类必须编写对应的 EntityRepository 类型，不再支持生成默认类型。
            ////如果上面在约定中没有找到，则直接生成一个默认的实体仓库。
            //return this.CreateDefaultRepository(entityType);
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
            var list = new List<IRepositoryExt>();

            this.LoadAllExtensions();

            //把对该类及它的基类的扩展都加入进来。
            var baseTypes = TypeHelper.GetHierarchy(repository.GetType(), typeof(EntityRepositoryQueryBase)).Reverse().ToArray();
            foreach (var baseType in baseTypes)
            {
                List<Type> extList = null;
                if (this._extByType.TryGetValue(baseType, out extList))
                {
                    foreach (var extType in extList)
                    {
                        var ext = this.CreateInstanceProxy(extType) as IRepositoryExt;
                        list.Add(ext);

                        (ext as IRepositoryExtInternal).BindRepository(repository);
                    }
                }
            }

            repository.Extensions = new ReadOnlyCollection<IRepositoryExt>(list);
        }

        /// <summary>
        /// 加载所有插件中的实体仓库扩展类
        /// </summary>
        private void LoadAllExtensions()
        {
            if (!this._extLoaded)
            {
                lock (this._extByType)
                {
                    if (!this._extLoaded)
                    {
                        //对于所有插件中的实体仓库扩展类，都需要检测仓库扩展类。
                        var baseClass = typeof(EntityRepositoryExt);
                        foreach (var plugin in RafyEnvironment.AllPlugins)
                        {
                            var types = plugin.Assembly.GetTypes();
                            foreach (var type in types)
                            {
                                //所有继承自 baseClass 都是仓库扩展类。
                                if (type.IsSubclassOf(baseClass) && !type.IsAbstract && !type.IsGenericTypeDefinition)
                                {
                                    var instance = Activator.CreateInstance(type, true) as IRepositoryExt;

                                    List<Type> list = null;
                                    if (!this._extByType.TryGetValue(instance.RepositoryType, out list))
                                    {
                                        list = new List<Type>();
                                        this._extByType.Add(instance.RepositoryType, list);
                                    }
                                    list.Add(type);
                                }
                            }
                        }
                    }
                }

                this._extLoaded = true;
            }
        }

        #endregion

        #region 生成代理、方法拦截

        private ProxyGenerator _proxyGenerator = new ProxyGenerator();

        /// <summary>
        /// 生成仓库、仓库扩展的代理类型。
        /// </summary>
        /// <param name="instanceType"></param>
        /// <returns></returns>
        private object CreateInstanceProxy(Type instanceType)
        {
            var options = new ProxyGenerationOptions(RepoQueryMethodDeterminationHook.Instance);
            var instance = _proxyGenerator.CreateClassProxy(instanceType, options, RepositoryInterceptor.Instance) ;
            return instance;
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

        /// <summary>
        /// 只有标记了 RepositoryQueryMethodAttribute 标记的方法，才需要进行拦截。
        /// </summary>
        private class RepoQueryMethodDeterminationHook : AllMethodsHook
        {
            public static readonly RepoQueryMethodDeterminationHook Instance = new RepoQueryMethodDeterminationHook();

            private RepoQueryMethodDeterminationHook() { }

            public override bool ShouldInterceptMethod(Type type, MethodInfo methodInfo)
            {
                var res = base.ShouldInterceptMethod(type, methodInfo);
                if (res)
                {
                    res = methodInfo.HasMarked<RepositoryQueryAttribute>();
                }
                return res;
            }
        }

        #endregion

        #region 生成默认Repository类（暂不使用）

        ///// <summary>
        ///// 如果上面在约定中没有找到（开发者没有编写仓库类型），则直接生成一个默认的实体仓库。
        ///// </summary>
        ///// <param name="entityType"></param>
        ///// <returns></returns>
        //private EntityRepository CreateDefaultRepository(Type entityType)
        //{
        //    var dynamicType = this.GenerateDefaultRepositoryType(entityType);

        //    var result = Activator.CreateInstance(dynamicType) as EntityRepository;
        //    //result.RealEntityType = entityType;
        //    return result;
        //}

        ///// <summary>
        ///// 为实体类型生成一个默认的实体类。
        ///// </summary>
        ///// <param name="entityType"></param>
        ///// <returns></returns>
        //private Type GenerateDefaultRepositoryType(Type entityType)
        //{
        //    var baseType = entityType.BaseType;

        //    //找到继承链条上，最近的一个非泛型父类的仓库类型
        //    Type baseRepositoryType = null;
        //    while (baseType != typeof(Entity))
        //    {
        //        if (baseType == null) throw new InvalidProgramException("此类并没有继承 Entity 类，不能创建 Repository。");
        //        if (!baseType.IsGenericType)
        //        {
        //            //不需要为基类创建仓库，这是因为基类声明的仓库类型可能是抽象类型。
        //            //var repository = this.FindWithoutLock(baseType);
        //            //baseRepositoryType = repository.GetType();

        //            var convention = EntityMatrix.FindByEntity(baseType);
        //            baseRepositoryType = convention.RepositoryType;
        //            break;
        //        }

        //        baseType = baseType.BaseType;
        //    }

        //    //如果没有，则直接从DefaultEntityRepository上继承。
        //    if (baseRepositoryType == null) { baseRepositoryType = typeof(EntityRepository); }

        //    //查找这个类型
        //    var module = EmitContext.Instance.GetDynamicModule();
        //    var className = EntityMatrix.RepositoryFullName(entityType);

        //    //尝试查找这个类型
        //    var exsitType = module.GetType(className, false);
        //    if (exsitType != null) return exsitType;

        //    //构造一个新的类型。
        //    var newType = module.DefineType(
        //        className,
        //        TypeAttributes.Public | TypeAttributes.Class,
        //        baseRepositoryType
        //        );
        //    return newType.CreateType();
        //}

        #endregion
    }
}
