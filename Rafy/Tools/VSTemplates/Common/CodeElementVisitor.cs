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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace Rafy.VSPackage
{
    /// <summary>
    /// CodeElement 的访问者。
    /// </summary>
    internal abstract class CodeElementVisitor
    {
        protected virtual void Visit(CodeElements elements)
        {
            foreach (CodeElement element in elements)
            {
                this.Visit(element);
            }
        }

        protected virtual void Visit(CodeElement element)
        {
            switch (element.Kind)
            {
                case vsCMElement.vsCMElementNamespace:
                    this.VisitNamespace(element as CodeNamespace);
                    break;
                case vsCMElement.vsCMElementClass:
                    this.VisitClass(element as CodeClass);
                    break;
                case vsCMElement.vsCMElementDelegate:
                    this.VisitDelegate(element as CodeDelegate);
                    break;
                case vsCMElement.vsCMElementEnum:
                    this.VisitEnum(element as CodeEnum);
                    break;
                case vsCMElement.vsCMElementFunction:
                    this.VisitFunction(element as CodeFunction);
                    break;
                case vsCMElement.vsCMElementAttribute:
                    this.VisitAttribute(element as CodeAttribute);
                    break;
                case vsCMElement.vsCMElementProperty:
                    this.VisitProperty(element as CodeProperty);
                    break;
                case vsCMElement.vsCMElementInterface:
                    this.VisitInterface(element as CodeInterface);
                    break;
                case vsCMElement.vsCMElementParameter:
                    this.VisitParameter(element as CodeParameter);
                    break;
                case vsCMElement.vsCMElementStruct:
                    this.VisitStruct(element as CodeStruct);
                    break;
                case vsCMElement.vsCMElementVariable:
                    this.VisitVariable(element as CodeVariable);
                    break;
                case vsCMElement.vsCMElementPropertySetStmt:
                case vsCMElement.vsCMElementAssignmentStmt:
                case vsCMElement.vsCMElementDeclareDecl:
                case vsCMElement.vsCMElementDefineStmt:
                case vsCMElement.vsCMElementEvent:
                case vsCMElement.vsCMElementEventsDeclaration:
                case vsCMElement.vsCMElementFunctionInvokeStmt:
                case vsCMElement.vsCMElementIDLCoClass:
                case vsCMElement.vsCMElementIDLImport:
                case vsCMElement.vsCMElementIDLImportLib:
                case vsCMElement.vsCMElementIDLLibrary:
                case vsCMElement.vsCMElementImplementsStmt:
                case vsCMElement.vsCMElementImportStmt:
                case vsCMElement.vsCMElementIncludeStmt:
                case vsCMElement.vsCMElementInheritsStmt:
                case vsCMElement.vsCMElementLocalDeclStmt:
                case vsCMElement.vsCMElementMacro:
                case vsCMElement.vsCMElementMap:
                case vsCMElement.vsCMElementMapEntry:
                case vsCMElement.vsCMElementModule:
                case vsCMElement.vsCMElementOptionStmt:
                case vsCMElement.vsCMElementOther:
                case vsCMElement.vsCMElementTypeDef:
                case vsCMElement.vsCMElementUDTDecl:
                case vsCMElement.vsCMElementUnion:
                case vsCMElement.vsCMElementUsingStmt:
                case vsCMElement.vsCMElementVBAttributeGroup:
                case vsCMElement.vsCMElementVBAttributeStmt:
                case vsCMElement.vsCMElementVCBase:
                    this.VisitOther(element);
                    break;
                default:
                    break;
            }
        }

        protected virtual void VisitStruct(CodeStruct codeStruct)
        {
        }

        protected virtual void VisitVariable(CodeVariable codeVariable)
        {
        }

        protected virtual void VisitParameter(CodeParameter codeParameter)
        {
        }

        protected virtual void VisitInterface(CodeInterface codeInterface)
        {
        }

        protected virtual void VisitProperty(CodeProperty codeProperty)
        {

        }

        protected virtual void VisitDelegate(CodeDelegate codeDelegate)
        {

        }

        protected virtual void VisitAttribute(CodeAttribute codeAttribute)
        {

        }

        protected virtual void VisitFunction(CodeFunction codeFunction)
        {

        }

        protected virtual void VisitNamespace(CodeNamespace codeNamespace)
        {
            this.Visit(codeNamespace.Members);
        }

        protected virtual void VisitEnum(CodeEnum codeEnum)
        {
            this.Visit(codeEnum.Members);
        }

        protected virtual void VisitClass(CodeClass codeClass)
        {
            this.Visit(codeClass.Members);
        }

        protected virtual void VisitOther(CodeElement element) { }
    }
}
