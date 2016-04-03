/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131211
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131211 14:21
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Rafy.Domain;
using Rafy.Domain.ORM.Query;
using Rafy.Domain.ORM.Query.Impl;
using Rafy.ManagedProperty;
using Rafy.MetaModel;

namespace Rafy.Domain.ORM.Linq
{
    /// <summary>
    /// 通过 Linq 表达式，调用 SqlSelect 来构造查询。
    /// </summary>
    class EntityQueryBuilder : ExpressionVisitor
    {
        /// <summary>
        /// 正在组织的查询对象。
        /// </summary>
        private IQuery _query;
        private IRepositoryInternal _repo;
        private QueryFactory f = QueryFactory.Instance;

        /// <summary>
        /// 是否需要反转查询中的所有条件操作符。
        /// 场景：当转换 Linq 表达式中的 All 方法到 Sql 的 NotExsits 时，需要把内部的条件都转换为反向操作符。
        /// </summary>
        private bool _reverseWhere = false;

        public EntityQueryBuilder(IRepositoryInternal repo) : this(repo, false) { }

        internal EntityQueryBuilder(IRepositoryInternal repo, bool reverseWhere)
        {
            _repo = repo;
            _reverseWhere = reverseWhere;
        }

        internal IQuery BuildQuery(Expression exp)
        {
            var mainTable = f.Table(_repo);

            _query = f.Query(mainTable);

            mainTable.Alias = QueryGenerationContext.Get(_query).NextTableAlias();

            this.Visit(exp);

            return _query;
        }

        internal void BuildQuery(Expression exp, IQuery query)
        {
            _query = query;

            this.Visit(exp);
        }

        #region 处理方法调用

        protected override Expression VisitMethodCall(MethodCallExpression exp)
        {
            var method = exp.Method;
            var methodType = method.DeclaringType;
            bool processed = false;

            //处理 Queryable 上的方法
            if (methodType == typeof(Queryable))
            {
                processed = VisitMethod_Queryable(exp);
            }
            //处理 string 上的方法
            else if (methodType == typeof(string))
            {
                processed = VisitMethod_String(exp);
            }
            else if (methodType == typeof(Enumerable))
            {
                processed = VisitMethod_Enumerable(exp);
            }
            else if (methodType.IsGenericType && methodType.GetGenericTypeDefinition() == typeof(List<>))
            {
                processed = VisitMethod_List(exp);
            }

            if (!processed) throw OperationNotSupported(method);

            return exp;
        }

        private bool VisitMethod_Queryable(MethodCallExpression exp)
        {
            var args = exp.Arguments;
            if (args.Count == 2)
            {
                this.Visit(args[0]);//visit queryable

                var lambda = StripQuotes(args[1]) as LambdaExpression;

                var previousWhere = _query.Where;

                this.Visit(lambda.Body);

                switch (exp.Method.Name)
                {
                    case LinqConsts.QueryableMethod_Where:
                        //如果现在不是第一次调用 Where 方法，那么需要把本次的约束和之前的约束进行 And 合并。
                        this.MakeBooleanConstraintIfNoValue();
                        if (_query.Where != null && previousWhere != null)
                        {
                            _query.Where = f.And(previousWhere, _query.Where);
                        }
                        break;
                    case LinqConsts.QueryableMethod_OrderBy:
                    case LinqConsts.QueryableMethod_ThenBy:
                        if (_propertyResult != null)
                        {
                            (_query as TableQuery).OrderBy.Add(f.OrderBy(_propertyResult) as OrderBy);
                            _propertyResult = null;
                        }
                        break;
                    case LinqConsts.QueryableMethod_OrderByDescending:
                    case LinqConsts.QueryableMethod_ThenByDescending:
                        if (_propertyResult != null)
                        {
                            (_query as TableQuery).OrderBy.Add(f.OrderBy(_propertyResult, OrderDirection.Descending) as OrderBy);
                            _propertyResult = null;
                        }
                        break;
                    default:
                        break;
                }

                return true;
            }

            return false;
        }

        private bool VisitMethod_String(MethodCallExpression exp)
        {
            var args = exp.Arguments;
            switch (exp.Method.Name)
            {
                case LinqConsts.StringMethod_Contains:
                    _operator = _hasNot ? PropertyOperator.NotContains : PropertyOperator.Contains;
                    this.Visit(exp.Object);
                    this.Visit(args[0]);
                    break;
                case LinqConsts.StringMethod_StartWith:
                    _operator = _hasNot ? PropertyOperator.NotStartsWith : PropertyOperator.StartsWith;
                    this.Visit(exp.Object);
                    this.Visit(args[0]);
                    break;
                case LinqConsts.StringMethod_EndWith:
                    _operator = _hasNot ? PropertyOperator.NotEndsWith : PropertyOperator.EndsWith;
                    this.Visit(exp.Object);
                    this.Visit(args[0]);
                    break;
                case LinqConsts.StringMethod_IsNullOrEmpty:
                    _valueResult = string.Empty;
                    _hasValueResult = true;
                    _operator = _hasNot ? PropertyOperator.NotEqual : PropertyOperator.Equal;
                    this.Visit(args[0]);
                    break;
                default:
                    throw OperationNotSupported(exp.Method);
            }

            this.MakeConstraint();

            return true;
        }

