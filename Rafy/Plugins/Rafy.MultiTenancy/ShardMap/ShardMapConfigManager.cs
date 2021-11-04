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

using Microsoft.Extensions.Configuration;
using Rafy.MultiTenancy.Exception;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Rafy.MultiTenancy.ShardMap
{
    /// <summary>
    /// 多租户配置管理类
    /// </summary>
    public class ShardMapConfigManager
    {
        private const string ConfigKeyNet45 = "multiTenancyConfigGroup/multiTenancyConfig";
        private const string ConfigKeyNotCore = "multiTenancyConfig";

        private static List<ShardMapInfo> _shardMapList = null;
        /// <summary>
        /// 映射关系列表
        /// </summary>
        public static List<ShardMapInfo> ShardMapList { get { return _shardMapList; } }

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Initialize()
        {
            var shardMap = new ShardMapConfigModels();
            shardMap.Domains = new List<MultiTenancyDomain>();

            var rafyRawSection = ConfigurationHelper.Configuration.GetSection(ConfigKeyNotCore);

            if (rafyRawSection == null)
            {
                throw new MultiTenancyConfigException();
            }

            var multiTenancyConfig = new MultiTenancyConfig();
            rafyRawSection.Bind(multiTenancyConfig);

            foreach (var domain in multiTenancyConfig.Domains)
            {
                var newdomain = new MultiTenancyDomain
                {
                    Name = domain.Name,
                    Rule = domain.Rule
                };

                if (domain.DataNodes == null) throw new MultiTenancyConfigException();

                newdomain.DataNodes = new List<MultiTenancyDataNode>();

                foreach (var dn in domain.DataNodes)
                {
                    newdomain.DataNodes.Add(new MultiTenancyDataNode
                    {
                        Sort = dn.Sort,
                        IdRightBound = dn.IdRightBound,
                        DbSettingName = dn.DbSettingName
                    });
                }

                shardMap.Domains.Add(newdomain);
            }

            ValidateAndSetDataNode(shardMap);
            InitShardMapList(shardMap);
        }

        /// <summary>
        /// 验证配置数据节点
        /// </summary>
        /// <param name="shardMap"></param>
        private static void ValidateAndSetDataNode(ShardMapConfigModels shardMap)
        {
            if (shardMap.Domains.Count == 0) throw new MultiTenancyConfigException();

            foreach (var domain in shardMap.Domains)
            {
                var idrangelist = new List<long[,]>();

                if (domain.DataNodes.Count < 1) throw new MultiTenancyConfigException();

                var datanodes = domain.DataNodes.OrderBy(t => t.Sort).ToList();

                if (domain.DataNodes.Count == 1 && string.IsNullOrEmpty(datanodes.Last().IdRightBound))
                {
                    datanodes[0].IdRange = new long[,] { { 1, long.MaxValue } };
                }
                else
                {
                    if (!string.IsNullOrEmpty(datanodes.Last().IdRightBound)) throw new MultiTenancyConfigException();

                    var lastidmax = 0L;

                    for (var i = 0; i < datanodes.Count; i++)
                    {
                        var idRightBound = datanodes[i].IdRightBound;

                        if (i < datanodes.Count - 1 && string.IsNullOrEmpty(idRightBound)) throw new MultiTenancyConfigException();

                        var idmax = 0L;
                        var idmin = 0L;

                        if (!string.IsNullOrEmpty(idRightBound))
                        {
                            if (!long.TryParse(idRightBound, out idmax)) throw new MultiTenancyConfigException();
                        }
                        else
                        {
                            idmax = long.MaxValue;
                        }

                        if (i == 0)
                        {
                            idmin = 1;
                        }
                        else
                        {
                            idmin = lastidmax + 1;
                        }

                        lastidmax = idmax;

                        var idRange = new long[,] { { idmin, idmax } };

                        idrangelist.Add(idRange);

                        datanodes[i].IdRange = idRange;
                    }

                    for (var i = 0; i < idrangelist.Count; i++)
                    {
                        for (var j = i + 1; j < idrangelist.Count; j++)
                        {
                            var first = idrangelist[i];
                            var second = idrangelist[j];

                            if (first[0, 0] >= first[0, 1]) throw new MultiTenancyConfigException();
                            if (second[0, 0] >= second[0, 1]) throw new MultiTenancyConfigException();
                            if (first[0, 1] >= second[0, 0]) throw new MultiTenancyConfigException();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 验证配置数据节点
        /// </summary>
        /// <param name="shardMap"></param>
        private static void InitShardMapList(ShardMapConfigModels shardMap)
        {
            _shardMapList = new List<ShardMapInfo>();

            foreach (var domain in shardMap.Domains)
            {
                foreach (var dataNode in domain.DataNodes)
                {
                    var shardMapInfo = new ShardMapInfo
                    {
                        DomainName = domain.Name,
                        DbSettingName = dataNode.DbSettingName,
                        TenantIdRange = dataNode.IdRange
                    };
                    _shardMapList.Add(shardMapInfo);
                }
            }
        }
    }
    
    /// <summary>
    /// 配置类
    /// </summary>
    public class MultiTenancyConfig
    {
        public MultiTenancyConfigDomain[] Domains { get; set; }
    }

    /// <summary>
    /// 配置域模型
    /// </summary>
    public class MultiTenancyConfigDomain
    {
        public string Name { get; set; }
        public string Rule { get; set; }
        public MultiTenancyConfigDomainNode[] DataNodes { get; set; }
    }

    /// <summary>
    /// 配置域节点模型
    /// </summary>
    public class MultiTenancyConfigDomainNode
    {
        public string DbSettingName { get; set; }
        public string IdRightBound { get; set; }
        public int Sort { get; set; }
    }
}