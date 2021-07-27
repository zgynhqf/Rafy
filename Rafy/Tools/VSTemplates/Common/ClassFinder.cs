/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20210727
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20210727 21:12
 * 
*******************************************************/

using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.VSPackage
{
    /// <summary>
    /// 一个类型及其对应的生成的代码文件的查找器。
    /// </summary>
    /// <seealso cref="Rafy.VSPackage.CodeElementVisitor" />
    abstract class ClassFinder : CodeElementVisitor
    {
        private List<CodeClass> _files = new List<CodeClass>();
        private List<ProjectItem> _filesGenerated = new List<ProjectItem>();
        private Predicate<CodeClass> _classFilter;

        protected bool StopVisiting { get; set; }
        public List<CodeClass> Files { get => _files; }
        protected List<ProjectItem> FilesGenerated { get => _filesGenerated; }

        public List<CodeWithGeneratedFile> FindPairs(object selectedItem, Predicate<CodeClass> classFilter = null)
        {
            this.Find(selectedItem, classFilter);

            return _files.Select(e => new CodeWithGeneratedFile
            {
                CodeClass = e,
                GeneratedCode = _filesGenerated.FirstOrDefault(f => f.Name == e.Name + GenerateFileExtension)
            }).ToList();
        }

        public List<CodeClass> FindFiles(object selectedItem, Predicate<CodeClass> classFilter = null)
        {
            this.Find(selectedItem, classFilter);
            return _files;
        }

        public void Find(object selectedItem, Predicate<CodeClass> classFilter)
        {
            _classFilter = classFilter;
            _files.Clear();
            _filesGenerated.Clear();

            var project = selectedItem as Project;
            if (project != null)
            {
                //对所有 CSharp 文件进行解析。
                foreach (var item in ProjectHelper.EnumerateCSharpFiles(project.ProjectItems))
                {
                    this.VisitProjectItem(item);
                }
            }
            else
            {
                this.VisitProjectItem(selectedItem as ProjectItem);
            }
        }

        private void VisitProjectItem(ProjectItem item)
        {
            if (!IsGeneratedFile(item))
            {
                StopVisiting = false;
                this.Visit(item.FileCodeModel.CodeElements);
            }
            else
            {
                _filesGenerated.Add(item);
            }
        }

        protected override void Visit(CodeElement element)
        {
            if (StopVisiting) return;

            base.Visit(element);
        }

        protected sealed override void VisitClass(CodeClass codeClass)
        {
            if (_classFilter != null && !_classFilter(codeClass)) return;

            this.VisitClassCore(codeClass);
        }

        protected virtual void VisitClassCore(CodeClass codeClass) { }

        protected override void VisitEnum(CodeEnum codeEnum)
        {
            //忽略 Enum，不用遍历其中的字段。
            //base.VisitEnum(codeEnum);
        }

        internal static bool IsGeneratedFile(ProjectItem item)
        {
            return item.Name.EndsWith(GenerateFileExtension);
        }

        internal static readonly string GenerateFileExtension = ".g.cs";
    }

    /// <summary>
    /// 某个类型与其对应的生成代码文件的。
    /// </summary>
    public class CodeWithGeneratedFile
    {
        public CodeClass CodeClass { get; set; }

        /// <summary>
        /// 如果这个属性是 null，则表示还没有对应的生成文件。
        /// 那么这时应该根据约定进行代码的生成。
        /// 如果找到对应的文件，则直接将对应的文件进行更新即可。
        /// </summary>
        public ProjectItem GeneratedCode { get; set; }
    }
}