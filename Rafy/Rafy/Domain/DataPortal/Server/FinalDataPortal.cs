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
        internal static readonly AppContextItem<object> CurrentQueryCriteriaItem =
            new AppContextItem<object>("Rafy.Domain.DataPortal.FinalDataPortal.CurrentCriteria");

        /// <summary>
        /// 当前正在使用的查询参数
        /// </summary>
        internal static IEQC CurrentIEQC
        {
            get
            {
                var ieqc = CurrentQueryCriteriaItem.Value as IEQC;
                if (ieqc == null) throw new InvalidProgramException("实体查询时必须使用正确的格式，查询方法必须是虚方法，并添加 RepositoryQuery 标记，否则无法判断查询中的返回值。");
                return ieqc;
            }
        }

        /// <summary>
        /// Get an existing business object.
        /// </summary>
        /// <param name="objectType">Type of business object to retrieve.</param>
        /// <param name="criteria">Criteria object describing business object.</param>
        /// <param name="context"><see cref="DataPortalContext" /> object passed to the server.</param>
        /// <returns></returns>
        public DataPortalResult Fetch(Type objectType, object criteria, DataPortalContext context)
        {
            using (CurrentQueryCriteriaItem.UseScopeValue(criteria))
            {
                var instance = RepositoryFactoryHost.Factory.Find(objectType) as EntityRepository;

                //如果是实体查询，则应该用其中的 Parameters 数组来查找对应的数据层方法。
                var res = instance.PortalFetch(criteria as IEQC);

                // return the populated business object as a result
                return new DataPortalResult(res);
            }
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
        /// 直接更新某个对象
        /// </summary>
        /// <param name="obj"></param>
        internal static void Update(object obj)
        {
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
            //// tell the business object to update itself
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