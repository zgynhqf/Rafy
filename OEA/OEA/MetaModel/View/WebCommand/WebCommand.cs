/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120312
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120312
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace OEA.MetaModel.View
{
    /// <summary>
    /// Javascript 命令的元数据
    /// </summary>
    public class WebCommand : ViewMeta
    {
        private string _JavascriptCode;
        /// <summary>
        /// 对应的 javascript 代码
        /// </summary>
        public string JavascriptCode
        {
            get { return this._JavascriptCode; }
            set { this.SetValue(ref this._JavascriptCode, value); }
        }

        private bool _LabelModified;
        /// <summary>
        /// 由于 Label 虽然有值，但是不一定要传输到客户端。所以用这个值来分辨是否需要传输。
        /// </summary>
        public bool LabelModified
        {
            get { return this._LabelModified; }
            set { this.SetValue(ref this._LabelModified, value); }
        }

        /// <summary>
        /// 继承自哪个类
        /// </summary>
        internal string Extend;

        public WebCommand CloneMutable()
        {
            return this.Clone(new CloneOption()) as WebCommand;
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Label")
            {
                this.LabelModified = true;
            }

            base.OnPropertyChanged(e);
        }
    }
}