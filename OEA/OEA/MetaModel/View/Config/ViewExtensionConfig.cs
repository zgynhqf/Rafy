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

        public static EntityViewMeta NotAllowEdit(this EntityViewMeta meta, bool value = true)
        {
            meta.NotAllowEdit = value;

            return meta;
        }

        public static EntityViewMeta HasTitle(this EntityViewMeta meta, IManagedProperty property)
        {
            meta.TitleProperty = meta.Property(property);

            return meta;
        }

        public static EntityViewMeta HasLabel(this EntityViewMeta meta, string label)
        {
            meta.Label = label;
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

        public static EntityViewMeta UseWPFCommands(this EntityViewMeta meta, params Type[] commands)
        {
            return meta.UseWPFCommands(commands as IEnumerable<Type>);
        }

        public static EntityViewMeta UseWPFCommands(this EntityViewMeta meta, IEnumerable<Type> commands)
        {
            if (!OEAEnvironment.IsWeb)
            {
                foreach (var cmd in commands)
                {
                    var command = meta.WPFCommands.Find(cmd);
                    if (command == null)
                    {
                        command = UIModel.WPFCommands[cmd].CloneMutable();
                        meta.WPFCommands.Add(command);
                    }
                }
            }

            return meta;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="commands">
        /// 这里的每一个字符串都是命令类型的全名称。
        /// </param>
        /// <returns></returns>
        public static EntityViewMeta UseWPFCommands(this EntityViewMeta meta, params string[] commands)
        {
            if (!OEAEnvironment.IsWeb)
            {
                foreach (var cmd in commands)
                {
                    var command = meta.WPFCommands.Find(cmd);
                    if (command == null)
                    {
                        command = UIModel.WPFCommands[cmd].CloneMutable();
                        meta.WPFCommands.Add(command);
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

        public static EntityViewMeta RemoveWPFCommands(this EntityViewMeta meta, params Type[] commands)
        {
            meta.WPFCommands.Remove(commands);
            return meta;
        }

        public static EntityViewMeta ClearWPFCommands(this EntityViewMeta meta)
        {
            meta.WPFCommands.Clear();
            return meta;
        }

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

        #region EntityPropertyViewMeta

        public static EntityPropertyViewMeta Readonly(this EntityPropertyViewMeta meta, bool value)
        {
            meta.IsReadonly = value;
            return meta;
        }

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

        public static EntityPropertyViewMeta HasLabel(this EntityPropertyViewMeta meta, string label)
        {
            meta.Label = label;
            return meta;
        }

        public static EntityPropertyViewMeta UseEditor(this EntityPropertyViewMeta meta, string editorName)
        {
            meta.EditorName = editorName;
            return meta;
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