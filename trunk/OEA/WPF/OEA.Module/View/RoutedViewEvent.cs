/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110328
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100328
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA
{
    /// <summary>
    /// ObjectView 路由事件的标记
    /// </summary>
    public class RoutedViewEvent
    {
        public Type OwnerType { get; private set; }

        public RoutedEventType Type { get; private set; }

        public static RoutedViewEvent Register(Type ownerType, RoutedEventType eventType)
        {
            return new RoutedViewEvent()
            {
                OwnerType = ownerType,
                Type = eventType
            };
        }
    }

    /// <summary>
    /// 路由类型
    /// </summary>
    public enum RoutedEventType
    {
        /// <summary>
        /// 向父路由
        /// </summary>
        ToParent,

        /// <summary>
        /// 向子路由
        /// </summary>
        ToChildren
    }

    /// <summary>
    /// ObjectView 路由事件的参数
    /// </summary>
    public class RoutedViewEventArgs : EventArgs
    {
        internal RoutedViewEventArgs() { }

        /// <summary>
        /// 事件源头的 ObjectView
        /// </summary>
        public ObjectView SourceView { get; internal set; }

        /// <summary>
        /// 发生的事件标记
        /// </summary>
        public RoutedViewEvent Event { get; internal set; }

        /// <summary>
        /// 事件参数
        /// </summary>
        public EventArgs Args { get; internal set; }
    }
}