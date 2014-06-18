/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130414 20:35
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130414 20:35
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.DomainModeling
{
    /// <summary>
    /// 连接的唯一标识。
    /// </summary>
    public interface IConnectionKey
    {
        /// <summary>
        /// 起始块
        /// </summary>
        string From { get; }

        /// <summary>
        /// 终止块
        /// </summary>
        string To { get; }

        /// <summary>
        /// 连接名称
        /// </summary>
        string Label { get; }
    }

    /// <summary>
    /// 一个默认实现 IConnectionKey 的类型。
    /// </summary>
    public class ConnectionKey : IConnectionKey
    {
        public ConnectionKey(string from, string to, string label)
        {
            this.From = from;
            this.To = to;
            this.Label = label;
        }

        /// <summary>
        /// 起始块
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// 终止块
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// 连接名称
        /// </summary>
        public string Label { get; set; }
    }
}
