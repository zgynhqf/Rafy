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

using Rafy.Domain;
namespace Rafy.RBAC.Old.Audit
{
    /// <summary>
    /// 所有“非服务端”节点，都使用这个类作为Provider
    /// </summary>
    public class ClientAuditLogProvider : IAuditLogProvider
    {
        private ServerAuditLogProvider _serverProvider = new ServerAuditLogProvider();

        #region IAuditLogProvider Members

        /// <summary>
        /// 调用通信机制，调用服务端进行记录。
        /// </summary>
        /// <param name="log"></param>
        public void Log(AuditLogItem log)
        {
            //如果是单机版时，是否在服务端这个位置判断会变化，所以再进行一次判断，以防止死循环。
            if (RafyEnvironment.IsOnClient())
            {
                var svc = ServiceFactory.Create<AuditServerService>();
                svc.LogItem = log;
                svc.Invoke();
            }
            else
            {
                this._serverProvider.Log(log);
            }
        }

        #endregion
    }
}
