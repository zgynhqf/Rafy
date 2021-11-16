/*******************************************************
 * 
 * 作者：CSLA
 * 创建日期：2008
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 周金根 2008
 * 
*******************************************************/

using System;
using System.Security.Principal;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Threading;
using System.Collections.Generic;

namespace Rafy.DataPortal
{
    /// <summary>
    /// 每次调用需要传输的上下文对象
    /// </summary>
    [DataContract, Serializable]
    public class DataPortalContext
    {
        /// <summary>
        /// 由调用方向被调用方单向传输的当前用户对象。
        /// </summary>
        [DataMember]
        public IPrincipal Principal { get; set; }

        /// <summary>
        /// 由调用方向被调用方单向传输的当前文化对象。
        /// </summary>
        [DataMember]
        public string ClientCulture { get; set; }

        /// <summary>
        /// 由调用方向被调用方单向传输的当前界面文化对象。
        /// </summary>
        [DataMember]
        public string ClientUICulture { get; set; }

        /// <summary>
        /// 由调用方向被调用方单向传输的上下文集合。
        /// </summary>
        [DataMember]
        public Dictionary<string, object> ClientContext { get; set; }

        /// <summary>
        /// 双向传输的上下文集合。
        /// </summary>
        [DataMember]
        public Dictionary<string, object> GlobalContext { get; set; }
    }
}
