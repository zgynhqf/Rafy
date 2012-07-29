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
using System.Linq;
using System.Text;

using OEA.MetaModel.View;

namespace OEA.MetaModel.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CommandAttribute : Attribute
    {
        private string _toolTip;

        public CommandAttribute()
        {
            this.Location = CommandLocation.Toolbar;
        }

        public string Label { get; set; }

        public string ToolTip
        {
            get
            {
                if (string.IsNullOrEmpty(this._toolTip))
                {
                    return this.Label;
                }
                return this._toolTip;
            }
            set
            {
                this._toolTip = value;
            }
        }

        public string ImageName { get; set; }

        #region WPF

        /// <summary>
        /// 格式：Ctrl+A，F2
        /// 
        /// WPF Only
        /// </summary>
        public string Gestures { get; set; }

        #endregion

        /// <summary>
        /// 为这个Command进行“生成分组”的算法类。
        /// 如果为null，则会使用默认生成算法。
        /// </summary>
        public Type UIAlgorithm { get; set; }

        /// <summary>
        /// 使用哪个类型的 CommandInfo 来承载元数据。
        /// 可以使用不同的 CommandInfo 的子类来实现元数据的多态。
        /// </summary>
        public Type CommandInfoType { get; set; }

        /// <summary>
        /// 此命令分组的分组名。
        /// 
        /// 当 CommandType 的值是 Menu 时，可以使用如下语法：一级/二级/三级
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// 命令的按钮分类类型
        /// </summary>
        public CommandGroupType GroupType { get; set; }

        public CommandLocation Location { get; set; }
    }
}