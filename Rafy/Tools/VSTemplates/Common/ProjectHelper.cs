/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130409 14:31
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130409 14:31
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace Rafy.VSPackage
{
    static class ProjectHelper
    {
        /// <summary>
        /// 用于查看项目的所有属性。
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        internal static string OutputProperties(Project project)
        {
            StringBuilder sb = new StringBuilder();
            foreach (EnvDTE.Property property in project.Properties)
            {
                sb.AppendFormat(property.Name);
                try
                {
                    sb.AppendFormat(": {0}", property.Value);
                }
                catch { }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// 获取项目的某个属性。
        /// </summary>
        /// <param name="project"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        internal static string GetProjectProperty(Project project, string property)
        {
            string value = project.Properties.Item(property).Value as string;
            return value;
        }

        internal static IEnumerable<ProjectItem> EnumerateCSharpFiles(ProjectItems list)
        {
            foreach (ProjectItem item in EnumerateItems(list))
            {
                if (item.Name.EndsWith(".cs"))
                {
                    yield return item;
                }
            }
        }

        internal static IEnumerable<ProjectItem> EnumerateItems(ProjectItems list)
        {
            if (list != null)
            {
                foreach (ProjectItem item in list)
                {
                    yield return item;

                    foreach (var child in EnumerateItems(item.ProjectItems))
                    {
                        yield return child;
                    }
                }
            }
        }

        internal static ProjectItem FindByName(this ProjectItems list, string name)
        {
            foreach (ProjectItem item in list)
            {
                if (item.Name == name)
                {
                    return item;
                }
            }

            return null;
        }
    }
}
