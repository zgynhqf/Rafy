/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110217
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100217
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Domain.ORM;
using Rafy.ManagedProperty;

namespace Rafy.Domain
{
    /// <summary>
    /// 查询对象的基类
    /// 
    /// 如果不使用这个类做基类的查询条件类，也可以在 WPF 下运行正常。但是无法在 Web 下运行。
    /// </summary>
    [Serializable]
    public abstract class Criteria : Entity, ILoadOptionsCriteria
    {
        private static IKeyProvider KeyProviderField = KeyProviders.Get(typeof(object));

        private PagingInfo _p;

        #region 构造函数

        public Criteria()
        {
            //    this.NotifyLoaded(null);
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected Criteria(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        /// <summary>
        /// 如果该条件类正在使用分页查询，则这个对象描述分页的信息。
        /// 
        /// 如果这个属性为 null 或者是<see cref="Rafy.PagingInfo.Empty"/>，表示不需要进行分页查询，直接返回整个结果集。
        /// </summary>
        public PagingInfo PagingInfo
        {
            get { return _p; }
            set { _p = value; }
        }

        /// <summary>
        /// 需要贪婪加载的属性列表。默认为 null 表示不进行贪婪加载。
        /// </summary>
        public EagerLoadOptions EagerLoad { get; set; }

        /// <summary>
        /// 此属性指示当前查询条件类型是否用于本地过滤。
        /// 
        /// 默认是 false。
        /// </summary>
        public virtual bool CanLocalFilter
        {
            get { return false; }
        }

        /// <summary>
        /// 如果本查询条件用于本地过滤查询时，子类需要实现此方法以指定过滤逻辑。
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual bool LocalFilter(Entity target)
        {
            throw new InvalidOperationException(string.Format(
                "当类型 {0} 用于本地过滤查询时，需要重写 Filter 方法以实现过滤逻辑。",
                this.GetType().FullName
                ));
        }

        protected override IKeyProvider IdProvider
        {
            get { return KeyProviderField; }
        }
    }

    /// <summary>
    /// 一个可以进行数据加载定义的查询条件
    /// </summary>
    public interface ILoadOptionsCriteria
    {
        /// <summary>
        /// 分页条件
        /// 
        /// 如果这个属性为 null 或者是<see cref="Rafy.PagingInfo.Empty"/>，表示不需要进行分页查询，直接返回整个结果集。
        /// </summary>
        PagingInfo PagingInfo { get; set; }

        /// <summary>
        /// 需要贪婪加载的属性列表。默认为 null 表示不进行贪婪加载。
        /// </summary>
        EagerLoadOptions EagerLoad { get; set; }
    }
}