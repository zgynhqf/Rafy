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

        public static EntityViewMeta ShowInList(this EntityViewMeta meta, params IManagedProperty[] properties)
        {
            foreach (var p in properties)
            {
                meta.Property(p).ShowInWhere |= ShowInWhere.List;
            }

            return meta;
        }

        public static EntityViewMeta ShowInDetail(this EntityViewMeta meta, params IManagedProperty[] properties)
        {
            foreach (var p in properties)
            {
                meta.Property(p).ShowInWhere |= ShowInWhere.Detail;
            }

            return meta;
        }

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

        public static EntityViewMeta HasLockProperty(this EntityViewMeta meta, params IManagedProperty[] properties)
        {
            foreach (var item in properties)
            {
                var property = meta.Property(item);
                meta.LockedProperties.Add(property);
            }

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
        /// 设置该属性在表单中显示时的详细信息
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="columnSpan"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="labelWidth"></param>
        /// <returns></returns>
        public static EntityPropertyViewMeta ShowInDetail(this EntityPropertyViewMeta meta,
            int? columnSpan = null, double? width = null, int? height = null, int? labelWidth = null
            )
        {
            meta.ShowInWhere |= ShowInWhere.Detail;

            meta.DetailColumnsSpan = columnSpan;
            meta.DetailLabelWidth = labelWidth;
            meta.DetailWidth = width;
            meta.DetailHeight = height;

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

            refView.DataSourceProperty = property.Name;

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

        #endregion

        public static EntityPropertyViewMeta HasOrderNo(this EntityPropertyViewMeta meta, int orderNo)
        {
            meta.OrderNo = orderNo;
            return meta;
        }

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
    }
}