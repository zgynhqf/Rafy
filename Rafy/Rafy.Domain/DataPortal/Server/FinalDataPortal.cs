/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：2011
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2011
 * 
*******************************************************/

using System;
using Rafy.Reflection;
using Rafy.Domain;
using Rafy;
using System.Diagnostics;

namespace Rafy.Domain.DataPortal
{
    /// <summary>
    /// 最终调用实体的 IDataPortalServer 门户实现。
    /// </summary>
    internal class FinalDataPortal : IDataPortalServer
    {
        /// <summary>
        /// 当前查询正在使用的单一条件。
        /// </summary>
        [ThreadStatic]
        internal static object CurrentCriteria;

        /// <summary>
        /// Get an existing business object.
        /// </summary>
        /// <param name="objectType">Type of business object to retrieve.</param>
        /// <param name="criteria">Criteria object describing business object.</param>
        /// <param name="context"><see cref="DataPortalContext" /> object passed to the server.</param>
        /// <returns></returns>
        public DataPortalResult Fetch(Type objectType, object criteria, DataPortalContext context)
        {
            var obj = Fetch(objectType, criteria);

            // return the populated business object as a result
            return new DataPortalResult(obj);
        }

        /// <summary>
        /// Update a business object.
        /// </summary>
        /// <param name="obj">Business object to update.</param>
        /// <param name="context">
        /// <see cref="DataPortalContext" /> object passed to the server.
        /// </param>
        public DataPortalResult Update(object obj, DataPortalContext context)
        {
            Update(obj);

            return new DataPortalResult(obj);
        }

        /// <summary>
        /// 非仓库类在使用 Fetch 时的回调方法名。
        /// </summary>
        private const string FetchMethod = "PortalFetch";

        /// <summary>
        /// 调用某个类型的查询方法以返回它的数据。
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        internal static object Fetch(Type objectType, object criteria)
        {
            object res = null;

            //为了防止在查询内部再次发起查询，需要存储旧的值。
            object oldValue = CurrentCriteria;

            try
            {
                CurrentCriteria = criteria;

                if (objectType.IsSubclassOf(typeof(EntityRepository)))
                {
                    var instance = RepositoryFactoryHost.Factory.Find(objectType) as EntityRepository;
                    //如果是实体查询，则应该用其中的 Parameters 数组来查找对应的数据层方法。
                    res = instance.PortalFetch(criteria as IEQC);
                }
                else
                {
                    //创建一个对象。
                    var instance = Activator.CreateInstance(objectType, true);
                    MethodCaller.CallMethodIfImplemented(instance, FetchMethod, new object[] { criteria }, out res);
                }
            }
            finally
            {
                CurrentCriteria = oldValue;
            }

            //直接返回该方法的返回值。
            return res;
        }

        /// <summary>
        /// 直接更新某个对象
        /// </summary>
        /// <param name="obj"></param>
        internal static void Update(object obj)
        {
            // tell the business object to update itself
            var component = obj as IDomainComponent;
            if (component != null)
            {
                var repo = component.GetRepository() as EntityRepository;
                repo.DataProvider.SubmitComposition(component);
            }
            else if (obj is Service)
            {
                (obj as Service).ExecuteByDataPortal();
            }
            else
            {
                throw new InvalidProgramException("目前只支持 Entity、EntityList、Service 三类对象的保存。");
            }
            //public interface IDataPortalUpdatable
            //{
            //    void DataPortal_Update();
            //}
            //var updatable = obj as IDataPortalUpdatable;
            //if (updatable != null)
            //{
            //    updatable.DataPortal_Update();
            //}
            //else
            //{
            //    throw new InvalidProgramException(string.Format("{0} 对象需要实体接口：IDataPortalUpdatable。", obj.GetType()));
            //}
        }
    }
}