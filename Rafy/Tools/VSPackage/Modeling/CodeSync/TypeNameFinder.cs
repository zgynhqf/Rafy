/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130414 19:07
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130414 19:07
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
    /// 一个查找某文件中所有类型全名称的类型。
    /// </summary>
    class TypeNameFinder : CodeElementVisitor
    {
        private List<string> _result;

        public IList<string> FindTypes(ProjectItem csharpItem)
        {
            _result = new List<string>();
            if (csharpItem.FileCodeModel != null)
            {
                this.Visit(csharpItem.FileCodeModel.CodeElements);
            }
            return _result;
        }

        protected override void VisitClass(CodeClass codeClass)
        {
            _result.Add(codeClass.FullName);
        }

        protected override void VisitEnum(CodeEnum codeEnum)
        {
            _result.Add(codeEnum.FullName);
        }
    }
}
