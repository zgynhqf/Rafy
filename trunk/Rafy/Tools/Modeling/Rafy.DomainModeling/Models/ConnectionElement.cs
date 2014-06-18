/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130403 22:28
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130403 22:28
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Rafy.DomainModeling.Models
{
    /// <summary>
    /// 连接元素
    /// </summary>
    public class ConnectionElement : DocumentViewModel, IConnectionKey
    {
        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public ConnectionElement(string from, string to)
        {
            this.From = from;
            this.To = to;
        }

        private string _From;
        /// <summary>
        /// 指定起始块的主键。
        /// </summary>
        public string From
        {
            get { return this._From; }
            set
            {
                if (_From != value)
                {
                    _From = value;
                    this.OnPropertyChanged("From");
                }
            }
        }

        private string _To;
        /// <summary>
        /// 指定终止块的主键。
        /// </summary>
        public string To
        {
            get { return this._To; }
            set
            {
                if (_To != value)
                {
                    _To = value;
                    this.OnPropertyChanged("To");
                }
            }
        }

        private string _Label;
        /// <summary>
        /// 连接显示的标签
        /// </summary>
        public string Label
        {
            get { return this._Label; }
            set
            {
                if (_Label != value)
                {
                    _Label = value;
                    this.OnPropertyChanged("Label");
                }
            }
        }

        private bool _LabelVisible = true;
        /// <summary>
        /// 是否显示 Label
        /// </summary>
        public bool LabelVisible
        {
            get { return this._LabelVisible; }
            set
            {
                if (_LabelVisible != value)
                {
                    _LabelVisible = value;
                    this.OnPropertyChanged("LabelVisible");
                }
            }
        }

        private bool _Hidden;
        /// <summary>
        /// 是否在界面中隐藏。
        /// </summary>
        public bool Hidden
        {
            get { return this._Hidden; }
            set
            {
                if (_Hidden != value)
                {
                    _Hidden = value;
                    this.OnPropertyChanged("Hidden");
                }
            }
        }

        private ConnectionType _ConnectionType;
        /// <summary>
        /// 连接的类型。
        /// </summary>
        public ConnectionType ConnectionType
        {
            get { return this._ConnectionType; }
            set
            {
                if (_ConnectionType != value)
                {
                    _ConnectionType = value;
                    this.OnPropertyChanged("ConnectionType");
                }
            }
        }

        private Point? _FromPointPos;
        /// <summary>
        /// 连接线连接到起始元素的点的相对位置。
        /// </summary>
        public Point? FromPointPos
        {
            get { return this._FromPointPos; }
            set
            {
                if (_FromPointPos != value)
                {
                    _FromPointPos = value;
                    this.OnPropertyChanged("FromPointPos");
                }
            }
        }

        private Point? _ToPointPos;
        /// <summary>
        /// 连接线连接到终止元素的点的相对位置。
        /// </summary>
        public Point? ToPointPos
        {
            get { return this._ToPointPos; }
            set
            {
                if (_ToPointPos != value)
                {
                    _ToPointPos = value;
                    this.OnPropertyChanged("ToPointPos");
                }
            }
        }
    }
}
