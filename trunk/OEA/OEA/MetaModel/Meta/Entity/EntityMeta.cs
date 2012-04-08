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
        #region 字段

        private Type _entityType;

        private bool _IsTreeEntity;

        private EntityMeta _parentEntityInfo;

        private IList<EntityMeta> _aggtChildren = new List<EntityMeta>();

        private IList<EntityPropertyMeta> _entityProperties = new List<EntityPropertyMeta>();

        private IList<ChildrenPropertyMeta> _childrenProperties = new List<ChildrenPropertyMeta>();

        private EntityCategory _EntityCategory;

        private TableMeta _TableMeta;

        private EntityPropertyMeta _DefaultOrderBy;

        private bool _DefaultOrderByAscending = true;

        #endregion

        #region 属性

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

        /// <summary>
        /// 当前模型是对应这个类型的。
        /// </summary>
        public Type EntityType
        {
            get { return this._entityType; }
            set { this.SetValue(ref this._entityType, value); }
        }

        /// <summary>
        /// 是否为树型实体。
        /// </summary>
        public bool IsTreeEntity
        {
            get { return this._IsTreeEntity; }
            set { this.SetValue(ref this._IsTreeEntity, value); }
        }

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

        /// <summary>
        /// 拥有的实体属性，即标记了：EntityPropertyAttribute
        /// </summary>
        public IList<EntityPropertyMeta> EntityProperties
        {
            get { return this._entityProperties; }
        }

        /// <summary>
        /// 拥有的关联属性，即标记了：AssociationAttribute
        /// </summary>
        public IList<ChildrenPropertyMeta> ChildrenProperties
        {
            get { return this._childrenProperties; }
        }

        public EntityCategory EntityCategory
        {
            get { return this._EntityCategory; }
            set { this.SetValue(ref this._EntityCategory, value); }
        }

        public TableMeta TableMeta
        {
            get { return this._TableMeta; }
            set { this.SetValue(ref this._TableMeta, value); }
        }

        public EntityPropertyMeta DefaultOrderBy
        {
            get { return this._DefaultOrderBy; }
            set { this.SetValue(ref this._DefaultOrderBy, value); }
        }

        public bool DefaultOrderByAscending
        {
            get { return this._DefaultOrderByAscending; }
            set { this.SetValue(ref this._DefaultOrderByAscending, value); }
        }

        #endregion

        #region 查询方法

        public EntityPropertyMeta FindParentReferenceProperty()
        {
            var result = this.EntityProperties
                .FirstOrDefault(p => p.ReferenceInfo != null && p.ReferenceInfo.Type == ReferenceType.Parent);
            return result;
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
}