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
    /// 枚举类型元素
    /// </summary>
    public class EnumElement : BlockElement
    {
        private NotifyChangedCollection<EnumItemElement> _items;

        public EnumElement(string fullName)
            : base(fullName)
        {
            _items = new NotifyChangedCollection<EnumItemElement>();
            AddChildren(_items);
        }

        /// <summary>
        /// 所有枚举项
        /// </summary>
        public NotifyChangedCollection<EnumItemElement> Items
        {
            get { return _items; }
        }
    }
}