/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：2012
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2012
 * 
*******************************************************/

using System;
using System.ComponentModel;
using OEA;
using OEA.Library;
using OEA.Reflection;
using OEA.Server;
using OEA.DataPortalClient;
using System.Security.Principal;

namespace OEA
{
    /// <summary>
    /// This is the client-side DataPortal as described in
    /// Chapter 4.
    /// </summary>
    public static class DataPortal
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="criteria"></param>
        /// <param name="runAtLocal">如果一个数据层方法需要在本地执行，应该在把本参数指明为 true。</param>
        /// <returns></returns>
        public static object Fetch(Type objectType, object criteria, DataPortalLocation loc = DataPortalLocation.Remote)
        {
            var proxy = GetDataPortalProxy(loc);

            var dpContext = new Server.DataPortalContext(GetPrincipal(), proxy.IsServerRemote);

            Server.DataPortalResult result = null;

            try
            {
                result = proxy.Fetch(objectType, criteria, dpContext);
            }
            finally
            {
                if (proxy.IsServerRemote && result != null) { ApplicationContext.SetGlobalContext(result.GlobalContext); }
            }

            return result.ReturnObject;
        }

        /// <summary>
        /// Called by the business object's Save() method to
        /// insert, update or delete an object in the database.
        /// </summary>
        /// <remarks>
        /// Note that this method returns a reference to the updated business object.
        /// If the server-side DataPortal is running remotely, this will be a new and
        /// different object from the original, and all object references MUST be updated
        /// to use this new object.
        /// </remarks>
        /// <param name="obj">A reference to the business object to be updated.</param>
        /// <returns>A reference to the updated business object.</returns>
        public static object Update(object obj, DataPortalLocation loc = DataPortalLocation.Remote)
        {
            var proxy = GetDataPortalProxy(loc);

            var dpContext = new Server.DataPortalContext(GetPrincipal(), proxy.IsServerRemote);

            var result = proxy.Update(obj, dpContext);

            if (proxy.IsServerRemote) ApplicationContext.SetGlobalContext(result.GlobalContext);

            return result.ReturnObject;
        }

        #region Helpers

        private static Type _proxyType;

        private static IDataPortalProxy GetDataPortalProxy(DataPortalLocation loc)
        {
            if (loc == DataPortalLocation.Local) return new LocalProxy();

            if (_proxyType == null)
            {
                string proxyTypeName = ApplicationContext.DataPortalProxy;
                if (proxyTypeName == "Local") return new LocalProxy();

                _proxyType = Type.GetType(proxyTypeName, true, true);
            }
            return Activator.CreateInstance(_proxyType) as IDataPortalProxy;
        }

        private static IPrincipal GetPrincipal()
        {
            if (ApplicationContext.AuthenticationType == "Windows")
            {
                // Windows integrated security
                return null;
            }
            else
            {
                // we assume using the CSLA framework security
                return ApplicationContext.User;
            }
        }

        #endregion
    }

    /// <summary>
    /// 数据访问层执行的地点
    /// </summary>
    public enum DataPortalLocation
    {
        /// <summary>
        /// 在远程服务端执行
        /// </summary>
        Remote,
        /// <summary>
        /// 在本地执行（可能是客户端也可能是服务端）。
        /// </summary>
        Local,
    }
}