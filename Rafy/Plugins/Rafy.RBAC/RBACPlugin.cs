/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120311
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120311
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;
using Rafy.ComponentModel;
using Rafy.Domain;
using Rafy.Domain.Caching;
using Rafy.Domain.ORM.DbMigration;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.RBAC.Audit;
using Rafy.RBAC.Security;
using Rafy.Web;

namespace Rafy.RBAC
{
    /// <summary>
    /// 比较简单的通用权限系统。
    /// </summary>
    internal class RBACPlugin : DomainPlugin
    {
        protected override int SetupLevel
        {
            get { return PluginSetupLevel.System; }
        }

        public override void Initialize(IApp app)
        {
            //设置权限提供程序为本模块中实体类
            PermissionMgr.Provider = new RafyPermissionProvider();

            //依赖注入
            if (RafyEnvironment.IsOnServer())
            {
                AuditLogService.SetProvider(new ServerAuditLogProvider());
            }
            else
            {
                AuditLogService.SetProvider(new ClientAuditLogProvider());
            }

            #region 记录登录信息。

            if (RafyEnvironment.IsOnClient())
            {
                (app as IClientApp).LoginSuccessed += (o, e) =>
                {
                    var identity = RafyEnvironment.Identity as RafyIdentity;
                    if (identity != null)
                    {
                        UserLoginLogService.NotifyLogin(identity.User);
                    }
                };

                app.Exit += (o, e) =>
                {
                    UserLoginLogService.NotifyLogout();
                };
            }

            #endregion
        }
    }
}