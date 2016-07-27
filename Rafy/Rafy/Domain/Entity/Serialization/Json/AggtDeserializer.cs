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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;
using Rafy;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.Reflection;
using Rafy.Utils;

namespace Rafy.Domain.Serialization.Json
{
    /// <summary>
    /// 实体反序列化器。
    /// 序列化后的数据只是暴露了实体的数据，而忽略了实体的状态。
    /// </summary>
    public class AggtDeserializer
    {
        private UpdatedEntityCreationMode _creationMode = UpdatedEntityCreationMode.CreateNewInstance;

        /// <summary>
        /// 更新实体时，实体的创建模式。
        /// 默认值：<see cref="UpdatedEntityCreationMode.CreateNewInstance"/>。
        /// </summary>
        public UpdatedEntityCreationMode UpdatedEntityCreationMode
        {
            get { return _creationMode; }
            set { _creationMode = value; }
        }

        /// <summary>
        /// 反序列化时，需要把哪个属性的认为是树型子属性。默认为 "TreeChildren".
        /// </summary>
        public string TreeChildrenProperty { get; set; } = AggtSerializer.TreeChildrenProperty;

        /// <summary>
        /// 实体的 Json 中可以使用这个属性来指定实体的状态。值是该枚举的名称。
        /// </summary>
        public string PersistenceStatusProperty { get; set; } = "persistenceStatus";

        /// <summary>
        /// 是否把未知的属性都反序列化为动态属性？
        /// 默认为 false。
        /// </summary>
        public bool UnknownAsDynamicProperties { get; set; }

        /// <summary>
        /// 实体或实体列表的自定义反序列化方法。
        /// </summary>
        /// <param name="type">传入实体类型或实体列表类型。</param>
        /// <param name="json">要反序列化的 Json。</param>
        /// <returns></returns>
        public IDomainComponent Deserialize(Type type, string json)
        {
            if (type.IsSubclassOf(typeof(Entity)))
            {
                var jObject = JObject.Parse(json);
                return this.DeserializeEntity(type, jObject);
            }

            if (type.IsSubclassOf(typeof(EntityList)))
            {
                var jArray = JArray.Parse(json);
                return this.DeserializeList(type, jArray);
            }

            throw new ArgumentOutOfRangeException("type", "只支持实体和实体列表类型的反序列化。");
        }

        /// <summary>
        /// 实体的自定义反序列化方法。
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="jObject">The j object.</param>
        /// <returns></returns>
        public Entity DeserializeEntity(Type type, JObject jObject)
        {
            var id = TryGetId(jObject);

            return DeserializeEntity(type, jObject, id);
        }

