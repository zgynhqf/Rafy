/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120312
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120312
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using System.IO;
using OEA.MetaModel.XmlConfig.Web;

namespace OEA.MetaModel.View
{
    /// <summary>
    /// Web 模型容器
    /// </summary>
    public static class UIModel
    {
        /// <summary>
        /// xml 配置管理器
        /// </summary>
        public static readonly XmlConfigManager XmlConfigMgr = new XmlConfigManager();

        public static readonly AggtBlocksRepository AggtBlocks = new AggtBlocksRepository(XmlConfigMgr);

        public static readonly EntityViewMetaFactory Views = new EntityViewMetaFactory(XmlConfigMgr);

        public static readonly WebCommandRepository WebCommands = new WebCommandRepository();

        public static readonly WPFCommandRepository WPFCommands = new WPFCommandRepository();

        /// <summary>
        /// 所有模块的元数据
        /// </summary>
        public static ModulesContainer Modules = CommonModel.Modules;

        public static void Freeze()
        {
            WebCommands.Freeze();
        }

        #region 初始化元数据

        private static bool _pluginsInitialized;

        /// <summary>
        /// 初始化程序中所有的实体DLL。
        /// 
        /// 此方法应该在程序初始化时调用。
        /// </summary>
        public static void InitCommandMetas()
        {
            if (_pluginsInitialized) throw new NotSupportedException("OEA框架已经初始化完成！");

            if (OEAEnvironment.IsWeb)
            {
                //放在 Commands 下的文件夹会自动加入进来
                var cmdDir = ConfigurationHelper.GetAppSettingOrDefault("OEACommandsDir", "Scripts/Commands/");
                var dir = OEAEnvironment.ToAbsolute(cmdDir);
                if (Directory.Exists(dir)) { WebCommands.AddByDirectory(dir); }

                //加入所有 Library 中 Commands 文件夹下的 js Resource。
                foreach (var plugin in OEAEnvironment.GetAllLibraries())
                {
                    WebCommands.AddByAssembly(plugin.Assembly);
                }
            }
            else
            {
                //加入所有 Module 中 Commands。
                foreach (var plugin in OEAEnvironment.GetAllPlugins())
                {
                    WPFCommands.AddByAssembly(plugin.Assembly);
                }
            }
        }

        /// <summary>
        /// 所有的插件初始化完毕后，框架会自动调用此方法。
        /// </summary>
        public static void NotifyPluginsMetaIntialized()
        {
            _pluginsInitialized = true;
        }

        #endregion
    }
}