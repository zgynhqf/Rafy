/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20230305
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20230305 02:53
 * 
*******************************************************/

using MongoDB.Bson;
using MongoDB.Driver;
using Rafy.Domain.ORM.Query;
using Rafy.Domain.Serialization.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Rafy.MongoDb
{
    /// <summary>
    /// 访问 IQuery，将其中的查询条件转换为 MongoDb Driver 中的查询条件的解析器
    /// https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/builders/#std-label-csharp-builders
    /// </summary>
    public class MongoFilterParser : QueryNodeVisitor
    {
        private FilterDefinitionBuilder<BsonDocument> f;

        /// <summary>
        /// 是把在序列化枚举时，把值输出为字符串。
        /// 默认为 <see cref="EnumSerializationMode.Integer"/>。
        /// </summary>
        public EnumSerializationMode EnumSerializationMode { get; set; }

        public FilterDefinition<BsonDocument> Parse(IQuery query, FilterDefinitionBuilder<BsonDocument> filterFactory)
        {
            f = filterFactory;
            _current = f.Empty;

            if (query.Where != null)
            {
                this.Visit(query.Where);
            }

            //if (node.OrderBy.Count > 0)
            //{
            //    for (int i = 0, c = node.OrderBy.Count; i < c; i++)
            //    {
            //        var item = node.OrderBy[i];
            //        this.Visit(item);
            //    }
            //}

            return _current;
        }

        private FilterDefinition<BsonDocument> _current;

        protected override IQueryNode VisitBinaryConstraint(IBinaryConstraint node)
        {
            this.Visit(node.Left);
            var leftConstraint = _current;
            _current = null;
            this.Visit(node.Right);
            var rightConstraint = _current;

            switch (node.Opeartor)
            {
                case Domain.BinaryOperator.And:
                    _current = f.And(leftConstraint, rightConstraint);
                    break;
                case Domain.BinaryOperator.Or:
                    _current = f.Or(leftConstraint, rightConstraint);
                    break;
                default:
                    break;
            }

            return node;
        }

        protected override IQueryNode VisitColumnConstraint(IColumnConstraint node)
        {
            var column = AggtSerializer.ToCamel(node.Column.ColumnName);
            var value = node.Value;
            value = EnumSerializer.ConvertEnumValue(value, this.EnumSerializationMode);
            switch (node.Operator)
            {
                case Domain.PropertyOperator.Equal:
                    _current = f.Eq(column, value);
                    break;
                case Domain.PropertyOperator.NotEqual:
                    _current = f.Ne(column, value);
                    break; 
                case Domain.PropertyOperator.Greater:
                    _current = f.Gt(column, value);
                    break;
                case Domain.PropertyOperator.GreaterEqual:
                    _current = f.Gte(column, value);
                    break;
                case Domain.PropertyOperator.Less:
                    _current = f.Lt(column, value);
                    break;
                case Domain.PropertyOperator.LessEqual:
                    _current = f.Lte(column, value);
                    break;
                case Domain.PropertyOperator.In:
                    _current = f.In(column, (value as IEnumerable).Cast<object>());
                    break;
                case Domain.PropertyOperator.NotIn:
                    _current = f.Nin(column, (value as IEnumerable).Cast<object>());
                    break;
                case Domain.PropertyOperator.Like:
                case Domain.PropertyOperator.NotLike:
                case Domain.PropertyOperator.Contains:
                case Domain.PropertyOperator.NotContains:
                case Domain.PropertyOperator.StartsWith:
                case Domain.PropertyOperator.NotStartsWith:
                case Domain.PropertyOperator.EndsWith:
                case Domain.PropertyOperator.NotEndsWith:
                    throw new NotSupportedException();
                default:
                    break;
            }
            return node;
        }
    }
}