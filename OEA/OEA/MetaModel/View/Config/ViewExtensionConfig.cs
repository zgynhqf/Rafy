/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111110
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111110
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.ManagedProperty;

namespace OEA.MetaModel.View
{
    /// <summary>
    /// 为元数据扩展的配置 API
    /// </summary>
    public static class ViewExtensionConfig
    {
        #region EntityViewMeta

        /// <summary>
        /// 设置实体的领域含义。
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public static EntityViewMeta DomainName(this EntityViewMeta meta, string label)
        {
            meta.Label = label;
            return meta;
        }

        /// <summary>
        /// 设置实体的主显示属性。
        /// 
        /// 例如，用户的主显示属性一般是用户姓名。
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static EntityViewMeta HasDelegate(this EntityViewMeta meta, IManagedProperty property)
        {
            meta.TitleProperty = meta.Property(property);

            return meta;
        }

        /// <summary>
        /// 设置该实体是否可以在界面上进行编辑
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static EntityViewMeta DisableEditing(this EntityViewMeta meta, bool value = true)
        {
            meta.NotAllowEdit = value;

            return meta;
        }

        /// <summary>
        /// 不再使用分页。
        /// 
        /// WebOnly
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static EntityViewMeta WithoutPaging(this EntityViewMeta meta)
        {
            meta.PageSize = 10000;

            return meta;
        }

        /// <summary>
        /// 隐藏所有属性的可见性。
        /// 
        /// 一般使用在扩展视图中。
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static EntityViewMeta HideProperties(this EntityViewMeta meta, params IManagedProperty[] properties)
        {
            foreach (var p in properties)
            {
                meta.Property(p).ShowInWhere = ShowInWhere.Hide;
            }

            return meta;
        }

        /// <summary>
        /// 设置实体在列表中显示时，按照哪个属性分组。
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static EntityViewMeta GroupBy(this EntityViewMeta meta, IManagedProperty property)
        {
            //foreach (var item in properties) { meta.GroupDescriptions.Add(item); }

            //暂时只支持一个属性分组
            meta.GroupBy = meta.Property(property);

            return meta;
        }

        /// <summary>
        /// 声明锁定属性列表
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static EntityViewMeta HasLockProperty(this EntityViewMeta meta, params IManagedProperty[] properties)
        {
            foreach (var item in properties)
            {
                var property = meta.Property(item);
                meta.LockedProperties.Add(property);
            }

            return meta;
        }

        /// <summary>
        /// 使用此方法定义的代码块中，自动设置块内所有属性的 GroupLabel。
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public static IDisposable DeclareGroup(this EntityViewMeta meta, string groupLabel)
        {
            return new DeclaringGroupWrapper(meta, groupLabel);
        }
        private class DeclaringGroupWrapper : IDisposable
        {
            private EntityViewMeta _owner;
            private string _groupLabel;

            public DeclaringGroupWrapper(EntityViewMeta owner, string groupLabel)
            {
                this._owner = owner;
                this._groupLabel = groupLabel;

                owner.PropertyFound += owner_PropertyFound;
            }

            private void owner_PropertyFound(object sender, EntityViewMeta.PropertyFoundEventArgs e)
            {
                e.Property.DetailGroupName = this._groupLabel;
            }

            public void Dispose()
            {
                this._owner.PropertyFound -= owner_PropertyFound;
            }
        }

        /// <summary>
        /// 使用此方法定义的代码块中，自动根据代码调用的顺序设置属性排列的顺序。
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public static IDisposable OrderProperties(this EntityViewMeta meta, int from = 1)
        {
            return new OrderPropertiesWrapper(meta, from);
        }
        private class OrderPropertiesWrapper : IDisposable
        {
            private EntityViewMeta _owner;
            private int _orderNo;

            public OrderPropertiesWrapper(EntityViewMeta owner, int orderNo)
            {
                this._owner = owner;
                this._orderNo = orderNo;

                owner.PropertyFound += owner_PropertyFound;
            }

            private void owner_PropertyFound(object sender, EntityViewMeta.PropertyFoundEventArgs e)
            {
                e.Property.OrderNo = this._orderNo++;
            }

            public void Dispose()
            {
                this._owner.PropertyFound -= owner_PropertyFound;
            }
        }

        /// <summary>
        /// 隐藏所有属性。
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        public static EntityViewMeta HideAllProperties(this EntityViewMeta meta)
        {
            foreach (var item in meta.EntityProperties)
            {
                item.ShowIn(ShowInWhere.Hide);
            }

            return meta;
        }

