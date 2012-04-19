using System;
using System.Configuration;
using System.Security.Principal;

using OEA.Library;
using OEA.DataPortalClient;
using OEA;

namespace OEA.Server
{
    /// <summary>
    /// Implements the server-side DataPortal 
    /// message router as discussed
    /// in Chapter 4.
    /// </summary>
    public class DataPortalFacade : IDataPortalServer
    {
        /// <summary>
        /// Get an existing business object.
        /// </summary>
        /// <param name="objectType">Type of business object to retrieve.</param>
        /// <param name="criteria">Criteria object describing business object.</param>
        /// <param name="context">
        /// <see cref="Server.DataPortalContext" /> object passed to the server.
        /// </param>
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
        /// <see cref="Server.DataPortalContext" /> object passed to the server.
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
            ApplicationContext.SetLogicalExecutionLocation(ApplicationContext.LogicalExecutionLocations.Server);

            // if the dataportal is not remote then
            // do nothing
            if (!context.IsRemotePortal) return;

            // set the context value so everyone knows the
            // code is running on the server
            ApplicationContext.SetExecutionLocation(ApplicationContext.ExecutionLocations.Server);

            // set the app context to the value we got from the
            // client
            ApplicationContext.SetContext(context.ClientContext, context.GlobalContext);

            // set the thread's culture to match the client
            System.Threading.Thread.CurrentThread.CurrentCulture =
              new System.Globalization.CultureInfo(context.ClientCulture);
            System.Threading.Thread.CurrentThread.CurrentUICulture =
              new System.Globalization.CultureInfo(context.ClientUICulture);

            if (ApplicationContext.AuthenticationType == "Windows")
            {
                // When using integrated security, Principal must be null
                if (context.Principal != null)
                {
                    System.Security.SecurityException ex =
                      new System.Security.SecurityException("Resources.NoPrincipalAllowedException");
                    ex.Action = System.Security.Permissions.SecurityAction.Demand;
                    throw ex;
                }
                // Set .NET to use integrated security
                AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
            }
            else
            {
                // We expect the some Principal object
                if (context.Principal == null)
                {
                    System.Security.SecurityException ex =
                      new System.Security.SecurityException(
                        "Resources.BusinessPrincipalException" + " Nothing");
                    ex.Action = System.Security.Permissions.SecurityAction.Demand;
                    throw ex;
                }
                ApplicationContext.User = context.Principal;
            }
        }

        private static void ClearContext(DataPortalContext context)
        {
            ApplicationContext.SetLogicalExecutionLocation(ApplicationContext.LogicalExecutionLocations.Client);
            // if the dataportal is not remote then
            // do nothing
            if (!context.IsRemotePortal) return;
            ApplicationContext.Clear();
            if (ApplicationContext.AuthenticationType != "Windows")
                ApplicationContext.User = null;
        }
    }
}