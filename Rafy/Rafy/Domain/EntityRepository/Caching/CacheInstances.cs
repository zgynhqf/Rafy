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
using Rafy.Utils.Caching;

namespace Rafy.Domain.Caching
{
    /// <summary>
    /// 缓存的几个默认实例。
    /// 
    /// 应用层可修改这些属性来实现自己的缓存逻辑。
    /// </summary>
    public static class CacheInstances
    {
        /// <summary>
        /// 默认使用的硬盘 sqlce 缓存文件。
        /// </summary>
        public static readonly string CACHE_FILE_NAME = "Rafy_Disk_Cache.sdf";

        private static ICache _memory;
        private static ICache _perHttpRequest;
        private static ICache _disk;
        private static ICache _memoryDisk;

        /// <summary>
        /// 内存缓存
        /// </summary>
        public static ICache Memory => _memory ?? (_memory = new MemoryCache());

        /// <summary>
        /// 硬盘缓存。
        /// 默认使用 SqlCe 的硬盘缓存
        /// </summary>
        public static ICache Disk
        {
            get
            {
                //由于有时并不会使用硬盘缓存，所以这个属性需要使用懒加载。
                if (_disk == null)
                {
                    var dbFileName = RafyEnvironment.MapAbsolutePath(CACHE_FILE_NAME);
                    _disk = new SQLCompactCache(dbFileName);
                }
                return _disk;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                _disk = value;
            }
        }

        /// <summary>
        /// 一个先用内存，后用硬盘的二级缓存
        /// 默认使用 SqlCe 作为二级缓存的硬盘缓存
        /// </summary>
        public static ICache MemoryDisk
        {
            get
            {
                //由于有时并不会使用硬盘缓存，所以这个属性需要使用懒加载。
                if (_memoryDisk == null)
                {
                    var dbFileName = RafyEnvironment.MapAbsolutePath(CACHE_FILE_NAME);
                    _memoryDisk = new HybirdCache(dbFileName);
                }
                return _memoryDisk;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));

                _memoryDisk = value;
            }
        }

        /// <summary>
        /// 获取一个当前 Http 请求的缓存提供者实例。
        /// </summary>
        public static ICache PerHttpRequest => _perHttpRequest ?? (_perHttpRequest = new PerHttpRequestCache());
    }
}
