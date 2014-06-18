/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130528
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130528 17:26
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Rafy.Data;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 表示会使用同一个括号的多组条件约束。
    /// 
    /// 不使用常用的树的形式，是为了减少对象的构造数量，提升性能。
    /// </summary>
    internal class ConstraintGroup : Constraint
    {
        private List<Constraint> _items;
        private List<string> _operators;

        internal ConstraintGroup()
        {
            _items = new List<Constraint>(3);
            _operators = new List<string>(2);
        }

        public override ConstraintType Type
        {
            get { return ConstraintType.Group; }
        }

        public List<Constraint> Items
        {
            get
            {
                return _items;
            }
        }

        /// <summary>
        /// 连接 Constraints 的连接符列表。
        /// 也就是说：Operators.Count + 1 = Constraints.Count。
        /// </summary>
        public List<string> Operators
        {
            get { return _operators; }
        }

        /// <summary>
        /// 在当前约束组中加入以 And 方式一个约束。
        /// </summary>
        /// <param name="constraint"></param>
        /// <returns></returns>
        public override IConstraintGroup And(IConstraintGroup constraint)
        {
            _items.Add(constraint as Constraint);
            _operators.Add(AndOperator);
            return this;
        }

        /// <summary>
        /// 在当前约束组中加入以 Or 方式一个约束。
        /// </summary>
        /// <param name="constraint"></param>
        /// <returns></returns>
        public override IConstraintGroup Or(IConstraintGroup constraint)
        {
            _items.Add(constraint as Constraint);
            _operators.Add(OrOperator);
            return this;
        }

        //public override void GetSql(TextWriter sql, FormattedSqlParameters paramaters)
        //{
        //    for (int i = 0, c = _items.Count; i < c; i++)
        //    {
        //        if (i > 0)
        //        {
        //            string op = _operators[i - 1];
        //            sql.Write(' ');
        //            sql.Write(op);
        //            sql.Write(' ');
        //        }

        //        var item = _items[i];
        //        var isMultiGroup = (item is ConstraintGroup) && (item as ConstraintGroup)._items.Count > 1;
        //        if (isMultiGroup)
        //        {
        //            sql.Write('(');
        //        }
        //        item.GetSql(sql, paramaters);
        //        if (isMultiGroup)
        //        {
        //            sql.Write(')');
        //        }
        //    }
        //}
    }
}