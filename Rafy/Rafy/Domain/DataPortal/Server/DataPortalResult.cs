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
using System.Collections.Generic;
using System.Collections.Specialized;
using Rafy.Serialization.Mobile;

namespace Rafy.Domain.DataPortal
{
    /// <summary>
    /// Returns data from the server-side DataPortal to the 
    /// client-side DataPortal. Intended for internal CSLA .NET
    /// use only.
    /// </summary>
    [Serializable]
    public class DataPortalResult : MobileObject
    {
        private object _returnObject;

        private Dictionary<string,object> _globalContext;

        /// <summary>
        /// The business object being returned from
        /// the server.
        /// </summary>
        public object ReturnObject
        {
            get { return _returnObject; }
        }

        /// <summary>
        /// The global context being returned from
        /// the server.
        /// </summary>
        public Dictionary<string, object> GlobalContext
        {
            get { return _globalContext; }
        }

        /// <summary>
        /// Creates an instance of the object.
        /// </summary>
        public DataPortalResult()
        {
            _globalContext = DistributionContext.GlobalContextItem.Value;
        }

        /// <summary>
        /// Creates an instance of the object.
        /// </summary>
        /// <param name="returnObject">Object to return as part
        /// of the result.</param>
        public DataPortalResult(object returnObject)
        {
            _returnObject = returnObject;
            _globalContext = DistributionContext.GlobalContextItem.Value;
        }
    }
}