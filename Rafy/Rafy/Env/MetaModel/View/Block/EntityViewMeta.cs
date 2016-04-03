/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110314
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100314
 * 把 BusinessObjectInfo 类分享为 EntityMeta 和 EntityViewMeta 两个类，及相应的属性类 EntityPropertyMeta、EntityPropertyViewMeta、ChildrentPropertyMeta、ChildrentPropertyViewMeta。 胡庆访 20100314
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Rafy;
using Rafy.MetaModel.Attributes;
using Rafy.ManagedProperty;
using Rafy.Utils;

namespace Rafy.MetaModel.View
{
    /// <summary>
    /// 一个实体类的视图元数据
    /// 
    /// 一个实体并不只对应一个视图元数据
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public abstract class EntityViewMeta : Meta
    {
        public override string Name
        {
            get { return this.EntityMeta.Name; }
            set { throw new NotSupportedException(); }
        }

        private string _label;
        /// <summary>
        /// 显示的标题
        /// </summary>
        public string Label
        {
            get { return this._label; }
            set { this.SetValue(ref this._label, value); }
        }

        private string _ExtendView;
        /// <summary>
        /// 如果当前的视图是一个扩展视图，则这个属性表示这个扩展视图的名称。
        /// </summary>
        public string ExtendView
        {
            get { return this._ExtendView; }
            set { this.SetValue(ref this._ExtendView, value); }
        }

        public Type EntityType
        {
            get { return this.EntityMeta.EntityType; }
        }

        [UnAutoFreeze]
        private EntityMeta _EntityMeta;
        /// <summary>
        /// 实体类元数据
        /// </summary>
        public EntityMeta EntityMeta
        {
            get { return this._EntityMeta; }
            set { this.SetValue(ref this._EntityMeta, value); }
        }

        private EntityPropertyViewMeta _TitleProperty;
        /// <summary>
        /// 实体的标题属性/主显示属性。
        /// （可能为 null）
        /// </summary>
        public EntityPropertyViewMeta TitleProperty
        {
            get { return this._TitleProperty; }
            set { this.SetValue(ref this._TitleProperty, value); }
        }

        private IList<EntityPropertyViewMeta> _EntityProperties = new List<EntityPropertyViewMeta>();
        public IList<EntityPropertyViewMeta> EntityProperties
        {
            get { return this._EntityProperties; }
        }

        #region Web

        /// <summary>
        /// 将当前对象转换为对应的 Web 界面元数据。
        /// </summary>
        /// <returns></returns>
        public WebEntityViewMeta AsWebView()
        {
            return this as WebEntityViewMeta;
        }

        #endregion

        #region WPF

        /// <summary>
        /// 将当前对象转换为对应的 WPF 界面元数据。
        /// </summary>
        /// <returns></returns>
        public WPFEntityViewMeta AsWPFView()
        {
            return this as WPFEntityViewMeta;
        }

        #endregion

        private int _DetailColumnsCount;
        /// <summary>
        /// 在Detail里显示为几列
        /// </summary>
        public int DetailColumnsCount
        {
            get { return this._DetailColumnsCount; }
            set { this.SetValue(ref this._DetailColumnsCount, value); }
        }

        private bool _NotAllowEdit;
        /// <summary>
        /// 获取是否不允许编辑
        /// </summary>
        public bool NotAllowEdit
        {
            get { return this._NotAllowEdit; }
            set { this.SetValue(ref this._NotAllowEdit, value); }
        }

        private EntityPropertyViewMeta _GroupBy;
        /// <summary>
        /// 默认分组属性值
        /// </summary>
        public EntityPropertyViewMeta GroupBy
        {
            get { return this._GroupBy; }
            set { this.SetValue(ref this._GroupBy, value); }
        }

        #region 其它方法

        internal abstract EntityPropertyViewMeta CreatePropertyViewMeta();

        protected override void OnFrozen()
        {
            //为提高性能，不需要冻结所有子。（上层开发人员需要自觉不要乱修改元数据）

            //base.OnFrozen();

            //this._LockedProperty = new ReadOnlyCollection<EntityPropertyViewMeta>(this._LockedProperty);
            //this._entityProperties = new ReadOnlyCollection<EntityPropertyViewMeta>(this._entityProperties);
            //this._childrenProperties = new ReadOnlyCollection<ChildrenPropertyViewMeta>(this._childrenProperties);
            //this._availableCommands.Freeze();
        }

        private string DebuggerDisplay
        {
            get
            {
                var diplayViewName = string.IsNullOrEmpty(this.ExtendView) ? "基础界面" : this.ExtendView;
                return this.EntityType.FullName + " " + diplayViewName;
            }
        }

        public IEnumerable<EntityPropertyViewMeta> OrderedEntityProperties()
        {
            return EntityPropertyViewMeta.Order(this.EntityProperties);
        }

        /// <summary>
        /// 根据名字查询实体属性
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public EntityPropertyViewMeta Property(IManagedProperty property)
        {
            return this.Property(property.Name);
        }

        /// <summary>
        /// 根据名字查询实体属性（忽略大小写）
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public EntityPropertyViewMeta Property(string name)
        {
            var res = this.EntityProperties.FirstOrDefault(item => item.Name.EqualsIgnoreCase(name));

            if (res != null) { this.OnPropertyFound(res); }

            return res;
        }

        /// <summary>
        /// 当使用 Property 方法查找到某个属性时，发生此事件。
        /// </summary>
        public event EventHandler<PropertyFoundEventArgs> PropertyFound;

        internal void OnPropertyFound(EntityPropertyViewMeta property)
        {
            var handler = this.PropertyFound;
            if (handler != null) handler(this, new PropertyFoundEventArgs(property));
        }

        public class PropertyFoundEventArgs : EventArgs
        {
            public PropertyFoundEventArgs(EntityPropertyViewMeta property)
            {
                this.Property = property;
            }

            public EntityPropertyViewMeta Property { get; private set; }
        }

        #endregion
    }

    /// <summary>
    /// 表单布局模式
    /// </summary>
    public enum DetailLayoutMode
    {
        /// <summary>
        /// 如果只有少量的属性，只需要显示一行时，使用 AutoGrid。
        /// 否则使用 Wrapping
        /// </summary>
        Dynamic,
        /// <summary>
        /// 所有编辑器可自动折行。
        /// 
        /// 可以使用 NewLineInDetail 主动使某个编辑器直接折行。
        /// </summary>
        Wrapping,
        /// <summary>
        /// 根据列数自动为所有编辑器分配到表格中。
        /// </summary>
        AutoGrid,
    }

    /// <summary>
    /// 表单中属性分组的模式
    /// </summary>
    public enum DetailGroupingMode
    {
        /// <summary>
        /// 使用 StackPanel + GroupBox 进行分组。
        /// </summary>
        GroupBox,
        /// <summary>
        /// 使用 TabControl + TabItem 进行分组。
        /// </summary>
        TabItem
    }
}