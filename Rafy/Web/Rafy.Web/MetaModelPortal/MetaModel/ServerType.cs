/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120220
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120220
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Web.ClientMetaModel
{
    /// <summary>
    /// 服务器类型的表示器
    /// </summary>
    public struct ServerType
    {
        /// <summary>
        /// 服务端类型的标识
        /// </summary>
        public SupportedServerType Name;

        /// <summary>
        /// 运行时类型
        /// </summary>
        public Type RuntimeType;

        /// <summary>
        /// 是否可空。
        /// </summary>
        public bool IsNullable;

        /// <summary>
        /// 对应的 javascript 类型
        /// </summary>
        public JavascriptType JSType;
    }
}