/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130609
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130609 14:45
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
    /// <summary>
    /// 在原始 Formatter 的基础上装饰 BinaryFormatterAdapter
    /// <remarks>
    /// BinaryFormatterOperationBehavior 为什么要实现为操作的行为：
    /// 因为只有当操作的 DataContractSerializerBehavior 行为应用功能后，才能拿到 DataContractSerializerFormatter 并包装到 BinaryFormatterAdapter 中。
    /// 
    /// 由于一个操作的操作契约在系统中只有一份。而我们期望序列化的行为只影响指定的终结点，所以这个行为在应用时，会检查是否传入的运行时，即是添加时的运行时。
    /// </remarks>
    /// </summary>
    internal class BinaryFormatterOperationBehavior : IOperationBehavior
    {
        private object _runtime;

        internal BinaryFormatterOperationBehavior(object runtime)
        {
            _runtime = runtime;
        }

        /// <summary>
        /// 本行为只为这个运行时起作用。
        /// </summary>
        public object ParentRuntime
        {
            get { return _runtime; }
        }

        public void ApplyClientBehavior(OperationDescription description, ClientOperation runtime)
        {
            if (_runtime == runtime.Parent)
            {
                //在之前的创建的 Formatter 的基础上，装饰新的 Formatter
                runtime.Formatter = new BinaryFormatterAdapter(description.Name, runtime.SyncMethod.GetParameters(), runtime.Formatter, runtime.Action);
            }
        }

        public void ApplyDispatchBehavior(OperationDescription description, DispatchOperation runtime)
        {
            if (_runtime == runtime.Parent)
            {
                runtime.Formatter = new BinaryFormatterAdapter(description.Name, description.SyncMethod.GetParameters(), runtime.Formatter);
            }
        }

        public void AddBindingParameters(OperationDescription description, BindingParameterCollection parameters) { }

        public void Validate(OperationDescription description) { }
    }
}