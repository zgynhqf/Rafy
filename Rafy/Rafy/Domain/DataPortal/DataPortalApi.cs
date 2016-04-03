/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：2012
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2012
 * 
*******************************************************/

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using Rafy;
using Rafy.Reflection;
using Rafy.Utils;

namespace Rafy.Domain.DataPortal
{
    /// <summary>
    /// 数据门户。
    /// 内部封装了对数据层的调用，如果是远程，则使用对应的代理来访问，这使得单机版、网络版的调用完全一致。
    /// </summary>
    internal static class DataPortalApi
    {
        /// <summary>
        /// 使用门户查询
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="criteria"></param>
        /// <param name="loc">如果一个数据层方法需要在本地执行，应该在把本参数指明为 Local。</param>
        /// <returns></returns>
        public static object Fetch(Type objectType, object criteria, DataPortalLocation loc = DataPortalLocation.Dynamic)
        {
            object res = null;

            ////只是不要纯客户端，都直接使用本地访问
            //if (loc == DataPortalLocation.Local || RafyEnvironment.Location.ConnectDataDirectly)
            //{
            //    res = FinalDataPortal.Fetch(objectType, criteria);
            //}
            //else
            //{
            var proxy = GetDataPortalProxy();

            var dpContext = CreateDataPortalContext();

            var result = proxy.Fetch(objectType, criteria, dpContext);

            res = ReadServerResult(result);
            //}

            return res;
        }

        /// <summary>
        /// Called by the business object's Save() method to
        /// insert, update or delete an object in the database.
        /// </summary>
        /// <param name="obj">A reference to the business object to be updated.</param>
        /// <param name="loc">The loc.</param>
        /// <returns>
        /// A reference to the updated business object.
        /// </returns>
        /// <remarks>
        /// Note that this method returns a reference to the updated business object.
        /// If the server-side DataPortal is running remotely, this will be a new and
        /// different object from the original, and all object references MUST be updated
        /// to use this new object.
        /// </remarks>
        public static object Update(object obj, DataPortalLocation loc = DataPortalLocation.Dynamic)
        {
            object res = null;

            //只是不要纯客户端，都直接使用本地访问
            if (loc == DataPortalLocation.Local || RafyEnvironment.Location.ConnectDataDirectly)
            {
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

                try
                {
                    RafyEnvironment.ThreadPortalCount++;

                    //ThreadPortalCount == 1 表示第一次进入数据门户
                    if (RafyEnvironment.Location.IsWPFUI && RafyEnvironment.Location.ConnectDataDirectly && RafyEnvironment.ThreadPortalCount == 1)
                    {
                        res = ObjectCloner.Clone(obj);
                    }
                    else
                    {
                        res = obj;
                    }

                    FinalDataPortal.Update(res);
                }
                finally
                {
                    RafyEnvironment.ThreadPortalCount--;
                }
            }
            else
            {
                var proxy = GetDataPortalProxy();

                var dpContext = CreateDataPortalContext();

                var result = proxy.Update(obj, dpContext);

                res = ReadServerResult(result);
            }

            return res;
        }

        #region Helpers

        private static Type _proxyType;

        private static IDataPortalProxy GetDataPortalProxy()
        {
            if (_proxyType == null)
            {
                _proxyType = Type.GetType(RafyEnvironment.Configuration.Section.DataPortalProxy, true, true);
            }
            return Activator.CreateInstance(_proxyType) as IDataPortalProxy;
        }

        private static object ReadServerResult(DataPortalResult result)
        {
            //同步服务端返回的统一的上下文，到本地的上下文对象中。
            DistributionContext.GlobalContextItem.Value = result.GlobalContext;

            return result.ReturnObject;
        }

        /// <summary>
        /// Creates the data portal context.
        /// </summary>
        /// <returns></returns>
        private static DataPortalContext CreateDataPortalContext()
        {
            var res = new DataPortalContext();

            res.Principal = RafyEnvironment.Principal;
            res.ClientCulture = Thread.CurrentThread.CurrentCulture.Name;
            res.ClientUICulture = Thread.CurrentThread.CurrentUICulture.Name;
            res.ClientContext = DistributionContext.ClientContextItem.Value;
            res.GlobalContext = DistributionContext.GlobalContextItem.Value;

            return res;
        }

        #endregion
    }
}