#if NET45

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

        public DataPortalResult Call(object obj, string method, object[] arguments, DataPortalContext context)
        {
            var request = new CallRequest
            {
                Instance = obj,
                Method = method,
                Arguments = arguments,
                Context = context,
            };

            var cf = GetChannelFactory();
            var proxy = cf.CreateChannel();
            try
            {
                var response = proxy.Call(request);

                cf.Close();

                object result = response.Result;
                if (result is Exception)
                {
                    throw new DataPortalException("连接服务器时，发生异常，请检查 InnerException。", result as Exception);
                }
                return (DataPortalResult)result;
            }
            catch
            {
                cf.Abort();
                throw;
            }
        }
    }
}

#endif