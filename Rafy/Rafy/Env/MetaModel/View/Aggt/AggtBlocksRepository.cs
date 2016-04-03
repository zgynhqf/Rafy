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
using Rafy.MetaModel.XmlConfig.Web;

namespace Rafy.MetaModel.View
{
    /// <summary>
    /// 系统中所有的聚合块定义都存储在这
    /// </summary>
    public class AggtBlocksRepository
    {
        private Dictionary<string, Func<ModuleMeta, AggtBlocks>> _memory = new Dictionary<string, Func<ModuleMeta, AggtBlocks>>();

        private XmlConfigManager _xmlCfgMgr;

        internal AggtBlocksRepository(XmlConfigManager xmlConfigMgr)
        {
            this._xmlCfgMgr = xmlConfigMgr;
        }

        /// <summary>
        /// 创建某个模块定义的界面块
        /// </summary>
        /// <param name="moduleMeta"></param>
        /// <returns></returns>
        public AggtBlocks GetModuleBlocks(ModuleMeta moduleMeta)
        {
            AggtBlocks blocks = null;

            //如果模块自己指定的模板类，则使用模板类生成聚合块。
            if (moduleMeta.BlocksTemplate != null)
            {
                var template = Activator.CreateInstance(moduleMeta.BlocksTemplate) as BlocksTemplate;
                if (template == null) throw new InvalidProgramException("模板类需要从 BlocksTemplate 类继承。");
                template.EntityType = moduleMeta.EntityType;
                blocks = template.GetBlocks();
            }
            else
            {
                if (!string.IsNullOrEmpty(moduleMeta.AggtBlocksName))
                {
                    blocks = GetDefinedBlocks(moduleMeta.AggtBlocksName);
                }
                else
                {
                    blocks = GetDefaultBlocks(moduleMeta.EntityType);
                }
            }

            return blocks;
        }

        /// <summary>
        /// 创建默认的聚合块对象
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public AggtBlocks GetDefaultBlocks(Type entityType)
        {
            //默认模板使用代码结构生成
            var template = new CodeBlocksTemplate
            {
                EntityType = entityType
            };
            return template.GetBlocks();
        }

        /// <summary>
        /// 创建某个自定义的聚合块
        /// </summary>
        /// <param name="blocksName"></param>
        /// <returns></returns>
        public AggtBlocks GetDefinedBlocks(string blocksName)
        {
            AggtBlocks res = null;
            Func<ModuleMeta, AggtBlocks> creator = null;
            if (this._memory.TryGetValue(blocksName, out creator))
            {
                res = creator(null);
            }
            else
            {
                res = this._xmlCfgMgr.GetBlocks(blocksName);
                if (res == null) throw new InvalidOperationException("没有找到相应的聚合视图配置文件：" + blocksName);
            }

            return res;
        }

        /// <summary>
        ///示例定义：
        ///var b = new CompositeBlocks
        ///{
        ///    MainBlock = new Block
        ///    {
        ///        EntityType = typeof(Book),
        ///        BlockType = BlockType.Detail
        ///    },
        ///    Children =
        ///    {
        ///        new ChildBlock{
        ///            ChildrenPropertyNameSetter = Book.ChapterListProperty
        ///        }
        ///    }
        ///};
        /// </summary>
        /// <param name="blocksName"></param>
        /// <param name="blocks"></param>
        public void DefineBlocks(string blocksName, Func<ModuleMeta, AggtBlocks> blocks)
        {
            this._memory[blocksName] = blocks;
        }
    }
}