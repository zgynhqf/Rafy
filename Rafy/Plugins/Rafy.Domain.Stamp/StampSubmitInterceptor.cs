/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20151207
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20151207 15:16
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rafy.Domain.Stamp
{
    /// <summary>
    /// 拦截数据层的提交操作。在添加、更新实体时，设置实体的跟踪戳。
    /// </summary>
    public class StampSubmitInterceptor : ISubmitInterceptor
    {
        int ISubmitInterceptor.SubmitInterceptorIndex { get; set; }

        /// <summary>
        /// 提交指定的实体，并在添加、更新实体时，设置实体的跟踪戳。
        /// </summary>
        /// <param name="e">The e.</param>
        /// <param name="link">The link.</param>
        void ISubmitInterceptor.Submit(SubmitArgs e, ISubmitInterceptorLink link)
        {
            bool disabled = false;

            if (StampContext.Disabled)
            {
                if (StampContext.ThreadId == Thread.CurrentThread.ManagedThreadId)
                {
                    //如果是 Disable 的线程，则忽略
                    disabled = true;
                }
                else
                {
                    //如果不是 Disable 的线程，则需要等待 Disable 的线程结束后，才能继续执行后续的操作。
                    lock (StampContext.DisabledLock) { }
                }
            }

            if (!disabled)
            {
                this.ResetStamp(e);
            }

            link.InvokeNext(this, e);
        }

        private void ResetStamp(SubmitArgs e)
        {
            switch (e.Action)
            {
                case SubmitAction.ChildrenOnly:
                case SubmitAction.Update:
                    var entity = e.Entity;
                    entity.SetUpdatedTime(DateTime.Now);
                    var user = RafyEnvironment.Identity;
                    if (user.IsAuthenticated)
                    {
                        entity.SetUpdatedUser(user.Name);
                    }
                    break;
                case SubmitAction.Insert:
                    var entity2 = e.Entity;
                    var now = DateTime.Now;
                    entity2.SetUpdatedTime(now);
                    entity2.SetCreatedTime(now);
                    var user2 = RafyEnvironment.Identity;
                    if (user2.IsAuthenticated)
                    {
                        entity2.SetUpdatedUser(user2.Name);
                        entity2.SetCreatedUser(user2.Name);
                    }
                    break;
                default:
                    //do nothing;
                    break;
            }
        }
    }
}
