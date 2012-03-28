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
using System.Linq;
using System.Text;
using SimpleCsla;
using System.Reflection.Emit;
using System.Threading;
using System.Reflection;
using OEA.MetaModel;

namespace OEA.Library
{
    /// <summary>
    /// 实体仓库工厂类
    /// 
    /// 用于创建指定实体的仓库。
    /// </summary>
    internal class RepositoryFactory : IRepositoryFactory
    {
        public static readonly RepositoryFactory Instance = new RepositoryFactory();

        //目前的Repository全部使用缓存
        private IDictionary<Type, EntityRepository> _all = new SortedDictionary<Type, EntityRepository>(TypeNameComparer.Instance);

        //使用这个字段高速缓存最后一次使用的Repository。
        private EntityRepository _lastRepository;

        private RepositoryFactory() { }

        /// <summary>
        /// 使用指定子类的Repository覆盖它的父类的Repository。
        /// 
        /// 这时，所有调用Create、Concreate方法为它的父类生成Repository时，都会返回指定子类的仓库
        /// </summary>
        /// <param name="baseType"></param>
        /// <param name="subType"></param>
        public void OverrideRepository(Type baseType, Type subType)
        {
            if (baseType == null) throw new ArgumentNullException("baseType");
            if (subType == null) throw new ArgumentNullException("subType");
            if (!baseType.IsAssignableFrom(subType)) throw new ArgumentException("subType 并不是 baseType 的子类，不能重写！");

            //重写前，必须保证这两个类型的Repository已经正常生成。
            var repositoryBase = this.Find(baseType);
            var repositorySub = this.Find(subType);

            //如果都正常生成，则直接覆盖。
            if (repositoryBase != null && repositorySub != null)
            {
                lock (this._all)
                {
                    this._all[baseType] = repositorySub;
                }
            }
        }

        /// <summary>
        /// 用于创建指定实体的仓库。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        internal EntityRepository Find(Type entityType)
        {
            var last = this._lastRepository;
            if (last != null && last.EntityType == entityType) return last;

            EntityRepository result = null;

            if (!this._all.TryGetValue(entityType, out result))
            {
                lock (this._all)
                {
                    result = this.FindWithoutLock(entityType);
                }
            }

            this._lastRepository = result;

            return result;
        }

        private EntityRepository FindWithoutLock(Type entityType)
        {
            EntityRepository result = null;

            if (!this._all.TryGetValue(entityType, out result))
            {
                result = this.DoCreate(entityType);
                this._all.Add(entityType, result);
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
            var rType = EntityConvention.FindByEntity(entityType).RepositoryType;
            if (rType != null) return Activator.CreateInstance(rType, true) as EntityRepository;

            //如果上面在约定中没有找到，则直接生成一个默认的实体仓库。
            return this.CreateDefaultRepository(entityType);
        }

        #region 生成默认Repository类

        /// <summary>
        /// 如果上面在约定中没有找到，则直接生成一个默认的实体仓库。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        private EntityRepository CreateDefaultRepository(Type entityType)
        {
            var dynamicType = this.GenerateDefaultRepositoryType(entityType);

            var result = Activator.CreateInstance(dynamicType) as EntityRepository;
            result.RealEntityType = entityType;
            return result;
        }

        /// <summary>
        /// 为实体类型生成一个默认的实体类。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        private Type GenerateDefaultRepositoryType(Type entityType)
        {
            var baseType = entityType.BaseType;

            //找到上一个非泛型父类的Repository类型
            Type baseRepositoryType = null;
            while (baseType != typeof(Entity))
            {
                if (baseType == null) throw new InvalidProgramException("此类并没有继承 Enitty 类，不能创建 Repository。");
                if (!baseType.IsGenericType)
                {
                    var repository = this.FindWithoutLock(baseType);
                    baseRepositoryType = repository.GetType();
                    break;
                }

                baseType = baseType.BaseType;
            }

            //如果没有，则直接从DefaultEntityRepository上继承。
            if (baseRepositoryType == null) { baseRepositoryType = typeof(EntityRepository); }

            //查找这个类型
            var module = EmitContext.Instance.GetDynamicModule();
            var className = entityType.FullName + "Repository";

            //尝试查找这个类型
            var exsitType = module.GetType(className, false);
            if (exsitType != null) return exsitType;

            //构造一个新的类型。
            var newType = module.DefineType(
                className,
                TypeAttributes.Public | TypeAttributes.Class,
                baseRepositoryType
                );
            return newType.CreateType();
        }

        #endregion

        #region IEntityRepository Members

        IRepository IRepositoryFactory.Create(Type entityType)
        {
            return this.Find(entityType);
        }

        #endregion
    }
}
