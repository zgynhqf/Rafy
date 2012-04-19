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
using OEA.ManagedProperty;
using System.Xml.Serialization;
using hxy;
using System.IO;
using System.Collections.ObjectModel;

namespace OEA.MetaModel.View
{
    /// <summary>
    /// 聚合块定义
    /// 
    /// 该类可以被 XML 序列化
    /// </summary>
    public class AggtBlocks
    {
        private List<AggtBlocks> _Surrounders = new List<AggtBlocks>();

        private AggtChildrenCollection _Children;

        public AggtBlocks()
        {
            this.Layout = new LayoutMeta();
            this._Children = new AggtChildrenCollection(this);
        }

        public LayoutMeta Layout { get; set; }

        public Block MainBlock { get; set; }

        public List<AggtBlocks> Surrounders
        {
            get { return this._Surrounders; }
        }

        public AggtChildrenCollection Children
        {
            get { return this._Children; }
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

        #region 查询 API

        #endregion
    }

    public class AggtChildrenCollection : Collection<AggtBlocks>
    {
        private AggtBlocks _owner;

        public AggtChildrenCollection(AggtBlocks owner)
        {
            this._owner = owner;
        }

        protected override void InsertItem(int index, AggtBlocks item)
        {
            base.InsertItem(index, item);

            (item.MainBlock as ChildBlock).Owner = this._owner;
        }

        protected override void SetItem(int index, AggtBlocks item)
        {
            base.SetItem(index, item);

            (item.MainBlock as ChildBlock).Owner = this._owner;
        }

        #region 查询

        /// <summary>
        /// 查找某个子块。
        /// </summary>
        /// <returns></returns>
        public AggtBlocks Find(Type childType)
        {
            return this.FirstOrDefault(b => b.MainBlock.EntityType == childType);
        }

        /// <summary>
        /// 获取某个子块
        /// </summary>
        /// <param name="childType"></param>
        /// <returns></returns>
        public AggtBlocks this[Type childType]
        {
            get
            {
                var item = this.Find(childType);
                if (item == null) throw new ArgumentOutOfRangeException("childType");
                return item;
            }
        }

        #endregion
    }
}