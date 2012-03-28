/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110422
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110422
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.Library
{
    [Serializable]
    internal class NonSerializableEntityLazyEntityRef<TEntity> : LazyEntityRefBase<TEntity>
        where TEntity : Entity
    {
        [NonSerialized]
        private TEntity _entity;

        protected override TEntity _entityField
        {
            get { return this._entity; }
            set { this._entity = value; }
        }

        public NonSerializableEntityLazyEntityRef(Func<int, Entity> staticLoader, IReferenceOwner owner, LazyEntityRefPropertyInfo refPropertyInfo)
            : base(staticLoader, owner, refPropertyInfo) { }

        public NonSerializableEntityLazyEntityRef(Func<int, object, Entity> instanceLoader, IReferenceOwner owner, LazyEntityRefPropertyInfo refPropertyInfo)
            : base(instanceLoader, owner, refPropertyInfo) { }
    }
}