/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20151119
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20151119 12:07
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Domain
{
    /// <summary>
    /// 提交数据的拦截器。
    /// 
    /// 实现拦截器模式的原因在于：提交数据的扩展点需要满足以下几个条件：
    /// * 支持高频调用。（使用传统的事件会在触发事件时生成事件的参数对象，这会导致为组合中的每一个实体都生成一个事件参数对象，垃圾回收压力过大。）
    /// * 可以在被拦截的功能代码之前、之后做相应扩展。（传统事件的话，需要同时声明两个事件才行。）
    /// * 在扩展点中需要能够控制是否继续调用下一个被拦截的功能代码，。（传统事件需要使用 <see cref="System.ComponentModel.CancelEventArgs"/> 才可以。)
    /// * 线程安全。（DataSaver 本身是线程安全的）。
    /// </summary>
    public abstract class SubmitInterceptor
    {
        /// <summary>
        /// 拦截器在拦截器链表中的位置。
        /// </summary>
        internal int SubmitInterceptorIndex;

        /// <summary>
        /// 提交指定的实体。
        /// 子类在此方法中，使用 locator.InvokeNext(e, this); 来调用被拦截的功能。
        /// </summary>
        /// <param name="e">提交参数，其中封装了需要对实体进行的操作。</param>
        /// <param name="link">使用此定位器来调用被拦截的实际提交器。</param>
        internal protected abstract void Submit(SubmitArgs e, ISubmitInterceptorLink link);
    }

    /// <summary>
    /// 使用此定位器来调用被拦截的实际提交器。
    /// </summary>
    public interface ISubmitInterceptorLink
    {
        /// <summary>
        /// 调用指定拦截器所拦截的功能。
        /// </summary>
        /// <param name="current">传入当前的拦截器，框架会调用该拦截器之后的拦截器。</param>
        /// <param name="e">提交参数，其中封装了需要对实体进行的操作。</param>
        void InvokeNext(SubmitInterceptor current, SubmitArgs e);
    }

    internal class SubmitInterceptorList : ISubmitInterceptorLink
    {
        private List<SubmitInterceptor> _submitters = new List<SubmitInterceptor>();

        internal void Add(Type submitterInterceptor)
        {
            var instance = Activator.CreateInstance(submitterInterceptor) as SubmitInterceptor;
            this.Add(instance);
        }

        internal void Add(SubmitInterceptor submitterInterceptor)
        {
            if (submitterInterceptor == null) throw new ArgumentNullException("submitterInterceptor");

            _submitters.Add(submitterInterceptor);
            submitterInterceptor.SubmitInterceptorIndex = _submitters.Count - 1;
        }

        internal void Submit(SubmitArgs e)
        {
            //最后一个就是第一个。
            var first = _submitters[_submitters.Count - 1];
            first.Submit(e, this);
        }

        void ISubmitInterceptorLink.InvokeNext(SubmitInterceptor current, SubmitArgs e)
        {
            //不需要检查 Index。
            //因为作为最后一个 Submitter(DataProvider)，不能再调用 GetNext 方法。
            var next = _submitters[current.SubmitInterceptorIndex - 1];
            next.Submit(e, this);
        }

        //private void ResetIndeces()
        //{
        //    for (int i = 0, c = _submitters.Count; i < c; i++)
        //    {
        //        var submitter = _submitters[i];
        //        submitter.Index = i;
        //    }
        //}
    }
}