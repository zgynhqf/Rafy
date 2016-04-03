/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130426
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130426 20:12
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Rafy.Domain.ORM.Linq
{
    static class Evaluator
    {
        /// <summary> 
        /// <![CDATA[ 
        /// Performs evaluation & replacement of independent sub-trees > 
        /// ]]>
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns> 
        public static Expression PartialEval(Expression expression)
        {
            var treeEvaluator = new SubtreeEvaluator();
            return treeEvaluator.Eval(expression);
        }

        /// <summary> 
        /// <![CDATA[ 
        /// Evaluates & replaces sub-trees when first candidate is reached (top-down) 
        /// ]]>
        /// </summary> 
        class SubtreeEvaluator : ExpressionVisitor
        {
            private EvaluableTreeFinder _nominator = new EvaluableTreeFinder();

            private HashSet<Expression> _candidates;

            internal Expression Eval(Expression exp)
            {
                _candidates = _nominator.Find(exp);

                return this.Visit(exp);
            }

            public override Expression Visit(Expression exp)
            {
                if (exp == null) { return null; }

                if (_candidates.Contains(exp))
                {
                    return this.Evaluate(exp);
                }

                return base.Visit(exp);
            }

            private Expression Evaluate(Expression e)
            {
                if (e.NodeType == ExpressionType.Constant) { return e; }

                var lambda = Expression.Lambda(e);
                Delegate func = lambda.Compile();
                return Expression.Constant(func.DynamicInvoke(null), e.Type);
            }
        }

        /// <summary> 
        /// Performs bottom-up analysis to determine which nodes can possibly 
        /// be part of an evaluated sub-tree. 
        /// </summary> 
        class EvaluableTreeFinder : ExpressionVisitor
        {
            private HashSet<Expression> _candidates;
            private bool _cannotBeEvaluated;

            /// <summary>
            /// 查找出所有可被计算值的表达式。
            /// </summary>
            /// <param name="expression"></param>
            /// <returns></returns>
            internal HashSet<Expression> Find(Expression expression)
            {
                _candidates = new HashSet<Expression>();
                this.Visit(expression);
                return _candidates;
            }

            public override Expression Visit(Expression expression)
            {
                if (expression != null)
                {
                    bool saveCannotBeEvaluated = _cannotBeEvaluated;

                    //重新计算出子树中是否可以被计算。
                    _cannotBeEvaluated = false;
                    base.Visit(expression);

                    if (!_cannotBeEvaluated)
                    {
                        if (CanEvaluate(expression))
                        {
                            //找到一个可以计算的分支，添加到结果中。
                            _candidates.Add(expression);
                        }
                        else
                        {
                            _cannotBeEvaluated = true;
                        }
                    }
                    _cannotBeEvaluated |= saveCannotBeEvaluated;
                }

                return expression;
            }

            private static bool CanEvaluate(Expression expression)
            {
                //树中如果包含参数，则不能被计算结果。
                return expression.NodeType != ExpressionType.Parameter;
            }
        }
    }
}
