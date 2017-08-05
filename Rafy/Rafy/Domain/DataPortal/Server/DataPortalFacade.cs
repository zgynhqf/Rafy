/*******************************************************
 * 
 * 作者：CSLA
 * 创建时间：2008
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2010
 * 
*******************************************************/

using System;
using System.Configuration;
using System.Globalization;
using System.Security.Principal;
using System.Threading;

namespace Rafy.Domain.DataPortal
{
    /// <summary>
    /// Implements the server-side DataPortal 
    /// message router as discussed
    /// in Chapter 4.
    /// </summary>
    internal class DataPortalFacade : IDataPortalServer
    {
        /// <summary>
        /// Get an existing business object.
        /// </summary>
        /// <param name="objectType">Type of business object to retrieve.</param>
        /// <param name="criteria">Criteria object describing business object.</param>
        /// <param name="context"><see cref="DataPortalContext" /> object passed to the server.</param>
        /// <returns></returns>
        public DataPortalResult Fetch(Type objectType, object criteria, DataPortalContext context)
        {
            try
            {
                SetContext(context);

                var portal = new FinalDataPortal();
                var result = portal.Fetch(objectType, criteria, context);

                return result;
            }
            finally
            {
                ClearContext(context);
            }
        }

        /// <summary>
        /// Update a business object.
        /// </summary>
        /// <param name="obj">Business object to update.</param>
        /// <param name="context">
        /// <see cref="DataPortalContext" /> object passed to the server.
        /// </param>
        public DataPortalResult Update(object obj, DataPortalContext context)
        {
            try
            {
                SetContext(context);

                var portal = new FinalDataPortal();
                var result = portal.Update(obj, context);

                return result;
            }
            finally
            {
                ClearContext(context);
            }
        }

        private static void SetContext(DataPortalContext context)
        {
            // set the app context to the value we got from the
            // client
            DistributionContext.ClientContextItem.Value = context.ClientContext;
            DistributionContext.GlobalContextItem.Value = context.GlobalContext;

            // set the thread's culture to match the client
            Thread.CurrentThread.CurrentCulture = new CultureInfo(context.ClientCulture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(context.ClientUICulture);

            // We expect the some Principal object
            if (context.Principal == null)
            {
                System.Security.SecurityException ex =
                  new System.Security.SecurityException(
                    "Resources.BusinessPrincipalException" + " Nothing");
                //ex.Action = System.Security.Permissions.SecurityAction.Demand;
                throw ex;
            }

            RafyEnvironment.Principal = context.Principal;
        }

        private static void ClearContext(DataPortalContext context)
        {
            DistributionContext.ClientContextItem.Value = null;
            DistributionContext.GlobalContextItem.Value = null;
            RafyEnvironment.Principal = null;
        }
    }
}