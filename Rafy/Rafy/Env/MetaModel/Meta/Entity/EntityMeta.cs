/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110315
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100315
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Rafy.MetaModel.Attributes;
using Rafy.ManagedProperty;
using Rafy;
using Rafy.Utils;

namespace Rafy.MetaModel
{
    /// <summary>
    /// 实体元数据
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public class EntityMeta : Meta
    {
        /// <summary>
        /// 此实体类对应的所有托管属性容器
        /// </summary>
        public ConsolidatedTypePropertiesContainer ManagedProperties { get; internal set; }

        /// <summary>
        /// 类型名
        /// </summary>
        public override string Name
        {
            get { return this.EntityType.Name; }
            set { throw new NotSupportedException(); }
        }

        private Type _entityType;
        /// <summary>
        /// 当前模型是对应这个类型的。
        /// </summary>
        public Type EntityType
        {
            get { return this._entityType; }
            set { this.SetValue(ref this._entityType, value); }
        }

        private Type _idType;
        /// <summary>
        /// 当前模型是对应这个类型的。
        /// </summary>
        public Type IdType
        {
            get { return this._idType; }
            set { this.SetValue(ref this._idType, value); }
        }

        private bool _IsTreeEntity;
        /// <summary>
        /// 是否为树型实体。
        /// </summary>
        public bool IsTreeEntity
        {
            get { return this._IsTreeEntity; }
            set { this.SetValue(ref this._IsTreeEntity, value); }
        }

        private TreeIndexOption _TreeIndexOption;
        /// <summary>
        /// 如果是树型实体，则可以通过这个属性来设置它的树型编码存储规则。
        /// </summary>
        public TreeIndexOption TreeIndexOption
        {
            get { return this._TreeIndexOption; }
            set { this.SetValue(ref this._TreeIndexOption, value); }
        }

        [UnAutoFreeze]
        private EntityMeta _AggtParent;
        /// <summary>
        /// 聚合父类的元数据
        /// </summary>
        public EntityMeta AggtParent
        {
            get { return this._AggtParent; }
            set { this.SetValue(ref this._AggtParent, value); }
        }

        /// <summary>
        /// 聚合根类的元数据
        /// </summary>
        public EntityMeta AggtRoot
        {
            get { return this._AggtParent != null ? this._AggtParent.AggtRoot : this; }
        }

        private IList<EntityMeta> _aggtChildren = new List<EntityMeta>();
        /// <summary>
        /// 所有的聚合子类的元数据
        /// 
        /// 注意！！！
        /// 暂时把这个属性的可见性设置为 internal，
        /// 原因是实体的关系可能需要考虑实体被客户化扩展的情况。
        /// 例如，当 ContractBudget 扩展 Budget 后，界面及实体应该完全不存在 Budget 类。
        /// </summary>
        internal IList<EntityMeta> AggtChildren
        {
            get { return this._aggtChildren; }
        }

        private IList<EntityPropertyMeta> _entityProperties = new List<EntityPropertyMeta>();
        /// <summary>
        /// 拥有的实体属性。
        /// </summary>
        public IList<EntityPropertyMeta> EntityProperties
        {
            get { return this._entityProperties; }
        }

        private IList<ChildrenPropertyMeta> _childrenProperties = new List<ChildrenPropertyMeta>();
        /// <summary>
        /// 拥有的关联属性。
        /// </summary>
        public IList<ChildrenPropertyMeta> ChildrenProperties
        {
            get { return this._childrenProperties; }
        }

        private EntityCategory _EntityCategory;
        /// <summary>
        /// 实体的类别
        /// </summary>
        public EntityCategory EntityCategory
        {
            get { return this._EntityCategory; }
            set { this.SetValue(ref this._EntityCategory, value); }
        }

        private TableMeta _TableMeta;
        /// <summary>
        /// 映射表的元数据。
        /// </summary>
        public TableMeta TableMeta
        {
            get { return this._TableMeta; }
            set { this.SetValue(ref this._TableMeta, value); }
        }

        private ClientCacheScope _ClientCacheDefinition;
        /// <summary>
        /// 缓存子系统元数据
        /// 
        /// 如果不为空，表示这个实体正在使用分布式缓存系统。
        /// </summary>
        public ClientCacheScope ClientCacheDefinition
        {
            get { return this._ClientCacheDefinition; }
            set { this.SetValue(ref this._ClientCacheDefinition, value); }
        }

        private bool _ServerCacheDefinition;
        /// <summary>
        /// 是否启用本类型在服务端的内存缓存功能。
        /// </summary>
        public bool ServerCacheEnabled
        {
            get { return this._ServerCacheDefinition; }
            set { this.SetValue(ref this._ServerCacheDefinition, value); }
        }

        private bool _DeletingChildrenInMemory;
        /// <summary>
        /// 强制该实体删除都使用内存中的级联删除。
        /// </summary>
        public bool DeletingChildrenInMemory
        {
            get { return this._DeletingChildrenInMemory; }
            set { this.SetValue(ref this._DeletingChildrenInMemory, value); }
        }

        private bool _IsPhantomEnabled;
        /// <summary>
        /// 是否启用了该实体的假删除功能。（假删除功能需要引用独立的插件：Rafy.Domain.EntityPhantom。）
        /// </summary>
        public bool IsPhantomEnabled
        {
            get { return this._IsPhantomEnabled; }
            internal set { this.SetValue(ref this._IsPhantomEnabled, value); }
        }

        #region 查询方法

        /// <summary>
        /// 找到实体中对应聚合关系中的父实体引用属性元数据。
        /// 
        /// 注意，此函数返回的是引用实体属性，而非引用 Id 属性。
        /// </summary>
        /// <returns></returns>
        public EntityPropertyMeta FindParentReferenceProperty()
        {
            var result = this.EntityProperties
                .Where(p => p.ReferenceInfo != null && p.ReferenceInfo.Type == ReferenceType.Parent).ToArray();

            if (result.Length > 1) throw new InvalidOperationException(string.Format("类 {0} 中定义了两个父引用属性。（一个类中只能定义一个父引用属性。）", this.Name));

            return result.Length > 0 ? result[0] : null;
        }

        /// <summary>
        /// 根据托管属性查询实体属性
        /// 如果没有找到，则返回 null。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public EntityPropertyMeta Property(IManagedProperty property)
        {
            var list = this.EntityProperties;
            for (int i = 0, c = list.Count; i < c; i++)
            {
                var item = list[i];
                if (item.ManagedProperty == property)
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        /// 根据名字查询实体属性（忽略大小写）
        /// 如果没有找到，则返回 null。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public EntityPropertyMeta Property(string property)
        {
            var list = this.EntityProperties;
            for (int i = 0, c = list.Count; i < c; i++)
            {
                var item = list[i];
                if (item.Name.EqualsIgnoreCase(property))
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        /// 根据托管属性查询组合子属性。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public ChildrenPropertyMeta ChildrenProperty(IManagedProperty property)
        {
            var list = this.ChildrenProperties;
            for (int i = 0, c = list.Count; i < c; i++)
            {
                var item = list[i];
                if (item.ManagedProperty == property)
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        /// 根据名字查询组合子属性（忽略大小写）
        /// 如果没有找到，则返回 null。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public ChildrenPropertyMeta ChildrenProperty(string property)
        {
            var list = this.ChildrenProperties;
            for (int i = 0, c = list.Count; i < c; i++)
            {
                var item = list[i];
                if (item.Name.EqualsIgnoreCase(property))
                {
                    return item;
                }
            }
            return null;
        }

        #endregion

        protected override void OnFrozen()
        {
            base.OnFrozen();

            this._aggtChildren = new ReadOnlyCollection<EntityMeta>(this._aggtChildren);
            this._entityProperties = new ReadOnlyCollection<EntityPropertyMeta>(this._entityProperties);
            this._childrenProperties = new ReadOnlyCollection<ChildrenPropertyMeta>(this._childrenProperties);
        }

        private string DebuggerDisplay
        {
            get
            {
                if (this._AggtParent == null)
                {
                    return string.Format("Name:{0}", this.Name);
                }
                return string.Format("Name:{0}, Root:{1}", this.Name, this.AggtRoot.Name);
            }
        }
    }

    /// <summary>
    /// 某个类型所使用的缓存更新范围。
    /// </summary>
    public class ClientCacheScope : MetaBase
    {
        /// <summary>
        /// 为这个类型定义的范围。
        /// </summary>
        public Type Class { get; set; }

        /// <summary>
        /// 此属性表示为Class作为范围的类型。
        /// （注意：应该是在聚合对象树中，Class的上层类型。）
        /// 如果此属性为null，表示Class以本身作为缓存范围。
        /// </summary>
        public Type ScopeClass { get; set; }

        /// <summary>
        /// 此属性表示为Class作为范围的类型的对象ID。
        /// 如果此属性为null，表示Class不以某一特定的范围对象作为范围，而是全体对象。
        /// </summary>
        public Func<ManagedPropertyObject, string> ScopeIdGetter { get; set; }

        /// <summary>
        /// 如果是使用的简单缓存方案，这个值存储这个方案的值。
        /// </summary>
        public ClientCacheScopeType? SimpleScopeType { get; set; }

        /// <summary>
        /// 表示Class以本身作为缓存范围。
        /// </summary>
        public bool ScopeBySelf
        {
            get { return this.ScopeClass == null; }
        }

        /// <summary>
        /// 表示Class是否以某一特定的范围对象作为范围。
        /// </summary>
        public bool ScopeById
        {
            get { return this.ScopeIdGetter != null; }
        }
    }

    /// <summary>
    /// 主要会被使用到的两种缓存方案
    /// </summary>
    public enum ClientCacheScopeType
    {
        /// <summary>
        /// 按照表的方案来缓存
        /// </summary>
        Table,

        /// <summary>
        /// 按照聚合树的方案来缓存
        /// </summary>
        ScopedByRoot
    }
}