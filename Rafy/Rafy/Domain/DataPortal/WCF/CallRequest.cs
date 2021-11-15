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
#if NET45

using System;
using System.Runtime.Serialization;

namespace Rafy.Domain.DataPortal.WCF
{
    /// <summary>
    /// WCF 调用请求对象。
    /// </summary>
    [DataContract, Serializable]
    public class CallRequest
    {
        [DataMember]
        public object Instance { get; set; }

        [DataMember]
        public string Method { get; set; }

        [DataMember]
        public object[] Arguments { get; set; }

        [DataMember]
        public DataPortalContext Context { get; set; }
    }
}

#endif