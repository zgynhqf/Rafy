using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Utils.Caching;
using OEA.MetaModel;
using OEA.Utils;
using OEA.Serialization;

namespace OEA.Library.Caching
{
    public static class CacheInstance
    {
        private static readonly string CACHE_FILE_NAME = "OEA_Entity_Cache.sdf";

        /// <summary>
        /// 内存缓存
        /// </summary>
        public static Cache Memory;

        /// <summary>
        /// 使用 SqlCe 的硬盘缓存
        /// </summary>
        public static Cache SqlCe;

        /// <summary>
        /// 一个先用内存，后用硬盘的二级缓存
        /// </summary>
        public static Cache MemoryDisk;

        static CacheInstance()
        {
            Memory = new Cache(new MemoryCacheProvider());

            string dbFileName = OEAEnvironment.ToAbsolute(CACHE_FILE_NAME);
            SetCacheFile(dbFileName);
        }

        /// <summary>
        /// 使用指定的数据库文件。
        /// （目前主要为测试工程提供此方法。）
        /// </summary>
        /// <param name="path"></param>
        public static void SetCacheFile(string path)
        {
            var service = new HybirdCacheProviderService();

            SqlCe = new Cache(new SQLCompactProvider(path, service));
            MemoryDisk = new Cache(new HybirdCacheProvider(path, service));
        }

        private class HybirdCacheProviderService : IHybirdCacheProviderService
        {
            public void AsyncInvokeSave(Action action)
            {
                Threading.ThreadHelper.SafeInvoke(action);
            }

            public byte[] Serialize(object value)
            {
                return Serializer.Serialize(value);
            }

            public object Deserialize(byte[] data)
            {
                return Serializer.Deserialize(data);
            }
        }
    }
}
