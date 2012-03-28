using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using OEA.Threading;

namespace OEA.MetaModel.Audit
{
    /// <summary>
    /// 审核功能
    /// </summary>
    public static class AuditLogService
    {
        private static IAuditLogProvider _provider;

        /// <summary>
        /// 初始化提供程序
        /// </summary>
        /// <param name="value"></param>
        public static void SetProvider(IAuditLogProvider value)
        {
            _provider = value;
        }

        /// <summary>
        /// 记录指定的日志
        /// </summary>
        /// <param name="logItem"></param>
        public static void Log(AuditLogItem logItem)
        {
            if (_provider == null) return;

            if (logItem == null) throw new ArgumentNullException("logItem");

            _provider.Log(logItem);
        }

        /// <summary>
        /// 异步记录指定的日志
        /// </summary>
        /// <param name="logItem"></param>
        public static void LogAsync(AuditLogItem logItem)
        {
            if (_provider == null) return;

            if (logItem == null) throw new ArgumentNullException("logItem");

            ThreadHelper.SafeInvoke(() =>
            {
                _provider.Log(logItem);
            });
        }
    }
}