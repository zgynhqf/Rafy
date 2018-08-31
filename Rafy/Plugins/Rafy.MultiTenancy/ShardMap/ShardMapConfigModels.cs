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

using System.Collections.Generic;

namespace Rafy.MultiTenancy.ShardMap
{
    /// <summary>
    /// 分片映射模型类
    /// </summary>
    public class ShardMapConfigModels
    {
        public List<MultiTenancyDomain> Domains { get; set; }
    }

    /// <summary>
    /// 领域模型类
    /// </summary>
    public class MultiTenancyDomain
    {
        public string Name { get; set; }
        public string Rule { get; set; }
        public List<MultiTenancyDataNode> DataNodes { get; set; }
    }

    /// <summary>
    /// 数据节模型类
    /// </summary>
    public class MultiTenancyDataNode
    {
        public string DbSettingName { get; set; }
        public string IdRightBound { get; set; }
        public int Sort { get; set; }
        public long[,] IdRange { get; set; }
    }

    /// <summary>
    /// 映射信息模型类
    /// </summary>
    public class ShardMapInfo
    {
        public string DbSettingName { get; set; }
        public string DomainName { get; set; }
        public long[,] TenantIdRange { get; set; }
    }
}
