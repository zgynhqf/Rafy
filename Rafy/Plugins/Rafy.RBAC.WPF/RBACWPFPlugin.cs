/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100326
 * 说明：当前工程所对应的模块类
 * 运行环境：.NET 3.5 SP1
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100326
 * 
*******************************************************/

using System;
using System.Data.SqlClient;
using Rafy;
using Rafy.ComponentModel;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.RBAC.Audit;
using Rafy.WPF;
using Rafy.WPF.Command;

namespace Rafy.RBAC.WPF
{
    /// <summary>
    /// 当前工程所对应的模块类。
    /// </summary>
    internal class RBACWPFPlugin : UIPlugin
    {
        public override void Initialize(IApp app)
        {
            app.ModuleOperations += (o, e) =>
            {
                var module = CommonModel.Modules.AddRoot(new WPFModuleMeta
                {
                    Label = "权限管理",
                    Children =
                    {
                        new WPFModuleMeta{ Label = "人员管理", EntityType = typeof(User)},
                        new WPFModuleMeta{ Label = "岗位管理", EntityType = typeof(Position)},
                        new WPFModuleMeta{ Label = "部门权限管理", EntityType = typeof(Org), BlocksTemplate=typeof(OrgModule)},
                        new WPFModuleMeta{ Label = "日志管理", EntityType = typeof(AuditItem)}
                    }
                });
            };

            this.LogSystem(app as IClientApp);
        }

        #region 记录日志

        private void LogSystem(IClientApp app)
        {
            var logSystem = ConfigurationHelper.GetAppSettingOrDefault("Rafy_RBAC_LogSystem", true);
            if (logSystem)
            {
                this.LogModuleSelected();

                this.LogLogin(app);

                this.LogCommand(app);
            }
        }

        private void LogModuleSelected()
        {
            App.Current.ModuleCreated += (o, e) =>
            {
                //日志
                var title = e.Window.Title;
                AuditLogService.LogAsync(new AuditLogItem()
                {
                    Title = "打开模块：" + title,
                    ModuleName = title,
                    Type = AuditLogType.OpenModule
                });
            };
        }

        private void LogLogin(IClientApp app)
        {
            app.LoginSuccessed += (o, e) =>
            {
                AuditLogService.LogAsync(new AuditLogItem()
                {
                    Title = "登录成功",
                    Type = AuditLogType.Login
                });
            };

            //app.LoginFailed += (o, e) =>
            //{
            //    AuditLogService.LogAsync(new AuditLogItem()
            //    {
            //        Title = "登录失败",
            //        Type = AuditLogType.Login
            //    });
            //};
        }

        #endregion

        #region 记录命令日志

        private void LogCommand(IClientApp app)
        {
            if (ConfigurationHelper.GetAppSettingOrDefault("Rafy_RBAC_LogCommand", true))
            {
                CommandRepository.CommandCreated += (o, e) =>
                {
                    var cmd = e.Instance;

                    cmd.Executed += (sender, ee) =>
                    {
                        var view = ee.Parameter as LogicalView;
                        if (view == null) return;

                        LogCommandSuccess((sender as ClientCommand).Meta, view);
                    };

                    cmd.ExecuteFailed += (sender, ee) =>
                    {
                        var view = ee.Parameter as LogicalView;
                        if (view == null) return;

                        LogCommandFailed((sender as ClientCommand).Meta, view, ee.Exception);
                    };
                };
            }
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="view"></param>
        private static void LogCommandSuccess(WPFCommand cmd, LogicalView view)
        {
            if (DisableLog(cmd, view)) return;

            string title = "执行命令完成：" + cmd.Label;
            string coderContent = string.Format(
@"类型名：{0}
命令名称：{1}", view.EntityType.Name, cmd.Name);

            LogAsync(title, coderContent, view);
        }

        /// <summary>
        /// 记录执行错误的日志
        /// </summary>
        /// <param name="view"></param>
        /// <param name="ex"></param>
        private static void LogCommandFailed(WPFCommand cmd, LogicalView view, Exception ex)
        {
            if (DisableLog(cmd, view)) return;

            string title = "执行命令失败：" + cmd.Label;
            string coderContent = string.Format(
@"类型名：{0}
命令类型：{1}
发生异常：{2}
堆栈:{3}", view.EntityType.Name, cmd.Name, ex.Message, ex.StackTrace);

            LogAsync(title, coderContent, view);
        }

        private static bool DisableLog(WPFCommand cmd, LogicalView view)
        {
            return view is QueryLogicalView;
        }

        private static void LogAsync(string title, string coderContent, LogicalView view)
        {
            int? entityId = null;
            var entity = view.Current as IntEntity;
            if (entity != null)
            {
                entityId = entity.Id;
            }

            AuditLogService.LogAsync(new AuditLogItem()
            {
                Title = title,
                FriendlyContent = string.Format(@"对象：{0}", view.Meta.Label),
                PrivateContent = coderContent,
                ModuleName = App.Current.Workspace.ActiveWindow.Title,
                Type = AuditLogType.Command,
                EntityId = entityId
            });
        }

        #endregion
    }
}
