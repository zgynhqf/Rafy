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
    public class ConsoleAppProjectWizard : Wizard
    {
        protected string _domainNamespace;
        protected string _domainName;

        public override void RunStarted(
            object automationObject,
            Dictionary<string, string> replacementsDictionary,
            WizardRunKind runKind, object[] customParams
            )
        {
            base.RunStarted(automationObject, replacementsDictionary, runKind, customParams);

            var win = new DomainLibraryProjectWindow();
            win.txtDomainNameSpace.Text = replacementsDictionary["$safeprojectname$"];
            win.ShowDialog();

            var items = win.txtDomainNameSpace.Text.Split(new char[] { ' ', '.' }, StringSplitOptions.RemoveEmptyEntries);
            _domainName = items.Last();
            _domainNamespace = win.txtDomainNameSpace.Text.Replace(' ', '_');

            replacementsDictionary.Add("$domainNamespace$", _domainNamespace);
            replacementsDictionary.Add("$domainName$", _domainName);
        }
    }
}