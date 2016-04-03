/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：2011
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2011
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using System.Diagnostics;
using Rafy;

namespace Rafy.Threading
{
    /// <summary>
    /// 异步线程的帮助方法。
    /// </summary>
    public static class AsyncHelper
    {
        /// <summary>
        /// 一个可监听 “任意开始，最终结束” 的任务管理器。
        /// </summary>
        public static IObservableActions CreateObservableActions()
        {
            return new AsyncMultiActions();
        }

        /// <summary>
        /// 一个多任务同时进行的任务管理器。
        /// </summary>
        /// <returns></returns>
        public static IParallelActions CreateParallelActions()
        {
            return new ParallelActions();
        }

        /// <summary>
        /// 安全地对任务进行异步调用。
        /// 
        /// 原因：在异步线程中调用任务，如果任务执行过程中出现异常，往往会使整个应用程序死机。
        /// </summary>
        /// <param name="action"></param>
        /// <param name="errorHandler"></param>
        public static void InvokeSafe(Action action, Action<Exception> errorHandler = null)
        {
            action = action.WrapByCurrentPrinciple();

            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    if (errorHandler != null)
                    {
                        try
                        {
                            errorHandler(ex);
                        }
                        catch
                        {
                            //错误处理再发生异常时，将会被忽略。
                        }
                    }
                    else
                    {
                        Logger.LogError("异步线程在 SafeInvoke 时发生异常", ex);
                    }
                }
            });
        }

        /// <summary>
        /// 对任务进行异步调用。
        /// </summary>
        /// <param name="action"></param>
        public static void Invoke(Action action)
        {
            action = action.WrapByCurrentPrinciple();

            ThreadPool.QueueUserWorkItem(o => action());
        }

        /// <summary>
        /// 这里生成的wrapper会保证，在执行action前后，新开的线程和主线程都使用同一个Principel。
        /// 
        /// 解决问题：
        /// 由于RafyEnvironment.User是基于线程的，
        /// 所以如果在同一次请求中，如果在服务端打开一个新的线程做一定的事情，
        /// 这个新开的线程可能会和打开者使用不同的Principle而造成代码异常。
        /// </summary>
        /// <param name="action">
        /// 可能会使用RafyEnvironment.User，并需要在服务端另开线程来执行的操作。
        /// </param>
        /// <returns></returns>
        public static Action WrapByCurrentPrinciple(this Action action)
        {
            if (RafyEnvironment.Location.IsWPFUI)
            {
                return action;
            }

            //当前线程的身份
            var principelNeed = RafyEnvironment.Principal;

            //以下代码执行在另一线程中。
            return () =>
            {
                //另一线程的身份，不是需要的身份，则设置身份。
                var oldPrincipel = RafyEnvironment.Principal;
                if (oldPrincipel != principelNeed)
                {
                    RafyEnvironment.Principal = principelNeed;
                }

                try
                {
                    action();
                }
                finally
                {
                    if (oldPrincipel != principelNeed)
                    {
                        RafyEnvironment.Principal = oldPrincipel;
                    }
                }
            };
        }
    }
}
