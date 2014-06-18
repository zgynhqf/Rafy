/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130409 13:01
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130409 13:01
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EnvDTE;

namespace Rafy.VSPackage.Modeling.CodeSync
{
    /// <summary>
    /// 测试使用的文本输出器。
    /// </summary>
    class TextVisitor : CodeElementVisitor
    {
        private int _level;

        private StringBuilder _buffer;

        public string FlushText(Project project)
        {
            _buffer = new StringBuilder();

            foreach (ProjectItem item in ProjectHelper.EnumerateCSharpFiles(project.ProjectItems))
            {
                _buffer.AppendFormat("文件：{0}", item.Name);
                _buffer.AppendLine();

                _level++;
                this.Visit(item.FileCodeModel.CodeElements);
                _level--;
            }

            //如果使用 project.CodeModel 来进行遍历，则会包含所有涉及到的类型。
            //这些类型甚至包括 Rafy、Microsoft、System 等命名空间中的大量类型。
            //var elements = project.CodeModel.CodeElements;
            //this.Visit(elements);

            var res = _buffer.ToString();

            File.WriteAllText(@"D:\TextVisitorResult.txt", res);

            return res;
        }

        private void ShowName(object element, string type)
        {
            var el = element as CodeElement;

            for (int i = 0; i < _level * 4; i++) { _buffer.Append(' '); }
            _buffer.AppendFormat("{0} ({1})", el.Name, type);
            _buffer.AppendLine();
        }

        protected override void VisitNamespace(CodeNamespace codeNamespace)
        {
            this.ShowName(codeNamespace, "NameSpace");

            _level++;

            base.VisitNamespace(codeNamespace);

            _level--;
        }

        protected override void VisitClass(CodeClass codeClass)
        {
            this.ShowName(codeClass, "Class");

            _level++;

            base.VisitClass(codeClass);

            _level--;
        }

        protected override void VisitProperty(CodeProperty codeProperty)
        {
            this.ShowName(codeProperty, "Property");

            base.VisitProperty(codeProperty);
        }
    }
}
