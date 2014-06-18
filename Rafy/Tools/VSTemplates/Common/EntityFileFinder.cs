/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130423
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130423 13:45
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace Rafy.VSPackage
{
    class EntityFileFinder : CodeElementVisitor
    {
        private bool _currentIsEntity = false;
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
                        _currentIsEntity = false;
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
                    _currentIsEntity = false;
                    this.Visit(item.FileCodeModel.CodeElements);
                }
            }
        }

        protected override void VisitClass(CodeClass codeClass)
        {
            if (Helper.IsEntity(codeClass))
            {
                _result.Add(codeClass);
                _currentIsEntity = true;
            }
        }

        protected override void Visit(CodeElement element)
        {
            if (_currentIsEntity) return;
            base.Visit(element);
        }

        protected override void VisitEnum(CodeEnum codeEnum)
        {
            //忽略 Enum
            //base.VisitEnum(codeEnum);
        }
    }
}
