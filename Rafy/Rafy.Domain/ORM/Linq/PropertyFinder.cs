/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150206
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150206 16:27
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
using Rafy.ManagedProperty;

namespace Rafy.Domain.ORM.Linq
{
    /// <summary>
    /// 访问一个属性的表达式，如 A.B.C.Name，并执行以下操作：
    /// * 对其中使用到的引用属性，在查询对象中添加表的 Join。
    /// * 返回找到的最终属性（如上面的 Name，或者是 A.Children 中的 Children），以及该属性对应的表对象。
    /// * 如果引用属性是可空引用属性，则同时还会生成该可空引用属性不为空的条件。（因为是对这个引用实体的属性进行判断，所以需要这个引用不能为空。）
    /// </summary>
    internal class PropertyFinder : ExpressionVisitor
    {
        private IQuery _query;
        private IRepository _repo;
        private bool _reverseConstraint;

        private QueryFactory f = QueryFactory.Instance;

        internal PropertyFinder(IQuery query, IRepository repo, bool reverseConstraint)
        {
            _query = query;
            _repo = repo;
            _reverseConstraint = reverseConstraint;
        }

        /// <summary>
        /// 是否需要在查询中反转所有条件。
        /// </summary>
        public bool ReverseConstraint
        {
            get { return _reverseConstraint; }
        }

        /// <summary>
        /// 查找到的属性。
        /// </summary>
        public IManagedProperty Property;

        /// <summary>
        /// 查找到的属性对应的表。
        /// </summary>
        public ITableSource PropertyOwnerTable;

        /// <summary>
        /// 如果使用了引用属性，而且是可空的引用属性，那么添加这可空外键不为空的条件。
        /// 这个属性将返回这个条件，外界使用时，需要主动将这个条件添加到查询中。
        /// 
        /// 例如：
        /// Book.Category.Name = 'a'
        /// 应该转换为
        /// Book.CategoryId IS NOT NULL AND BookCategory.Name = 'a'；
        /// 如果同时 <see cref="_reverseConstraint"/> 是 true，则应该转换为
        /// Book.CategoryId IS NULL OR BookCategory.Name != 'a'；
        /// </summary>
        public IConstraint NullableRefConstraint;

        public void Find(Expression m)
        {
            this.NullableRefConstraint = null;
            this.Visit(m);
        }

        /// <summary>
        /// 关联操作的最后一个引用属性。
        /// 用于在访问 A.B.C.Name 时记录 C；在访问完成后，值回归到 null。
        /// </summary>
        private IRefEntityProperty _lastJoinRefResult;
        private ITableSource _lastJoinTable;

        /// <summary>
        /// 是否当前正在访问引用对象中的属性。
        /// 主要用于错误提示，引用属性不能进行对比。
        /// </summary>
        private bool _visitRefProperties;

        protected override Expression VisitMember(MemberExpression m)
        {
            //只能访问属性
            var clrProperty = m.Member as PropertyInfo;
            if (clrProperty == null) throw EntityQueryBuilder.OperationNotSupported(m.Member);
            var ownerExp = m.Expression;
            if (ownerExp == null) throw EntityQueryBuilder.OperationNotSupported(m.Member);

            //exp 如果是: A 或者 A.B.C，都可以作为属性查询。
            var nodeType = ownerExp.NodeType;
            if (nodeType != ExpressionType.Parameter && nodeType != ExpressionType.MemberAccess) throw EntityQueryBuilder.OperationNotSupported(m.Member);

            //如果是 A.B.C.Name，则先读取 A.B.C，记录最后一个引用实体类型 C；剩下 .Name 给本行后面的代码读取。
            VisitRefEntity(ownerExp);

            //属性的拥有类型对应的仓库。
            //获取当前正在查询的实体对应的仓库对象。如果是级联引用表达式，则使用最后一个实体即可。
            var ownerTable = _query.MainTable;
            var ownerRepo = _repo;
            if (_lastJoinRefResult != null)
            {
                //如果已经有引用属性在列表中，说明上层使用了 A.B.C.Name 这样的语法。
                //这时，Name 应该是 C 这个实体的值属性。
                ownerRepo = RepositoryFactoryHost.Factory.FindByEntity(_lastJoinRefResult.RefEntityType);
                ownerTable = _lastJoinTable;
                _lastJoinRefResult = null;
                _lastJoinTable = null;
            }

            //查询托管属性
            var mp = EntityQueryBuilder.FindProperty(ownerRepo, clrProperty);
            if (mp == null) throw EntityQueryBuilder.OperationNotSupported("Linq 查询的属性必须是一个托管属性。");
            if (mp is IRefEntityProperty)
            {
                //如果是引用属性，说明需要使用关联查询。
                var refProperty = mp as IRefEntityProperty;
                var refTable = f.FindOrCreateJoinTable(_query, ownerTable, refProperty);

                if (refProperty.Nullable)
                {
                    var column = ownerTable.Column(refProperty.RefIdProperty);
                    NullableRefConstraint = _reverseConstraint ?
                        f.Or(NullableRefConstraint, column.Equal(null as object)) :
                        f.And(NullableRefConstraint, column.NotEqual(null as object));
                }

                //存储到字段中，最后的值属性会使用这个引用属性对应的引用实体类型来查找对应仓库。
                _lastJoinRefResult = refProperty;
                _lastJoinTable = refTable;
                return m;
            }

            if (_visitRefProperties)
            {
                throw EntityQueryBuilder.OperationNotSupported(string.Format("不支持使用属性：{0}。这是因为它的拥有者是一个值属性，值属性只支持直接对比。", mp.Name));
            }

            //访问值属性
            PropertyOwnerTable = ownerTable;
            Property = mp;

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
    }
}
