/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130423
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130423 12:08
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EnvDTE;
using Rafy.VSPackage.Modeling.CodeSync;
using VSTemplates.Wizards;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;

namespace Rafy.VSPackage.Commands.RefreshCodeSnippets
{
    /// <summary>
    /// 注册所有的 CodeSnippets
    /// </summary>
    class InstallCodeSnippetsCommand : Command
    {
        public InstallCodeSnippetsCommand()
        {
            this.CommandID = new CommandID(GuidList.guidVSPackageCmdSet, PkgCmdIDList.cmdidRefreshCodeSnippetsCommand);
        }

        protected override void ExecuteCore()
        {
            var res = MessageBox.Show("点击‘是’安装代码段，点击‘否’删除代码段。", "安装/卸载", MessageBoxButton.YesNoCancel);

            if (res != MessageBoxResult.Cancel)
            {
                string[] files = GetFiles();
                var dir = GetSnippetsDir();

                try
                {
                    if (res == MessageBoxResult.Yes)
                    {
                        foreach (var name in files)
                        {
                            var resource = string.Format("Rafy.VSPackage._CodeSnippets.{0}.snippet", name);
                            var content = Helper.GetResourceContent(typeof(RafySDKPackage).Assembly, resource);

                            var path = Path.Combine(dir, name + ".snippet");
                            File.WriteAllText(path, content);
                        }
                    }
                    else if (res == MessageBoxResult.No)
                    {
                        foreach (var name in files)
                        {
                            var path = Path.Combine(dir, name + ".snippet");
                            File.Delete(path);
                        }
                    }

                    //MessageBox.Show("操作完成，需要重启后才能生效。", "提示");
                    var res2 = MessageBox.Show("操作完成，可能需要重启 VS 后才能生效，是否打开代码段文件夹检查？", "提示", MessageBoxButton.YesNo);
                    if (res2 == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(dir);
                    }
                }
                //catch (IOException ex)
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("没有权限访问代码段文件夹，请以管理员启动 Visual Studio。");
                }
            }
        }

        private static string[] GetFiles()
        {
            string[] files = new string[]{
                "Hxy_Extension",
                //"Rafy_Command",
                "Rafy_Criteria",
                "Rafy_DataProvider",
                //"Rafy_Entity_Criteria",
                "Rafy_Property",
                "Rafy_Property_CoerceGetValue",
                "Rafy_Property_PropertyChanged",
                "Rafy_Property_PropertyChanging",
                "Rafy_PropertyExtension",
                "Rafy_PropertyExtensionList",
                "Rafy_PropertyExtensionReadOnly",
                "Rafy_PropertyExtensionRedundancy",
                "Rafy_PropertyExtensionReference",
                "Rafy_PropertyExtensionReferenceNullable",
                "Rafy_PropertyList",
                "Rafy_PropertyList_Full",
                "Rafy_PropertyLOB",
                "Rafy_PropertyReadOnly",
                "Rafy_PropertyRedundancy",
                "Rafy_PropertyReference",
                "Rafy_PropertyReferenceNullable",
                "Rafy_Query",
                "Rafy_Query_Common",
                "Rafy_Query_TableQueryContent",
            };
            return files;
        }

        private string GetSnippetsDir()
        {
            string path = Helper.GetVsShellDir(ServiceProvider);
            var res = Path.Combine(path, @"VC#\Snippets\2052\Visual C#");//中文版本
            if (!Directory.Exists(res))
            {
                res = Path.Combine(path, @"VC#\Snippets\1033\Visual C#");
            }
            return res;
        }
    }
}