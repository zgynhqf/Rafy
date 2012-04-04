using System;
using System.ComponentModel;
using OEA;
using OEA.Library;
using OEA.Reflection;
using OEA.Server;

namespace OEA
{
    /// <summary>
    /// This is the client-side DataPortal as described in
    /// Chapter 4.
    /// </summary>
    public static class DataPortal
    {
        public static object Fetch(Type objectType, object criteria)
        {
            var proxy = GetDataPortalProxy();

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
        public static object Update(object obj)
        {
            var proxy = GetDataPortalProxy();

            var dpContext = new Server.DataPortalContext(GetPrincipal(), proxy.IsServerRemote);

            var result = proxy.Update(obj, dpContext);

            if (proxy.IsServerRemote)
                ApplicationContext.SetGlobalContext(result.GlobalContext);

            return result.ReturnObject;
        }

        /// <summary>
        /// Inserts, updates or deletes an existing
        /// child business object.
        /// </summary>
        /// <param name="child">
        /// Business object to update.
        /// </param>
        /// <param name="parameters">
        /// Parameters passed to child update method.
        /// </param>
        public static void UpdateChild(object obj, Entity parent)
        {
            if (obj == null) return;

            var entity = obj as Entity;
            // tell the business object to update itself
            if (entity != null)
            {
                // if the object isn't dirty, then just exit
                if (!entity.IsDirty) { return; }

                if (entity.IsDeleted)
                {
                    if (!entity.IsNew)
                    {
                        // tell the object to delete itself
                        entity.Child_Delete(parent);
                        entity.MarkNew();
                    }
                }
                else
                {
                    if (entity.IsNew)
                    {
                        // tell the object to insert itself
                        entity.Child_Insert(parent);
                    }
                    else
                    {
                        // tell the object to update itself
                        entity.Child_Update(parent);
                    }
                    entity.MarkOld();
                }
            }
            else if (obj is EntityList)
            {
                (obj as EntityList).Child_Update(parent);
            }
            else
            {
                // this is an updatable collection or some other
                // non-BusinessBase type of object
                // tell the object to update itself
                MethodCaller.CallMethodIfImplemented(obj, "Child_Update", parent);
            }
        }

        #region Helpers

        private static Type _proxyType;

        private static DataPortalClient.IDataPortalProxy GetDataPortalProxy()
        {
            if (_proxyType == null)
            {
                string proxyTypeName = ApplicationContext.DataPortalProxy;
                if (proxyTypeName == "Local")
                    _proxyType = typeof(DataPortalFacade);
                else
                    _proxyType = Type.GetType(proxyTypeName, true, true);
            }
            return (DataPortalClient.IDataPortalProxy)Activator.CreateInstance(_proxyType);
        }

        private static System.Security.Principal.IPrincipal GetPrincipal()
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
}