        /// <summary>
        /// 实体的自定义反序列化方法。
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="jObject">The j object.</param>
        /// <param name="id">实体的标识。如果已经在 jObject 中指定时，此参数可以传入 null。</param>
        /// <returns></returns>
        public Entity DeserializeEntity(Type type, JObject jObject, object id)
        {
            Entity entity = null;
            if (id != null)
            {
                if (_creationMode == UpdatedEntityCreationMode.RequeryFromRepository)
                {
                    var repository = RF.Find(type);
                    entity = repository.GetById(id);
                }
                else
                {
                    entity = Activator.CreateInstance(type) as Entity;
                    entity.PersistenceStatus = PersistenceStatus.Unchanged;
                }
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
        /// 反序列化指定的数组为一个实体的列表。
        /// </summary>
        /// <param name="listType"></param>
        /// <param name="jArray"></param>
        /// <returns></returns>
        public EntityList DeserializeList(Type listType, JArray jArray)
        {
            var entityType = EntityMatrix.FindByList(listType).EntityType;
            var repo = RF.Find(entityType);

            //构造或查询出数据对应的实体列表。
            EntityList list = null;
            if (_creationMode == UpdatedEntityCreationMode.RequeryFromRepository)
            {
                //先从数据库中找出所有提供了 Id 的实体。
                var idList = jArray.Cast<JObject>().Select(item => TryGetId(item))
                    .Where(id => id != null).ToArray();
                list = repo.GetByIdList(idList);
            }
            else
            {
                list = repo.NewList();
            }

            //依次反序列化数组中的实体：
            //如果有 Id，则在数据库中查询出的列表 list 中查找出对应的实体，然后反序列化值。否则，直接构造新实体。
            for (int i = 0, c = jArray.Count; i < c; i++)
            {
                var jEntity = jArray[i] as JObject;
                var child = FindOrCreate(list, jEntity);
                DeserializeProperties(jEntity, child);
            }

            return list;
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
                var mp = properties.Find(propertyName, true) as IProperty;
                if (mp != null)
                {
                    //只读属性不需要反序列化。
                    if (mp.IsReadOnly) continue;
                    //幽灵属性也不需要处理。
                    if (mp == EntityConvention.Property_IsPhantom) continue;

                    if (mp is IListProperty)
                    {
                        DeserializeList(entity, mp as IListProperty, jValue as JArray);
                    }
                    else if (mp is IRefEntityProperty)
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
                    //一般属性。
                    else
                    {
                        #region 处理不同类型的 value

                        object value = null;

                        if (jValue is JArray)
                        {
                            #region 对于数组的泛型列表类型，需要进行特殊的处理。

                            var jArray = jValue as JArray;
                            var propertyType = mp.PropertyType;
                            if (propertyType.IsArray)//string[]
                            {
                                var elementType = propertyType.GetElementType();
                                var array = Array.CreateInstance(elementType, jArray.Count);
                                for (int i = 0, c = jArray.Count; i < c; i++)
                                {
                                    var itemTyped = TypeHelper.CoerceValue(elementType, jArray[i]);
                                    array.SetValue(itemTyped, i);
                                }
                                value = array;
                            }
                            else if (TypeHelper.IsGenericType(propertyType, typeof(List<>)))//List<string>
                            {
                                var elementType = propertyType.GetGenericArguments()[0];
                                var list = Activator.CreateInstance(propertyType) as IList;
                                for (int i = 0, c = jArray.Count; i < c; i++)
                                {
                                    var itemTyped = TypeHelper.CoerceValue(elementType, jArray[i]);
                                    list.Add(itemTyped);
                                }
                                value = list;
                            }
                            else
                            {
                                //如果不是数组类型或者泛型列表类型的属性，则不支持反序列化。
                                //do nothing;
                            }

                            #endregion
                        }
                        else
                        {
                            value = (jValue as JValue).Value;

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
                        }

                        #endregion

                        entity.SetProperty(mp, value, ManagedPropertyChangedSource.FromUIOperating);
                    }
                }
                else
                {
                    //PersistenceStatus:如果指定了状态，则主动设置该实体的状态。
                    if (propertyName.EqualsIgnoreCase(this.PersistenceStatusProperty))
                    {
                        #region 处理：PersistenceStatus

                        var value = (jValue as JValue).Value;
                        var status = (PersistenceStatus)Enum.Parse(typeof(PersistenceStatus), value.ToString(), true);
                        entity.PersistenceStatus = status;

                        #endregion
                    }
                    //TreeChildren:如果指定了树子节点列表，则也需要加载进来。
                    else if (propertyName.EqualsIgnoreCase(this.TreeChildrenProperty))
                    {
                        #region 处理：TreeChildren

                        var jArray = jValue as JArray;
                        if (jArray != null)
                        {
                            var treeChildren = entity.TreeChildren;
                            for (int i = 0, c = jArray.Count; i < c; i++)
                            {
                                var child = this.DeserializeEntity(entity.GetType(), jArray[i] as JObject);
                                treeChildren.LoadAdd(child);
                            }
                            treeChildren.MarkLoaded();
                        }

                        #endregion
                    }
                    else if (this.UnknownAsDynamicProperties)
                    {
                        #region 处理动态属性

                        var jValueObj = jValue as JValue;
                        if (jValueObj != null)
                        {
                            entity.SetDynamicProperty(propertyName, jValueObj.Value);
                        }

                        #endregion
                    }
                }
            }

            //可以使用 Json.NET 来遍历给实体属性赋值。
            //using (var jsonTextReader = new StringReader(strContent))
            //{
            //    var jsonSerializer = JsonSerializer.Create(this.SerializerSettings);
            //    jsonSerializer.Populate(jsonTextReader, entity);
            //}
        }

        private void DeserializeList(Entity entity, IListProperty listProperty, JArray jArray)
        {
            //构造 List 对象
            EntityList list = null;
            if (entity.FieldExists(listProperty) || entity.IsNew)
            {
                list = entity.GetLazyList(listProperty);
            }
            else
            {
                if (_creationMode == UpdatedEntityCreationMode.RequeryFromRepository)
                {
                    list = entity.GetLazyList(listProperty);
                }
                else
                {
                    var listRepository = RepositoryFactoryHost.Factory.FindByEntity(listProperty.ListEntityType);
                    list = listRepository.NewList();
                    entity.LoadProperty(listProperty, list);
                }
            }

            //反序列化其中的每一个元素。
            if (entity.IsNew)
            {
                var listRepository = list.GetRepository();
                foreach (JObject jChild in jArray)
                {
                    var child = listRepository.New();
                    DeserializeProperties(jChild, child);
                    list.Add(child);
                }
            }
            else
            {
                //这里会发起查询，获取当前在数据库中的实体。
                for (int i = 0, c = jArray.Count; i < c; i++)
                {
                    var jChild = jArray[i] as JObject;
                    var child = FindOrCreate(list, jChild);
                    DeserializeProperties(jChild, child);
                }
            }
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

                //在创建实体时，如果有 Id，表示是更新，这时创建的实体的初始状态应该是   
                if (id != null)
                {
                    child.PersistenceStatus = PersistenceStatus.Unchanged;
                }
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

    /// <summary>
    /// 更新实体时，实体的创建模式。
    /// 更新时是否需要把实体从仓库中查询出来。
    /// </summary>
    public enum UpdatedEntityCreationMode
    {
        /// <summary>
        /// 直接创建一个新的实体实例，然后在这个实例上进行属性的反序列化。
        /// 如果反序列化时，JSON 中给出了所有的实体属性，则可以需要使用此配置。这样可以减少数据库访问次数。
        /// </summary>
        CreateNewInstance,
        /// <summary>
        /// 根据 Id 从仓库中把旧实体查询出来，然后在这个实例上进行属性的反序列化。
        /// 如果反序列化时，JSON 中没有给出所有的实体属性，则需要使用此配置，否则会导致一些属性值丢失。
        /// </summary>
        RequeryFromRepository
    }
}