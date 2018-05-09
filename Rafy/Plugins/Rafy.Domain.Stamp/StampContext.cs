using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rafy.Domain.Stamp
{
    /// <summary>
    /// Stamp 的控制器。
    /// </summary>
    public static class StampContext
    {
        internal static bool Disabled = false;

        internal static object DisabledLock = new object();

        internal static int? ThreadId;

        /// <summary>
        /// 禁用自动设置 Stamp 的功能。
        /// </summary>
        /// <returns></returns>
        public static IDisposable DisableAutoSetStamps()
        {
            return new StampContextDisabler();
        }

        class StampContextDisabler : IDisposable
        {
            public StampContextDisabler()
            {
                Monitor.Enter(DisabledLock);
                Disabled = true;
                ThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            public void Dispose()
            {
                ThreadId = null;
                Disabled = false;
                Monitor.Exit(DisabledLock);
            }
        }
    }
}
