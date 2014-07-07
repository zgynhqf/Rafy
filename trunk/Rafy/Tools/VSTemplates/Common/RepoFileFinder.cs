using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace Rafy.VSPackage
{
    /// <summary>
    /// 查找某一个只写了 Repository 代码的文件。
    /// </summary>
    class RepoFileFinder : CodeElementVisitor
    {
        public static List<CodeClass> FindFiles(object selectedItem)
        {
            var finder = new RepoFileFinder();
            finder.Find(selectedItem);
            return finder.Result;
        }

        private bool _fileFinised = false;
        private List<CodeClass> _result = new List<CodeClass>();

        public List<CodeClass> Result
        {
            get { return _result; }
        }

        public void Find(object selectedItem)
        {
            _result.Clear();

            var project = selectedItem as Project;
            if (project != null)
            {
                //对所有 CSharp 文件进行解析。
                foreach (var item in ProjectHelper.EnumerateCSharpFiles(project.ProjectItems))
                {
                    //忽略自动生成的文件。
                    if (!item.Name.Contains(".g.cs"))
                    {
                        _fileFinised = false;
                        this.Visit(item.FileCodeModel.CodeElements);
                    }
                }
            }
            else
            {
                //忽略自动生成的文件。
                var item = selectedItem as ProjectItem;
                if (!item.Name.Contains(".g.cs"))
                {
                    _fileFinised = false;
                    this.Visit(item.FileCodeModel.CodeElements);
                }
            }
        }

        protected override void VisitClass(CodeClass codeClass)
        {
            if (Helper.IsEntity(codeClass))
            {
                _fileFinised = true;
            }
            else if (Helper.IsRepository(codeClass))
            {
                _result.Add(codeClass);
                _fileFinised = true;
            }
        }

        protected override void Visit(CodeElement element)
        {
            if (_fileFinised) return;
            base.Visit(element);
        }

        protected override void VisitEnum(CodeEnum codeEnum)
        {
            //忽略 Enum，不用遍历其中的字段。
            //base.VisitEnum(codeEnum);
        }
    }
}
