/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110407
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110407
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace OEA.Module.WPF.CommandAutoUI
{
    /// <summary>
    /// 多种生成方案的组合
    /// </summary>
    internal class CompoundGenerator : GroupGenerator
    {
        private List<GroupGenerator> _list = new List<GroupGenerator>();

        internal int Count { get { return this._list.Count; } }

        internal void Add(GroupGenerator item) { this._list.Add(item); }

        /// <summary>
        /// 直接代理到所有组合的 Generator 中。
        /// </summary>
        public override void CreateControlToContext()
        {
            foreach (var item in this._list)
            {
                item.CreateControlToContext();
            }
        }
    }
}