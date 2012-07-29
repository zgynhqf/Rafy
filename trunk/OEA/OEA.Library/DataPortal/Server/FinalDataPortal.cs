using System;
using OEA.Reflection;

using OEA.Library;
using OEA;

namespace OEA.Server
{
    /// <summary>
    /// Implements the server-side DataPortal as discussed
    /// in Chapter 4.
    /// </summary>
    public class FinalDataPortal : IDataPortalServer
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
            // create an instance of the business object.
            var obj = Activator.CreateInstance(objectType, true);

            MethodCaller.CallMethodIfImplemented(obj, "QueryBy", criteria);

            // return the populated business object as a result
            return new DataPortalResult(obj);
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
            // tell the business object to update itself
            var target = obj as Entity;
            if (target != null)
            {
                target.SaveRoot();
            }
            else if (obj is Service)
            {
                (obj as Service).ExecuteByDataPortal();
            }
            else if (obj is EntityList)
            {
                (obj as EntityList).SaveRootList();
            }
            else
            {
                // this is an updatable collection or some other
                // non-BusinessBase type of object
                // tell the object to update itself
                MethodCaller.CallMethodIfImplemented(obj, "DataPortal_Update");
            }

            return new DataPortalResult(obj);
        }
    }
}