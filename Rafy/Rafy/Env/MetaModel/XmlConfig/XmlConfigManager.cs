/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120226
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120226
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using Rafy.MetaModel.View;
using Rafy;

namespace Rafy.MetaModel.XmlConfig.Web
{
    /// <summary>
    /// Web 模型的 Xml 配置文件管理器
    /// </summary>
    public class XmlConfigManager
    {
        /// <summary>
        /// 把某个 BlockConfig 保存为 XML 文件。
        /// </summary>
        /// <param name="blockCfg"></param>
        public void Save(BlockConfig blockCfg)
        {
            var path = blockCfg.Key.GetFilePath();

            if (blockCfg.IsChanged())
            {
                var doc = new XDocument(blockCfg.Xml);

                var dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir)) { Directory.CreateDirectory(dir); }

                doc.Save(path);
            }
            else
            {
                //UI配置没有变化时，Xml 文件应该删除。
                if (File.Exists(path)) { File.Delete(path); }
            }
        }

        /// <summary>
        /// 通过 key 查找 BlockConfig。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public BlockConfig GetBlockConfig(BlockConfigKey key)
        {
            if (key.Type == BlockConfigType.Config || XmlConfigFileSystem.IsCustomizing)
            {
                var path = key.GetFilePath();

                if (File.Exists(path))
                {
                    var xDoc = XDocument.Load(path);

                    var blockCfg = new BlockConfig
                    {
                        Key = key,
                        Xml = xDoc.Root
                    };

                    return blockCfg;
                }
            }

            return null;
        }

        /// <summary>
        /// 把某个自定义的聚合块保存到硬盘上。
        /// </summary>
        /// <param name="blocksName"></param>
        /// <param name="blocks"></param>
        public void Save(string blocksName, AggtBlocks blocks)
        {
            var path = XmlConfigFileSystem.GetCompositeBlocksFilePath(blocksName);
            var xml = blocks.ToXmlString();
            File.WriteAllText(path, xml);
        }

        /// <summary>
        /// 创建某个自定义的聚合块
        /// </summary>
        /// <param name="blocksName"></param>
        /// <returns></returns>
        public AggtBlocks GetBlocks(string blocksName)
        {
            var path = XmlConfigFileSystem.GetCompositeBlocksFilePath(blocksName);

            if (File.Exists(path))
            {
                var xml = File.ReadAllText(path);

                var blocks = AggtBlocks.FromXml(xml);
                return blocks;
            }

            return null;
        }

        //public BlockXmlPair GetBlockConfigPair(Type entityType, string viewName)
        //{
        //    throw new NotImplementedException();//huqf
        //}
    }
}