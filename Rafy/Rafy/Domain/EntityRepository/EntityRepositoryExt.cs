/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130306 17:28
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130306 17:28
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.ManagedProperty;

namespace Rafy.Domain
{
    /// <summary>
    /// 实体仓库类的扩展类。
    /// 仓库扩展将是一个非常轻量级的对象。
    /// 
    /// 实体仓库扩展，支持在基类上进行扩展，这样所有的仓库子类都直接被扩展。
    /// 
    /// 如果本扩展是扩展在某个抽象的仓库基类上，那么系统会为被扩展的仓库基类的所有子类都建立一个本扩展的实例。
    /// </summary>
    public abstract class EntityRepositoryExt : EntityRepositoryQueryBase, IRepositoryExt
    {
        public abstract Type RepositoryType { get; }

        public EntityRepository Repository
        {
            get { return Repo as EntityRepository; }
        }

        IRepository IRepositoryExt.Repository
        {
            get { return Repo as EntityRepository; }
        }
    }

    /// <summary>
    /// 泛型 API，简化上层使用。
    /// <see cref="EntityRepositoryExt"/>
    /// </summary>
    /// <typeparam name="TRepository"></typeparam>
    public abstract class EntityRepositoryExt<TRepository> : EntityRepositoryExt, IRepositoryExtInternal
        where TRepository : EntityRepository
    {
        private EntityRepository _repo;

        /// <summary>
        /// 被扩展的仓库类型。
        /// </summary>
        public override Type RepositoryType
        {
            get { return typeof(TRepository); }
        }

        internal override IRepositoryInternal Repo
        {
            get { return _repo; }
        }

        /// <summary>
        /// 被扩展的仓库对象。
        /// </summary>
        public new TRepository Repository
        {
            get { return _repo as TRepository; }
        }

        #region IRepositoryExtInternal 成员

        bool IRepositoryExtInternal.IsExtending(IRepository repository)
        {
            return repository is TRepository;
        }

        void IRepositoryExtInternal.BindRepository(EntityRepository repository)
        {
            this._repo = repository;
        }

        #endregion
    }

    internal interface IRepositoryExtInternal
    {
        /// <summary>
        /// 判断是否当前扩展是为指定的仓库编写的。
        /// </summary>
        /// <param name="repository"></param>
        /// <returns></returns>
        bool IsExtending(IRepository repository);

        /// <summary>
        /// 使用一个具体的仓库子类来绑定这个对象
        /// </summary>
        /// <param name="repository"></param>
        void BindRepository(EntityRepository repository);
    }
}
