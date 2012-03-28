/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100331
 * 说明：延迟加载外键实体 的实现
 * 运行环境：.NET 3.5 SP1
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100331
 * 添加非泛型版本的基类，添加InheritanceLazyEntityRef 胡庆访 20100925
 * 
*******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections;
using OEA.Serialization.Mobile;

namespace OEA.Library
{
    [Serializable]
    internal class LazyEntityRef<TEntity> : LazyEntityRefBase<TEntity>
        where TEntity : Entity
    {
        private TEntity _entity;

        protected override TEntity _entityField
        {
            get { return this._entity; }
            set { this._entity = value; }
        }

        public LazyEntityRef(Func<int, Entity> staticLoader, IReferenceOwner owner, LazyEntityRefPropertyInfo refPropertyInfo)
            : base(staticLoader, owner, refPropertyInfo) { }

        public LazyEntityRef(Func<int, object, Entity> instanceLoader, IReferenceOwner owner, LazyEntityRefPropertyInfo refPropertyInfo)
            : base(instanceLoader, owner, refPropertyInfo) { }

        protected override void OnMobileSerializeRef(ISerializationContext context)
        {
            base.OnMobileSerializeRef(context);

            context.AddRef("e", this._entity);
        }

        protected override void OnMobileDeserializeRef(ISerializationContext context)
        {
            this._entity = context.GetRef<TEntity>("e");

            base.OnMobileDeserializeRef(context);
        }
    }
}
