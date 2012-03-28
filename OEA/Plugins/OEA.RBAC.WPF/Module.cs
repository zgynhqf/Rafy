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
using Common;
using Itenso.Windows.Input;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.Audit;
using OEA.MetaModel.View;
using OEA.WPF.Command;
using RBAC;
using OEA.RBAC;

namespace OEA.Module.WPF
{
    /// <summary>
    /// 当前工程所对应的模块类。
    /// 详细功能，见Initailize方法。
    /// </summary>
    internal class Module : WPFModuleBase
    {
        public override ReuseLevel ReuseLevel
        {
            get { return ReuseLevel._System; }
        }

        /// <summary>
        /// 把 ModuleListPad.xaml 加入到 Region 中。
        /// 
        /// 加入 ComboListControl.xaml 到Resource中
        /// </summary>
        protected override void InitializeCore(IClientApp app)
        {
            base.InitializeCore(app);

            app.AllPluginsMetaIntialized += (o, e) =>
            {
                var orgBlocks = new AggtBlocks
                {
                    MainBlock = new Block(typeof(Org)),
                    Layout = new LayoutMeta()
                    {
                        IsLayoutChildrenHorizonal = true,
                        ParentChildProportion = new ParentChildProportion(15, 85)
                    },
                    Children = 
                    {
                        new AggtBlocks
                        {
                            MainBlock = new ChildBlock("岗位", Org.OrgPositionListProperty),
                            Layout = new LayoutMeta() { IsLayoutChildrenHorizonal = true },
                            Children = 
                            {
                                new ChildBlock("岗位成员", OrgPosition.OrgPositionUserListProperty),
                                new ChildBlock("岗位功能权限", OrgPosition.OrgPositionOperationDenyListProperty)
                                {
                                    CustomViewType = typeof(OperationSelectionView).AssemblyQualifiedName
                                },
                            }
                        }
                    }
                };
                UIModel.AggtBlocks.DefineBlocks("部门模块布局", orgBlocks);
            };

            app.ModuleOperations += (s, e) =>
            {
                var m = CommonModel.Modules.FindModule("部门管理");
                m.AggtBlocksName = "部门模块布局";
            };

            this.LogSystem(app);
        }

        #region 记录日志

        private void LogSystem(IClientApp app)
        {
            this.LogModuleSelected();

            this.LogLogin(app);

            this.LogCommandException(app);
        }

        private void LogModuleSelected()
        {
            App.Current.ModuleCreated += (o, e) =>
            {
                var win = e.ResultWindow;

                //日志
                AuditLogService.LogAsync(new AuditLogItem()
                {
                    Title = "打开模块：" + win.Title,
                    ModuleName = win.Title,
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

        private void LogCommandException(IClientApp app)
        {
            if (ConfigurationHelper.GetAppSettingOrDefault("LogCommandException", true))
            {
                CommandRepository.CommandCreated += (o, e) =>
                {
                    var cmd = e.Instance;
                    var cmdInfo = cmd.CommandInfo;
                    cmd.ExecuteFailed += (oo, ee) =>
                    {
                        var view = ee.Parameter as ObjectView;
                        if (view == null) return;

                        LogCommandFailed(cmdInfo, view, ee.Exception);

                        var sqlex = ee.Exception.GetBaseException() as SqlException;
                        if (sqlex != null)
                        {
                            var sqlerr = SqlErrorInfo.GetSqlError(sqlex.Number);
                            if (sqlerr == null) return;

                            App.Current.MessageBox.Show(sqlerr.ErrorMessage);
                            ee.Cancel = true;
                        }
                    };
                };
            }
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="view"></param>
        private static void LogCommandSuccess(WPFCommand cmd, ObjectView view)
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
        private static void LogCommandFailed(WPFCommand cmd, ObjectView view, Exception ex)
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

        private static bool DisableLog(WPFCommand cmd, ObjectView view)
        {
            return view is QueryObjectView;
        }

        private static void LogAsync(string title, string coderContent, ObjectView view)
        {
            var evm = view.Meta;

            int? entityId = null;
            var entity = view.Current as IEntity;
            if (entity != null)
            {
                entityId = entity.Id;
            }

            AuditLogService.LogAsync(new AuditLogItem()
            {
                Title = title,
                FriendlyContent = string.Format(@"对象：{0}", evm.Label),
                PrivateContent = coderContent,
                ModuleName = App.Current.Workspace.ActiveWindow.Title,
                Type = AuditLogType.Command,
                EntityId = entityId
            });
        }

        #endregion
    }
}
