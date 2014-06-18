/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130413 19:38
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130413 19:38
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.DomainModeling.Models;

namespace Rafy.VSPackage.Modeling
{
    /// <summary>
    /// 块元素的属性。
    /// </summary>
    internal class BlockProperties
    {
        private BlockElement _element;

        internal BlockElement Element
        {
            get { return _element; }
        }

        public BlockProperties(BlockElement element)
        {
            _element = element;
        }

        [DisplayName("类型全名称")]
        public string FullName
        {
            get { return _element.FullName; }
            //set { _element.FullName = value; }
        }

        [DisplayName("领域名")]
        public string Label
        {
            get { return _element.Label; }
            //set { _element.Label = value; }
        }
    }
}