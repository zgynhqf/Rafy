/*******************************************************
 * 
 * 作者：CSLA
 * 创建日期：2008
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 周金根 2008
 * 2.0.0.0 直接让行为起作用，否则优先级的效果会不对。 胡庆访 20130609 17:18
 * 
*******************************************************/

using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Rafy.WCF
{
    /// <summary>
    /// Specify that WCF should serialize objects in a .NET
    /// specific manner to as to preserve complex object
    /// references and to be able to deserialize the graph
    /// into the same type as the original objets.
    /// </summary>
    public class UseNetDataContractAttribute : Attribute, IOperationBehavior
    {
        /// <summary>
        /// Apply the client behavior by requiring
        /// the use of the NetDataContractSerializer.
        /// </summary>
        /// <param name="description">Operation description.</param>
        /// <param name="proxy">Client operation object.</param>
        public void ApplyClientBehavior(OperationDescription description, ClientOperation proxy)
        {
            description.Behaviors.Remove<DataContractSerializerOperationBehavior>();

            //直接让行为起作用，否则优先级的效果会不对。
            IOperationBehavior ndcob = new NetDataContractOperationBehavior(description);
            ndcob.ApplyClientBehavior(description, proxy);
            //description.Behaviors.Add(new NetDataContractOperationBehavior(description));
        }

        /// <summary>
        /// Apply the dispatch behavior by requiring
        /// the use of the NetDataContractSerializer.
        /// </summary>
        /// <param name="description">Operation description.</param>
        /// <param name="dispatch">Dispatch operation object.</param>
        public void ApplyDispatchBehavior(OperationDescription description, DispatchOperation dispatch)
        {
            description.Behaviors.Remove<DataContractSerializerOperationBehavior>();

            IOperationBehavior ndcob = new NetDataContractOperationBehavior(description);
            ndcob.ApplyDispatchBehavior(description, dispatch);
        }

        void IOperationBehavior.AddBindingParameters(OperationDescription description, BindingParameterCollection parameters) { }

        void IOperationBehavior.Validate(OperationDescription description) { }
    }
}