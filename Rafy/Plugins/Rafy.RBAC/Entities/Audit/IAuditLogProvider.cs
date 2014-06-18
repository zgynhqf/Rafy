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

namespace Rafy.RBAC.Audit
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
