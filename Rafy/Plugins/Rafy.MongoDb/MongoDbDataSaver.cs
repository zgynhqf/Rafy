/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20230304
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20230304 21:31
 * 
*******************************************************/

using MongoDB.Bson;
using MongoDB.Driver;
using Rafy.Domain;
using Rafy.Domain.Serialization.Json;
using Rafy.ManagedProperty;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy.MongoDb
{
    public class MongoDbDataSaver : DataSaver
    {
        protected override void Submit(SubmitArgs e)
        {
            //只更新子实体时，也是更新整个聚合。
            if (e.Action == SubmitAction.ChildrenOnly)
            {
                e.Action = SubmitAction.Update;
            }

            base.Submit(e);
        }

        protected override void SubmitChildren(Entity entity)
        {
            //MongoDb 只处理聚合根类型；子类型不再处理。
            //base.SubmitChildren(entity);
        }

        public override void DeleteFromPersistence(Entity data)
        {
            var collection = GetCollectionForSave();
            var f = Builders<BsonDocument>.Filter.Eq(Consts.MongoDbIdName, BsonObjectId.Create(data.Id));
            collection.DeleteOne(f);
        }

        public override void InsertToPersistence(Entity data)
        {
            var collection = GetCollectionForSave();

            var doc = Serialize(data);

            collection.InsertOne(doc);
            var id = doc[Consts.MongoDbIdName];
            data.LoadProperty(Entity.IdProperty, id);

            //如果实体的 Id 是在插入的过程中生成的，
            //那么需要在插入组合子对象前，先把新生成的父对象 Id 都同步到子列表中。
            data.SyncIdToChildren();

            data.MarkPropertiesUnchanged();
        }

        public override void UpdateToPersistence(Entity data)
        {
            var collection = GetCollectionForSave();

            var doc = Serialize(data);

            var f = Builders<BsonDocument>.Filter.Eq(Consts.MongoDbIdName, BsonObjectId.Create(data.Id));
            collection.ReplaceOne(f, doc);

            data.MarkPropertiesUnchanged();
        }

        private IMongoCollection<BsonDocument> GetCollectionForSave()
        {
            var dp = this.DataProvider as MongoDbDataProvider;
            if (dp.Repository.SupportTree) { throw new NotSupportedException("不支持树型实体。"); }
            var meta = dp.Repository.EntityMeta;
            //if (meta.EntityCategory != MetaModel.EntityCategory.Root) { throw new NotSupportedException("不支持非聚合根实体的保存。"); }

            var db = dp.MongoDatabase;
            var tableName = meta.TableMeta?.TableName ?? meta.EntityType.Name;
            var collection = db.GetCollection<BsonDocument>(tableName);
            return collection;
        }

        private static BsonDocument Serialize(Entity data)
        {
            var serializer = new BsonAggtWriter();
            var json = serializer.Serialize(data);
            return json;
        }

        #region Not Support

        public override RedundanciesUpdater CreateRedundanciesUpdater()
        {
            throw new NotSupportedException();
        }

        protected override void DeleteRefCore(Entity entity, IRefProperty refProperty)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
