using System;
using OEA.Server.Hosts.WcfChannel;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace OEA.Server.Hosts
{
    /// <summary>
    /// Exposes server-side DataPortal functionality
    /// through WCF.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class WcfPortal : IWcfPortal
    {
        /// <summary>
        /// Get an existing business object.
        /// </summary>
        /// <param name="request">The request parameter object.</param>
        public WcfResponse Fetch(FetchRequest request)
        {
            OEA.Server.DataPortalFacade portal = new OEA.Server.DataPortalFacade();
            object result;
            try
            {
                result = portal.Fetch(request.ObjectType, request.Criteria, request.Context);
            }
            catch (Exception ex)
            {
                result = ex;
            }
            return new WcfResponse(result);
        }

        /// <summary>
        /// Update a business object.
        /// </summary>
        /// <param name="request">The request parameter object.</param>
        public WcfResponse Update(UpdateRequest request)
        {
            OEA.Server.DataPortalFacade portal = new OEA.Server.DataPortalFacade();
            object result;
            try
            {
                result = portal.Update(request.Object, request.Context);
            }
            catch (Exception ex)
            {
                result = ex;
            }
            return new WcfResponse(result);
        }
    }
}