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
using Rafy.DomainModeling;
using Rafy.DomainModeling.Models;

namespace Rafy.VSPackage.Modeling
{
    /// <summary>
    /// 连接线的属性。
    /// </summary>
    internal class ConnectionProperties
    {
        private ConnectionElement _element;

        internal ConnectionElement Element
        {
            get { return _element; }
        }

        public ConnectionProperties(ConnectionElement element)
        {
            _element = element;
        }

        [DisplayName("关系名")]
        public string Label
        {
            get { return _element.Label; }
        }

        [DisplayName("隐藏")]
        public bool Hidden
        {
            get { return _element.Hidden; }
            set { _element.Hidden = value; }
        }

        //[DisplayName("关系类型")]
        //public ConnectionType ConnectionType
        //{
        //    get { return _element.ConnectionType; }
        //    set { _element.ConnectionType = value; }
        //}
    }
}
