using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;
using Rafy.ComponentModel;
using Rafy.Domain;
using Rafy.Domain.Caching;

namespace Rafy.Domain.Caching
{
    /// <summary>
    /// 本模块主要用于为 Rafy 的缓存系统服务端提供一个数据表存放缓存相关的信息。
    /// </summary>
    public class DiskCachingPlugin : DomainPlugin
    {
        public static int CacheExpiredSeconds = 30;

        public override void Initialize(IApp app)
        {
            VersionSyncMgr.SetProvider(new EntityListVersionRepository());

            //服务端在启动时，需要清空缓存信息库，这会导致所有客户端都重新下载最新的数据。
            if (RafyEnvironment.IsOnServer())
            {
                app.StartupCompleted += (o, e) =>
                {
                    if (VersionSyncMgr.IsEnabled)
                    {
                        try
                        {
                            VersionSyncMgr.Repository.Clear();
                        }
                        catch
                        {
                            //当数据库还没有生成时，这里会报错，这时应该忽略这种错误。
                        }
                    }
                };
            }
        }
    }
}