        /// <summary>
        /// 使用自定义表单作为表单控件。
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        public static EntityViewMeta UseDetailPanel<TControl>(this EntityViewMeta meta)
        {
            meta.DetailPanelType = typeof(TControl);
            return meta;
        }

        /// <summary>
        /// 使用动态表单作为表单控件。
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        public static EntityViewMeta UseAutoDetailPanel(this EntityViewMeta meta)
        {
            meta.DetailPanelType = null;
            return meta;
        }

        /// <summary>
        /// 声明当前表单使用的布局模式。
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static EntityViewMeta UseDetailLayoutMode(this EntityViewMeta meta, DetailLayoutMode value)
        {
            meta.DetailLayoutMode = value;

            return meta;
        }

        /// <summary>
        /// 声明当前表单使用的属性分组模式。
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static EntityViewMeta UseDetailGroupingMode(this EntityViewMeta meta, DetailGroupingMode value)
        {
            meta.DetailGroupingMode = value;

            return meta;
        }

        /// <summary>
        /// 声明当前表单使用水平布局。
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static EntityViewMeta UseDetailAsHorizontal(this EntityViewMeta meta, bool value = true)
        {
            meta.DetailAsHorizontal = value;

            return meta;
        }

        /// <summary>
        /// 在 DetailPanel 中显示的 Label 的宽度或者高度。
        /// 不指定，则使用系统默认宽度。
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static EntityViewMeta HasDetailLabelSize(this EntityViewMeta meta, double? value)
        {
            meta.DetailLabelSize = value;

            return meta;
        }

        /// <summary>
        /// 声明在Detail里显示为几列。
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static EntityViewMeta HasDetailColumnsCount(this EntityViewMeta meta, int value)
        {
            meta.DetailColumnsCount = value;
            return meta;
        }

        #endregion

        #region EntityViewMeta Commands

        /// <summary>
        /// 注意，使用此方法后，返回值是 JsCommand，可对其继续进行配置。
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static WebCommand UseWebCommand(this EntityViewMeta meta, string cmd)
        {
            if (OEAEnvironment.IsWeb)
            {
                var jsCmd = meta.WebCommands.Find(cmd);
                if (jsCmd == null)
                {
                    jsCmd = UIModel.WebCommands[cmd].CloneMutable() as WebCommand;
                    meta.WebCommands.Add(jsCmd);
                }

                return jsCmd;
            }

            //如果不是在 Web 环境下，则直接返回一个无用的 WebCommand
            return new WebCommand();
        }

        public static EntityViewMeta UseWebCommands(this EntityViewMeta meta, params string[] commands)
        {
            return meta.UseWebCommands(commands as IEnumerable<string>);
        }

        public static EntityViewMeta UseWebCommands(this EntityViewMeta meta, IEnumerable<string> commands)
        {
            if (OEAEnvironment.IsWeb)
            {
                foreach (var cmd in commands)
                {
                    var jsCmd = meta.WebCommands.Find(cmd);
                    if (jsCmd == null)
                    {
                        jsCmd = UIModel.WebCommands[cmd].CloneMutable();
                        meta.WebCommands.Add(jsCmd);
                    }
                }
            }

            return meta;
        }

        /// <summary>
        /// 指定某个类型使用指定的命令列表。
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="commands">
        /// 只支持两个类型：String、Type
        /// 如果是字符串，则是命令类型的全名称。
        /// </param>
        /// <returns></returns>
        public static EntityViewMeta UseWPFCommands(this EntityViewMeta meta, params object[] commands)
        {
            return meta.UseWPFCommands(commands as IEnumerable<object>);
        }

        /// <summary>
        /// 指定某个类型使用指定的命令列表。
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="commands">
        /// 只支持两个类型：String、Type
        /// 如果是字符串，则是命令类型的全名称。
        /// </param>
        /// <returns></returns>
        public static EntityViewMeta UseWPFCommands(this EntityViewMeta meta, IEnumerable<object> commands)
        {
            if (!OEAEnvironment.IsWeb)
            {
                foreach (var cmd in commands)
                {
                    var cmdStr = cmd as string;
                    if (cmdStr != null)
                    {
                        var command = meta.WPFCommands.Find(cmdStr);
                        if (command == null)
                        {
                            command = UIModel.WPFCommands[cmdStr].CloneMutable();
                            meta.WPFCommands.Add(command);
                        }
                    }
                    else
                    {
                        var cmdType = cmd as Type;
                        if (cmdType == null) throw new ArgumentNullException("只支持两个类型：String、Type");

                        var command = meta.WPFCommands.Find(cmdType);
                        if (command == null)
                        {
                            command = UIModel.WPFCommands[cmdType].CloneMutable();
                            meta.WPFCommands.Add(command);
                        }
                    }
                }
            }

            return meta;
        }

