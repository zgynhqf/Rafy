using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using OEA.MetaModel.Attributes;

namespace OEA.MetaModel.View
{
    [DebuggerDisplay("Label:{Label} Commands:{RuntimeType.Name}")]
    public class WPFCommand : ViewMeta
    {
        #region 只读属性

        /// <summary>
        /// KeyName
        /// </summary>
        public override string Name
        {
            get { return this.RuntimeType.FullName; }
            set { }
        }

        /// <summary>
        /// 对应的命令的运行时类型
        /// </summary>
        public Type RuntimeType { get; internal set; }

        #endregion

        #region 配置属性

        internal IList<string> _groups = new List<string>();
        /// <summary>
        /// 此命令如何分组。
        /// </summary>
        public IList<string> Groups
        {
            get { return this._groups; }
        }

        private string _toolTip;
        public string ToolTip
        {
            get { return this._toolTip; }
            set { this.SetValue(ref _toolTip, value); }
        }

        private string _imageName;
        public string ImageName
        {
            get { return this._imageName; }
            set { this.SetValue(ref _imageName, value); }
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

        private CommandGroupType _GroupType;
        public CommandGroupType GroupType
        {
            get { return this._GroupType; }
            set { this.SetValue(ref _GroupType, value); }
        }

        private CommandLocation _location;
        public CommandLocation Location
        {
            get { return this._location; }
            set { this.SetValue(ref _location, value); }
        }

        public bool HasLocation(CommandLocation value)
        {
            return (this.Location & value) == value;
        }

        #endregion

        public WPFCommand CloneMutable()
        {
            return this.Clone(new CloneOption()) as WPFCommand;
        }
    }
}