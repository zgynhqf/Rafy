/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130423
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130423 12:08
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EnvDTE;
using Rafy.VSPackage.Modeling.CodeSync;
using RafySDK;
using VSTemplates.Wizards;

namespace Rafy.VSPackage.Commands.RefreshAutoCode
{
    /// <summary>
    /// 为选中的文件或者项目中的所有实体类生成最新的 .g.cs 文件。
    /// </summary>
    class RefreshAutoCodeFileCommand : Command
    {
        public RefreshAutoCodeFileCommand()
        {
            this.CommandID = new CommandID(GuidList.guidVSPackageCmdSet, PkgCmdIDList.cmdidAddGenericInterfaceFileCommand);
        }

        protected override void OnQueryStatus()
        {
            this.MenuCommand.Enabled = base.DTE.SelectedItems.Count > 0;
        }

        protected override void ExecuteCore()
        {
            //只做一个项目。
            SelectedItem item = base.DTE.SelectedItems.Item(1);

            object selectedItem = item.Project;
            if (selectedItem == null) selectedItem = item.ProjectItem;
            this.Execute(selectedItem);
        }

        private void Execute(object selectedItem)
        {
            string selectedType = "项目";
            if (selectedItem is ProjectItem) selectedType = "文件";

            var res = MessageBox.Show(
                string.Format("执行前请先保存所有的代码文件。将为当前选中的{0}中所有实体生成最新的泛型接口代码。确定执行吗？", selectedType),
                "提示", MessageBoxButton.OKCancel
                );
            if (res == MessageBoxResult.OK)
            {
                var finder = new EntityFileFinder();
                finder.Find(selectedItem);
                var entities = finder.Result;

                if (entities.Count <= 0)
                {
                    MessageBox.Show("无法生成：选中的项中没有找到任何实体。");
                    return;
                }

                int count = 0;

                foreach (var entity in entities)
                {
                    //所有抽象类都不生成自动代码。
                    if (entity.IsAbstract) continue;

                    var item = entity.ProjectItem;
                    RefreshAutoCodeForEntity(entity, item);

                    count++;
                }

                MessageBox.Show(string.Format("生成完毕。一共生成 {0} 个文件。", count));
            }
        }

        private void RefreshAutoCodeForEntity(CodeClass entity, ProjectItem item)
        {
            var fileName = item.get_FileNames(1);

            //标记文件中的类型都为分部类
            this.MarkPartial(fileName);

            //计算路径
            var gFileName = Path.GetFileNameWithoutExtension(fileName) + ".g.cs";
            var gFile = Path.Combine(Path.GetDirectoryName(fileName), gFileName);

            //生成代码，写入文件。
            var code = RenderByTemplate(entity);
            File.WriteAllText(gFile, code);

            //添加到项目中
            var children = item.ProjectItems;
            var gFileItem = children.FindByName(gFileName);
            if (gFileItem == null)
            {
                children.AddFromFile(gFile);
            }
        }

        private void MarkPartial(string fileName)
        {
            var code = File.ReadAllText(fileName);
            if (code.Contains("public class"))
            {
                code = code.Replace("public class", "public partial class");
                File.WriteAllText(fileName, code);
            }
        }

        #region RenderByTemplate

        private string RenderByTemplate(CodeClass entity)
        {
            string concreteNew = AddNewToConcrete(entity);

            //如果实体类文件中还包含了仓库的文件，则需要同时在自动代码中加入仓库的自动代码。
            bool renderRepository = HasRepository(entity);

            var res = ItemCodeTemplate.GetDomainEntityAutoCode(
                entity.Namespace.Name, concreteNew, entity.Name, renderRepository
                );

            return res;
        }

        #region AddNewToConcrete & HasRepository

        /// <summary>
        /// 如果实体列表的基类中，也定义了 Concrete 方法，则应该在本类的 Concrete 方法上添加 new 字样。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private string AddNewToConcrete(CodeClass entity)
        {
            string concreteNew = string.Empty;

            var baseClass = Helper.GetBaseClass(entity);
            if (baseClass != null && baseClass.Name != "Entity")
            {
                ProjectItem item = null;
                try
                {
                    //有些类不在这个项目中，则找不到。
                    item = baseClass.ProjectItem;
                }
                catch { }

                if (item != null)
                {
                    var fileName = Path.GetFileName(item.get_FileNames(1));
                    var gFileName = fileName.Replace(".cs", ".g.cs");
                    var gItem = item.ProjectItems.FindByName(gFileName);
                    if (gItem != null)
                    {
                        _listFinder.Find(gItem);
                        var listBaseClass = _listFinder._list;
                        if (listBaseClass != null)
                        {
                            if (listBaseClass.Members.OfType<CodeFunction>().Any(f => f.Name == "Concrete"))
                            {
                                concreteNew = "new ";
                            }
                        }
                    }
                }
            }

            //_listFinder.Find(entity.ProjectItem);
            //var res = _listFinder._list;
            //if (res != null)
            //{
            //    var baseClass = _listFinder._listBase;
            //    if (baseClass.Members.OfType<CodeFunction>().Any(f => f.Name == "Concrete"))
            //    {
            //        concreteNew = "new ";
            //    }
            //}

            return concreteNew;
        }

        private bool HasRepository(CodeClass entity)
        {
            ProjectItem item = null;
            try
            {
                //有些类不在这个项目中，则找不到。
                item = entity.ProjectItem;
            }
            catch { }

            if (item != null)
            {
                _repositoryFinder.Find(item);
                return _repositoryFinder._repository != null;
            }

            return false;
        }

        private ListFinder _listFinder = new ListFinder();

        private class ListFinder : CodeElementVisitor
        {
            internal CodeClass _list;
            internal CodeClass _listBase;

            public void Find(ProjectItem item)
            {
                _listBase = null;
                _list = null;
                this.Visit(item.FileCodeModel.CodeElements);
            }

            protected override void VisitClass(CodeClass codeClass)
            {
                if (codeClass.Name.EndsWith("List"))
                {
                    var baseClass = Helper.GetBaseClass(codeClass);
                    if (baseClass != null && baseClass.Name.EndsWith("List"))
                    {
                        _listBase = baseClass;
                        _list = codeClass;
                        return;
                    }
                }

                base.VisitClass(codeClass);
            }

            protected override void Visit(CodeElement element)
            {
                if (_list != null) return;

                base.Visit(element);
            }
        }

        private RepositoryFinder _repositoryFinder = new RepositoryFinder();

        private class RepositoryFinder : CodeElementVisitor
        {
            internal CodeClass _repository;

            public void Find(ProjectItem item)
            {
                _repository = null;
                this.Visit(item.FileCodeModel.CodeElements);
            }

            protected override void VisitClass(CodeClass codeClass)
            {
                if (codeClass.Name.EndsWith(Consts.RepositorySuffix))
                {
                    var baseClass = Helper.GetBaseClass(codeClass);
                    if (baseClass != null && baseClass.Name.EndsWith(Consts.RepositorySuffix))
                    {
                        _repository = codeClass;
                        return;
                    }
                }

                base.VisitClass(codeClass);
            }

            protected override void Visit(CodeElement element)
            {
                if (_repository != null) return;

                base.Visit(element);
            }
        }

        #endregion

        #endregion
    }
}