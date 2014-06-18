/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130402 17:20
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130402 17:20
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Rafy;

namespace Rafy.DomainModeling.Controls
{
    /// <summary>
    /// 关系集合
    /// </summary>
    public class BlockRelationCollection : NotifyChangedCollection<BlockRelation>
    {
        //private ModelingDesigner _designer;

        //public BlockRelationCollection(ModelingDesigner designer)
        //{
        //    _designer = designer;
        //}

        //public BlockRelation FindByKey(string from, string to, string label)
        //{
        //    return this.FirstOrDefault(r => r.FromBlock == from && r.ToBlock == to && r.Label == label);
        //}

        /// <summary>
        /// 通过连接键，查找指定的关系
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public BlockRelation FindByKey(IConnectionKey key)
        {
            return this.FirstOrDefault(r => r.FromBlock == key.From && r.ToBlock == key.To && r.Label == key.Label);
        }
    }
}
