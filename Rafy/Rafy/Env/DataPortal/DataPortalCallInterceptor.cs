/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20211114
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20211114 23:01
 * 
*******************************************************/

using Castle.DynamicProxy;
using Rafy.DataPortal;
using Rafy.Reflection;
using Rafy.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy.DataPortal
{
    /// <summary>
    /// 内部封装了对数据层的调用，如果是远程，则使用对应的代理来访问，这使得单机版、网络版的调用完全一致。详见：<see cref="DataPortalCallInterceptor"/>。
    /// </summary>
    internal class DataPortalCallInterceptor : IInterceptor
    {
        public static readonly DataPortalCallInterceptor Instance = new DataPortalCallInterceptor();

        private DataPortalCallInterceptor() { }

        public void Intercept(IInvocation invocation)
        {
            var context = new DataPortalCallContext
            {
                Method = invocation.Method,
                Arguments = invocation.Arguments
            };

            var target = invocation.InvocationTarget as IDataPortalTarget;
            //var callable = target as IDataPortalCallable ?? EmptyPortalCallable.Instance;

            try
            {
                //如果目标确定了当前使用的是本地调用，或者当前环境只支持本地调用；那么都只使用本地调用，否则使用远程调用。
                var isLocal = target.DataPortalLocation == DataPortalLocation.Local 
                    || DataPortalApi.ConnectDataDirectly
                    || DataPortalApi.IsRunning;
                context.CallType = isLocal ? PortalCallType.Local : PortalCallType.Remote;

                //如果 OnRemoteCalling 已经得出结果，那么就结束整个调用了。
                target.OnPortalCalling(context);
                if (context.Result != null)
                {
                    invocation.ReturnValue = context.Result;
                    return;
                }

                if (isLocal)
                {
                    this.CallOnLocal(invocation);
                }
                else
                {
                    this.CallOnRemote(invocation, target);
                }

                context.Result = invocation.ReturnValue;
            }
            catch (Exception ex)
            {
                context.Result = ex;
                throw;
            }
            finally
            {
                target.OnPortalCalled(context);
            }
        }

        private void CallOnRemote(IInvocation invocation, IDataPortalTarget target)
        {
            var factoryInfo = target.TryUseFactory();

            //调用数据门户，使得在服务端才执行真正的数据层方法。
            invocation.ReturnValue = DataPortalApi.Call(factoryInfo as object ?? target, invocation.Method.Name, invocation.Arguments);
        }

        private void CallOnLocal(IInvocation invocation)
        {
            invocation.Proceed();
        }
    }
}
