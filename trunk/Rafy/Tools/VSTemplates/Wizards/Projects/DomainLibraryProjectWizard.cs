/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130419
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130419 12:36
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;

namespace VSTemplates.Wizards
{
    public class DomainLibraryProjectWizard : ConsoleAppProjectWizard
    {
        //public override bool ShouldAddProjectItem(string filePath)
        //{
        //    //if (filePath == "DomainLibraryEntity.cs") return false;

        //    return true;
        //}

        public override void ProjectFinishedGenerating(Project project)
        {
            base.ProjectFinishedGenerating(project);

            //其实，后来发现，可以在 vstemplate 文件使用 $fileinputname$ 就可以了。（但是经测试，还是无效）
            //重命名两个文件。
            var entities = project.ProjectItems.Item("Entities");
            var baseEntityItem = entities.ProjectItems.Item("DomainLibraryEntity.cs");
            RenameItem(baseEntityItem, _domainName + "Entity.cs");

            var pluginItem = project.ProjectItems.Item("DomainLibraryPlugin.cs");
            RenameItem(pluginItem, _domainName + "Plugin.cs");
        }

        private void RenameItem(ProjectItem item, string newFileName)
        {
            var templateFile = item.get_FileNames(1);

            var newFile = Path.Combine(Path.GetDirectoryName(templateFile), newFileName);
            File.Copy(templateFile, newFile);
            item.Collection.AddFromFile(newFile);

            item.Remove();
            File.Delete(templateFile);
        }
    }
}