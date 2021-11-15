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
using Rafy.Domain.DataPortal;
using Rafy.Reflection;
using Rafy.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy.Domain.DataPortal
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
                var isLocal = target.DataPortalLocation == DataPortalLocation.Local || RafyEnvironment.ConnectDataDirectly;
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
            try
            {
                RafyEnvironment.ThreadPortalCount++;
                /*********************** 代码块解释 *********************************
                 * 
                 * 由于开发人员平时会使用单机版本开发，而正式部署时，又会选用 C/S 架构。
                 * 所以需要保证单机版本和 C/S 架构版本的模式是一样的。也就是说，在单机模式下，
                 * 在通过门户访问时，模拟网络版，clone 出一个新的对象。
                 * 这样，在底层 Update 更改 obj 时，不会影响上层的实体。
                 * 而是以返回值的形式把这个被修改的实体返回给上层。
                 * 
                 * 20120828 
                 * 但是，当在服务端本地调用时，不需要此模拟功能。
                 * 这是因为在服务端本地调用时（例如服务端本地调用 RF.Save），
                 * 在开发体验上，数据层和上层使用的实体应该是同一个，数据层的修改应该能够带回到上层，不需要克隆。
                 * 
                **********************************************************************/

                //ThreadPortalCount == 1 表示第一次进入数据门户
                var arguments = invocation.Arguments;
                if (DataPortalApi.FakeRemote && arguments.Length > 0 &&
                    RafyEnvironment.ConnectDataDirectly && RafyEnvironment.ThreadPortalCount == 1)
                {
                    for (int i = 0, c = arguments.Length; i < c; i++)
                    {
                        var item = arguments[i];
                        if (item != null && item.GetType().IsClass && !(item is string))
                        {
                            item = BinarySerializer.Clone(item);
                            arguments[i] = item;
                        }
                    }

                    if (MethodCaller.CallMethodIfImplemented(
                        invocation.InvocationTarget, invocation.Method.Name, arguments, out object result
                        ))
                    {
                        invocation.ReturnValue = result;
                        return;
                    }
                }

                invocation.Proceed();
            }
            finally
            {
                RafyEnvironment.ThreadPortalCount--;
            }
        }
    }
}
