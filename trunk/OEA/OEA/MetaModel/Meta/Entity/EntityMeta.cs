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
using OEA.MetaModel.Attributes;
using OEA.ManagedProperty;

namespace OEA.MetaModel
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

        private bool _IsTreeEntity;
        /// <summary>
        /// 是否为树型实体。
        /// </summary>
        public bool IsTreeEntity
        {
            get { return this._IsTreeEntity; }
            set { this.SetValue(ref this._IsTreeEntity, value); }
        }

        private TreeCodeOption _TreeCodeOption;
        /// <summary>
        /// 如果是树型实体，则可以通过这个属性来设置它的树型编码存储规则。
        /// </summary>
        public TreeCodeOption TreeCodeOption
        {
            get { return this._TreeCodeOption; }
            set { this.SetValue(ref this._TreeCodeOption, value); }
        }

        private EntityMeta _parentEntityInfo;
        /// <summary>
        /// 聚合父类的元数据
        /// </summary>
        public EntityMeta AggtParent
        {
            get { return this._parentEntityInfo; }
            set { this.SetValue(ref this._parentEntityInfo, value); }
        }

        /// <summary>
        /// 聚合根类的元数据
        /// </summary>
        public EntityMeta AggtRoot
        {
            get { return this._parentEntityInfo != null ? this._parentEntityInfo.AggtRoot : this; }
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
        /// 拥有的实体属性，即标记了：EntityPropertyAttribute
        /// </summary>
        public IList<EntityPropertyMeta> EntityProperties
        {
            get { return this._entityProperties; }
        }

        private IList<ChildrenPropertyMeta> _childrenProperties = new List<ChildrenPropertyMeta>();
        /// <summary>
        /// 拥有的关联属性，即标记了：AssociationAttribute
        /// </summary>
        public IList<ChildrenPropertyMeta> ChildrenProperties
        {
            get { return this._childrenProperties; }
        }

        private EntityCategory _EntityCategory;
        public EntityCategory EntityCategory
        {
            get { return this._EntityCategory; }
            set { this.SetValue(ref this._EntityCategory, value); }
        }

        private TableMeta _TableMeta;
        public TableMeta TableMeta
        {
            get { return this._TableMeta; }
            set { this.SetValue(ref this._TableMeta, value); }
        }

        private EntityPropertyMeta _DefaultOrderBy;
        /// <summary>
        /// 实体的数据默认按照某个属性排序。
        /// </summary>
        public EntityPropertyMeta DefaultOrderBy
        {
            get { return this._DefaultOrderBy; }
            internal set { this.SetValue(ref this._DefaultOrderBy, value); }
        }

        private bool _DefaultOrderByAscending = true;
        /// <summary>
        /// 实体的数据默认按照这个次序排序。
        /// </summary>
        public bool DefaultOrderByAscending
        {
            get { return this._DefaultOrderByAscending; }
            internal set { this.SetValue(ref this._DefaultOrderByAscending, value); }
        }

        private CacheScope _CacheDefinition;
        /// <summary>
        /// 缓存子系统元数据
        /// 
        /// 如果不为空，表示这个实体正在使用分布式缓存系统。
        /// </summary>
        public CacheScope CacheDefinition
        {
            get { return this._CacheDefinition; }
            set { this.SetValue(ref this._CacheDefinition, value); }
        }

        #region 查询方法

        public EntityPropertyMeta FindParentReferenceProperty()
        {
            var result = this.EntityProperties
                .Where(p => p.ReferenceInfo != null && p.ReferenceInfo.Type == ReferenceType.Parent).ToArray();

            if (result.Length > 1) throw new InvalidOperationException("一个类中只能定义一个父引用属性。");

            return result.Length > 0 ? result[0] : null;
        }

        /// <summary>
        /// 根据名字查询属性（忽略大小写）
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public PropertyMeta FindProperty(IManagedProperty property)
        {
            var ep = this.Property(property);
            if (ep != null) return ep;

            return this.ChildrenProperty(property);
        }

        /// <summary>
        /// 根据名字查询实体属性（忽略大小写）
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public EntityPropertyMeta Property(IManagedProperty property)
        {
            return this.Property(property.GetMetaPropertyName(this.EntityType));
        }

        /// <summary>
        /// 根据名字查询实体属性（忽略大小写）
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public EntityPropertyMeta Property(string name)
        {
            return this.EntityProperties.FirstOrDefault(item => item.Name.EqualsIgnorecase(name));
        }

        /// <summary>
        /// 根据名字查询关联属性（忽略大小写）
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ChildrenPropertyMeta ChildrenProperty(IManagedProperty property)
        {
            var name = property.GetMetaPropertyName(this.EntityType);
            return this.ChildrenProperties.FirstOrDefault(item => item.Name.EqualsIgnorecase(name));
        }

        public ChildrenPropertyMeta ChildrenProperty(string property)
        {
            return this.ChildrenProperties.FirstOrDefault(item => item.Name.EqualsIgnorecase(property));
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
            get { return string.Format("Name:{0}, Root:{1}", this.Name, this.AggtRoot.Name); }
        }
    }

    /// <summary>
    /// 某个类型所使用的缓存更新范围。
    /// </summary>
    public class CacheScope : MetaBase
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
        public SimpleCacheType? SimpleCacheType { get; set; }

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
    public enum SimpleCacheType
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