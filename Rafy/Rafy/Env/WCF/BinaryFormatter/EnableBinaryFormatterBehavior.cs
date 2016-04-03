/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130609
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130609 14:23
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;

namespace Rafy.WCF
{
    class EnableBinaryFormatterBehavior : IEndpointBehavior
    {
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            foreach (var operation in endpoint.Contract.Operations)
            {
                DecorateFormatterBehavior(operation, clientRuntime);
            }
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            foreach (var operation in endpoint.Contract.Operations)
            {
                DecorateFormatterBehavior(operation, endpointDispatcher.DispatchRuntime);
            }
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }

        public void Validate(ServiceEndpoint endpoint) { }

        private static void DecorateFormatterBehavior(OperationDescription operation, object runtime)
        {
            //在配置文件中配置的终结点行为，排在行为列表中的后面。
            //IOperationBehavior formatterBehavior = operationDescription.Behaviors.Find<UseNetDataContractAttribute>();
            //if (formatterBehavior == null)
            //{
            //    // look for and remove the DataContract behavior if it is present
            //    formatterBehavior = operationDescription.Behaviors.Find<DataContractSerializerOperationBehavior>();
            //    if (formatterBehavior == null)
            //    {
            //        // look for and remove the XmlSerializer behavior if it is present
            //        formatterBehavior = operationDescription.Behaviors.Find<XmlSerializerOperationBehavior>();
            //        if (formatterBehavior == null)
            //        {
            //            throw new InvalidOperationException("Could not find DataContractFormatter or XmlSerializer on the contract");
            //        }
            //    }
            //}

            //这个行为附加一次。
            var dfBehavior = operation.Behaviors.Find<BinaryFormatterOperationBehavior>();
            if (dfBehavior == null)
            {
                //装饰新的操作行为
                //这个行为是操作的行为，但是我们期望只为当前终结点做操作的序列化，所以传入 runtime 进行区分。
                dfBehavior = new BinaryFormatterOperationBehavior(runtime);
                operation.Behaviors.Add(dfBehavior);
            }
        }
    }
}