        public static EntityViewMeta RemoveWebCommands(this EntityViewMeta meta, params string[] commands)
        {
            meta.WebCommands.Remove(commands);
            return meta;
        }

        public static EntityViewMeta RemoveWebCommands(this EntityViewMeta meta, IEnumerable<string> commands)
        {
            meta.WebCommands.Remove(commands);
            return meta;
        }

        public static EntityViewMeta RemoveWPFCommands(this EntityViewMeta meta, params Type[] commands)
        {
            meta.WPFCommands.Remove(commands);
            return meta;
        }

        public static EntityViewMeta RemoveWPFCommands(this EntityViewMeta meta, IEnumerable<Type> commands)
        {
            meta.WPFCommands.Remove(commands);
            return meta;
        }

        public static EntityViewMeta ClearWebCommands(this EntityViewMeta meta, bool removeCustomizeUI = true)
        {
            meta.WebCommands.Clear();

            if (!removeCustomizeUI && OEAEnvironment.IsDebuggingEnabled)
            {
                meta.UseWebCommands(WebCommandNames.CustomizeUI);
            }

            return meta;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="removeCustomizeUI">
        /// 是否在清除的同时，保留“界面配置按钮。”。
        /// 一般情况下，在 ConfigView 方法内部调用时，此参数需要传入 false，其它情况则使用默认值 true
        /// </param>
        /// <returns></returns>
        public static EntityViewMeta ClearWPFCommands(this EntityViewMeta meta, bool removeCustomizeUI = true)
        {
            meta.WPFCommands.Clear();

            if (!removeCustomizeUI && OEAEnvironment.IsDebuggingEnabled)
            {
                meta.UseWPFCommands(WPFCommandNames.CustomizeUI);
            }

            return meta;
        }

        #endregion

        #region EntityPropertyViewMeta

        /// <summary>
        /// 设置该属性是否为只读
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static EntityPropertyViewMeta Readonly(this EntityPropertyViewMeta meta, bool value = true)
        {
            if (OEAEnvironment.IsWeb)
            {
                meta.IsReadonly = value;
            }
            else
            {
                meta.ReadonlyIndicator.Status = value ? PropertyReadonlyStatus.Readonly : PropertyReadonlyStatus.None;
            }

            return meta;
        }

        /// <summary>
        /// 设置该属性为动态检查是否只读
        /// 
        /// WPF Only
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="indicator">动态根据此属性来检查是否只读。</param>
        /// <returns></returns>
        public static EntityPropertyViewMeta Readonly(this EntityPropertyViewMeta meta, IManagedProperty indicator)
        {
            meta.ReadonlyIndicator.Status = PropertyReadonlyStatus.Dynamic;
            meta.ReadonlyIndicator.Property = indicator;

            return meta;
        }

        /// <summary>
        /// 设置该属性为动态检查是否可见
        /// 
        /// WPF Only
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="value">是否可见。</param>
        /// <returns></returns>
        public static EntityPropertyViewMeta Visibility(this EntityPropertyViewMeta meta, bool value = false)
        {
            meta.VisibilityIndicator.VisiblityType = value ? VisiblityType.AlwaysHide : VisiblityType.AlwaysShow;

            return meta;
        }

        /// <summary>
        /// 设置该属性为动态检查是否可见
        /// 
        /// WPF Only
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="indicator">动态根据此属性来检查是否可见。</param>
        /// <returns></returns>
        public static EntityPropertyViewMeta Visibility(this EntityPropertyViewMeta meta, IManagedProperty indicator)
        {
            meta.VisibilityIndicator.VisiblityType = VisiblityType.Dynamic;
            meta.VisibilityIndicator.Property = indicator;

            return meta;
        }

        /// <summary>
        /// 设置该属性可显示的范围
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static EntityPropertyViewMeta ShowIn(this EntityPropertyViewMeta meta, ShowInWhere value)
        {
            if (value == ShowInWhere.Hide)
            {
                meta.ShowInWhere = ShowInWhere.Hide;
            }
            else
            {
                meta.ShowInWhere |= value;
            }

            return meta;
        }

        /// <summary>
        /// 设置该属性需要显示在列表中，并设置其列表中的信息
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="gridWidth">
        /// 用于初始化表格控件的宽度属性
        /// </param>
        /// <returns></returns>
        public static EntityPropertyViewMeta ShowInList(this EntityPropertyViewMeta meta, double? gridWidth = null)
        {
            meta.ShowInWhere |= ShowInWhere.List;
            meta.GridWidth = gridWidth;

            return meta;
        }

        /// <summary>
        /// 设置该属性在表单中显示时的详细信息
        /// 
        /// 注意，并不是所有属性在所有模式下都有用，所以请选择性设置。
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="contentWidth">
        /// 表单中该属性所占的格子宽度。
        /// 
        /// 如果值在 0 到 1 之间，表示百分比，只有 DetailLayoutMode.AutoGrid 模式下可用。
        /// 否则表示绝对值。
        /// 
        /// 不指定，则使用系统默认值。
        /// </param>
        /// <param name="height">
        /// 表单中该属性所占的总高度
        /// 不指定，则使用系统默认宽度。
        /// </param>
        /// <param name="labelSize">
        /// 在 DetailPanel 中显示的 Label 的宽度或者高度。
        /// 不指定，则使用系统默认值。
        /// </param>
        /// <param name="needNewLine">
        /// 指定某个属性在表单中是否需要开启新行。
        /// 此属性只在 DetailLayoutMode.Wrapping 下有用。
        /// </param>
        /// <param name="columnSpan">
        /// 表单中该属性所占的列数。
        /// 只在 DetailLayoutMode.AutoGrid 模式下有用。
        /// </param>
        /// <returns></returns>
        public static EntityPropertyViewMeta ShowInDetail(this EntityPropertyViewMeta meta,
            //common
            double? contentWidth = null, double? height = null, double? labelSize = null,
            //WrappingMode
            bool needNewLine = false,
            //AutoGridMode
            int? columnSpan = null
            )
        {
            meta.ShowInWhere |= ShowInWhere.Detail;

            meta.DetailLabelSize = labelSize;
            meta.DetailContentWidth = contentWidth;
            meta.DetailHeight = height;
            meta.DetailColumnsSpan = columnSpan;
            meta.DetailNewLine = needNewLine;

            return meta;
        }

        /// <summary>
        /// 设置属性的显示名称
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public static EntityPropertyViewMeta HasLabel(this EntityPropertyViewMeta meta, string label)
        {
            meta.Label = label;
            return meta;
        }

        /// <summary>
        /// 设置属性的编辑器
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="editorName"></param>
        /// <returns></returns>
        public static EntityPropertyViewMeta UseEditor(this EntityPropertyViewMeta meta, string editorName)
        {
            meta.EditorName = editorName;
            return meta;
        }

        /// <summary>
        /// 设置属性的编辑器
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="editorName"></param>
        /// <returns></returns>
        public static EntityPropertyViewMeta UseLookupDataSource(this EntityPropertyViewMeta meta, IManagedProperty property)
        {
            var refView = meta.ReferenceViewInfo;
            if (refView == null) throw new InvalidOperationException("只有引用实体属性才可以为其设置数据源。");

            refView.DataSourceProperty = property;

            return meta;
        }

        /// <summary>
        /// 设置该属性为导航项。
        /// 此时，如果该属性变更，会自动触发导航查询
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static NavigationPropertyMeta FireNavigation(this EntityPropertyViewMeta meta)
        {
            meta.NavigationMeta = new NavigationPropertyMeta();

            return meta.NavigationMeta;
        }

        /// <summary>
        /// 具体指定某个属性的排序号
        /// 
        /// 一般使用在扩展视图中。
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="orderNo"></param>
        /// <returns></returns>
        public static EntityPropertyViewMeta HasOrderNo(this EntityPropertyViewMeta meta, double orderNo)
        {
            meta.OrderNo = orderNo;
            return meta;
        }

        #endregion

        #region 其它

        public static WebCommand HasLabel(this WebCommand meta, string label)
        {
            meta.Label = label;
            meta.LabelModified = true;
            return meta;
        }

        public static ViewMeta HasLabel(this ViewMeta meta, string label)
        {
            meta.Label = label;
            return meta;
        }

        public static ViewMeta IsVisible(this ViewMeta meta, bool value)
        {
            meta.IsVisible = value;
            return meta;
        }

        #endregion
    }
}