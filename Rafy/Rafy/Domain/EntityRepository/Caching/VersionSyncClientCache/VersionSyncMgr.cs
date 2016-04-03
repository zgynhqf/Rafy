/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130319 14:41
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130319 14:41
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain.Caching
{
    /// <summary>
    /// 基于版本号同步方案的缓存 API
    /// </summary>
    public static class VersionSyncMgr
    {
        /// <summary>
        /// 是否启用了缓存同步方案。
        /// </summary>
        public static bool IsEnabled
        {
            get { return Repository != null; }
        }

        /// <summary>
        /// 版本号同步方案的服务端提供程序。
        /// 
        /// 如果没有使用 SetProvider 来设置服务端提供程序，则基于版本号同步方案的缓存则不起作用。
        /// </summary>
        public static IEntityListVersionRepository Repository { get; private set; }

        /// <summary>
        /// 设置版本号同步方案的服务端提供程序。
        /// </summary>
        /// <param name="provider"></param>
        public static void SetProvider(IEntityListVersionRepository provider)
        {
            Repository = provider;
        }

        internal static IDisposable BatchSaveScope()
        {
            if (IsEnabled)
            {
                return Repository.BatchSaveScope();
            }

            return EmptyDisposiable.Instance;
        }

        private class EmptyDisposiable : IDisposable
        {
            public static readonly EmptyDisposiable Instance = new EmptyDisposiable();

            private EmptyDisposiable() { }

            public void Dispose() { }
        }
    }
}
