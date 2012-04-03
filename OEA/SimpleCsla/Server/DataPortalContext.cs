using System;
using System.Security.Principal;
using System.Collections.Specialized;
using SimpleCsla.Core;

namespace SimpleCsla.Server
{
    /// <summary>
    /// Provides consistent context information between the client
    /// and server DataPortal objects. 
    /// </summary>
    [Serializable()]
    public class DataPortalContext
    {
        private IPrincipal _principal;
        private bool _remotePortal;
        private string _clientCulture;
        private string _clientUICulture;
        private ContextDictionary _clientContext;
        private ContextDictionary _globalContext;

        /// <summary>
        /// The current principal object
        /// if CSLA security is being used.
        /// </summary>
        public IPrincipal Principal
        {
            get { return _principal; }
        }

        /// <summary>
        /// Returns <see langword="true" /> if the 
        /// server-side DataPortal is running
        /// on a remote server via remoting.
        /// </summary>
        public bool IsRemotePortal
        {
            get { return _remotePortal; }
        }

        /// <summary>
        /// The culture setting on the client
        /// workstation.
        /// </summary>
        public string ClientCulture
        {
            get { return _clientCulture; }
        }

        /// <summary>
        /// The culture setting on the client
        /// workstation.
        /// </summary>
        public string ClientUICulture
        {
            get { return _clientUICulture; }
        }
        internal ContextDictionary ClientContext
        {
            get { return _clientContext; }
        }

        internal ContextDictionary GlobalContext
        {
            get { return _globalContext; }
        }

        /// <summary>
        /// Creates a new DataPortalContext object.
        /// </summary>
        /// <param name="principal">The current Principal object.</param>
        /// <param name="isRemotePortal">Indicates whether the DataPortal is remote.</param>
        public DataPortalContext(IPrincipal principal, bool isRemotePortal)
        {
            if (isRemotePortal)
            {
                _principal = principal;
                _remotePortal = isRemotePortal;
                _clientCulture =
                  System.Threading.Thread.CurrentThread.CurrentCulture.Name;
                _clientUICulture =
                  System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
                _clientContext = SimpleCsla.ApplicationContext.GetClientContext();
                _globalContext = SimpleCsla.ApplicationContext.GetGlobalContext();
            }
        }
    }
}
