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
using System.Linq;
using Rafy.ManagedProperty;
using Rafy.MetaModel;

namespace Rafy.Domain.Validation
{
    /// <summary>
    /// 一个规则的容器
    /// </summary>
    public interface IRulesContainer
    {
        /// <summary>
        /// 判断容器中是否已经存在指定类型的规则。
        /// </summary>
        /// <param name="ruleType"></param>
        /// <returns></returns>
        bool HasRule(Type ruleType);
    }

    /// <summary>
    /// 简单的规则列表。
    /// 
    /// 提供排序的方法
    /// </summary>
    internal class RulesContainer : IRulesContainer
    {
        private List<IRule> _list = new List<IRule>();
        private bool _sorted;

        public int Count
        {
            get { return _list.Count; }
        }

        public void Add(IRule item)
        {
            _list.Add(item);
            _sorted = false;
        }

        public List<IRule> GetList(bool applySort)
        {
            if (applySort && !_sorted)
            {
                lock (_list)
                {
                    if (applySort && !_sorted)
                    {
                        _list.Sort((t1, t2) =>
                        {
                            return t1.Meta.Priority.CompareTo(t2.Meta.Priority);
                        });
                        _sorted = true;
                    }
                }
            }
            return _list;
        }

        public void Clear()
        {
            _list.Clear();
            _sorted = true;
        }

        public bool HasRule(Type ruleType)
        {
            return _list.Any(r => ruleType.IsInstanceOfType(r.ValidationRule));
        }
    }
}
