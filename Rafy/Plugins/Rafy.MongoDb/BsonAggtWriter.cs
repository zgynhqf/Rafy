/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20230305
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20230305 17:40
 * 
*******************************************************/

using MongoDB.Bson;
using Rafy.Domain;
using Rafy.Domain.Serialization.Json;
using Rafy.ManagedProperty;
using Rafy.Utils;
using SharpCompress.Writers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Rafy.MongoDb
{
    /// <summary>
    /// 实现将实体的属性值序列化为 BsonDocument 对象。
    /// 大部分代码都拷贝自：<see cref="Rafy.Domain.Serialization.Json.AggtSerializer"/>。
    /// </summary>
    public class BsonAggtWriter
    {
        /// <summary>
        /// 是否不输出动态属性。默认为 false。
        /// </summary>
        public bool IgnoreDynamicProperties { get; set; }

        /// <summary>
        /// 是否需要在序列化时忽略只读属性。
        /// 默认为 false。
        /// </summary>
        public bool IgnoreROProperties { get; set; }

        /// <summary>
        /// 如果使用了幽灵框架，那么此属性表示是否需要同时序列化幽灵属性。
        /// 默认为 false。
        /// </summary>
        public bool SerializeIsPhantom { get; set; }

        /// <summary>
        /// 是否需要在序列化时忽略默认值的属性。
        /// 默认为 false。
        /// </summary>
        public bool IgnoreDefault { get; set; }

        /// <summary>
        /// 是把在序列化枚举时，把值输出为字符串。
        /// 默认为 <see cref="EnumSerializationMode.Integer"/>。
        /// </summary>
        public EnumSerializationMode EnumSerializationMode { get; set; }

        public BsonDocument Serialize(Entity entity)
        {
            if (entity.SupportTree) { throw new NotSupportedException("不支持树型实体。"); }

            var doc = this.SerializeEntityContent(entity);

            return doc;
        }

        /// <summary>
        /// 向 JSON 中序列化指定实体的所有内容。
        /// </summary>
        /// <param name="entity"></param>
        protected virtual BsonDocument SerializeEntityContent(Entity entity)
        {
            var doc = new BsonDocument();

            //序列化所有的编译期属性。
            this.SerializeCompiledProperties(entity, doc);

            if (!this.IgnoreDynamicProperties)
            {
                this.SerializeDynamicProperties(entity, doc);
            }

            return doc;
        }

        /// <summary>
        /// 序列化所有的编译期属性。
        /// </summary>
        /// <param name="entity"></param>
        protected virtual void SerializeCompiledProperties(Entity entity, BsonDocument doc)
        {
            var isTree = entity.SupportTree;

            var properties = entity.PropertiesContainer.GetAvailableProperties();
            for (int i = 0, c = properties.Count; i < c; i++)
            {
                var property = properties[i] as IProperty;
                if (entity.IsDisabled(property)) continue;

                if (property.IsReadOnly && this.IgnoreROProperties) continue;
                if (!isTree && (property == Entity.TreePIdProperty || property == Entity.TreeIndexProperty)) { continue; }
                if (!this.SerializeIsPhantom && property == EntityConvention.Property_IsPhantom) continue;

                var value = entity.GetProperty(property);
                if (this.IgnoreDefault)
                {
                    var defaultValue = property.GetMeta(entity).DefaultValue;
                    if (object.Equals(defaultValue, value)) { continue; }
                }

                var element = this.SerializeProperty(property, value);
                if (element != null)
                {
                    doc.Add(element.Value);
                }
            }
        }

        /// <summary>
        /// 序列化所有的编译期属性。
        /// </summary>
        /// <param name="entity"></param>
        protected virtual void SerializeDynamicProperties(Entity entity, BsonDocument doc)
        {
            if (entity.DynamicPropertiesCount > 0)
            {
                var properties = entity.GetDynamicProperties();
                foreach (var kv in properties)
                {
                    var value = this.SerializeValue(kv.Value);
                    var element = this.CreatePropertyElement(kv.Key, BsonValue.Create(value));
                    doc.Add(element);
                }
            }
        }

        /// <summary>
        /// 序列化某个指定的属性
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        protected virtual BsonElement? SerializeProperty(IProperty property, object value)
        {
            if (property == Entity.IdProperty)
            {
                if (!string.IsNullOrWhiteSpace(value as string))
                {
                    return new BsonElement(Consts.MongoDbIdName, BsonObjectId.Create(value as string));
                }

                return null;
            }

            value = this.SerializeValue(value);

            switch (property.Category)
            {
                case PropertyCategory.List:
                    if (value != null)
                    {
                        BsonArray bsonArray = new BsonArray();
                        var list = value as EntityList;
                        for (int i = 0; i < list.Count; i++)
                        {
                            var child = list[i];
                            var childElement = this.Serialize(child);
                            bsonArray.Add(childElement);
                        }

                        return this.CreatePropertyElement(property.Name, bsonArray);
                    }
                    break;
                case PropertyCategory.LOB:
                case PropertyCategory.Normal:
                case PropertyCategory.Readonly:
                case PropertyCategory.Redundancy:
                    return this.CreatePropertyElement(property.Name, BsonValue.Create(value));
                case PropertyCategory.ReferenceEntity:
                    if (value != null && (property as IRefProperty).ReferenceType != ReferenceType.Parent)
                    {
                        var childDoc = this.Serialize(value as Entity);
                        return this.CreatePropertyElement(property.Name, childDoc);
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }

            return null;
        }

        /// <summary>
        /// 序列化某个指定的属性的值。
        /// </summary>
        /// <param name="value"></param>
        protected virtual object SerializeValue(object value)
        {
            //if (value is byte[])
            //{
            //    var base64 = Convert.ToBase64String(value as byte[]);
            //    _writer.WriteValue(base64);
            //}
            //else
            //if (value is IList && !(value is byte[]))
            //{
            //    _writer.WriteStartArray();
            //    var list = value as IList;
            //    for (int i = 0, c = list.Count; i < c; i++)
            //    {
            //        var item = list[i];
            //        _writer.WriteValue(item);
            //    }
            //    _writer.WriteEndArray();
            //}
            //else 
            value = EnumSerializer.ConvertEnumValue(value, this.EnumSerializationMode);
            return value;
        }

        protected virtual BsonElement CreatePropertyElement(string property, BsonValue value)
        {
            property = AggtSerializer.ToCamel(property);

            return new BsonElement(property, value);
        }
    }
}