/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140614
 * 说明：见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140614 10:56
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Domain
{
    /// <summary>
    /// 表明某个类型是一个服务的契约。
    /// </summary>
    /// 暂时没有用到，未来可能需要对所有的契约遍历。
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public sealed class ContractAttribute : Attribute { }
}
