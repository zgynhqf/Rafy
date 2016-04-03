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
    /// 加上这个标记以后，将会使用 NetDataContractSerializer 来替换原来的 DataContractSerializer
    /// 来对数据进行序列化与反序列化。
    /// 二者的区别在于：前者会将类型的 AssemblyQualifiedName 输出到 Xml 中。这使得接口 IWcfPortal 可以为所有类型进行通用的序列化。
    /// 反之，如果不使用 DataContractSerializer 进行序列化，那么必须使用 KnownTypes 标记在接口上标记它可序列化的所有类型，使得 IWcfPortal 将不能作为一个通用的接口程序。
    /// 
    /// 但是，使用了这个标记之后，也会带来相应的不利的影响：由于使用 .NET 内部的序列化规范，这个接口将不再满足 WSDL 的规范，使得其它平台无法调用此接口，也导致了无法为 .NET 的项目直接使用服务的地址来添加服务的引用。
    /// 
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