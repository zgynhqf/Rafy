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
using OEA.MetaModel.XmlConfig.Web;

namespace OEA.MetaModel.View
{
    /// <summary>
    /// 系统中所有的聚合块定义都存储在这
    /// </summary>
    public class AggtBlocksRepository
    {
        private Dictionary<string, AggtBlocks> _memory = new Dictionary<string, AggtBlocks>();

        private CodeBlocksReader _codeReader = new CodeBlocksReader();

        private XmlConfigManager _xmlCfgMgr;

        internal AggtBlocksRepository(XmlConfigManager xmlConfigMgr)
        {
            this._xmlCfgMgr = xmlConfigMgr;
        }

        /// <summary>
        /// 获取某个自定义的聚合块
        /// </summary>
        /// <param name="blocksName"></param>
        /// <returns></returns>
        public AggtBlocks GetDefinedBlocks(string blocksName)
        {
            AggtBlocks res = null;
            if (!this._memory.TryGetValue(blocksName, out res))
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
        public void DefineBlocks(string blocksName, AggtBlocks blocks)
        {
            this._memory[blocksName] = blocks;
        }

        /// <summary>
        /// 获取默认的聚合块对象
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public AggtBlocks GetDefaultBlocks(Type entityType)
        {
            return this._codeReader.Read(entityType);
        }
    }
}