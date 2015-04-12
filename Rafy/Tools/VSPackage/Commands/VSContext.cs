/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130422
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130422 16:21
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Rafy.VSPackage
{
    public abstract class VSContext
    {
        protected internal Package Package { get; internal set; }

        protected internal IServiceProvider ServiceProvider
        {
            get { return this.Package; }
        }

        protected internal DTE DTE
        {
            get { return this.GetService(typeof(DTE)) as DTE; }
        }

        protected object GetService(Type type)
        {
            return this.ServiceProvider.GetService(type);
        }

        /// <summary>
        /// 返回当前选择的项目列表。
        /// 注意，如果没有选中某个项目，则传入的列表是空列表。
        /// </summary>
        /// <returns></returns>
        protected List<Project> GetSelectedProjects()
        {
            var projects = new List<Project>();

            foreach (SelectedItem item in this.DTE.SelectedItems)
            {
                var project = item.Project;
                //if (project == null)
                //{
                //    var projectItem = item.ProjectItem;
                //    if (projectItem != null)
                //    {
                //        project = projectItem.ContainingProject;
                //    }
                //}

                if (project != null)
                {
                    if (!projects.Contains(project))
                    {
                        projects.Add(project);
                    }
                }
            }
            return projects;
        }
    }
}
