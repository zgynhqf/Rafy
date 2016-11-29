/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130821
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130821 13:42
 * 
*******************************************************/

using System;
using System.Data.SqlClient;
using Rafy;
using Rafy.ComponentModel;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.RBAC.Old.Audit;
using Rafy.Web;

namespace Rafy.RBAC.Old.WPF
{
    /// <summary>
    /// 当前工程所对应的模块类。
    /// </summary>
    internal class RBACWebPlugin : UIPlugin
    {
        public override void Initialize(IApp app)
        {
            app.ModuleOperations += (o, e) =>
            {
                var module = CommonModel.Modules.AddRoot(new WebModuleMeta
                {
                    Label = "权限管理",
                    Children =
                    {
                        new WebModuleMeta{ Label = "人员管理", EntityType = typeof(User)},
                        new WebModuleMeta{ Label = "岗位管理", EntityType = typeof(Position)},
                        new WebModuleMeta{ Label = "部门权限管理", EntityType = typeof(Org), BlocksTemplate=typeof(OrgModuleTemplate)},
                        new WebModuleMeta{ Label = "日志管理", EntityType = typeof(AuditItem)},
                        //new WebModuleMeta{ Label = "部门管理(iframe)", Url = "Module?module=部门权限管理" }
                        //new ModuleMeta{ Label = "部门管理(iframe)", CustomUI = "EntityModule?isAggt=1&type=" + ClientEntities.GetClientName(typeof(Org)) }
                    }
                });
            };
        }
    }
}