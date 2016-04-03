/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140703
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140703 12:13
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Rafy.ManagedProperty;

namespace Rafy.Domain
{
    /// <summary>
    /// 贪婪加载选项。
    /// 其中的每一个项都是一个需要即时加载属性。
    /// </summary>
    [Serializable]
    public sealed class EagerLoadOptions
    {
        private List<ConcreteProperty> _eagerList = new List<ConcreteProperty>();

        internal bool LoadTreeChildren;

        internal List<ConcreteProperty> CoreList
        {
            get { return _eagerList; }
        }

        /// <summary>
        /// 贪婪加载树实体的子节点。
        /// 
        /// 如果设置了此选项，那么会先加载所有的树子节点，然后再加载其它的贪婪属性。
        /// </summary>
        /// <returns></returns>
        public EagerLoadOptions LoadWithTreeChildren()
        {
            this.LoadTreeChildren = true;
            return this;
        }

        /// <summary>
        /// 加载某个指定的组合子属性。
        /// </summary>
        /// <param name="childrenProperty">组合子属性。</param>
        /// <returns></returns>
        public EagerLoadOptions LoadWith(IListProperty childrenProperty)
        {
            _eagerList.Add(new ConcreteProperty(childrenProperty));
            return this;
        }

        /// <summary>
        /// 加载某个指定的组合子属性。
        /// </summary>
        /// <param name="childrenProperty">组合子属性。</param>
        /// <param name="owner">该属性对应的具体类型。
        /// 这个具体的类型必须是属性的拥有类型或者它的子类型。如果传入 null，则默认为属性的拥有类型。</param>
        /// <returns></returns>
        public EagerLoadOptions LoadWith(IListProperty childrenProperty, Type owner)
        {
            _eagerList.Add(new ConcreteProperty(childrenProperty, owner));
            return this;
        }

        /// <summary>
        /// 加载某个指定的组合子属性。
        /// </summary>
        /// <param name="refProperty">引用实体属性。</param>
        /// <returns></returns>
        public EagerLoadOptions LoadWith(IRefEntityProperty refProperty)
        {
            _eagerList.Add(new ConcreteProperty(refProperty));
            return this;
        }

        /// <summary>
        /// 加载某个指定的组合子属性。
        /// </summary>
        /// <param name="refProperty">引用实体属性。</param>
        /// <param name="owner">该属性对应的具体类型。
        /// 这个具体的类型必须是属性的拥有类型或者它的子类型。如果传入 null，则默认为属性的拥有类型。</param>
        /// <returns></returns>
        public EagerLoadOptions LoadWith(IRefEntityProperty refProperty, Type owner)
        {
            _eagerList.Add(new ConcreteProperty(refProperty, owner));
            return this;
        }
    }
}