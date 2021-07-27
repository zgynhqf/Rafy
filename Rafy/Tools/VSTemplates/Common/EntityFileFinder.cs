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
    /// <summary>
    /// 查找所有的实体文件，以及其一一对应的实体自动生成代码文件
    /// </summary>
    /// <seealso cref="Rafy.VSPackage.CodeElementVisitor" />
    class EntityFileFinder : ClassFinder
    {
        protected override void VisitClassCore(CodeClass codeClass)
        {
            if (Helper.IsEntity(codeClass))
            {
                this.Files.Add(codeClass);
                StopVisiting = true;
            }
        }
    }
}
