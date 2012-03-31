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
using OEA.MetaModel;
using OEA.ORM;
using OEA.ManagedProperty;

namespace OEA.Library
{
    /// <summary>
    /// 树型实体的编码生成规则
    /// </summary>
    public class TreeCodeOption
    {
        public static readonly TreeCodeOption Default = new TreeCodeOption
        {
            Seperator = '.',
            Layers = new string[][] {
                new string[]{"1", "2", "3", "4", "5", "6", "7", "8", "9"}
            }
        };

        /// <summary>
        /// 每一层间的分隔符
        /// </summary>
        public char Seperator;

        /// <summary>
        /// 每一层的字符串定义
        /// </summary>
        public string[][] Layers;

        private string[] GetLayer(int index)
        {
            if (index < this.Layers.Length)
            {
                return this.Layers[index];
            }

            return this.Layers[this.Layers.Length - 1];
        }

        private string GetSingleCode(int layerIndex, int nodeIndex)
        {
            var layer = this.GetLayer(layerIndex);

            if (nodeIndex < layer.Length)
            {
                return layer[nodeIndex];
            }

            return layer[layer.Length - 1];
        }

        internal string CalculateCode(string parentCode, int nodeIndex)
        {
            int layerIndex = -1;
            if (!string.IsNullOrEmpty(parentCode))
            {
                //父结点中有几个分隔符，就表示第几层。
                for (int i = 0; i < parentCode.Length; i++) { if (parentCode[i] == Seperator) layerIndex++; }
                if (layerIndex == -1) layerIndex = 0;
            }
            else
            {
                parentCode = string.Empty;
            }

            var code = this.GetSingleCode(layerIndex, nodeIndex);

            return parentCode + code + Seperator;
        }
    }
}