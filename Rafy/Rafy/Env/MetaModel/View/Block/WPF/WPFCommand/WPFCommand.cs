/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：2010
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2010
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Rafy.MetaModel.Attributes;
using Rafy.Utils;

namespace Rafy.MetaModel.View
{
    /// <summary>
    /// WPF 客户端命令元数据。
    /// </summary>
    [DebuggerDisplay("Label:{Label} Commands:{RuntimeType.Name}")]
    public class WPFCommand : ViewMeta
    {
        #region 只读属性

        /// <summary>
        /// 获取或设置命令的唯一名称。
        /// 名称即代表了这个命令。
        /// <remarks>
        /// 默认返回命令的全名称。如果想要重复使用同一个命令，则可以为其设置不同的名称。
        /// </remarks>
        /// </summary>
        public override string Name
        {
            get
            {
                var name = base.Name;
                if (!string.IsNullOrEmpty(name)) return name;

                return this.RuntimeType.FullName;
            }
            set { base.Name = value; }
        }

        /// <summary>
        /// 对应的命令的运行时类型
        /// </summary>
        public Type RuntimeType { get; internal set; }

        #endregion

        #region 配置属性

        internal IList<string> _hierarchy = new List<string>();
        /// <summary>
        /// 此命令所处的分级位置。
        /// 
        /// 当命令生成在菜单中时，可以使用如下语法：一级/二级/三级
        /// 当命令生成在工具栏中时，同一组的命令都会生成一个下拉按钮列表中。
        /// </summary>
        public IList<string> Hierarchy
        {
            get { return this._hierarchy; }
        }

        private string _toolTip;
        /// <summary>
        /// 命令显示的鼠标划过文本。
        /// </summary>
        public string ToolTip
        {
            get { return this._toolTip; }
            set { this.SetValue(ref _toolTip, value); }
        }

        private string _imageName;
        /// <summary>
        /// 命令显示的图片名称。
        /// <remarks>
        /// <para>这个图片需要存放在命令所在程序集的 Images 文件夹中。</para>
        /// </remarks>
        /// </summary>
        public string ImageName
        {
            get { return this._imageName; }
            set { this.SetValue(ref _imageName, value); }
        }

        private string _Gestures;
        /// <summary>
        /// 快捷键表达式
        /// </summary>
        public string Gestures
        {
            get { return this._Gestures; }
            set { this.SetValue(ref this._Gestures, value); }
        }

        private Type _groupAlgorithmType;
        /// <summary>
        /// 为这个Command进行“生成分组”的算法类。
        /// 如果为null，则会使用默认生成算法。
        /// </summary>
        public Type GroupAlgorithmType
        {
            get { return this._groupAlgorithmType; }
            set { this.SetValue(ref _groupAlgorithmType, value); }
        }

        private int _GroupType;
        /// <summary>
        /// 命令的分组位置
        /// 
        /// 常用的分组位置参见类型 <see cref="CommandGroupType"/>
        /// </summary>
        public int GroupType
        {
            get { return this._GroupType; }
            set { this.SetValue(ref _GroupType, value); }
        }

        private CommandLocation _location;
        /// <summary>
        /// 命令生成的控件的位置。
        /// </summary>
        public CommandLocation Location
        {
            get { return this._location; }
            set { this.SetValue(ref _location, value); }
        }

        /// <summary>
        /// 判断是否已经指定了要生成在某个位置。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool HasLocation(CommandLocation value)
        {
            return (this.Location & value) == value;
        }

        #endregion

        #region 其它方法

        /// <summary>
        /// 复制出一个可变的元数据对象。
        /// </summary>
        /// <returns></returns>
        public WPFCommand CloneMutable()
        {
            return this.Clone(new FreezableCloneOptions()) as WPFCommand;
        }

        #endregion
    }
}