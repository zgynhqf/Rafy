using SimpleCsla;

namespace OEA.MetaModel.Audit
{
    /// <summary>
    /// 所有“非服务端”节点，都使用这个类作为Provider
    /// </summary>
    public class ClientAuditLogProvider : IAuditLogProvider
    {
        #region IAuditLogProvider Members

        /// <summary>
        /// 调用通信机制，调用服务端进行记录。
        /// </summary>
        /// <param name="log"></param>
        public void Log(AuditLogItem log)
        {
            //如果不是单机版
            if (OEAEnvironment.Location != OEALocation.LocalVersion)
            {
                new AuditServerService(log).Invoke();
            }
        }

        #endregion
    }
}
