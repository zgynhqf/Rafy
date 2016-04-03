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
using System.Runtime.Serialization;

namespace Rafy.Domain.DataPortal.WCF
{
    /// <summary>
    /// Request message for updating
    /// a business object.
    /// </summary>
    [DataContract, Serializable]
    public class UpdateRequest
    {
        /// <summary>
        /// Business object to be updated.
        /// </summary>
        [DataMember]
        public object Object { get; set; }

        /// <summary>
        /// Data portal context from client.
        /// </summary>
        [DataMember]
        public DataPortalContext Context { get; set; }
    }
}