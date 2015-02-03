/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150202
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150202 11:09
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Rafy.Domain.ORM.Query;
using Rafy.Domain.ORM.Query.Impl;
using Rafy.ManagedProperty;

namespace Rafy.Domain.ORM.Linq
{
    class SubEntityQueryBuilder : ExpressionVisitor
    {
        private QueryFactory f = QueryFactory.Instance;
        private IQuery _parentQuery;
        private IQuery _query;
        private bool _isAny;

        /// <summary>
        /// <![CDATA[
        /// 将聚合子表达式解析为嵌入式子查询条件。
        /// 例如：
        /// 将表达式
        /// book.ChapterList.Cast<Chapter>().Any(c => c.Name == chapterName)
        /// 转换为：
        /// f.Exists(f.Query(chapter,
        ///     where: f.And(
        ///         f.Constraint(chapter.Column(Chapter.BookIdProperty), book.IdColumn),
        ///         f.Constraint(chapter.Column(Chapter.NameProperty), chapterName)
        ///     )
        /// ))
        /// SQL：
        /// SELECT * FROM [Book] b
        /// WHERE EXISTS(
        ///     SELECT * FROM [Chapter] c
        ///     WHERE c.BookId = b.Id AND
        ///         c.Name = {0}
        /// )
        /// ]]>
        /// </summary>
        /// <param name="exp">需要解析的表达式</param>
        /// <param name="parentQuery"></param>
        internal IConstraint Build(Expression exp, IQuery parentQuery)
        {
            _parentQuery = parentQuery;

            this.Visit(exp);

            if (_isAny)
            {
                return f.Exists(_query);
            }

            return f.Not(f.Exists(_query));
        }

        protected override Expression VisitMethodCall(MethodCallExpression exp)
        {
            var method = exp.Method;
            _isAny = method.Name == LinqConsts.EnumerableMethod_Any;
            var pkTable = _parentQuery.MainTable;
            var ownerRepo = pkTable.EntityRepository;//Book

            //先访问表达式 book.ChapterList.Cast<Chapter>()，获取列表属性。
            var invoker = exp.Arguments[0];
            var lpFinder = new ListPropertyFinder();
            lpFinder.Visit(invoker);
            var listProperty = EntityQueryBuilder.FindProperty(ownerRepo, lpFinder.ListProperty) as IListProperty;
            if (listProperty == null) throw EntityQueryBuilder.OperationNotSupported(invoker);

            //为该列表对应的实体创建表对象、查询对象。
            var childRepo = RepositoryFactoryHost.Factory.FindByEntity(listProperty.ListEntityType);
            var childTable = f.Table(childRepo);
            _query = f.Query(
                from: childTable,
                selection: f.Literal("1")
                );
            var qgc = QueryGenerationContext.Get(_parentQuery);
            qgc.Bind(_query);
            childTable.Alias = qgc.NextTableAlias();

            //Any、All 方法如果有第二个参数，那么第二个参数就是条件表达式。如：c => c.Name == chapterName
            if (exp.Arguments.Count == 2)
            {
                var queryBuilder = new EntityQueryBuilder(childRepo);
                queryBuilder.ReverseOperator = !_isAny;//如果是 All，则需要反转里面的所有操作符。
                queryBuilder.BuildQuery(exp.Arguments[1], _query);
            }

            //添加子表查询与父实体的关系条件：WHERE c.BookId = b.Id
            var parentProperty = childRepo.FindParentPropertyInfo(true);
            var parentRefIdProperty = (parentProperty.ManagedProperty as IRefProperty).RefIdProperty;
            var toParentConstraint = f.Constraint(childTable.Column(parentRefIdProperty), pkTable.IdColumn);
            _query.Where = f.And(toParentConstraint, _query.Where);

            return exp;
        }

        private class ListPropertyFinder : ExpressionVisitor
        {
            internal PropertyInfo ListProperty;

            /// <summary>
            /// <![CDATA[
            /// 负责解析：
            /// book.ChapterList.Cast<Chapter>()
            /// 获取 book 的类型与表、获取 ChapterList 对应的列表
            /// ]]>
            /// </summary>
            /// <param name="m"></param>
            /// <returns></returns>
            protected override Expression VisitMember(MemberExpression m)
            {
                //只能访问属性
                var clrProperty = m.Member as PropertyInfo;
                if (clrProperty == null) throw EntityQueryBuilder.OperationNotSupported(m.Member);
                var ownerExp = m.Expression;
                if (ownerExp == null) throw EntityQueryBuilder.OperationNotSupported(m.Member);
                if (ownerExp.NodeType != ExpressionType.Parameter)
                    throw EntityQueryBuilder.OperationNotSupported(string.Format("只支持直接对表的直接聚合子表进行查询，不支持这个表达式：{0}", ownerExp));

                ListProperty = clrProperty;

                return m;
            }
        }
    }
}