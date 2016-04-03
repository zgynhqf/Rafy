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

namespace Rafy.Domain.DataPortal
{
    /// <summary>
    /// Provides consistent context information between the client
    /// and server DataPortal objects. 
    /// </summary>
    [DataContract, Serializable]
    public class DataPortalContext
    {
        /// <summary>
        /// The current principal object
        /// if CSLA security is being used.
        /// </summary>
        [DataMember]
        public IPrincipal Principal { get; set; }

        /// <summary>
        /// The culture setting on the client
        /// workstation.
        /// </summary>
        [DataMember]
        public string ClientCulture { get; set; }

        /// <summary>
        /// The culture setting on the client
        /// workstation.
        /// </summary>
        [DataMember]
        public string ClientUICulture { get; set; }

        [DataMember]
        public Dictionary<string, object> ClientContext { get; set; }

        [DataMember]
        public Dictionary<string, object> GlobalContext { get; set; }
    }
}
