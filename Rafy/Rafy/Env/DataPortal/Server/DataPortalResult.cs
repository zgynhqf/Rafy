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

namespace Rafy.DataPortal
{
    /// <summary>
    /// Returns data from the server-side DataPortal to the 
    /// client-side DataPortal. Intended for internal CSLA .NET
    /// use only.
    /// </summary>
    [Serializable]
    public class DataPortalResult
    {
        private object _res;
        private object[] _outParameters;
        private Dictionary<string, object> _gc;

        /// <summary>
        /// Creates an instance of the object.
        /// </summary>
        /// <param name="returnObject">Object to return as part
        /// of the result.</param>
        /// <param name="outParameters"></param>
        public DataPortalResult(object returnObject, object[] outParameters)
        {
            _res = returnObject;
            _outParameters = outParameters;
            _gc = DistributionContext.GlobalContextItem.Value;
        }

        /// <summary>
        /// The business object being returned from
        /// the server.
        /// </summary>
        public object ReturnObject => _res;

        /// <summary>
        /// The global context being returned from
        /// the server.
        /// </summary>
        public Dictionary<string, object> GlobalContext => _gc;

        public object[] OutParameters => _outParameters;
    }
}