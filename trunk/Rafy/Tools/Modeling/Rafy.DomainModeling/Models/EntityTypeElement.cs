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
    /// 实体类型元素
    /// </summary>
    public class EntityTypeElement : BlockElement
    {
        private NotifyChangedCollection<PropertyElement> _properties;

        public EntityTypeElement(string fullName)
            : base(fullName)
        {
            _properties = new NotifyChangedCollection<PropertyElement>();
            AddChildren(_properties);
        }

        /// <summary>
        /// 实体中的所有属性的列表。
        /// </summary>
        public NotifyChangedCollection<PropertyElement> Properties
        {
            get { return _properties; }
        }

        private bool _HideProperties;
        /// <summary>
        /// 是否需要隐藏属性列表
        /// </summary>
        public bool HideProperties
        {
            get { return this._HideProperties; }
            set
            {
                if (_HideProperties != value)
                {
                    _HideProperties = value;
                    this.OnPropertyChanged("HideProperties");
                }
            }
        }

        private bool _IsAggtRoot;
        /// <summary>
        /// 当前的实体类型是否为一个聚合根。
        /// </summary>
        public bool IsAggtRoot
        {
            get { return this._IsAggtRoot; }
            set
            {
                if (_IsAggtRoot != value)
                {
                    _IsAggtRoot = value;
                    this.OnPropertyChanged("IsAggtRoot");
                }
            }
        }
    }
}