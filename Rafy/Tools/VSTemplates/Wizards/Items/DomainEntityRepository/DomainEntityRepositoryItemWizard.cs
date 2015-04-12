/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130420
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130420 00:20
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using Rafy.VSPackage;
using RafySDK;
using VSTemplates.Wizards.Items.DomainEntity;

namespace VSTemplates.Wizards
{
    public class DomainEntityRepositoryItemWizard : Wizard
    {
        /// <summary>
        /// 是否被用户取消。
        /// </summary>
        private bool _canceled = false;
        private Dictionary<string, string> _replacementsDictionary;

        public override void RunStarted(
            object automationObject,
            Dictionary<string, string> replacementsDictionary,
            WizardRunKind runKind, object[] customParams)
        {
            _replacementsDictionary = replacementsDictionary;

            base.RunStarted(automationObject, _replacementsDictionary, runKind, customParams);

            var repoNamespace = _replacementsDictionary["$rootnamespace$"];
            var dnsItems= repoNamespace.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            var vm = new DomainEntityRepositoryWizardWindowViewModel();

            //如果文件的名称中没有以 Repository 结尾，则主动加上 Repository。
            var safeItemName = _replacementsDictionary["$safeitemname$"];
            if (safeItemName.EndsWith(Consts.RepositorySuffix))
            {
                vm.EntityTypeName = safeItemName.Substring(0, safeItemName.Length - Consts.RepositorySuffix.Length);
            }
            else
            {
                vm.EntityTypeName = safeItemName;
            }
            vm.BaseTypeName = dnsItems.Last() + Consts.EntityRepositorySuffix;
            vm.DTE = automationObject as DTE;

            //显示向导窗口
            var win = new DomainEntityRepositoryWizardWindow();
            win.DataContext = vm;
            var res = win.ShowDialog();
            if (res != true)
            {
                _canceled = true;
                return;
            }

            //输出
            var repositoryAutoCode = ItemCodeTemplate.GetRepositoryFileCoreAutoCode(vm.EntityTypeName);
            _replacementsDictionary.Add("$repositoryAutoCode$", repositoryAutoCode);
            _replacementsDictionary.Add("$domainNamespace$", vm.DomainNamespace);
            _replacementsDictionary.Add("$domainEntityName$", vm.EntityTypeName);
            _replacementsDictionary.Add("$baseRepositoryName$", vm.BaseTypeName);
        }

        #region 重写基类属性

        public override bool ShouldAddProjectItem(string filePath)
        {
            if (_canceled) return false;

            return base.ShouldAddProjectItem(filePath);
        }

        private ProjectItem _entityRepoFile;

        public override void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
            if (!projectItem.Name.Contains(".g.cs"))
            {
                _entityRepoFile = projectItem;
            }
            else
            {
                //把生成文件变为实体文件的子文件。
                if (_entityRepoFile != null)
                {
                    projectItem.Remove();
                    _entityRepoFile.ProjectItems.AddFromFile(projectItem.get_FileNames(1));
                }
            }

            base.ProjectItemFinishedGenerating(projectItem);
        }

        #endregion
    }
}