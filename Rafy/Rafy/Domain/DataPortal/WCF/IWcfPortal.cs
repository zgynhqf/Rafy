/*******************************************************
 * 
 * 作者：CSLA
 * 创建日期：2008
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 周金根 2008
 * 
*******************************************************/

using System;
using System.ServiceModel;
using Rafy.WCF;

namespace Rafy.Domain.DataPortal.WCF
{
    /// <summary>
    /// Defines the service contract for the WCF data
    /// portal.
    /// </summary>
    [ServiceContract]
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

        [OperationContract]
        string Test(string msg);
    }
}
