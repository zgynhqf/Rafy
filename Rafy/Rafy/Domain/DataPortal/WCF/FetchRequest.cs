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
    /// Request message for retrieving
    /// an existing business object.
    /// </summary>
    [DataContract, Serializable]
    public class FetchRequest
    {
        /// <summary>
        /// The type of the business object
        /// to be retrieved.
        /// </summary>
        [DataMember]
        public Type ObjectType { get; set; }

        /// <summary>
        /// Criteria object describing business object.
        /// </summary>
        [DataMember]
        public object Criteria { get; set; }

        /// <summary>
        /// Data portal context from client.
        /// </summary>
        [DataMember]
        public DataPortalContext Context { get; set; }
    }
}