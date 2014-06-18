/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121108 12:56
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121108 12:56
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.WPF;
using Rafy.WPF.Command.UI;

namespace Rafy.WPF.Command
{
    /// <summary>
    /// 一个简单的过滤命令的基类。
    /// </summary>
    [Command(Label = "过滤", GroupType = CommandGroupType.View, UIAlgorithm = typeof(GenericItemAlgorithm<TextBoxItemGenerator>))]
    public class FilterCommand : ListViewCommand
    {
        private ListLogicalView _view;

        /// <summary>
        /// 是否在过滤时忽略大小写。
        /// </summary>
        protected bool IgnoreCase = true;

        public override void Execute(ListLogicalView view)
        {
            this._view = view;

            var input = TextBoxItemGenerator.GetTextBoxParameter(this);
            if (string.IsNullOrEmpty(input))
            {
                view.Filter = null;
            }
            else
            {
                view.Filter = e => this.CanShow(e, input);
            }
        }

        /// <summary>
        /// 子类重写以实现过滤逻辑
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        protected virtual bool CanShow(Entity entity, string input)
        {
            if (this.IgnoreCase) input = input.ToLower();

            foreach (var filterProperty in this.GetFilterProperty(entity))
            {
                var value = entity.GetProperty(filterProperty) as string;
                if (value != null)
                {
                    if (this.IgnoreCase) value = value.ToLower();
                    if(value.Contains(input)) return true;
                }
            }
            return false;
        }

        private IManagedProperty[] _listPropertyList;

        /// <summary>
        /// 获取用于过滤的属性。
        /// 
        /// 默认返回所有在列表中显示的字符串属性。
        /// </summary>
        /// <returns></returns>
        protected virtual IManagedProperty[] GetFilterProperty(Entity entity)
        {
            if (this._listPropertyList == null)
            {
                this._listPropertyList = this._view.Meta.OrderedEntityProperties()
                    .Where(p => p.CanShowIn(ShowInWhere.List) && p.PropertyMeta.ManagedProperty.PropertyType == typeof(string))
                    .Select(vm => vm.PropertyMeta.ManagedProperty)
                    .ToArray();
                if (this._listPropertyList.Length == 0) throw new InvalidProgramException("过滤命令需要视图至少有一个显示在列表中的字符串属性。");
            }
            return this._listPropertyList;
        }
    }
}