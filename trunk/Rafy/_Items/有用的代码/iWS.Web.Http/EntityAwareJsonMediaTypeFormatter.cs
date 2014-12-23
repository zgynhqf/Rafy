/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20141201
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20141201 21:03
 * 只要是实体，不论是否有 Id 值，都使用自定义序列化 胡庆访 20141211 21:03
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rafy;
using Rafy.Domain;
using Rafy.ManagedProperty;

namespace iWS.Web.Http
{
    /// <summary>
    /// 负责完成实体的序列化、反序列化。
    /// 同时使用 JSON.NET 完成其它对象的序列化、反序列化。
    /// </summary>
    public class EntityAwareJsonMediaTypeFormatter : MediaTypeFormatter
    {
        #region 实现基类方法

        public EntityAwareJsonMediaTypeFormatter()
        {
            this.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
        }

        public override bool CanReadType(Type type)
        {
            return true;
            //return type.IsSubclassOf(typeof(Entity));
        }

        public override bool CanWriteType(Type type)
        {
            return true;
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            return Task.Run(() => Serialize(type, value, writeStream, content));
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            return Deserialize(type, readStream, content, formatterLogger);
        }

        #endregion

        private void Serialize(Type type, object value, Stream writeStream, HttpContent content)
        {
            //对实体的属性进行特殊的处理。
            if (value is IDomainComponent)
            {
                var serializer = new EntitySerializer();
                var writer = new StreamWriter(writeStream, this.SelectCharacterEncoding((content == null) ? null : content.Headers));
                serializer.Serialize(value as IDomainComponent, writer);
            }
            else
            {
                SerializeByJsonSerializer(value, writeStream, content);
            }
        }

        private async Task<object> Deserialize(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            var headers = (content == null) ? null : content.Headers;
            if (headers != null && headers.ContentLength == 0L)
            {
                return MediaTypeFormatter.GetDefaultValueForType(type);
            }

            object result = null;
            try
            {
                var strContent = await content.ReadAsStringAsync();

                //对实体的属性进行特殊的处理。
                if (type.IsSubclassOf(typeof(Entity)))
                {
                    var jObject = JObject.Parse(strContent);

                    result = DesrializeEntity(type, jObject);
                }
                //else if (type == typeof(CommonQueryCriteria))
                //{
                //    result = DesrializeCommonQueryCriteria(strContent);
                //}
                else
                {
                    result = DeserializeByJsonSerializer(type, readStream, headers);
                }
            }
            catch (Exception exception)
            {
                if (formatterLogger != null)
                {
                    formatterLogger.LogError(string.Empty, exception);
                }

                result = MediaTypeFormatter.GetDefaultValueForType(type);
            }

            return result;
        }

        #region CommonQueryCriteria

        //private CommonQueryCriteria DesrializeCommonQueryCriteria(string strContent)
        //{
        //    var criteria = new CommonQueryCriteria();

        //    var json = JObject.Parse(strContent);

        //    var orderBy = json.Property("$orderby");
        //    if (orderBy != null)
        //    {
        //        criteria.OrderBy = orderBy.Value.Value<string>();
        //    }

        //    var jPageNumber = json.Property("$pageNumber");
        //    if (jPageNumber != null)
        //    {
        //        int pageNumber = jPageNumber.Value.Value<int>();
        //        int pageSize = 10;
        //        var jPageSize = json.Property("$pageSize");
        //        if (jPageSize != null)
        //        {
        //            pageSize = jPageSize.Value.Value<int>();
        //        }

        //        var pagingInfo = new PagingInfo(pageNumber, pageSize);
        //        criteria.PagingInfo = pagingInfo;
        //    }

        //    //filter
        //    var jFilter = json.Property("$filter");
        //    if (jFilter != null)
        //    {
        //        var filter = jFilter.Value.Value<string>();
        //        ParseFilter(criteria, filter);
        //    }

        //    return criteria;
        //}

        //private void ParseFilter(CommonQueryCriteria criteria, string filter)
        //{
        //    //to do
        //}

