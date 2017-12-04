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
    public abstract class TreeIndexOption
    {
        private char _seperator = '.';

        #region TreeIndexOption.Default

        static TreeIndexOption()
        {
            Default = new TreeIndexOptionByNumber
            {
                LayersMaxNumber = new int[]
                {
                    //999999,
                    999
                }
            };

            //使用 Layers
            ////最大可以到 999.
            //var numbers = new string[999];
            //for (int i = 1; i <= 999; i++)
            //{
            //    numbers[i - 1] = i.ToString("d3");
            //}

            //Default = new TreeIndexOptionByChar
            //{
            //    _seperator = '.',
            //    Layers = new string[][] { numbers }
            //};
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
        /// 每一层间的分隔符。默认使用 '.'。
        /// </summary>
        public char Seperator
        {
            get { return _seperator; }
            set { _seperator = value; }
        }

        /// <summary>
        /// 通过父对象的编码以及当前的索引来生成可用的树型编码
        /// </summary>
        /// <param name="parentIndex"></param>
        /// <param name="nodeIndex"></param>
        /// <returns></returns>
        public string CalculateChildIndex(string parentIndex, int nodeIndex)
        {
            int layerIndex = 0;
            if (!string.IsNullOrEmpty(parentIndex))
            {
                //父结点中有几个分隔符，就表示第几层。
                for (int i = 0; i < parentIndex.Length; i++)
                {
                    if (parentIndex[i] == _seperator) layerIndex++;
                }
            }
            else
            {
                parentIndex = string.Empty;
            }

            var code = this.GetSingleIndex(layerIndex, nodeIndex);

            return parentIndex + code + _seperator;
        }

        /// <summary>
        /// 获取指定索引对应节点的父节点的索引号。
        /// 如果传入的已经是根节点，则返回 null。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string CalculateParentIndex(string index)
        {
            //反向找到倒数第二个 _seperator 即可。
            for (int i = index.Length - 2; i >= 0; i--)
            {
                if (index[i] == _seperator)
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
                if (cItem == _seperator) res++;
            }
            return res;
        }

        /// <summary>
        /// 获取指定层级指定索引号对应的 TreeIndex。
        /// </summary>
        /// <param name="layerIndex"></param>
        /// <param name="nodeIndex"></param>
        /// <returns></returns>
        protected abstract string GetSingleIndex(int layerIndex, int nodeIndex);

        /// <summary>
        /// 通过目前最大的 TreeIndex，来推算下一个根节点应该使用的 TreeIndex。
        /// </summary>
        /// <param name="maxTreeIndex"></param>
        /// <returns></returns>
        internal string GetNextRootTreeIndex(string maxTreeIndex)
        {
            var seperatorIndex = maxTreeIndex.IndexOf(_seperator);
            if (seperatorIndex < 0)
            {
                throw new InvalidOperationException(string.Format("树索引 {0} 中应该包含分隔符，格式有误！", maxTreeIndex));
            }

            var rootIndexCode = maxTreeIndex.Substring(0, seperatorIndex);

            var res = this.DoGetNextRootTreeIndex(rootIndexCode);

            return res;
        }

        protected abstract string DoGetNextRootTreeIndex(string rootIndexCode);
    }

    /// <summary>
    /// 通过设置每层最大数字来生成 TreeIndex 的 TreeIndexOption
    /// </summary>
    public class TreeIndexOptionByNumber : TreeIndexOption
    {
        /// <summary>
        /// 获取或设置一个数组，该数组中的每一个数字代表每一层所使用的数字的最大位数（最大3位表示001-999）。
        /// 如果使用的层数超过数组的个数，则会继续数组的最后一个。
        /// </summary>
        public int[] LayersMaxNumber { get; set; }

        protected override string GetSingleIndex(int layerIndex, int nodeIndex)
        {
            var layerMaxNo = this.GetLayerMaxNumber(layerIndex);

            var value = Math.Min(nodeIndex + 1, layerMaxNo);

            var bits = layerMaxNo.ToString().Length;

            return value.ToString("d" + bits);
        }

        protected override string DoGetNextRootTreeIndex(string rootIndexCode)
        {
            var nextRootIndex = int.Parse(rootIndexCode);

            return this.CalculateChildIndex(null, nextRootIndex);
        }

        private int GetLayerMaxNumber(int layerIndex)
        {
            var layersMaxNumber = this.LayersMaxNumber;

            if (layerIndex < layersMaxNumber.Length)
            {
                return layersMaxNumber[layerIndex];
            }

            return layersMaxNumber[layersMaxNumber.Length - 1];
        }
    }

    /// <summary>
    /// 通过设置每层可用的字符来生成 TreeIndex 的 TreeIndexOption
    /// </summary>
    public class TreeIndexOptionByChar : TreeIndexOption
    {
        /// <summary>
        /// 获取或设置一个数组，该数组中的每一个数字代表每一层所使用的字符串定义。
        /// 如果使用的层数超过数组的个数，则会继续数组的最后一个。
        /// </summary>
        public string[][] Layers { get; set; }

        protected override string DoGetNextRootTreeIndex(string rootIndexCode)
        {
            var rootLayer = this.GetLayer(0);
            for (int i = 0, c = rootLayer.Length; i < c; i++)
            {
                var item = rootLayer[i];
                if (item == rootIndexCode)
                {
                    return this.CalculateChildIndex(null, i + 1);
                }
            }

            throw new InvalidOperationException(string.Format("没有找到 {0} 的树索引，数据有误。", rootIndexCode));
        }

        protected override string GetSingleIndex(int layerIndex, int nodeIndex)
        {
            var layer = this.GetLayer(layerIndex);

            if (nodeIndex < layer.Length)
            {
                return layer[nodeIndex];
            }

            return layer[layer.Length - 1];
        }

        private string[] GetLayer(int layerIndex)
        {
            var layers = this.Layers;

            if (layerIndex < layers.Length)
            {
                return layers[layerIndex];
            }

            return layers[layers.Length - 1];
        }
    }
}