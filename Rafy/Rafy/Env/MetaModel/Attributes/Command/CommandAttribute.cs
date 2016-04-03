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

using Rafy.MetaModel.View;

namespace Rafy.MetaModel.Attributes
{
    /// <summary>
    /// 用于描述一个命令元数据的标签。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CommandAttribute : Attribute
    {
        private string _toolTip;

        public CommandAttribute()
        {
            this.Location = CommandLocation.Toolbar;
        }

        /// <summary>
        /// 命令显示的文本。
        /// <remarks>
        /// <para>
        /// 如果没有设置，默认值为 null，则系统会使用命令的类型名称作为显示。
        /// 如果希望不显示任何文本，需要默认为 <see cref="String.Empty"/> 。
        /// </para>
        /// </remarks>
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// 命令显示的鼠标划过文本。
        /// </summary>
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

        /// <summary>
        /// 命令显示的图片名称。
        /// <remarks>
        /// <para>这个图片需要存放在命令所在程序集的 Images 文件夹中。</para>
        /// </remarks>
        /// </summary>
        public string ImageName { get; set; }

        #region WPF

        /// <summary>
        /// 快捷键标识。
        /// <remarks>
        /// 可以用以下格式：Ctrl+A；F2；
        /// 
        /// WPF Only
        /// </remarks>
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
        /// 此命令所处的分级位置。
        /// 
        /// 当命令生成在菜单中时，可以使用如下语法：一级/二级/三级
        /// 当命令生成在工具栏中时，同一组的命令都会生成一个下拉按钮列表中。
        /// </summary>
        public string Hierarchy { get; set; }

        /// <summary>
        /// 命令的按钮分类类型。
        /// 
        /// 不同分类类型的按钮，将会以 '|' 分隔起来。
        /// 
        /// 常用的分组位置参见类型 <see cref="CommandGroupType"/>
        /// </summary>
        public int GroupType { get; set; }

        /// <summary>
        /// 命令生成的控件的位置。
        /// </summary>
        public CommandLocation Location { get; set; }
    }
}