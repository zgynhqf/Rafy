/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130412 17:28
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130412 17:28
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.DomainModeling.Models
{
    /// <summary>
    /// 块状元素
    /// </summary>
    public abstract class BlockElement : DocumentViewModel
    {
        public BlockElement(string fullName)
        {
            this.FullName = fullName;
        }

        private string _Name;
        /// <summary>
        /// 类型名。
        /// </summary>
        public string Name
        {
            get { return this._Name; }
            private set
            {
                if (_Name != value)
                {
                    _Name = value;
                    this.OnPropertyChanged("Name");
                }
            }
        }

        private string _FullName;
        /// <summary>
        /// 带上命名空间的类型名。
        /// </summary>
        public string FullName
        {
            get { return this._FullName; }
            set
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentNullException("value");

                if (_FullName != value)
                {
                    _FullName = value;
                    this.OnPropertyChanged("FullName");

                    //计算 _Name 的值。
                    var index = value.LastIndexOf('.');
                    if (index >= 0)
                    {
                        this.Name = value.Substring(index + 1);
                    }
                    else
                    {
                        this.Name = value;
                    }
                }
            }
        }

        private string _Label;
        /// <summary>
        /// 标题
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

        private double _Left;
        /// <summary>
        /// 位置：左
        /// </summary>
        public double Left
        {
            get { return this._Left; }
            set
            {
                if (_Left != value)
                {
                    _Left = value;
                    this.OnPropertyChanged("Left");
                }
            }
        }

        private double _Top;
        /// <summary>
        /// 位置：右
        /// </summary>
        public double Top
        {
            get { return this._Top; }
            set
            {
                if (_Top != value)
                {
                    _Top = value;
                    this.OnPropertyChanged("Top");
                }
            }
        }

        private double _Width;
        /// <summary>
        /// 大小：宽
        /// </summary>
        public double Width
        {
            get { return this._Width; }
            set
            {
                if (_Width != value)
                {
                    _Width = value;
                    this.OnPropertyChanged("Width");
                }
            }
        }

        private double _Height;
        /// <summary>
        /// 大小：高
        /// </summary>
        public double Height
        {
            get { return this._Height; }
            set
            {
                if (_Height != value)
                {
                    _Height = value;
                    this.OnPropertyChanged("Height");
                }
            }
        }
    }
}