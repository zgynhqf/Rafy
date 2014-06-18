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
using Rafy.Domain;

namespace Rafy.RBAC.Audit
{
    public class ServerAuditLogProvider : IAuditLogProvider
    {
        #region IAuditLogProvider Members

        public void Log(AuditLogItem log)
        {
            var dbItem = new AuditItem();

            dbItem.Title = log.Title;
            dbItem.Content = log.FriendlyContent;
            dbItem.PrivateContent = log.PrivateContent;
            dbItem.User = log.User;
            dbItem.MachineName = log.MachineName;
            dbItem.ModuleName = log.ModuleName;
            dbItem.Type = log.Type;
            dbItem.LogTime = log.LogTime;
            dbItem.EntityId = log.EntityId;

            RF.Save(dbItem);
        }

        #endregion
    }
}
