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
using OEA.MetaModel.Attributes;
using OEA.ManagedProperty;

namespace OEA.MetaModel.View
{
    /// <summary>
    /// 一个实体类的视图元数据
    /// 
    /// 一个实体并不只对应一个视图元数据
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public class EntityViewMeta : Meta
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
            set { this.SetValue(ref this._label, value, "Label"); }
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

        private int _PageSize = 25;
        /// <summary>
        /// 超过 10000 就不分页了。
        /// </summary>
        public int PageSize
        {
            get { return this._PageSize; }
            set { this.SetValue(ref this._PageSize, value); }
        }

        private CommandCollection<WebCommand> _WebCommands = new CommandCollection<WebCommand>();
        /// <summary>
        /// 为这个类可用的命令
        /// </summary>
        public CommandCollection<WebCommand> WebCommands
        {
            get { return _WebCommands; }
        }

        private IList<EntityPropertyViewMeta> _LockedProperty = new List<EntityPropertyViewMeta>();
        public IList<EntityPropertyViewMeta> LockedProperties
        {
            get { return this._LockedProperty; }
        }

        #endregion

        #region WPF

        private WPFCommandCollection _WPFCommands = new WPFCommandCollection();
        /// <summary>
        /// 为这个类可用的命令
        /// </summary>
        public WPFCommandCollection WPFCommands
        {
            get { return _WPFCommands; }
        }

        #endregion

        private int _ColumnsCountShowInDetail;
        /// <summary>
        /// 在Detail里显示为几列
        /// </summary>
        public int ColumnsCountShowInDetail
        {
            get { return this._ColumnsCountShowInDetail; }
            set { this.SetValue(ref this._ColumnsCountShowInDetail, value); }
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
                var diplayViewName = string.IsNullOrEmpty(this.ExtendView) ? "全局界面" : this.ExtendView;
                return this.EntityType.FullName + " " + diplayViewName;
            }
        }

        public IEnumerable<EntityPropertyViewMeta> OrderedEntityProperties()
        {
            return this.Ordered(this.EntityProperties).Cast<EntityPropertyViewMeta>();
        }

        /// <summary>
        /// 把000010000300002排序为000000000000123
        /// 
        /// 不把0的位置改变。
        /// </summary>
        private EntityPropertyViewMeta[] Ordered(IEnumerable<EntityPropertyViewMeta> properties)
        {
            //这个方法会打乱都是0的节点原有的顺序。
            //properties.Sort((a, b) => a.OrderNo.CompareTo(b.OrderNo));

            var res = new EntityPropertyViewMeta[properties.Count()];

            var lesszerolist = properties.Where(p => p.OrderNo < 0).OrderBy(p => p.OrderNo).ToArray();
            var zeroList = properties.Where(p => p.OrderNo == 0).ToArray();
            var greaterzerolist = properties.Where(p => p.OrderNo > 0).OrderBy(p => p.OrderNo).ToArray();

            int i = 0;
            foreach (var item in lesszerolist) res[i++] = item;
            foreach (var item in zeroList) res[i++] = item;
            foreach (var item in greaterzerolist) res[i++] = item;

            return res;
        }

        #endregion
    }
}