        private bool VisitMethod_Enumerable(MethodCallExpression exp)
        {
            var args = exp.Arguments;
            switch (exp.Method.Name)
            {
                case LinqConsts.EnumerableMethod_Contains:
                    if (args.Count == 2)
                    {
                        _operator = _hasNot ? PropertyOperator.NotIn : PropertyOperator.In;
                        this.Visit(args[1]);//先访问属性
                        this.Visit(args[0]);//再访问列表常量
                        this.MakeConstraint();
                        return true;
                    }
                    break;
                case LinqConsts.EnumerableMethod_Any:
                case LinqConsts.EnumerableMethod_All:
                    var subQueryBuilder = new SubEntityQueryBuilder();
                    _constraintResult = subQueryBuilder.Build(exp, _query, this.PropertyFinder);
                    this.MakeConstraint();
                    return true;
                default:
                    break;
            }
            return false;
        }

        private bool VisitMethod_List(MethodCallExpression exp)
        {
            switch (exp.Method.Name)
            {
                case LinqConsts.ListGenericMethod_Contains:
                    _operator = _hasNot ? PropertyOperator.NotIn : PropertyOperator.In;
                    this.Visit(exp.Arguments[0]);//先访问属性
                    this.Visit(exp.Object);//再访问列表常量
                    this.MakeConstraint();
                    return true;
                default:
                    break;
            }

            return false;
        }

        #endregion

        #region 处理 Not

        /// <summary>
        /// 是否有 Not 操作。
        /// </summary>
        private bool _hasNot;

        protected override Expression VisitUnary(UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Not:
                    _hasNot = true;
                    this.Visit(node.Operand);
                    this.MakeBooleanConstraintIfNoValue();
                    _hasNot = false;
                    break;
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    this.Visit(node.Operand);
                    break;
                default:
                    throw OperationNotSupported(node);
            }
            return node;
        }

        #endregion

        #region 属性访问

        private PropertyFinder _propertyFinder;

        private PropertyFinder PropertyFinder
        {
            get
            {
                if (_propertyFinder == null)
                {
                    _propertyFinder = new PropertyFinder(_query, _repo, _reverseWhere);
                }
                return _propertyFinder;
            }
        }

        private IConstraint _nullableRefConstraint;

        protected override Expression VisitMember(MemberExpression m)
        {
            var pf = this.PropertyFinder;
            pf.Find(m);
            _nullableRefConstraint = pf.NullableRefConstraint;

            //访问值属性
            VisitValueProperty(pf.Property, pf.PropertyOwnerTable);

            return m;
        }

        #endregion

        #region 构造属性约束条件

        //当前的属性条件
        private IColumnNode _propertyResult;

        //操作符
        private PropertyOperator? _operator;

        //对比的目标值或属性
        private bool _hasValueResult;//由于 _valueResult 可以表示 null，所以需要一个额外的字段来判断当前是否有值。
        private object _valueResult;
        private IColumnNode _rightPropertyResult;

        private IConstraint _constraintResult;

        protected override Expression VisitBinary(BinaryExpression binaryExp)
        {
            if (binaryExp.NodeType == ExpressionType.AndAlso || binaryExp.NodeType == ExpressionType.OrElse)
            {
                //先计算左边的约束结果
                this.Visit(binaryExp.Left);
                this.MakeBooleanConstraintIfNoValue();
                var left = _query.Where;

                //再计算右边的约束结果
                this.Visit(binaryExp.Right);
                this.MakeBooleanConstraintIfNoValue();
                var right = _query.Where;

                //使用 AndOrConstraint 合并约束的结果。
                var op = binaryExp.NodeType == ExpressionType.AndAlso ?
                    BinaryOperator.And : BinaryOperator.Or;
                if (_reverseWhere)
                {
                    op = binaryExp.NodeType == ExpressionType.AndAlso ? BinaryOperator.Or : BinaryOperator.And;
                }
                _query.Where = f.Binary(left, op, right);
            }
            else
            {
                this.VisitPropertyComparison(binaryExp);
            }

            return binaryExp;
        }

        private void VisitPropertyComparison(BinaryExpression binaryExp)
        {
            //收集属性、值
            this.Visit(binaryExp.Left);
            this.Visit(binaryExp.Right);

            //转换为操作符
            this.MakeOperator(binaryExp);

            //生成属性条件
            this.MakeConstraint();
        }