        #endregion

        #region Entity

        /// <summary>
        /// 实体的自定义反序列化方法。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="strContent"></param>
        /// <returns></returns>
        private static Entity DesrializeEntity(Type type, JObject jObject)
        {
            Entity entity = null;

            JToken jId = null;
            if (jObject.TryGetValue(Entity.IdProperty.Name, StringComparison.OrdinalIgnoreCase, out jId))
            {
                var id = (jId as JValue).Value;
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
        private static void DeserializeProperties(JObject jObject, Entity entity)
        {
            var properties = entity.PropertiesContainer.GetAvailableProperties();
            foreach (var mp in properties)
            {
                if (mp != Entity.IdProperty)
                {
                    JToken jValue = null;
                    if (jObject.TryGetValue(mp.Name, StringComparison.OrdinalIgnoreCase, out jValue))
                    {
                        if (mp is IListProperty)
                        {
                            DeserializeList(entity, mp as IListProperty, jValue as JArray);
                        }
                        //一般属性。
                        else
                        {
                            var value = (jValue as JValue).Value;
                            entity.SetProperty(mp, value, ManagedPropertyChangedSource.FromUIOperating);
                        }
                    }
                }
            }

            //using (var jsonTextReader = new StringReader(strContent))
            //{
            //    var jsonSerializer = JsonSerializer.Create(this.SerializerSettings);
            //    jsonSerializer.Populate(jsonTextReader, entity);
            //}
        }

        private static void DeserializeList(Entity entity, IListProperty listProperty, JArray jArray)
        {
            var isNew = entity.PersistenceStatus == PersistenceStatus.New;
            if (isNew)
            {
                var list = entity.GetLazyList(listProperty);
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
                var list = entity.GetLazyList(listProperty);
                foreach (JObject jChild in jArray)
                {
                    Entity child = null;
                    JToken jId = null;
                    if (jChild.TryGetValue(Entity.IdProperty.Name, StringComparison.OrdinalIgnoreCase, out jId))
                    {
                        child = list.Find((jId as JValue).Value);
                    }
                    if (child == null)
                    {
                        child = list.GetRepository().New();
                        list.Add(child);
                    }
                    DeserializeProperties(jChild, child);
                }
            }
        }

        #endregion

        #region Other

        /// <summary>
        /// 这个方法 Copy 自 ASP.NET MVC 源码。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="readStream"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        private object DeserializeByJsonSerializer(Type type, Stream readStream, HttpContentHeaders headers)
        {
            Encoding encoding = this.SelectCharacterEncoding(headers);
            using (var jsonTextReader = new JsonTextReader(new StreamReader(readStream, encoding)))
            //using (var jsonTextReader = new JsonTextReader(new StringReader(strContent)))
            {
                jsonTextReader.CloseInput = false;
                //jsonTextReader.MaxDepth = this.MaxDepth;
                var jsonSerializer = JsonSerializer.Create();
                //var jsonSerializer = JsonSerializer.Create(this.SerializerSettings);
                //if (formatterLogger != null)
                //{
                //    jsonSerializer.Error += delegate(object sender, ErrorEventArgs e)
                //    {
                //        Exception error = e.get_ErrorContext().get_Error();
                //        formatterLogger.LogError(e.get_ErrorContext().get_Path(), error);
                //        e.get_ErrorContext().set_Handled(true);
                //    });
                //}
                return jsonSerializer.Deserialize(jsonTextReader, type);
            }
        }

        private void SerializeByJsonSerializer(object value, Stream writeStream, HttpContent content)
        {
            Encoding encoding = this.SelectCharacterEncoding((content == null) ? null : content.Headers);

            using (var jsonTextWriter = new JsonTextWriter(new StreamWriter(writeStream, encoding)))
            {
                jsonTextWriter.CloseOutput = false;
                JsonSerializer jsonSerializer = JsonSerializer.Create();
                jsonSerializer.Serialize(jsonTextWriter, value);
                jsonTextWriter.Flush();
            }
        }

        #endregion
    }
}