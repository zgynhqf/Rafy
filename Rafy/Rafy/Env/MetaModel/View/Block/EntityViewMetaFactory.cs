/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110314
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100314
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rafy.MetaModel.Attributes;
using Rafy.Utils;
using Rafy.ManagedProperty;

using Rafy.MetaModel.XmlConfig;
using Rafy.MetaModel.XmlConfig.Web;

namespace Rafy.MetaModel.View
{
    /// <summary>
    /// 实体默认视图及实体信息的仓库
    /// </summary>
    public class EntityViewMetaFactory
    {
        private CodeEVMReader _codeReader = new CodeEVMReader();

        private XmlConfigManager _xmlCfgMgr;

        internal EntityViewMetaFactory(XmlConfigManager xmlConfigMgr)
        {
            this._xmlCfgMgr = xmlConfigMgr;
        }

        /// <summary>
        /// 获取某个类型的默认视图或扩展视图
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="extendViewName"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public EntityViewMeta Create(Type entityType, string extendViewName = null, BlockConfigType? destination = BlockConfigType.Customization)
        {
            if (string.IsNullOrEmpty(extendViewName))
            {
                return this.CreateBaseView(entityType, destination);
            }

            return this.CreateExtendView(entityType, extendViewName, destination);
        }

        /// <summary>
        /// 查询某个实体类型所对应的基础视图信息
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public EntityViewMeta CreateBaseView(Type entityType, BlockConfigType? destination = BlockConfigType.Customization)
        {
            var res = this.CreateBaseViewCore(entityType, destination);

            //res.Freeze();

            return res;
        }

        private EntityViewMeta CreateBaseViewCore(Type entityType, BlockConfigType? destination)
        {
            var meta = CommonModel.Entities.Get(entityType);

            var raw = this._codeReader.Read(meta);

            this.UseSysCommands(raw);

            //使用配置对象进行配置
            if (raw is WebEntityViewMeta)
            {
                foreach (var config in RafyEnvironment.WebConfigurations.FindViewConfigurations(entityType))
                {
                    lock (config)
                    {
                        config.View = raw as WebEntityViewMeta;
                        config.ConfigView();
                    }
                }
            }
            else
            {
                foreach (var config in RafyEnvironment.WPFConfigurations.FindViewConfigurations(entityType))
                {
                    lock (config)
                    {
                        config.View = raw as WPFEntityViewMeta;
                        config.ConfigView();
                    }
                }
            }

            if (destination != null)
            {
                var key = new BlockConfigKey
                {
                    EntityType = entityType,
                    Type = BlockConfigType.Config
                };

                var blockCfg = this._xmlCfgMgr.GetBlockConfig(key);
                if (blockCfg != null) { blockCfg.Config(raw); }
            }

            if (destination == BlockConfigType.Customization)
            {
                var key = new BlockConfigKey
                {
                    EntityType = entityType,
                    Type = BlockConfigType.Customization
                };

                var blockCfg = this._xmlCfgMgr.GetBlockConfig(key);
                if (blockCfg != null) { blockCfg.Config(raw); }
            }

            return raw;
        }

        private void UseSysCommands(EntityViewMeta evm)
        {
            //初始化实体视图中的命令按钮
            var em = evm.EntityMeta;
            if (!RafyEnvironment.Location.IsWebUI)
            {
                if (em.EntityCategory == EntityCategory.QueryObject)
                {
                    evm.AsWPFView().UseCommands(WPFCommandNames.SysQueryCommands);
                }
                else
                {
                    evm.AsWPFView().UseCommands(WPFCommandNames.SysCommands);
                }
            }
            else
            {
                if (em.EntityCategory == EntityCategory.QueryObject)
                {
                    evm.AsWebView().UseCommands(WebCommandNames.SysQueryCommands);
                }
                else
                {
                    evm.AsWebView().UseCommands(WebCommandNames.SysCommands);
                }
            }
        }

        /// <summary>
        /// 获取某个类型的扩展视图
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="extendViewName"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public EntityViewMeta CreateExtendView(Type entityType, string extendViewName, BlockConfigType? destination = BlockConfigType.Customization)
        {
            var raw = this.CreateBaseViewCore(entityType, destination);

            //使用扩展视图配置对象进行配置
            if (raw is WebEntityViewMeta)
            {
                foreach (var config in RafyEnvironment.WebConfigurations.FindViewConfigurations(entityType, extendViewName))
                {
                    lock (config)
                    {
                        config.View = raw as WebEntityViewMeta;
                        config.ConfigView();
                    }
                }
            }
            else
            {
                foreach (var config in RafyEnvironment.WPFConfigurations.FindViewConfigurations(entityType, extendViewName))
                {
                    lock (config)
                    {
                        config.View = raw as WPFEntityViewMeta;
                        config.ConfigView();
                    }
                }
            }

            if (destination != null)
            {
                //Config
                var key = new BlockConfigKey
                {
                    EntityType = entityType,
                    ExtendView = extendViewName,
                    Type = BlockConfigType.Config
                };

                var blockCfg = this._xmlCfgMgr.GetBlockConfig(key);
                if (blockCfg != null) { blockCfg.Config(raw); }

                //Customization
                if (destination == BlockConfigType.Customization)
                {
                    key = new BlockConfigKey
                    {
                        EntityType = entityType,
                        ExtendView = extendViewName,
                        Type = BlockConfigType.Customization
                    };

                    blockCfg = this._xmlCfgMgr.GetBlockConfig(key);
                    if (blockCfg != null) { blockCfg.Config(raw); }
                }
            }

            raw.ExtendView = extendViewName;

            //raw.Freeze();

            return raw;
        }

        //public EntityPropertyViewMeta CreateExtensionPropertyViewMeta(IManagedProperty mp, EntityViewMeta evm)
        //{
        //    return this._codeReader.CreateExtensionPropertyViewMeta(mp, evm);
        //}
    }
}