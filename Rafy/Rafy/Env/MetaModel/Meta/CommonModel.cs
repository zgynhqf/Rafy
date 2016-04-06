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

namespace Rafy.MetaModel
{
    /// <summary>
    /// WPF Web 通用的模型
    /// </summary>
    public static class CommonModel
    {
        private static EntityMetaRepository _entities;

        private static ModulesContainer _modules;

        static CommonModel()
        {
            Reset();
        }

        /// <summary>
        /// 所有实体元数据
        /// </summary>
        public static EntityMetaRepository Entities
        {
            get { return _entities; }
        }

        /// <summary>
        /// 所有模块的元数据
        /// </summary>
        public static ModulesContainer Modules
        {
            get { return _modules; }
        }

        internal static void Reset()
        {
            _entities = new EntityMetaRepository();
            _entities.FreezeItems();

            _modules = new ModulesContainer();
        }
    }
}