/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140625
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140625 15:54
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.ComponentModel
{
    /// <summary>
    /// 事件总线
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// 向总线发布一个指定的事件。
        /// </summary>
        /// <param name="eventModel"></param>
        void Publish(object eventModel);

        /// <summary>
        /// 向总线发布一个指定的事件。
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="eventModel"></param>
        void Publish<TEvent>(TEvent eventModel);

        /// <summary>
        /// 向总线订阅一个指定的事件。
        /// </summary>
        /// <typeparam name="TEvent">事件类型。</typeparam>
        /// <param name="owner">事件的监听者。</param>
        /// <param name="handler">监听函数。</param>
        void Subscribe<TEvent>(object owner, Action<TEvent> handler);

        /// <summary>
        /// 向总线取消一个指定的事件的订阅。
        /// </summary>
        /// <typeparam name="TEvent">事件类型。</typeparam>
        /// <param name="owner">事件的监听者。</param>
        void Unsubscribe<TEvent>(object owner);

        /// <summary>
        /// 获取指定事件的所有监听者。
        /// </summary>
        /// <typeparam name="TEvent">指定的事件类型。</typeparam>
        /// <returns></returns>
        IEventSubscribers GetSubscribers<TEvent>();

        ///// <summary>
        ///// 获取指定事件的所有监听者。
        ///// </summary>
        ///// <param name="eventType">指定的事件类型。</param>
        ///// <returns></returns>
        //IEventSubscribers GetSubscribers(Type eventType);
    }
}