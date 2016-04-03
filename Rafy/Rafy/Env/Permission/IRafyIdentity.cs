/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120312
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120312
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;

namespace Rafy
{
    /// <summary>
    /// 当前用户的接口定义
    /// 为后期扩展预留的接口。
    /// </summary>
    public interface IRafyIdentity : IIdentity
    {
        /// <summary>
        /// 用户的 Id。
        /// 
        /// 如果未被验证，则此值返回 0。
        /// </summary>
        object Id { get; }
    }
}