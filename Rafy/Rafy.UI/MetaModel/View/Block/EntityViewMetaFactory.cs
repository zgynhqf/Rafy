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
using Rafy.UI;

namespace Rafy.MetaModel.View
{
    /// <summary>
    /// 实体默认视图及实体信息的仓库
    /// </summary>
    public class EntityViewMetaFactory
    {
        private CodeEVMReader _codeReader;
        private XmlConfigManager _xmlCfgMgr;

        public EntityViewMetaFactory(XmlConfigManager xmlConfigMgr) : this(xmlConfigMgr, new CodeEVMReader()) { }

        internal EntityViewMetaFactory(XmlConfigManager xmlConfigMgr, CodeEVMReader codeEVMReader)
        {
            _xmlCfgMgr = xmlConfigMgr;
            _codeReader = codeEVMReader;
        }

        /// <summary>
        /// 获取某个类型的默认视图或扩展视图
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="extendViewName"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public EntityViewMeta Create(Type entityType, string extendViewName = null, BranchDestination destination = BranchDestination.ActiveBranch)
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
        public EntityViewMeta CreateBaseView(Type entityType, BranchDestination destination = BranchDestination.ActiveBranch)
        {
            var res = this.CreateBaseViewCore(entityType, destination);

            //res.Freeze();

            return res;
        }

        private EntityViewMeta CreateBaseViewCore(Type entityType, BranchDestination destination = BranchDestination.ActiveBranch)
        {
            var meta = CommonModel.Entities.Get(entityType);

            var raw = this._codeReader.Read(meta);

            this.UseSysCommands(raw);

            //使用配置对象进行配置
            if (raw is WebEntityViewMeta)
            {
                foreach (WebViewConfig config in UIEnvironment.WebConfigurations.FindViewConfigurations(entityType))
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
                foreach (WPFViewConfig config in UIEnvironment.WPFConfigurations.FindViewConfigurations(entityType))
                {
                    lock (config)
                    {
                        config.View = raw as WPFEntityViewMeta;
                        config.ConfigView();
                    }
                }
            }

            this.ConfigBlock(raw, destination);

            return raw;
        }

        private void UseSysCommands(EntityViewMeta evm)
        {
            //初始化实体视图中的命令按钮
            var em = evm.EntityMeta;
            if (!UIEnvironment.IsWebUI)
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
        public EntityViewMeta CreateExtendView(Type entityType, string extendViewName, BranchDestination destination = BranchDestination.ActiveBranch)
        {
            var raw = this.CreateBaseViewCore(entityType, destination);
            raw.ExtendView = extendViewName;

            //使用扩展视图配置对象进行配置
            if (raw is WebEntityViewMeta)
            {
                foreach (WebViewConfig config in UIEnvironment.WebConfigurations.FindViewConfigurations(entityType, extendViewName))
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
                foreach (WPFViewConfig config in UIEnvironment.WPFConfigurations.FindViewConfigurations(entityType, extendViewName))
                {
                    lock (config)
                    {
                        config.View = raw as WPFEntityViewMeta;
                        config.ConfigView();
                    }
                }
            }

            this.ConfigBlock(raw, destination);

            //raw.Freeze();

            return raw;
        }

        /// <summary>
        /// 使用 BlockConfig 配置界面
        /// </summary>
        /// <param name="raw"></param>
        /// <param name="destination"></param>
        private void ConfigBlock(EntityViewMeta raw, BranchDestination destination)
        {
            if (destination != BranchDestination.Empty)
            {
                var key = new BlockConfigKey
                {
                    EntityType = raw.EntityType,
                    ExtendView = raw.ExtendView
                };
                var blockCfgs = _xmlCfgMgr.GetBlockConfig(key, destination);
                foreach (var blockCfg in blockCfgs)
                {
                    blockCfg.Config(raw);
                }
            }
        }

        //public EntityPropertyViewMeta CreateExtensionPropertyViewMeta(IManagedProperty mp, EntityViewMeta evm)
        //{
        //    return this._codeReader.CreateExtensionPropertyViewMeta(mp, evm);
        //}
    }
}