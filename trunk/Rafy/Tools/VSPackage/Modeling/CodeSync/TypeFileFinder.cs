/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130413 23:14
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130413 23:14
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace Rafy.VSPackage.Modeling.CodeSync
{
    /// <summary>
    /// 一个通过类名，查找 ProjectItem 的查找类。
    /// </summary>
    class TypeFileFinder : CodeElementVisitor
    {
        private ProjectItem _result;

        private string _classFullName;

        public ProjectItem FindClassFile(Project project, string classFullName)
        {
            _result = null;
            _classFullName = classFullName;

            //对所有 CSharp 文件进行解析。
            foreach (var item in ProjectHelper.EnumerateCSharpFiles(project.ProjectItems))
            {
                this.Visit(item.FileCodeModel.CodeElements);
            }

            return _result;
        }

        protected override void VisitClass(CodeClass codeClass)
        {
            if (codeClass.FullName == _classFullName)
            {
                _result = codeClass.ProjectItem;
            }
        }

        protected override void VisitEnum(CodeEnum codeEnum)
        {
            if (codeEnum.FullName == _classFullName)
            {
                _result = codeEnum.ProjectItem;
            }
        }

        protected override void Visit(CodeElement element)
        {
            //结果已经找到，退出访问。
            if (_result != null) return;

            base.Visit(element);
        }
    }
}