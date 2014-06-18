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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Rafy;

namespace Rafy.DomainModeling.Models
{
    /// <summary>
    /// 属性元素
    /// </summary>
    public class PropertyElement : DocumentViewModel
    {
        /// <summary>
        /// 构造器。
        /// </summary>
        /// <param name="name"></param>
        public PropertyElement(string name)
        {
            this.Name = name;
        }

        private string _Name;
        /// <summary>
        /// 属性名
        /// </summary>
        public string Name
        {
            get { return this._Name; }
            set
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentNullException("value");

                if (_Name != value)
                {
                    _Name = value;
                    this.OnPropertyChanged("Name");
                }
            }
        }

        private string _Label;
        /// <summary>
        /// 属性标题
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

        private string _PropertyType;
        /// <summary>
        /// 属性类型
        /// </summary>
        public string PropertyType
        {
            get { return this._PropertyType; }
            set
            {
                if (_PropertyType != value)
                {
                    _PropertyType = value;
                    this.OnPropertyChanged("PropertyType");
                }
            }
        }
    }
}