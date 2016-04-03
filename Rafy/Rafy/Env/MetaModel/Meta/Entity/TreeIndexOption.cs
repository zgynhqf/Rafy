/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120210
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120210
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.MetaModel;
using Rafy.ManagedProperty;

namespace Rafy.MetaModel
{
    /// <summary>
    /// 树型实体的编码生成规则
    /// </summary>
    public class TreeIndexOption
    {
        #region TreeIndexOption.Default

        static TreeIndexOption()
        {
            //最大可以到 999.
            var numbers = new string[999];
            for (int i = 1; i <= 999; i++)
            {
                numbers[i - 1] = i.ToString("d3");
            }

            var value = new TreeIndexOption
            {
                Seperator = '.',
                Layers = new string[][] { numbers }
            };

            //public static readonly TreeIndexOption Default = new TreeIndexOption
            //{
            //    Seperator = '.',
            //    Layers = new string[][] {
            //        new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" }
            //    }
            //};
            Default = value;
        }

        private static TreeIndexOption _default;

        /// <summary>
        /// 默认的树节点的索引规则。
        /// </summary>
        public static TreeIndexOption Default
        {
            get
            {
                return _default;
            }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                _default = value;
            }
        }

        #endregion

        /// <summary>
        /// 每一层间的分隔符
        /// </summary>
        public char Seperator { get; set; }

        /// <summary>
        /// 每一层的字符串定义
        /// </summary>
        public string[][] Layers { get; set; }

        private string[] GetLayer(int index)
        {
            if (index < this.Layers.Length)
            {
                return this.Layers[index];
            }

            return this.Layers[this.Layers.Length - 1];
        }

        /// <summary>
        /// 通过父对象的编码以及当前的索引来生成可用的树型编码
        /// </summary>
        /// <param name="parentIndex"></param>
        /// <param name="nodeIndex"></param>
        /// <returns></returns>
        public string CalculateChildIndex(string parentIndex, int nodeIndex)
        {
            int layerIndex = -1;
            if (!string.IsNullOrEmpty(parentIndex))
            {
                //父结点中有几个分隔符，就表示第几层。
                for (int i = 0; i < parentIndex.Length; i++) { if (parentIndex[i] == Seperator) layerIndex++; }
                if (layerIndex == -1) layerIndex = 0;
            }
            else
            {
                layerIndex = 0;
                parentIndex = string.Empty;
            }

            var code = this.GetSingleIndex(layerIndex, nodeIndex);

            return parentIndex + code + Seperator;
        }

        /// <summary>
        /// 获取指定索引对应节点的父节点的索引号。
        /// 如果传入的已经是根节点，则返回 null。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string CalculateParentIndex(string index)
        {
            //反向找到倒数第二个 Seperator 即可。
            for (int i = index.Length - 2; i >= 0; i--)
            {
                if (index[i] == Seperator)
                {
                    return index.Substring(0, i + 1);
                }
            }

            return null;
        }

        /// <summary>
        /// 计算指定索引中表示的级别。
        /// （内部为统计分隔符的个数。）
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal int CountLevel(string index)
        {
            if (string.IsNullOrWhiteSpace(index))
            {
                return 0;
            }

            var res = 0;
            for (int i = 0, c = index.Length; i < c; i++)
            {
                var cItem = index[i];
                if (cItem == Seperator) res++;
            }
            return res;
        }

        private string GetSingleIndex(int layerIndex, int nodeIndex)
        {
            var layer = this.GetLayer(layerIndex);

            if (nodeIndex < layer.Length)
            {
                return layer[nodeIndex];
            }

            return layer[layer.Length - 1];
        }

        internal string GetNextRootTreeIndex(string treeIndex)
        {
            var seperatorIndex = -1;
            for (int i = 0, c = treeIndex.Length; i < c; i++)
            {
                var item = treeIndex[i];
                if (item == Seperator)
                {
                    seperatorIndex = i;
                    break;
                }
            }
            if (seperatorIndex < 0)
            {
                throw new InvalidOperationException(string.Format("树索引 {0} 中应该包含分隔符，格式有误！", treeIndex));
            }

            var indexCode = treeIndex.Substring(0, seperatorIndex);

            var rootLayer = this.GetLayer(0);
            for (int i = 0, c = rootLayer.Length; i < c; i++)
            {
                var item = rootLayer[i];
                if (item == indexCode)
                {
                    return this.CalculateChildIndex(null, i + 1);
                }
            }

            throw new InvalidOperationException(string.Format("没有找到 {0} 的树索引，数据有误。", indexCode));
        }
    }
}