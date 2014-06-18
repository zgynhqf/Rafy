/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130422
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130422 20:58
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace Rafy.VSPackage.Commands
{
    abstract class SelectedProjectsCommand : Command
    {
        protected override void OnQueryStatus()
        {
            this.MenuCommand.Enabled = GetSelectedProjects().Count > 0;
        }

        protected override void ExecuteCore()
        {
            var projects = GetSelectedProjects();
            if (projects.Count > 0)
            {
                this.ExecuteOnProject(projects);
            }
        }

        private List<Project> GetSelectedProjects()
        {
            var projects = new List<Project>();

            foreach (SelectedItem item in base.DTE.SelectedItems)
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

        protected abstract void ExecuteOnProject(IList<Project> projects);
    }
}