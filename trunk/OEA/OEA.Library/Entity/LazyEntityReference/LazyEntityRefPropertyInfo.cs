/*******************************************************
 * 
 * 作者：杜强
 * 创建时间：20110915
 * 说明：用于传递特定懒加载属性的相关信息
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 杜强 20110915
 * 
*******************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Serialization.Mobile;
using OEA.ManagedProperty;

namespace OEA.Library
{
    /// <summary>
    /// 用于存储特定的懒加载属性的信息
    /// </summary>
    [Serializable]
    public class LazyEntityRefPropertyInfo : MobileObject
    {
        private bool _notifyRefEntityChanged;

        private string _idProperty;

        private string _refEntityProperty;

        [NonSerialized]
        private bool _hasRefManagedPropertyMeta;
        [NonSerialized]
        private IRefPropertyMetadata _refManagedPropertyMeta;

        //for serialization
        private LazyEntityRefPropertyInfo() { }

        public LazyEntityRefPropertyInfo(string refEntityProperty)
        {
            this._refEntityProperty = refEntityProperty;
        }

        /// <summary>
        /// 是否通知引用实体的更改
        /// </summary>
        public bool NotifyRefEntityChanged
        {
            get { return this._notifyRefEntityChanged; }
            set { this._notifyRefEntityChanged = value; }
        }

        /// <summary>
        /// 引用的懒加载实体的Id名称
        /// 可以为空
        /// </summary>
        public string IdProperty
        {
            get { return this._idProperty; }
            set { this._idProperty = value; }
        }

        /// <summary>
        /// 引用的懒加载实体的属性名称
        /// </summary>
        public string RefEntityProperty
        {
            get { return this._refEntityProperty; }
        }

        public IRefPropertyMetadata CorrespondingManagedPropertyMeta(Entity owner)
        {
            if (!this._hasRefManagedPropertyMeta)
            {
                //由于子类可能会重写父类的实体引用属性并修改名字，所以这里不使用 Property.Name 来查找，
                //而是使用 IdProperty + RefEntityProperty 的方式来查找。
                var mp = owner.PropertiesContainer.GetAvailableProperties().FirstOrDefault(p =>
                {
                    if (p is IRefProperty)
                    {
                        var meta = p.GetMeta(owner) as IRefPropertyMetadata;
                        return meta.IdProperty == this.IdProperty &&
                            meta.RefEntityProperty == this.RefEntityProperty;
                    }
                    return false;
                });
                if (mp != null)
                {
                    this._refManagedPropertyMeta = mp.GetMeta(owner) as IRefPropertyMetadata;
                }
                this._hasRefManagedPropertyMeta = true;
            }

            return this._refManagedPropertyMeta;
        }

        internal static string CorrespondingManagedProperty(string refEntityProperty)
        {
            return refEntityProperty + "_Ref";
        }

        protected override void OnMobileSerializeState(ISerializationContext info)
        {
            base.OnMobileSerializeState(info);

            info.AddState("n", this._notifyRefEntityChanged);
            info.AddState("id", this._idProperty);
            info.AddState("e", this._refEntityProperty);
        }

        protected override void OnMobileDeserializeState(ISerializationContext info)
        {
            base.OnMobileDeserializeState(info);

            this._notifyRefEntityChanged = info.GetState<bool>("n");
            this._idProperty = info.GetState<string>("id");
            this._refEntityProperty = info.GetState<string>("e");
        }
    }
}
