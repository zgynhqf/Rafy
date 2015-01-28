/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131212
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131212 10:12
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain.ORM.Query;
using Rafy.Domain.ORM.SqlTree;
using Rafy.ManagedProperty;

namespace Rafy.Domain.ORM.Query.Impl
{
    class TableQuery : SqlSelect, IQuery
    {
        internal IRepositoryInternal Repo;

        QueryNodeType IQueryNode.NodeType
        {
            get { return QueryNodeType.Query; }
        }

        bool IQuery.IsCounting
        {
            get
            {
                return base.IsCounting;
            }
            set
            {
                base.IsCounting = value;
            }
        }

        bool IQuery.IsDistinct
        {
            get
            {
                return base.IsDistinct;
            }
            set
            {
                base.IsDistinct = value;
            }
        }

        int? IQuery.Top
        {
            get
            {
                return base.Top;
            }
            set
            {
                base.Top = value;
            }
        }

        IQueryNode IQuery.Selection
        {
            get
            {
                return base.Selection as IQueryNode;
            }
            set
            {
                base.Selection = value as SqlNode;
            }
        }

        ISource IQuery.From
        {
            get
            {
                return base.From as ISource;
            }
            set
            {
                base.From = value as SqlSource;
            }
        }

        IConstraint IQuery.Where
        {
            get
            {
                return base.Where as IConstraint;
            }
            set
            {
                base.Where = value as SqlConstraint;
            }
        }

        IList<IOrderBy> IQuery.OrderBy
        {
            get
            {
                if (!base.HasOrdered())
                {
                    base.OrderBy = new List<IOrderBy>();
                }

                //如果集合是一个弱类型的集合，则需要使用强类型集合来替换。
                var res = base.OrderBy as IList<IOrderBy>;
                if (res == null)
                {
                    var list = new List<IOrderBy>();
                    if (base.OrderBy.Count > 0)
                    {
                        foreach (IOrderBy item in base.OrderBy)
                        {
                            list.Add(item);
                        }
                    }
                    res = list;
                    base.OrderBy = list;
                }

                return res;
            }
        }
        internal void SetOrderBy(List<IOrderBy> value)
        {
            base.OrderBy = value;
        }

        /// <summary>
        /// 除基础表外，所有连接的表。
        /// </summary>
        private List<SqlTableSource> _allJoinTables = new List<SqlTableSource>();

        private string NextJoinTableAlias()
        {
            return "T" + (_allJoinTables.Count + 1);
        }

        /// <summary>
        /// 在查询对象中查找或者创建指定引用属性对应的连接表对象。
        /// </summary>
        /// <param name="ownerTable">引用属性对应外键所在的表。</param>
        /// <param name="refProperty">指定的引用属性。</param>
        /// <returns></returns>
        internal ITableSource FindOrCreateJoinTable(ITableSource ownerTable, IRefEntityProperty refProperty)
        {
            var query = this as IQuery;
            var refTableSource = _allJoinTables.FirstOrDefault(
                ts => ts.OwnerTable == ownerTable && ts.Property == refProperty
                );
            if (refTableSource == null)
            {
                var f = QueryFactory.Instance;

                var refEntityType = refProperty.RefEntityType;
                var refRepo = RepositoryFactoryHost.Factory.FindByEntity(refEntityType);
                var refTable = f.Table(refRepo, this.NextJoinTableAlias());

                var joinType = refProperty.Nullable ? JoinType.LeftOuter : JoinType.Inner;
                query.From = f.Join(query.From, refTable, f.Constraint(
                    ownerTable.Column(refProperty.RefIdProperty),
                    refTable.Column(Entity.IdProperty)
                    ), joinType);

                refTableSource = new SqlTableSource
                {
                    OwnerTable = ownerTable,
                    Property = refProperty,
                    RefTable = refTable,
                };
                _allJoinTables.Add(refTableSource);
            }

            return refTableSource.RefTable;
        }

        /// <summary>
        /// 本查询所对应的基础表。
        /// </summary>
        public ITableSource MainTable { get; internal set; }
    }

    /// <summary>
    /// 这个类型用于表示某一个 SqlTable 是由哪一个属性关联出来的。
    /// 属性可以是一个引用属性，也可以是一个组合子属性。
    /// 
    /// 同一个属性关联出来的表，在整个 Select 中，应该是唯一的。这样可以防止为同一个引用属性生成多个重复的表。
    /// </summary>
    class SqlTableSource
    {
        internal ITableSource OwnerTable;
        internal IManagedProperty Property;
        internal ITableSource RefTable;
    }
}