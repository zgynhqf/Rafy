using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using System.Diagnostics;

namespace OEA.Threading
{
    public static class ThreadHelper
    {
        public static IAsyncMultiActions AsyncMultiActions
        {
            get
            {
                return OEA.Threading.AsyncMultiActions.Instance;
            }
        }

        public static IParallelActions CreateParallelActions()
        {
            return new ParallelActions();
        }

        /// <summary>
        /// 安全地对任务进行异步调用。
        /// 
        /// 原因：在异步线程中调用任务，如果出现异常，往往会使整个应用程序死机。
        /// </summary>
        /// <param name="action"></param>
        public static void SafeInvoke(Action action)
        {
            action = action.AsynPrincipleWrapper();

            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("异步线程在 SafeInvoke 时发生异常：" + ex.Message);
                }
            });
        }

        /// <summary>
        /// 对任务进行异步调用。
        /// </summary>
        /// <param name="action"></param>
        public static void AsyncInvoke(Action action)
        {
            action = action.AsynPrincipleWrapper();

            ThreadPool.QueueUserWorkItem(o => action());
        }

        /// <summary>
        /// 这里生成的wrapper会保证，在执行action前后，新开的线程和主线程都使用同一个Principel。
        /// 
        /// 解决问题：
        /// 由于ApplicationContext.User是基于线程的，
        /// 所以如果在同一次请求中，如果在服务端打开一个新的线程做一定的事情，
        /// 这个新开的线程可能会和打开者使用不同的Principle而造成代码异常。
        /// </summary>
        /// <param name="action">
        /// 可能会使用ApplicationContext.User，并需要在服务端另开线程来执行的操作。
        /// </param>
        /// <returns></returns>
        public static Action AsynPrincipleWrapper(this Action action)
        {
            if (ApplicationContext.ExecutionLocation == ApplicationContext.ExecutionLocations.Client)
            {
                return action;
            }

            var principelNeed = ApplicationContext.User;

            return () =>
            {
                var oldPrincipel = ApplicationContext.User;
                if (oldPrincipel != principelNeed)
                {
                    ApplicationContext.User = principelNeed;
                }

                try
                {
                    action();
                }
                finally
                {
                    if (oldPrincipel != principelNeed)
                    {
                        ApplicationContext.User = oldPrincipel;
                    }
                }
            };
        }
    }
}
