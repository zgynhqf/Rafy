using System;
using System.ServiceModel;
using OEA.Server.Hosts.WcfChannel;

namespace OEA.Server.Hosts
{
    /// <summary>
    /// Defines the service contract for the WCF data
    /// portal.
    /// </summary>
    [ServiceContract(Namespace = "http://ws.lhotka.net/WcfDataPortal")]
    public interface IWcfPortal
    {
        /// <summary>
        /// Get an existing business object.
        /// </summary>
        /// <param name="request">The request parameter object.</param>
        [OperationContract]
        [UseNetDataContract]
        WcfResponse Fetch(FetchRequest request);

        /// <summary>
        /// Update a business object.
        /// </summary>
        /// <param name="request">The request parameter object.</param>
        [OperationContract]
        [UseNetDataContract]
        WcfResponse Update(UpdateRequest request);
    }
}
