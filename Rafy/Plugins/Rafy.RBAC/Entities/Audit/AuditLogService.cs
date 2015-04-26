/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110414
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110414
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Rafy.Threading;

namespace Rafy.RBAC.Audit
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
            /*********************** 代码块解释 *********************************
             * 暂时不支持直接使用多线程进行日志记录操作。
             * 否则会与主线程的数据库访问操作因共享连接而产生冲突。
            **********************************************************************/
            Log(logItem);

            //if (_provider == null) return;

            //if (logItem == null) throw new ArgumentNullException("logItem");

            //AsyncHelper.InvokeSafe(() =>
            //{
            //    _provider.Log(logItem);
            //});
        }
    }
}