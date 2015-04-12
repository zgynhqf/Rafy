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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using VSTemplates.Wizards.Items.DomainEntity;

namespace VSTemplates.Wizards
{
    public class DomainEntityItemWizard : Wizard
    {
        /// <summary>
        /// 是否被用户取消。
        /// </summary>
        private bool _canceled = false;
        private bool _isChild;
        private string _domainEntityName;
        /// <summary>
        /// 组合父实体名
        /// </summary>
        private string _parentEntityName;
        /// <summary>
        /// 继承父实体名。
        /// </summary>
        private string _domainBaseEntityName;
        private Dictionary<string, string> _replacementsDictionary;

        public override void RunStarted(
            object automationObject,
            Dictionary<string, string> replacementsDictionary,
            WizardRunKind runKind, object[] customParams)
        {
            _replacementsDictionary = replacementsDictionary;

            base.RunStarted(automationObject, _replacementsDictionary, runKind, customParams);

            var domainNamespace = _replacementsDictionary["$rootnamespace$"];
            if (domainNamespace.EndsWith(".Entities"))
            {
                domainNamespace = domainNamespace.Substring(0, domainNamespace.Length - ".Entities".Length);
            }
            var dnsItems= domainNamespace.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            //显示向导窗口
            var win = new DomainEntityWizardWindow();
            win.txtClassName.Text = _replacementsDictionary["$safeitemname$"];
            win.txtBaseEntityName.Text = dnsItems[dnsItems.Length - 1] + "Entity";
            win.txtDomainName.Text = "实体的领域名称";
            win.txtEntityKeyType.Text = "int";
            var res = win.ShowDialog();
            if (res != true)
            {
                _canceled = true;
                return;
            }

            //获取用户输入
            _domainEntityName = win.txtClassName.Text;
            _parentEntityName = win.txtParentEntityName.Text;
            _domainBaseEntityName = win.txtBaseEntityName.Text;
            string entityKeyType = win.txtEntityKeyType.Text;
            var domainEntityLabel = win.txtDomainName.Text;
            var hasRepository = win.cbGenerateRepository.IsChecked.Value;
            var isConfigView = false;

            //开始动态输出模板内容
            _isChild = !string.IsNullOrWhiteSpace(_parentEntityName);
            var entityAttributes = _isChild ? "[ChildEntity, Serializable]" : "[RootEntity, Serializable]";

            RenderParentRefProperty(entityKeyType);

            RenderRepository(hasRepository, domainEntityLabel);

            RenderViewConfiguration(isConfigView, domainEntityLabel);

            _replacementsDictionary.Add("$domainNamespace$", domainNamespace);
            _replacementsDictionary.Add("$domainEntityLabel$", domainEntityLabel);
            _replacementsDictionary.Add("$entityAttributes$", entityAttributes);
            _replacementsDictionary.Add("$domainEntityName$", _domainEntityName);
            _replacementsDictionary.Add("$domainBaseEntityName$", _domainBaseEntityName);
            _replacementsDictionary.Add("$tableConfig$", @"//配置实体的所有属性都映射到数据表中。
            Meta.MapTable().MapAllProperties();");
            //以下替换字符，由自动生成的命令使用，这里不使用。
            _replacementsDictionary.Add("$normalProperties$", string.Empty);
            _replacementsDictionary.Add("$concreteNew$", string.Empty);
            _replacementsDictionary.Add("$columnConfig$", string.Empty);
        }

        private void RenderRepository(bool hasRepository, string domainEntityLabel)
        {
            var repositoryCode = string.Empty;
            var repositoryAutoCode = string.Empty;
            if (hasRepository)
            {
                repositoryCode = ItemCodeTemplate.GetRepositoryCodeInEntityFile(_domainEntityName, domainEntityLabel, _domainBaseEntityName);
                repositoryAutoCode = ItemCodeTemplate.GetRepositoryFileCoreAutoCode(_domainEntityName);
            }
            _replacementsDictionary.Add("$repositoryCode$", repositoryCode);
            _replacementsDictionary.Add("$repositoryAutoCode$", repositoryAutoCode);
        }

        private void RenderViewConfiguration(bool isConfigView, string domainEntityLabel)
        {
            var viewConfiguration = string.Empty;
            if (isConfigView)
            {
                viewConfiguration = ItemCodeTemplate.GetViewConfigurationCode(domainEntityLabel, _domainEntityName);
            }
            _replacementsDictionary.Add("$viewConfiguration$", viewConfiguration);
        }

        private void RenderParentRefProperty(string entityKeyType)
        {
            var parentRefPropertyCode = string.Empty;
            if (_isChild)
            {
                parentRefPropertyCode = ItemCodeTemplate.GetRefPropertyCode(
                    _domainEntityName, _parentEntityName, _parentEntityName, entityKeyType, true, true
                    );
            }
            _replacementsDictionary.Add("$refProperties$", parentRefPropertyCode);
        }

        #region 重写基类属性

        public override bool ShouldAddProjectItem(string filePath)
        {
            if (_canceled) return false;

            return base.ShouldAddProjectItem(filePath);
        }

        private ProjectItem _entityFile;

        public override void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
            if (!projectItem.Name.Contains(".g.cs"))
            {
                _entityFile = projectItem;

                //给父实体添加列表属性。
                if (_isChild)
                {
                    WriteChildrenProperty(projectItem);
                }
            }
            else
            {
                //把生成文件变为实体文件的子文件。
                if (_entityFile != null)
                {
                    projectItem.Remove();
                    _entityFile.ProjectItems.AddFromFile(projectItem.get_FileNames(1));
                }
            }

            base.ProjectItemFinishedGenerating(projectItem);
        }

        #endregion

        #region 给父实体添加列表属性。

        private void WriteChildrenProperty(ProjectItem projectItem)
        {
            var parentItem = FindParentClassItem(projectItem);
            if (parentItem != null)
            {
                var doc = OpenDocument(parentItem);
                WriteChildrenProperty(doc);
            }
        }

        private void WriteChildrenProperty(Document doc)
        {
            var textSelection = doc.Selection as TextSelection;
            if (textSelection != null)
            {
                textSelection.StartOfDocument(false);
                var found = textSelection.FindText("#region 子属性") || textSelection.FindText("#region 组合子属性");
                if (found)
                {
                    var childrenPropertyCode = @"#region 组合子属性" + ItemCodeTemplate.GetChildrenPropertyCode(_parentEntityName, _domainEntityName);
                    textSelection.Insert(childrenPropertyCode);
                }
            }
        }

        private Document OpenDocument(ProjectItem projectItem)
        {
            var doc = projectItem.Document;
            if (doc != null) { return doc; }

            var win = projectItem.Open();
            win.Visible = true;
            return win.Document;
        }

        private ProjectItem FindParentClassItem(ProjectItem projectItem)
        {
            var parentItemName = _parentEntityName + ".cs";

            var items = projectItem.ContainingProject.ProjectItems;
            var allItems = EnumerateCSharpFiles(items);
            foreach (var item in allItems)
            {
                if (item.Name == parentItemName)
                {
                    return item;
                }
            }

            return null;
        }

        private static IEnumerable<ProjectItem> EnumerateCSharpFiles(ProjectItems list)
        {
            foreach (ProjectItem item in EnumerateItems(list))
            {
                if (item.Name.EndsWith(".cs"))
                {
                    yield return item;
                }
            }
        }

        private static IEnumerable<ProjectItem> EnumerateItems(ProjectItems list)
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

        #endregion
    }
}