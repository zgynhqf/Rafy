using System;
using OEA.Server.Hosts.WcfChannel;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace OEA.Server.Hosts
{
    /// <summary>
    /// 使用 WCF 实现的统一的数据门户。
    /// 
    /// 标记了 ConcurrencyMode.Multiple 来表示多线程进行
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
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