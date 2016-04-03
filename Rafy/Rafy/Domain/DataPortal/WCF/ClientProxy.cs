using System;
using System.ServiceModel;

namespace Rafy.Domain.DataPortal.WCF
{
    /// <summary>
    /// Implements a data portal proxy to relay data portal
    /// calls to a remote application server by using WCF.
    /// </summary>
    public class ClientProxy : IDataPortalProxy
    {
        #region IDataPortalProxy Members

        private const string _endPoint = "ClientProxyEndPoint";

        /// <summary>
        /// Returns an instance of the channel factory
        /// used by GetProxy() to create the WCF proxy
        /// object.
        /// </summary>
        private ChannelFactory<IWcfPortal> GetChannelFactory()
        {
            return new ChannelFactory<IWcfPortal>(_endPoint);
        }

        /// <summary>
        /// Called by <see cref="DataPortal" /> to load an
        /// existing business object.
        /// </summary>
        /// <param name="objectType">Type of business object to create.</param>
        /// <param name="criteria">Criteria object describing business object.</param>
        /// <param name="context"><see cref="DataPortalContext" /> object passed to the server.</param>
        /// <returns></returns>
        public DataPortalResult Fetch(Type objectType, object criteria, DataPortalContext context)
        {
            var cf = GetChannelFactory();
            var proxy = cf.CreateChannel();
            try
            {
                var request = new FetchRequest
                {
                    ObjectType = objectType,
                    Criteria = criteria,
                    Context = context,
                };
                var response = proxy.Fetch(request);
                if (cf != null) cf.Close();
                return ReturnResult(response);
            }
            catch
            {
                cf.Abort();
                throw;
            }
        }

        /// <summary>
        /// Called by <see cref="DataPortal" /> to update a
        /// business object.
        /// </summary>
        /// <param name="obj">The business object to update.</param>
        /// <param name="context">
        /// <see cref="DataPortalContext" /> object passed to the server.
        /// </param>
        public DataPortalResult Update(object obj, DataPortalContext context)
        {
            var cf = GetChannelFactory();
            var proxy = cf.CreateChannel();
            try
            {
                var request = new UpdateRequest { Object = obj, Context = context };
                var response = proxy.Update(request);
                if (cf != null) cf.Close();
                return ReturnResult(response);
            }
            catch
            {
                cf.Abort();
                throw;
            }
        }

        private static DataPortalResult ReturnResult(WcfResponse response)
        {
            object result = response.Result;
            if (result is Exception)
            {
                throw new DataPortalException("连接服务器时，发生异常，请检查 InnerException。", result as Exception);
            }
            return (DataPortalResult)result;
        }

        #endregion
    }
}