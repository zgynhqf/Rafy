/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120311
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120311
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.MetaModel
{
    /// <summary>
    /// WPF Web 通用的模型
    /// </summary>
    public static class CommonModel
    {
        /// <summary>
        /// 所有实体元数据
        /// </summary>
        public static EntityMetaRepository Entities = new EntityMetaRepository();

        /// <summary>
        /// 所有模块的元数据
        /// </summary>
        public static ModulesContainer Modules = new ModulesContainer();

        #region 初始化元数据

        private static bool _pluginsInitialized;

        /// <summary>
        /// 初始化程序中所有的实体DLL。
        /// 
        /// 此方法应该在程序初始化时调用。
        /// </summary>
        internal static void InitEntityMetas()
        {
            if (_pluginsInitialized) throw new NotSupportedException("OEA框架已经初始化完成！");

            //加入业务模型，Library, Module 中都有业务模型。
            foreach (var type in OEAEnvironment.GetAllRootTypes())
            {
                Entities.AddRootPrime(type);
            }

            //All QueryObject
            foreach (var type in OEAEnvironment.GetCriteriaTypes())
            {
                Entities.AddRootPrime(type);
            }

            //运行写在 EntityConfig 中的客户化
            foreach (var kv in OEAEnvironment.GetTypeConfigurations())
            {
                var type = kv.Key;
                var configList = kv.Value.OrderByDescending(o => o.ReuseLevel).ThenBy(o => o.InheritanceCount);
                foreach (var config in configList)
                {
                    config.UseDefaultMeta();
                    config.CustomizeMeta();
                }
            }

            CommonModel.Entities.InitRelations();

            _pluginsInitialized = true;
        }

        #endregion
    }
}