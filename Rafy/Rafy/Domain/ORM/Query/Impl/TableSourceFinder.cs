/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131213
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131213 10:13
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain.ORM.Query.Impl
{
    /// <summary>
    /// 从指定的数据源中查找指定仓库对应的实体数据源的查询器类型。
    /// </summary>
    public class TableSourceFinder : QueryNodeVisitor
    {
        private IRepository _repo;
        private string _alias;
        private ISource _source;
        private bool _stop;
        private ITableSource _result;

        public ITableSource Find(ISource source, IRepository repo, string alias)
        {
            _repo = repo;
            _alias = alias;
            return this.Search(source);
        }

        private ITableSource Search(ISource source)
        {
            _source = source;
            _result = null;
            _stop = false;

            base.Visit(_source);

            return _result;
        }

        protected override IQueryNode Visit(IQueryNode node)
        {
            if (_stop) { return node; }

            return base.Visit(node);
        }

        protected override IQueryNode VisitTableSource(ITableSource tableSource)
        {
            if (_repo == null)
            {
                //如果 _repo 传入 null，查找主实体数据源。
                if (tableSource.Join == null)
                {
                    _stop = true;
                    _result = tableSource;
                }
            }
            else if (tableSource.EntityRepository == _repo)
            {
                //如果还没有匹配的实体源，则设置结果为第一个匹配的实体源。
                if (_result == null)
                {
                    _result = tableSource;
                }

                //如果同时还匹配了别名，那么整个搜索结束。
                if (tableSource.Alias == _alias)
                {
                    _stop = true;

                    //不论 tableSource 是不是第一个匹配的实体源，都需要设置为结果。
                    _result = tableSource;
                }
            }

            return tableSource;
        }
    }
}