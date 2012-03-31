/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120330
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 适配到 Entity、托管属性上。 胡庆访 20120330
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using OEA.ManagedProperty;

namespace OEA.Library.Validation
{
    /// <summary>
    /// 简单的规则列表。
    /// 
    /// 提供排序的方法
    /// </summary>
    internal class RulesContainer
    {
        private List<IRuleMethod> _list = new List<IRuleMethod>();
        private bool _sorted;

        public void Add(IRuleMethod item)
        {
            _list.Add(item);
            _sorted = false;
        }

        public List<IRuleMethod> GetList(bool applySort)
        {
            if (applySort && !_sorted)
            {
                lock (_list)
                {
                    if (applySort && !_sorted)
                    {
                        _list.Sort();
                        _sorted = true;
                    }
                }
            }
            return _list;
        }
    }
}
