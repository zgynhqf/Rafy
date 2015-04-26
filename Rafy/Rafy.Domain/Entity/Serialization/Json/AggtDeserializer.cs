/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150325
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150325 10:06
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;
using Rafy;
using Rafy.Domain;
using Rafy.ManagedProperty;

namespace Rafy.Domain.Serialization.Json
{
    /// <summary>
    /// 实体反序列化器。
    /// 序列化后的数据只是暴露了实体的数据，而忽略了实体的状态。
    /// </summary>
    public class AggtDeserializer
    {
        /// <summary>
        /// 实体的 Json 中可以使用这个属性来指定实体的状态。值是该枚举的名称。
        /// </summary>
        public const string PersistenceStatusProperty = "persistenceStatus";

        /// <summary>
        /// 实体或实体列表的自定义反序列化方法。
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        public IDomainComponent Deserialize(Type type, string json)
        {
            if (type.IsSubclassOf(typeof(Entity)))
            {
                var jObject = JObject.Parse(json);
                return this.DeserializeEntity(type, jObject);
            }

            var jArray = JArray.Parse(json);
            return this.DeserializeList(type, jArray);
        }

        /// <summary>
        /// 实体的自定义反序列化方法。
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="jObject">The j object.</param>
        /// <returns></returns>
        private Entity DeserializeEntity(Type type, JObject jObject)
        {
            Entity entity = null;

            var id = TryGetId(jObject);
            if (id != null)
            {
                var repository = RF.Find(type);
                entity = repository.GetById(id);
            }
            if (entity == null)
            {
                entity = Activator.CreateInstance(type) as Entity;
            }

            //反序列化一般属性
            DeserializeProperties(jObject, entity);

            return entity;
        }

        /// <summary>
        /// 遍历 JSON 对象的属性，并使用托管属性 API 来设置一般属性的值。
        /// </summary>
        /// <param name="jObject"></param>
        /// <param name="entity"></param>
        private void DeserializeProperties(JObject jObject, Entity entity)
        {
            var properties = entity.PropertiesContainer.GetAvailableProperties();
            foreach (var propertyValue in jObject)
            {
                var propertyName = propertyValue.Key;
                var jValue = propertyValue.Value;
                var mp = properties.Find(propertyName, true);
                if (mp != null)
                {
                    if (mp != Entity.IdProperty)
                    {
                        if (mp is IListProperty)
                        {
                            DeserializeList(entity, mp as IListProperty, jValue as JArray);
                        }
                        //一般属性。
                        else
                        {
                            var value = (jValue as JValue).Value;
                            if (value is string && mp.PropertyType == typeof(byte[]))
                            {
                                value = Encoding.UTF8.GetBytes(value as string);
                            }
                            entity.SetProperty(mp, value, ManagedPropertyChangedSource.FromUIOperating);
                        }
                    }
                }
                else
                {
                    //如果指定了状态，则主动设置该实体的状态。
                    if (propertyName.EqualsIgnoreCase(PersistenceStatusProperty))
                    {
                        var value = (jValue as JValue).Value;
                        var status = (PersistenceStatus)Enum.Parse(typeof(PersistenceStatus), value.ToString(), true);
                        entity.PersistenceStatus = status;
                    }
                }
            }

            //using (var jsonTextReader = new StringReader(strContent))
            //{
            //    var jsonSerializer = JsonSerializer.Create(this.SerializerSettings);
            //    jsonSerializer.Populate(jsonTextReader, entity);
            //}
        }

        private void DeserializeList(Entity entity, IListProperty listProperty, JArray jArray)
        {
            var list = entity.GetLazyList(listProperty);
            var isNew = entity.PersistenceStatus == PersistenceStatus.New;
            if (isNew)
            {
                foreach (JObject jChild in jArray)
                {
                    var child = list.GetRepository().New();
                    DeserializeProperties(jChild, child);
                    list.Add(child);
                }
            }
            else
            {
                //这里会发起查询，获取当前在数据库中的实体。
                foreach (JObject jChild in jArray)
                {
                    var child = FindOrCreate(list, jChild);
                    DeserializeProperties(jChild, child);
                }
            }
        }

        private EntityList DeserializeList(Type listType, JArray jArray)
        {
            var entityType = EntityMatrix.FindByList(listType).EntityType;
            var repo = RF.Find(entityType);

            //先从数据库中找出所有提供了 Id 的实体。
            var idList = jArray.Cast<JObject>().Select(item => TryGetId(item))
                .Where(id => id != null).ToArray();
            var list = repo.GetByIdList(idList);

            //依次反序列化数组中的实体：
            //如果有 Id，则在数据库中查询出的列表 list 中查找出对应的实体，然后反序列化值。否则，直接构造新实体。
            foreach (JObject jEntity in jArray)
            {
                var child = FindOrCreate(list, jEntity);
                DeserializeProperties(jEntity, child);
            }

            return list;
        }

        private static Entity FindOrCreate(EntityList list, JObject jEntity)
        {
            var id = TryGetId(jEntity);
            Entity child = null;
            if (id != null)
            {
                child = list.Find(id, true);
            }
            if (child == null)
            {
                child = list.GetRepository().New();
                list.Add(child);
            }
            return child;
        }

        private static object TryGetId(JObject jEntity)
        {
            JToken jId = null;
            if (jEntity.TryGetValue(Entity.IdProperty.Name, StringComparison.OrdinalIgnoreCase, out jId))
            {
                return (jId as JValue).Value;
            }
            return null;
        }
    }
}