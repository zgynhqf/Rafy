using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;

namespace VSTemplates.Wizards
{
    public abstract class Wizard : IWizard
    {
        internal DTE _dte;

        public virtual void RunStarted(
            object automationObject,
            Dictionary<string, string> replacementsDictionary,
            WizardRunKind runKind, object[] customParams
            )
        {
            //MessageBox.Show("Debugging");

            _dte = automationObject as DTE;
        }

        public virtual bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }

        public virtual void ProjectFinishedGenerating(Project project) { }

        public virtual void ProjectItemFinishedGenerating(ProjectItem projectItem) { }

        public virtual void BeforeOpeningFile(ProjectItem projectItem) { }

        public virtual void RunFinished() { }
    }
}