        private void MakeOperator(BinaryExpression binaryExp)
        {
            //var method = _queryMethod.Peek();
            //if (method == QueryMethod.Queryable)
            {
                if (_hasNot) throw OperationNotSupported("不支持操作符：'!'，请使用相反的操作符。");

                switch (binaryExp.NodeType)
                {
                    case ExpressionType.Equal:
                        _operator = PropertyOperator.Equal;
                        break;
                    case ExpressionType.NotEqual:
                        _operator = PropertyOperator.NotEqual;
                        break;
                    case ExpressionType.LessThan:
                        _operator = PropertyOperator.Less;
                        break;
                    case ExpressionType.LessThanOrEqual:
                        _operator = PropertyOperator.LessEqual;
                        break;
                    case ExpressionType.GreaterThan:
                        _operator = PropertyOperator.Greater;
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        _operator = PropertyOperator.GreaterEqual;
                        break;
                    default:
                        //throw new InvalidProgramException("两个属性间的比较只支持以下操作：=、!=、>、>=、<、<=。");
                        throw OperationNotSupported(binaryExp);
                }
            }
        }

        /// <summary>
        /// 通过目前已经收集到的属性、操作符、值，来生成一个属性条件结果。
        /// 并清空已经收集的信息。
        /// </summary>
        private bool MakeConstraint()
        {
            if (_propertyResult != null && _operator.HasValue)
            {
                var op = _operator.Value;
                if (_reverseWhere) op = PropertyOperatorHelper.Reverse(op);

                if (_hasValueResult)
                {
                    _constraintResult = f.Constraint(_propertyResult, op, _valueResult);
                    _valueResult = null;
                    _hasValueResult = false;
                }
                else
                {
                    _constraintResult = f.Constraint(_propertyResult, op, _rightPropertyResult);
                    _rightPropertyResult = null;
                }
                _propertyResult = null;
                _operator = null;
            }

            if (_constraintResult != null)
            {
                if (_nullableRefConstraint != null)
                {
                    var concat = _reverseWhere ? BinaryOperator.Or : BinaryOperator.And;
                    _constraintResult = f.Binary(_nullableRefConstraint, concat, _constraintResult);
                    _nullableRefConstraint = null;
                }

                _query.Where = _constraintResult;
                _constraintResult = null;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 如果只读取到了一个 Boolean 属性，没有读取到操作符、对比值，
        /// 而这时已经完成了条件的组装，那么必须把这个属性变成一个对判断条件。
        /// </summary>
        private void MakeBooleanConstraintIfNoValue()
        {
            if (_propertyResult != null && _propertyResult.Property.PropertyType == typeof(bool) &&
                !_operator.HasValue &&
                _valueResult == null && _rightPropertyResult == null
                )
            {
                _operator = PropertyOperator.Equal;
                _valueResult = _hasNot ? BooleanBoxes.False : BooleanBoxes.True;
                _hasValueResult = true;
                this.MakeConstraint();
            }
        }

        private void VisitValueProperty(IManagedProperty mp, ITableSource mpOwnerTable)
        {
            //如果已经记录了条件的属性，那么当前的 mp 就是用于对比的第二个属性。（A.Code = A.Name 中的 Name）
            if (_propertyResult != null)
            {
                _rightPropertyResult = mpOwnerTable.Column(mp);
            }
            //如果还没有记录属性，说明当前条件要比较的属性就是 mp；(A.Code = 1 中的 Code）
            else
            {
                _propertyResult = mpOwnerTable.Column(mp);
            }
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            //访问到根，直接返回。见 EntityQueryable 构造函数。
            if (node.Value is IQueryable) return node;

            _valueResult = node.Value;
            _hasValueResult = true;

            return node;
        }

        #endregion

        #region 帮助方法

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = (e as UnaryExpression).Operand;
            }
            return e;
        }

        internal static IManagedProperty FindProperty(IEntityInfoHost info, PropertyInfo clrProperty)
        {
            return info.EntityMeta.ManagedProperties.GetCompiledProperties().Find(clrProperty.Name);
        }

        internal static Exception OperationNotSupported(MemberInfo member)
        {
            return new NotSupportedException(string.Format("不支持这个成员调用：'{1}'.'{0}'。", member.Name, member.DeclaringType.Name));
        }

        internal static Exception OperationNotSupported(Expression node)
        {
            return new NotSupportedException(string.Format("不支持类型为 {1} 的表达式：'{0}'。", node, node.NodeType));
        }

        internal static Exception OperationNotSupported(string msg)
        {
            return new NotSupportedException(msg);
            //return new NotSupportedException(string.Format("不支持这个操作：'{0}'。", action));
        }

        #endregion
    }
}
