/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20230304
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20230304 21:32
 * 
*******************************************************/

using MongoDB.Bson;
using MongoDB.Driver;
using Rafy.Data;
using Rafy.Domain;
using Rafy.Domain.ORM.Query;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Rafy.MongoDb
{
    public class MongoDbDataQueryer : DataQueryer
    {
        public override LiteDataTable QueryTable(IQuery query, PagingInfo paging = null)
        {
            throw new NotSupportedException("不支持查询表格。");
        }

        protected override void QueryDataCore(ORMQueryArgs args, EntityList entityList)
        {
            if (args.Filter != null) { throw new NotSupportedException("不支持内在过滤 Filter。"); }
            if (args.LoadOptions != null) { throw new NotSupportedException("不支持定义加载选项 LoadOptions。"); }

            var dp = this.DataProvider as MongoDbDataProvider;
            if (dp.Repository.SupportTree) { throw new NotSupportedException("不支持树型实体。"); }
            var meta = dp.Repository.EntityMeta;
            if (meta.EntityCategory != MetaModel.EntityCategory.Root) { throw new NotSupportedException("不支持非聚合根实体的查询。"); }

            var db = dp.MongoDatabase;
            var tableName = meta.TableMeta?.TableName ?? meta.EntityType.Name;
            var collection = db.GetCollection<BsonDocument>(tableName);

            if (args.QueryType == RepositoryQueryType.Count)
            {
                var count = this.Count(collection, args.Query);
                entityList.SetTotalCount(count);
            }
            else
            {
                //是否需要为 PagingInfo 设置统计值。
                var pi = args.PagingInfo;
                var pagingInfoCount = !PagingInfo.IsNullOrEmpty(pi) && pi.IsNeedCount;

                //如果 pagingInfoCount 为真，则在访问数据库时，会设置好 PagingInfo 的总行数。
                this.QueryList(collection, args, dp);

                //最后，还需要设置列表的 TotalCount。
                if (pagingInfoCount) { entityList.SetTotalCount(pi.TotalCount); }
            }
        }

        private void QueryList(IMongoCollection<BsonDocument> collection, ORMQueryArgs args, MongoDbDataProvider dp)
        {
            var parser = new MongoFilterParser();
            var filter = parser.Parse(args.Query, Builders<BsonDocument>.Filter);

            var result = collection.Find(filter);
            if (args.QueryType == RepositoryQueryType.First)
            {
                result = result.Limit(1);
            }
            else
            {
                var pi = args.PagingInfo;
                if (!PagingInfo.IsNullOrEmpty(pi))
                {
                    if (pi.IsNeedCount)
                    {
                        pi.TotalCount = collection.CountDocuments(filter);
                    }

                    result = result.Skip((int)(pi.PageNumber - 1) * pi.PageSize).Limit(pi.PageSize);
                }
            }

            var documents = result.ToList();

            var reader = new BsonAggtReader();
            reader.ReadData(documents, args.EntityList, dp.Repository);
        }

        /// <summary>
        /// 查询 Count
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private long Count(IMongoCollection<BsonDocument> collection, IQuery query)
        {
            var parser = new MongoFilterParser();
            var filter = parser.Parse(query, Builders<BsonDocument>.Filter);

            var count = collection.CountDocuments(filter);
            return count;
        }
    }
}