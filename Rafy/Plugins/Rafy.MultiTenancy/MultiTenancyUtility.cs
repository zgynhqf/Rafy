/*******************************************************
 * 
 * 作者：王国超
 * 创建日期：20180126
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 王国超 20180126 16:22
 * 
*******************************************************/

using Rafy.MultiTenancy.Exception;
using Rafy.MultiTenancy.ShardMap;
using System.Collections.Generic;
using System.Linq;

namespace Rafy.MultiTenancy
{
    /// <summary>
    /// 多租户工具类
    /// </summary>
    public class MultiTenancyUtility
    {
        /// <summary>
        /// 获取当前租户ID
        /// </summary>
        /// <returns></returns>
        public static long GetTenantId()
        {
            return long.Parse(TenantContext.TenantId.Value ?? "0");
        }

        /// <summary>
        /// 获取当前租户数据库配置信息
        /// </summary>
        /// <returns></returns>
        public static string GetDbSettingName()
        {
            return GetDbSettingName(GetTenantId());
        }

        /// <summary>
        /// 获取当前租户数据库配置信息
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <returns></returns>
        public static string GetDbSettingName(long tenantId)
        {
            if (tenantId <= 0) return string.Empty;
            var info = GetShardMapInfo(tenantId);
            if (info == null) return string.Empty;
            return info.DbSettingName;
        }

        /// <summary>
        /// 获取当前租户数据库配置信息
        /// </summary>
        /// <returns></returns>
        public static List<string> GetDbSettingNames()
        {
            var shardMaps = GetShardMapInfos();
            return shardMaps.Select(t => t.DbSettingName).ToList();
        }

        /// <summary>
        /// 获取租户ID对象数据库映射信息
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <returns></returns>
        private static ShardMapInfo GetShardMapInfo(long tenantId)
        {
            var shardMaps = GetShardMapInfos();
            var item = shardMaps.FirstOrDefault(t => t.TenantIdRange[0, 0] <= tenantId && tenantId <= t.TenantIdRange[0, 1]);
            if (item == null) throw new MultiTenancyShardMapUnfoundException();
            return item;
        }

        /// <summary>
        /// 获取租户ID对象数据库映射信息
        /// </summary>
        /// <returns></returns>
        private static List<ShardMapInfo> GetShardMapInfos()
        {
            var shardMapList = ShardMapConfigManager.ShardMapList;
            if (shardMapList == null) ShardMapConfigManager.Initialize();
            return ShardMapConfigManager.ShardMapList;
        }
    }
}
