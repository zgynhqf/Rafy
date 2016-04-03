/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130830
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130830 14:46
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;
using Rafy.ManagedProperty;

namespace Rafy.MetaModel.View
{
    /// <summary>
    /// WPF 界面中的实体视图元数据。
    /// </summary>
    public class WPFEntityViewMeta : EntityViewMeta
    {
        private Type _DetailPanelType;
        /// <summary>
        /// 生成 DetailPanel 时使用的控件类型
        /// </summary>
        public Type DetailPanelType
        {
            get { return this._DetailPanelType; }
            set { this.SetValue(ref this._DetailPanelType, value); }
        }

        private double? _DetailLabelSize;
        /// <summary>
        /// 在 DetailPanel 中显示的 Label 的宽度或者高度。
        /// 不指定，则使用系统默认宽度。
        /// </summary>
        public double? DetailLabelSize
        {
            get { return this._DetailLabelSize; }
            set { this.SetValue(ref this._DetailLabelSize, value); }
        }

        private bool _DetailAsHorizontal;
        /// <summary>
        /// 是否需要表单设置为横向布局
        /// </summary>
        public bool DetailAsHorizontal
        {
            get { return this._DetailAsHorizontal; }
            set { this.SetValue(ref this._DetailAsHorizontal, value); }
        }

        private DetailLayoutMode _DetailLayoutMode = DetailLayoutMode.AutoGrid;
        /// <summary>
        /// 表单布局模式。
        /// </summary>
        public DetailLayoutMode DetailLayoutMode
        {
            get { return this._DetailLayoutMode; }
            set { this.SetValue(ref this._DetailLayoutMode, value); }
        }

        private DetailGroupingMode _DetailGroupingMode = DetailGroupingMode.GroupBox;
        /// <summary>
        /// 表单中属性分组的模式
        /// </summary>
        public DetailGroupingMode DetailGroupingMode
        {
            get { return this._DetailGroupingMode; }
            set { this.SetValue(ref this._DetailGroupingMode, value); }
        }

        private List<WPFDetailPropertyGroup> _DetailGroups = new List<WPFDetailPropertyGroup>();
        /// <summary>
        /// 表单中的属性分组信息。
        /// </summary>
        public List<WPFDetailPropertyGroup> DetailGroups
        {
            get { return this._DetailGroups; }
        }

        private WPFCommandCollection _Commands = new WPFCommandCollection();
        /// <summary>
        /// 这个界面块中可用的 WPF 命令。
        /// </summary>
        public WPFCommandCollection Commands
        {
            get { return _Commands; }
        }

        private string _ReportPath;
        /// <summary>
        /// RDLC 报表文件的路径。
        /// 
        /// 报表 RDLC 文件中默认使用实体作为数据源，数据源的名称必须和实体名相同。
        /// </summary>
        public string ReportPath
        {
            get { return this._ReportPath; }
            set { this.SetValue(ref this._ReportPath, value); }
        }

        internal override EntityPropertyViewMeta CreatePropertyViewMeta()
        {
            return new WPFEntityPropertyViewMeta();
        }

        /// <summary>
        /// 根据名字查询实体属性
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public new WPFEntityPropertyViewMeta Property(IManagedProperty property)
        {
            return base.Property(property) as WPFEntityPropertyViewMeta;
        }

        /// <summary>
        /// 根据名字查询实体属性（忽略大小写）
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public new WPFEntityPropertyViewMeta Property(string name)
        {
            return base.Property(name) as WPFEntityPropertyViewMeta;
        }

        /// <summary>
        /// 使用此方法定义的代码块中，自动设置块内所有属性的 GroupLabel。
        /// </summary>
        /// <param name="groupLabel"></param>
        /// <returns></returns>
        public WPFDetailPropertyGroup DeclareGroup(string groupLabel)
        {
            var group = this._DetailGroups.FirstOrDefault(g => g.GroupLabel == groupLabel);
            if (group == null)
            {
                group = new WPFDetailPropertyGroup { GroupLabel = groupLabel };
                this._DetailGroups.Add(group);
                group._owner = this;
            }

            group.StartListening();

            return group;
        }
    }

    /// <summary>
    /// 表单中的一个属性组。
    /// </summary>
    public class WPFDetailPropertyGroup : MetaBase, IDisposable
    {
        private string _GroupLabel;
        /// <summary>
        /// 分组的名称
        /// </summary>
        public string GroupLabel
        {
            get { return this._GroupLabel; }
            internal set { this.SetValue(ref this._GroupLabel, value); }
        }

        private DetailLayoutMode? _LayoutMode;
        /// <summary>
        /// 本组属性应该按照什么方式来布局。
        /// 
        /// 如果没有指定这个属性，则表示使用 WPFEntityViewMeta 中定义的 DetailLayoutMode 进行布局。
        /// </summary>
        public DetailLayoutMode? LayoutMode
        {
            get { return this._LayoutMode; }
            set { this.SetValue(ref this._LayoutMode, value); }
        }

        private List<WPFEntityPropertyViewMeta> _Properties = new List<WPFEntityPropertyViewMeta>();
        /// <summary>
        /// 这个组中的所有属性。
        /// </summary>
        public List<WPFEntityPropertyViewMeta> Properties
        {
            get { return this._Properties; }
            set { this.SetValue(ref this._Properties, value); }
        }

        internal WPFEntityViewMeta _owner;

        internal void StartListening()
        {
            _owner.PropertyFound += owner_PropertyFound;
        }

        private void owner_PropertyFound(object sender, EntityViewMeta.PropertyFoundEventArgs e)
        {
            var item = e.Property as WPFEntityPropertyViewMeta;
            if (!this.Properties.Contains(item))
            {
                this.Properties.Add(item);
            }
        }

        public void Dispose()
        {
            _owner.PropertyFound -= owner_PropertyFound;
        }
    }
}
