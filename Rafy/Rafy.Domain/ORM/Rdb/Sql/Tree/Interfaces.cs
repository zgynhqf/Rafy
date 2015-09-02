/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150822
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150822 15:05
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Domain.ORM.SqlTree
{
    /*********************** 代码块解释 *********************************
     * 以下接口只为了作为开发过程中的强类型限制使用。
     * 文本可以使用在所有需要的类型上。
    **********************************************************************/

    interface ISqlNode
    {
        SqlNodeType NodeType { get; }
    }
    interface ISqlSelect : ISqlNode { }
    interface ISqlConstraint : ISqlNode { }
    interface ISqlSource : ISqlNode { }
    interface ISqlLiteral : ISqlSelect, ISqlConstraint, ISqlSource { }
}
