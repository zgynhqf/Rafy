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
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using System.ComponentModel;

namespace Rafy.Domain
{
    /// <summary>
    /// 引用属性元数据
    /// </summary>
    public interface IListPropertyMetadata : IRafyListPropertyMetadata
    {
        /// <summary>
        /// 自定义列表数据提供器
        /// </summary>
        ListLoaderProvider DataProvider { get; }
    }

    /// <summary>
    /// 泛型版本的引用属性元数据
    /// </summary>
    /// <typeparam name="TEntityList">The type of the entity list.</typeparam>
    public class ListPropertyMetadata<TEntityList> : PropertyMetadata<TEntityList>, IListPropertyMetadata, IRafyListPropertyMetadata
        where TEntityList : EntityList
    {
        private ListLoaderProvider _dataProvider;

        /// <summary>
        /// for merge
        /// </summary>
        internal ListPropertyMetadata() { }

        internal ListPropertyMetadata(ListLoaderProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        /// <summary>
        /// 自定义列表数据提供器
        /// </summary>
        public ListLoaderProvider DataProvider
        {
            get { return _dataProvider; }
        }
    }

    /// <summary>
    /// 列表属性注册参数
    /// </summary>
    /// 名称取为 Meta，主要是使用者可以更好地理解为元数据。
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
    /// <param name="owner">The owner.</param>
    /// <returns></returns>
    public delegate EntityList ListLoaderProvider(Entity owner);
}