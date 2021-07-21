/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20210721
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20210721 01:02
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
    public class DomainLibraryNET5ProjectWizard : ConsoleAppProjectWizard
    {
        public override void ProjectFinishedGenerating(Project project)
        {
            base.ProjectFinishedGenerating(project);

            var projectDir = Path.GetDirectoryName(project.FileName);
            var entityFile = Path.Combine(projectDir, "Entities\\DomainLibraryEntity.cs");
            RenameFile(entityFile, _domainName + "Entity.cs");

            RenameFile(Path.Combine(projectDir, "DomainLibraryPlugin.cs"), _domainName + "Plugin.cs");

            //这两个文件是模板文件。需要移除。
            //VS2019 中，添加 NET5 类型的项目时，由于项目文件中并没有显式指明文件，而是直接使用文件系统。
            //如果这里直接使用这种项目文件来创建项目模板，这会导致创建出来的文件夹中没有所有的模板文件。
            //所以，这里不得已，又在项目文件模板中，把这些文件显式地指出来。这样，在最后，需要将这些模板文件，在 ProjectItems 中移除。
            project.ProjectItems.Item("DomainLibraryPlugin.cs").Remove();
            project.ProjectItems.Item("Entities").ProjectItems.Item("DomainLibraryEntity.cs").Remove();
        }

        private void RenameFile(string templateFile, string newFileName)
        {
            var newFile = Path.Combine(Path.GetDirectoryName(templateFile), newFileName);
            File.Copy(templateFile, newFile);
            File.Delete(templateFile);
        }
    }
}