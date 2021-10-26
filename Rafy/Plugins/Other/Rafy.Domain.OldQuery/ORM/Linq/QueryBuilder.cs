/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130426
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：2.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130426 20:27
 * 2.0.0 条件的创建都放到外层对应的整个表达式的地方，而不是分散在各地。 胡庆访 20130608
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
using Rafy.ManagedProperty;
using Rafy.MetaModel;

namespace Rafy.Domain.ORM.Linq
{
    /// <summary>
    /// 通过 Linq 表达式，调用 IQuery 来构造查询。
    /// </summary>
    class QueryBuilder : ExpressionVisitor
    {
        /// <summary>
        /// 正在组织的查询对象。
        /// </summary>
        internal PropertyQuery _query;
        /// <summary>
        /// 当前操作计算出来的约束
        /// </summary>
        private Constraint _whereResult;

        internal void BuildQuery(Expression exp)
        {
            this.Visit(exp);

            //把最后的 Where 约束添加到 Query 中。
            if (_whereResult != null)
            {
                _query.Where = _query.Where.And(_whereResult);
                _whereResult = null;
            }
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

                var previousWhere = _whereResult;

                this.Visit(lambda.Body);

                switch (exp.Method.Name)
                {
                    case LinqConsts.QueryableMethod_Where:
                        //如果现在不是第一次调用 Where 方法，那么需要把本次的约束和之前的约束进行 And 合并。
                        if (_whereResult != null && previousWhere != null)
                        {
                            _whereResult = new AndOrConstraint
                            {
                                Context = _query,
                                IsAnd = true,
                                Left = previousWhere,
                                Right = _whereResult
                            };
                        }
                        break;
                    case LinqConsts.QueryableMethod_OrderBy:
                    case LinqConsts.QueryableMethod_ThenBy:
                        if (_propertyResult != null)
                        {
                            _query.OrderBy(_propertyResult, OrderDirection.Ascending);
                            _propertyResult = null;
                        }
                        break;
                    case LinqConsts.QueryableMethod_OrderByDescending:
                    case LinqConsts.QueryableMethod_ThenByDescending:
                        if (_propertyResult != null)
                        {
                            _query.OrderBy(_propertyResult, OrderDirection.Descending);
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
                    if (_hasNot) throw OperationNotSupported("字符串操作不支持操作符：'!'。");
                    _operator = PropertyCompareOperator.Contains;
                    this.Visit(exp.Object);
                    this.Visit(args[0]);
                    break;
                case LinqConsts.StringMethod_StartWith:
                    if (_hasNot) throw OperationNotSupported("字符串操作不支持操作符：'!'。");
                    _operator = PropertyCompareOperator.StartWith;
                    this.Visit(exp.Object);
                    this.Visit(args[0]);
                    break;
                case LinqConsts.StringMethod_EndWith:
                    if (_hasNot) throw OperationNotSupported("字符串操作不支持操作符：'!'。");
                    _operator = PropertyCompareOperator.EndWith;
                    this.Visit(exp.Object);
                    this.Visit(args[0]);
                    break;
                case LinqConsts.StringMethod_IsNullOrEmpty:
                    _valueResult = string.Empty;
                    _hasValueResult = true;
                    _operator = _hasNot ? PropertyCompareOperator.NotEqual : PropertyCompareOperator.Equal;
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
            if (args.Count == 2)
            {
                switch (exp.Method.Name)
                {
                    case LinqConsts.EnumerableMethod_Contains:
                        _operator = _hasNot ? PropertyCompareOperator.NotIn : PropertyCompareOperator.In;
                        this.Visit(args[1]);//先访问属性
                        this.Visit(args[0]);//再访问列表常量
                        this.MakeConstraint();
                        return true;
                    default:
                        break;
                }
            }
            return false;
        }

        private bool VisitMethod_List(MethodCallExpression exp)
        {
            switch (exp.Method.Name)
            {
                case LinqConsts.ListGenericMethod_Contains:
                    _operator = _hasNot ? PropertyCompareOperator.NotIn : PropertyCompareOperator.In;
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

        /// <summary>
        /// 关联操作的最后一个引用属性。
        /// 用于在访问 A.B.C.Name 时记录 C；在访问完成后，值回归到 null。
        /// </summary>
        private IRefEntityProperty _lastJoinRefResult;

        /// <summary>
        /// 是否当前正在访问引用对象中的属性。
        /// 主要用于错误提示，引用属性不能进行对比。
        /// </summary>
        private bool _visitRefProperties;

        protected override Expression VisitMember(MemberExpression m)
        {
            //只能访问属性
            var clrProperty = m.Member as PropertyInfo;
            if (clrProperty == null) throw OperationNotSupported(m.Member);
            var ownerExp = m.Expression;
            if (ownerExp == null) throw OperationNotSupported(m.Member);

            //exp 如果是: A 或者 A.B.C，都可以作为属性查询。
            var nodeType = ownerExp.NodeType;
            if (nodeType != ExpressionType.Parameter && nodeType != ExpressionType.MemberAccess) throw OperationNotSupported(m.Member);

            //如果是 A.B.C.Name，则先读取 A.B.C，记录最后一个引用实体类型 C；剩下 .Name 给本行后面的代码读取。
            VisitRefEntity(ownerExp);

            //属性的拥有类型对应的仓库。
            //获取当前正在查询的实体对应的仓库对象。如果是级联引用表达式，则使用最后一个实体即可。
            var ownerRepo = _query.Repo;
            if (_lastJoinRefResult != null)
            {
                //如果已经有引用属性在列表中，说明上层使用了 A.B.C.Name 这样的语法。
                //这时，Name 应该是 C 这个实体的值属性。
                ownerRepo = RepositoryFactoryHost.Factory.FindByEntity(_lastJoinRefResult.RefEntityType);
                _lastJoinRefResult = null;
            }

            //查询托管属性
            var mp = FindProperty(ownerRepo, clrProperty);
            if (mp == null) throw OperationNotSupported("Linq 查询的属性必须是一个托管属性。");
            if (mp is IListProperty) throw OperationNotSupported("暂时不支持面向组合子对象的查询。");
            if (mp is IRefEntityProperty)
            {
                //如果是引用属性，说明需要使用关联查询。
                var refProperty = mp as IRefEntityProperty;
                _query.JoinRef(refProperty, ownerRepo.EntityType);

                //存储到字段中，最后的值属性会使用这个引用属性对应的引用实体类型来查找对应仓库。
                _lastJoinRefResult = refProperty;
                return m;
            }

            //访问值属性
            VisitValueProperty(clrProperty, mp, ownerRepo);

            return m;
        }

        /// <summary>
        /// 如果是 A.B.C.Name，则先读取 A.B.C
        /// </summary>
        /// <param name="exp"></param>
        private void VisitRefEntity(Expression exp)
        {
            if (exp.NodeType == ExpressionType.MemberAccess)
            {
                var oldValue = _visitRefProperties;
                _visitRefProperties = true;

                this.Visit(exp);

                _visitRefProperties = oldValue;
            }
        }

        #endregion

        #region 构造属性约束条件

        //当前的属性条件
        private IManagedProperty _propertyResult;
        private IRepositoryInternal _propertyResultRepo;

        //操作符
        private PropertyCompareOperator? _operator;

        //对比的目标值或属性
        private bool _hasValueResult;
        private object _valueResult;
        private IManagedProperty _rightPropertyResult;
        private IRepositoryInternal _rightPropertyResultRepo;

        protected override Expression VisitBinary(BinaryExpression binaryExp)
        {
            if (binaryExp.NodeType == ExpressionType.AndAlso || binaryExp.NodeType == ExpressionType.OrElse)
            {
                //先计算左边的约束结果
                this.Visit(binaryExp.Left);
                var left = _whereResult;

                //再计算右边的约束结果
                this.Visit(binaryExp.Right);
                var right = _whereResult;

                //使用 AndOrConstraint 合并约束的结果。
                _whereResult = new AndOrConstraint
                {
                    Context = _query,
                    Left = left,
                    Right = right,
                    IsAnd = binaryExp.NodeType == ExpressionType.AndAlso
                };
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
            if (_hasNot) throw OperationNotSupported("不支持操作符：'!'，请使用相反的操作符。");

            switch (binaryExp.NodeType)
            {
                case ExpressionType.Equal:
                    _operator = PropertyCompareOperator.Equal;
                    break;
                case ExpressionType.NotEqual:
                    _operator = PropertyCompareOperator.NotEqual;
                    break;
                case ExpressionType.LessThan:
                    _operator = PropertyCompareOperator.Less;
                    break;
                case ExpressionType.LessThanOrEqual:
                    _operator = PropertyCompareOperator.LessEqual;
                    break;
                case ExpressionType.GreaterThan:
                    _operator = PropertyCompareOperator.Greater;
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    _operator = PropertyCompareOperator.GreaterEqual;
                    break;
                default:
                    //throw new InvalidProgramException("两个属性间的比较只支持以下操作：=、!=、>、>=、<、<=。");
                    throw OperationNotSupported(binaryExp);
            }
        }

        /// <summary>
        /// 通过目前已经收集到的属性、操作符、值，来生成一个属性条件结果。
        /// 并清空已经收集的信息。
        /// </summary>
        private void MakeConstraint()
        {
            if (_propertyResult != null && _operator.HasValue)
            {
                if (_hasValueResult)
                {
                    _whereResult = new PropertyConstraint
                    {
                        Context = _query,
                        Property = _propertyResult,
                        ConcreteType = _propertyResultRepo.EntityType,
                        Operator = _operator.Value,
                        Value = _valueResult
                    };
                    _hasValueResult = false;
                }
                else
                {
                    _whereResult = new TwoPropertiesConstraint
                    {
                        Context = _query,
                        LeftProperty = _propertyResult,
                        LeftPropertyOwner = _propertyResultRepo.EntityType,
                        Operator = _operator.Value,
                        RightProperty = _rightPropertyResult,
                        RightPropertyOwner = _rightPropertyResultRepo.EntityType,
                    };
                    _rightPropertyResult = null;
                    _rightPropertyResultRepo = null;
                }
                _propertyResult = null;
                _propertyResultRepo = null;
                _operator = null;
            }
        }

        private void VisitValueProperty(PropertyInfo clrProperty, IManagedProperty mp, IRepositoryInternal mpOwnerRepo)
        {
            //接下来，是一般属性的处理
            if (_visitRefProperties)
            {
                throw OperationNotSupported(string.Format("不支持使用属性：{0}。这是因为它的拥有者是一个值属性，值属性只支持直接对比。", clrProperty.Name));
            }

            //如果已经记录了条件的属性，那么当前的 mp 就是用于对比的第二个属性。（A.Code = A.Name 中的 Name）
            if (_propertyResult != null)
            {
                _rightPropertyResult = mp;
                _rightPropertyResultRepo = mpOwnerRepo;
            }
            //如果还没有记录属性，说明当前条件要比较的属性就是 mp；(A.Code = 1 中的 Code）
            else
            {
                _propertyResult = mp;
                _propertyResultRepo = mpOwnerRepo;
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

        private static IManagedProperty FindProperty(IEntityInfoHost info, PropertyInfo clrProperty)
        {
            return info.EntityMeta.ManagedProperties.GetCompiledProperties().Find(clrProperty.Name);
        }

        internal static Exception OperationNotSupported(MemberInfo member)
        {
            return new NotSupportedException(string.Format("不支持这个成员调用：'{1}'.'{0}'。", member.Name, member.DeclaringType.Name));
        }

        private static Exception OperationNotSupported(Expression node)
        {
            return new NotSupportedException(string.Format("不支持类型为 {1} 的表达式：'{0}'。", node, node.NodeType));
        }

        private static Exception OperationNotSupported(string msg)
        {
            return new NotSupportedException(msg);
            //return new NotSupportedException(string.Format("不支持这个操作：'{0}'。", action));
        }

        #endregion
    }

    //暂留
    ///// <summary>
    ///// Queryable 中所有支持的方法
    ///// </summary>
    //enum QueryableAction
    //{
    //    None,
    //    Where, OrderBy, OrderByDescending, ThenBy, ThenByDescending,
    //}
    //enum StringAction
    //{
    //    None,
    //    Contains, StartsWith, EndsWith, IsNullOrEmpty
    //}
    //enum EnumerableAction
    //{
    //    None,
    //    Contains
    //}
}