/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120412
 * 说明：列表托管属性需要的元数据。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120412
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.ManagedProperty;
using OEA.MetaModel;
using System.ComponentModel;

namespace OEA.Library
{
    /// <summary>
    /// 引用属性元数据
    /// </summary>
    public interface IListPropertyMetadata : IOEAListPropertyMetadata
    {
        /// <summary>
        /// 自定义列表数据提供器
        /// </summary>
        ListLoaderProvider DataProvider { get; }
    }

    /// <summary>
    /// 泛型版本的引用属性元数据
    /// </summary>
    /// <typeparam name="TPropertyType"></typeparam>
    public class ListPropertyMetadata<TEntityList> : PropertyMetadata<TEntityList>, IListPropertyMetadata, IOEAListPropertyMetadata
        where TEntityList : EntityList
    {
        public ListPropertyMetadata(ListPropertyMeta core)
        {
            this.Core = core;
        }

        internal ListPropertyMeta Core { get; private set; }

        /// <summary>
        /// 自定义列表数据提供器
        /// </summary>
        public ListLoaderProvider DataProvider
        {
            get { return this.Core.DataProvider; }
        }

        /// <summary>
        /// 一对多子属性的类型
        /// </summary>
        public HasManyType HasManyType
        {
            get { return this.Core.HasManyType; }
        }
    }

    /// <summary>
    /// 非泛型的外键元数据
    /// </summary>
    public class ListPropertyMeta
    {
        public ListPropertyMeta()
        {
            this.HasManyType = HasManyType.Composition;
        }

        /// <summary>
        /// 自定义列表数据提供器
        /// </summary>
        public ListLoaderProvider DataProvider { get; set; }

        /// <summary>
        /// 一对多子属性的类型
        /// </summary>
        public HasManyType HasManyType { get; set; }
    }

    /// <summary>
    /// 列表数据提供程序
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate EntityList ListLoaderProvider(Entity owner);
}