/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130403 19:50
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130403 19:50
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
    /// 枚举项元素
    /// </summary>
    public class EnumItemElement : DocumentViewModel
    {
        public EnumItemElement(string name)
        {
            this.Name = name;
        }

        private string _Name;
        /// <summary>
        /// 枚举项名称
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

        //private int _Value;
        ///// <summary>
        ///// 对应的整形数据
        ///// </summary>
        //public int Value
        //{
        //    get { return this._Value; }
        //    set
        //    {
        //        if (_Value != value)
        //        {
        //            _Value = value;
        //            this.OnPropertyChanged("Value");
        //        }
        //    }
        //}
    }
}