using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.MetaModel.Audit
{
    /// <summary>
    /// 审计功能提供程序
    /// </summary>
    public interface IAuditLogProvider
    {
        /// <summary>
        /// 记录指定的日志
        /// </summary>
        /// <param name="log"></param>
        void Log(AuditLogItem log);

        ///// <summary>
        ///// 获取所有的日志
        ///// </summary>
        ///// <returns></returns>
        //AuditLogItem[] GetAllLogs();
    }
}
