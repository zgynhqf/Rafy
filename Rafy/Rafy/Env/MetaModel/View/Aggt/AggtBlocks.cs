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
using Rafy.ManagedProperty;
using System.Xml.Serialization;
using Rafy;
using System.IO;
using System.Collections.ObjectModel;

namespace Rafy.MetaModel.View
{
    /// <summary>
    /// 聚合块定义
    /// 
    /// 该类可以被 XML 序列化
    /// </summary>
    public class AggtBlocks
    {
        private SurrounderCollection _Surrounders;

        private AggtChildrenCollection _Children;

        public AggtBlocks()
        {
            this.Layout = new LayoutMeta();
            this._Surrounders = new SurrounderCollection();
            this._Children = new AggtChildrenCollection(this);
        }

        public LayoutMeta Layout { get; set; }

        public Block MainBlock { get; set; }

        /// <summary>
        /// 聚合块中的主块的环绕块。
        /// 
        /// 环绕块跟主块没有直接关系。
        /// </summary>
        public SurrounderCollection Surrounders
        {
            get { return this._Surrounders; }
        }

        /// <summary>
        /// 聚合块中主块对应实体的聚合子类的聚合子块。
        /// 
        /// 聚合子块对应的实体跟主块对应的实体是聚合父子关系。
        /// </summary>
        public AggtChildrenCollection Children
        {
            get { return this._Children; }
        }

        /// <summary>
        /// 深度递归遍历所有的聚合块。
        /// 
        /// 先递归遍历聚合子块，再递归遍历环绕块。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AggtBlocks> EnumerateAllBlocks()
        {
            yield return this;

            foreach (var child in this.Children)
            {
                foreach (var item in child.EnumerateAllBlocks()) { yield return item; }
            }

            foreach (var surrounder in this.Surrounders)
            {
                foreach (var item in surrounder.EnumerateAllBlocks()) { yield return item; }
            }
        }

        public string ToXmlString()
        {
            var serializer = new XmlSerializer(typeof(AggtBlocks), new Type[]{
                typeof(Block),typeof(SurrounderBlock),typeof(ChildBlock)
            });
            var w = new StringWriter();
            serializer.Serialize(w, this);
            var xml = w.ToString();
            return xml;
        }

        public static AggtBlocks FromXml(string xml)
        {
            var serializer = new XmlSerializer(typeof(AggtBlocks), new Type[]{
                typeof(Block),typeof(SurrounderBlock),typeof(ChildBlock)
            });
            var sr = new StringReader(xml);
            var blocks = serializer.Deserialize(sr) as AggtBlocks;
            return blocks;
        }

        /// <summary>
        /// 一个简单的块也可以直接转换为一个聚合块。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator AggtBlocks(Block value)
        {
            return new AggtBlocks
            {
                MainBlock = value
            };
        }
    }
}