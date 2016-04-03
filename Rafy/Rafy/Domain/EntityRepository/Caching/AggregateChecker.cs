/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101020
 * 说明：组合检测条件的检测器
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101020
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Utils.Caching;

namespace Rafy.Domain.Caching
{
    /// <summary>
    /// 组合检测条件的检测器
    /// </summary>
    public class AggregateChecker : ChangeChecker
    {
        private List<ChangeChecker> _checkers = new List<ChangeChecker>();

        /// <summary>
        /// 添加一个新的检测器
        /// </summary>
        /// <param name="item"></param>
        public void Add(ChangeChecker item)
        {
            for (int i = 0, c = this._checkers.Count; i < c; i++)
            {
                var checker = this._checkers[i];
                if (checker.Equals(item)) return;
            }

            this._checkers.Add(item);
        }

        /// <summary>
        /// 所有条件通过，才算通过。
        /// </summary>
        public override void Check()
        {
            for (int i = 0, c = this._checkers.Count; i < c; i++)
            {
                var checker = this._checkers[i];
                checker.Check();
                if (checker.HasChanged)
                {
                    this.NotifyChanged();
                    break;
                }
            }
        }
    }
}
