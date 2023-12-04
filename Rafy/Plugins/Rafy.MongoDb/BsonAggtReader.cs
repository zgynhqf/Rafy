/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20230305
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20230305 03:00
 * 
*******************************************************/

using MongoDB.Bson;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.Reflection;
using Rafy.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy.MongoDb
{
    /// <summary>
    /// 实现从 BsonDocument 中读取实体的属性值。
    /// 大部分代码都拷贝自：<see cref="Rafy.Domain.Serialization.Json.AggtDeserializer"/>。
    /// </summary>
    internal class BsonAggtReader
    {
        public void ReadData(List<BsonDocument> documents, EntityList list, EntityRepository repository)
        {
            foreach (var doc in documents)
            {
                Entity entity = this.ReadEnity(doc, repository);

                list.Add(entity);
            }
        }

        private Entity ReadEnity(BsonDocument doc, IRepository repository)
        {
            var entity = repository.New();
            this.LoadProperties(doc, entity);
            entity.PersistenceStatus = PersistenceStatus.Saved;
            return entity;
        }

        private void LoadProperties(BsonDocument doc, Entity entity)
        {
            var properties = entity.GetRepository().EntityMeta.ManagedProperties.GetAvailableProperties();
            foreach (var element in doc.Elements)
            {
                var propertyName = element.Name;
                var bsonValue = element.Value;
                var value = TryReadCLRValue(bsonValue);

                if (propertyName.EqualsIgnoreCase(Consts.MongoDbIdName))
                {
                    entity.Id = value;
                    continue;
                }

                var mp = properties.Find(propertyName, true) as IProperty;
                if (mp != null)
                {
                    //只读属性不需要反序列化。
                    if (mp.IsReadOnly) continue;
                    //幽灵属性也不需要处理。
                    if (mp == EntityConvention.Property_IsPhantom) continue;

                    if (mp is IListProperty)
                    {
                        this.LoadChildren(entity, mp as IListProperty, bsonValue as BsonArray);
                    }
                    else if (mp is IRefProperty)
                    {
                        //一般引用属性不支持反序列化。
                    }
                    else if (mp.IsRedundant)
                    {
                        /*********************** 代码块解释 *********************************
                         * 冗余属性不支持反序列化，而是自动根据引用属性来自动赋值。
                         * 冗余属性在反序列化时，如果值是错误的（例如在客户端刚创建的实体），而且它在对应的引用属性之后进行反序列化的话，就会导致值出错。
                         * 另外，由于引用属性在反序列化时，都会计算相应的冗余属性，所以这里不再需要对冗余属性进行反序列化。
                         * 
                         * 这种方式，对于有冗余属性的实体，在反序列化时，都会造成额外的数据库读取（同步冗余属性的值）。
                         * 要解决这个性能问题，可以在更新时，设置 UpdatedEntityCreationMode = RequeryFromRepository，从数据库中取出所有的属性。这样，在设置外键时，就不会产品额外的数据库访问。
                         * 但是，这样就会导致必然会产生一次额外的数据库访问（查询初始的实体的值）。
                        **********************************************************************/
                    }
                    //else if (mp is IRefIdProperty)
                    else
                    {
                        if (bsonValue.BsonType == BsonType.Array)
                        {
                            throw new NotSupportedException("暂时不支持数组类型的属性的保存。");
                        }

                        if (value is string)
                        {
                            var propertyType = mp.PropertyType;
                            if (propertyType == typeof(byte[]))
                            {
                                value = Convert.FromBase64String(value as string);
                            }
                            else
                            {
                                //兼容处理枚举的 Label 值。
                                var innerType = TypeHelper.IgnoreNullable(propertyType);
                                if (innerType.IsEnum)
                                {
                                    value = EnumViewModel.Parse(value as string, innerType);
                                }
                            }
                        }

                        entity.LoadProperty(mp, value);
                    }
                }
                else
                {
                    entity.SetDynamicProperty(propertyName, value);
                }
            }
        }

        private void LoadChildren(Entity entity, IListProperty listProperty, BsonArray jArray)
        {
            //构造 List 对象
            EntityList list = null;
            if (entity.HasLocalValue(listProperty) || entity.PersistenceStatus == PersistenceStatus.New)
            {
                list = entity.GetLazyList(listProperty);
            }
            else
            {
                var listRepository = RepositoryFactoryHost.Factory.FindByEntity(listProperty.ListEntityType, true);
                list = listRepository.NewList();
                entity.LoadProperty(listProperty, list);
            }

            var repo = list.GetRepository();
            for (int i = 0; i < jArray.Count; i++)
            {
                var jChild = jArray[i].AsBsonDocument;

                var child = this.ReadEnity(jChild, repo);

                list.Add(child);

                //由于添加到子集合中，会导致 ParentId 的值变化。所以这里需要设置实体的状态。
                child.PersistenceStatus = PersistenceStatus.Saved;
            }
        }

        private static object TryReadCLRValue(BsonValue value)
        {
            switch (value.BsonType)
            {
                case BsonType.ObjectId:
                    return value.AsObjectId.ToString();
                case BsonType.String:
                    return value.AsString;
                case BsonType.Double:
                    return value.AsDouble;
                case BsonType.Boolean:
                    return value.AsBoolean;
                case BsonType.DateTime:
                case BsonType.Timestamp:
                    return value.ToLocalTime();
                case BsonType.Null:
                    return null;
                case BsonType.RegularExpression:
                    return value.AsBsonRegularExpression.AsString;
                case BsonType.Int32:
                    return value.AsInt32;
                case BsonType.Int64:
                    return value.AsInt64;
                case BsonType.Decimal128:
                    return value.AsDecimal128;
                default:
                    return null;
                    //throw new NotSupportedException("不支持读取这个类型的数据：" + value.BsonType);
            }
        }
    }
}
