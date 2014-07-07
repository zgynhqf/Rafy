using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using Rafy.VSPackage;

namespace RafySDK.Templates.Wizards.Items.DomainEntityRepository
{
    public class SelectEntityWindowViewModel : ViewModel
    {
        public SelectEntityWindowViewModel(DTE dte)
        {
            var list = new List<SelectEntityWindowViewModelProject>();

            ////添加所有程序集。
            //foreach (Project item in dte.Solution.Projects)
            //{
            //    list.Add(new SelectEntityWindowViewModelProject(item));
            //}

            //获取当前项目
            Project repositoryProject = null;
            foreach (Project project in dte.ActiveSolutionProjects as IEnumerable)
            {
                var all = project.Collection;
                //添加所有程序集。
                foreach (Project item in all)
                {
                    list.Add(new SelectEntityWindowViewModelProject(item));
                }

                repositoryProject = project;
                break;
            }

            this.Projects = list;
            //优先通过命名约定来绑定
            foreach (var item in list)
            {
                if (item.FullName + ".Repository" == repositoryProject.FullName)
                {
                    this.SelectedProject = item;
                    break;
                }
            }
        }

        /// <summary>
        /// 可供选择的项目。
        /// </summary>
        public List<SelectEntityWindowViewModelProject> Projects { get; set; }

        public SelectEntityWindowViewModelProject SelectedProject { get; set; }
    }

    public class SelectEntityWindowViewModelProject : ViewModel
    {
        private List<CodeClass> _types;

        public SelectEntityWindowViewModelProject(Project project)
        {
            this.Project = project;
        }

        public string FullName
        {
            get { return this.Project.Name; }
        }

        public Project Project { get; set; }

        public List<CodeClass> EntityTypes
        {
            get
            {
                if (_types == null)
                {
                    //查找其中的所有实体类的全名称。
                    _types = EntityFileFinder.FindFiles(this.Project).OrderBy(c => c.FullName).ToList();
                }

                return _types;
            }
        }

        private CodeClass _SelectedEntityType;
        public CodeClass SelectedEntityType
        {
            get { return this._SelectedEntityType; }
            set { this._SelectedEntityType = value; }
        }
    }
}
