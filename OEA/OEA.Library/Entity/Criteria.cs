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
using System.Text;

namespace OEA.Library
{
    /// <summary>
    /// 所有查询对象的基类
    /// </summary>
    [Serializable]
    public abstract class Criteria : Entity
    {
        //public Criteria()
        //{
        //    this.NotifyLoaded(null);
        //}

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
    }
}
