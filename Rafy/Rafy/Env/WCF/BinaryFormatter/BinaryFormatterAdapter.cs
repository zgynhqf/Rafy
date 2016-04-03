/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130609
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130609 14:48
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using Rafy.Serialization;

namespace Rafy.WCF
{
    /// <summary>
    /// 在内部序列化器的基础上添加 Remoting 二进制序列化的功能。
    /// </summary>
    internal class BinaryFormatterAdapter : IClientMessageFormatter, IDispatchMessageFormatter
    {
        private IClientMessageFormatter _innerClientFormatter;
        private IDispatchMessageFormatter _innerDispatchFormatter;
        private ParameterInfo[] _parameterInfos;
        private string _operationName;
        private string _action;

        /// <summary>
        /// for client
        /// </summary>
        /// <param name="operationName"></param>
        /// <param name="parameterInfos"></param>
        /// <param name="innerClientFormatter"></param>
        /// <param name="action"></param>
        public BinaryFormatterAdapter(
            string operationName,
            ParameterInfo[] parameterInfos,
            IClientMessageFormatter innerClientFormatter,
            string action
            )
        {
            if (operationName == null) throw new ArgumentNullException("methodName");
            if (parameterInfos == null) throw new ArgumentNullException("parameterInfos");
            if (innerClientFormatter == null) throw new ArgumentNullException("innerClientFormatter");
            if (action == null) throw new ArgumentNullException("action");

            this._innerClientFormatter = innerClientFormatter;
            this._parameterInfos = parameterInfos;
            this._operationName = operationName;
            this._action = action;
        }

        /// <summary>
        /// for server
        /// </summary>
        /// <param name="operationName"></param>
        /// <param name="parameterInfos"></param>
        /// <param name="innerDispatchFormatter"></param>
        public BinaryFormatterAdapter(
            string operationName,
            ParameterInfo[] parameterInfos,
            IDispatchMessageFormatter innerDispatchFormatter
            )
        {
            if (operationName == null) throw new ArgumentNullException("operationName");
            if (parameterInfos == null) throw new ArgumentNullException("parameterInfos");
            if (innerDispatchFormatter == null) throw new ArgumentNullException("innerDispatchFormatter");

            this._innerDispatchFormatter = innerDispatchFormatter;
            this._operationName = operationName;
            this._parameterInfos = parameterInfos;
        }

        Message IClientMessageFormatter.SerializeRequest(MessageVersion messageVersion, object[] parameters)
        {
            var result = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++) { result[i] = Serializer.SerializeBytes(parameters[i]); }

            return _innerClientFormatter.SerializeRequest(messageVersion, result);
        }

        object IClientMessageFormatter.DeserializeReply(Message message, object[] parameters)
        {
            var result = _innerClientFormatter.DeserializeReply(message, parameters);

            for (int i = 0; i < parameters.Length; i++) { parameters[i] = Serializer.DeserializeBytes(parameters[i] as byte[]); }
            result = Serializer.DeserializeBytes(result as byte[]);

            return result;
        }

        void IDispatchMessageFormatter.DeserializeRequest(Message message, object[] parameters)
        {
            _innerDispatchFormatter.DeserializeRequest(message, parameters);

            for (int i = 0; i < parameters.Length; i++) { parameters[i] = Serializer.DeserializeBytes(parameters[i] as byte[]); }
        }

        Message IDispatchMessageFormatter.SerializeReply(MessageVersion messageVersion, object[] parameters, object result)
        {
            var seralizedParameters = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++) { seralizedParameters[i] = Serializer.SerializeBytes(parameters[i]); }
            var serialzedResult = Serializer.SerializeBytes(result);

            return _innerDispatchFormatter.SerializeReply(messageVersion, seralizedParameters, serialzedResult);
        }
    }
}