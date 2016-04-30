/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20141201
 * 运行环境：.NET 4.0
 * 版本号：1.1.16
 * 
 * 历史记录：
 * 创建文件 胡庆访 20141201 21:03
 * 只要是实体，不论是否有 Id 值，都使用自定义序列化 胡庆访 20141211 21:03
 * 支持实体列表的反序列化。 胡庆访 20150325
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
using Rafy.Domain.Serialization.Json;
using Rafy.ManagedProperty;
using Newtonsoft.Json.Serialization;
using System.Web.Http;

namespace Rafy.Web.Http
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
            base.SupportedEncodings.Add(new UTF8Encoding(false, true));
            base.SupportedEncodings.Add(new UnicodeEncoding(false, true, true));
            base.MediaTypeMappings.Add(new XmlHttpRequestHeaderMapping());
            this.SerializeAsCamelProperty = true;
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

        /// <summary>
        /// 是否使用舵峰式。 默认为 true。
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use camel property]; otherwise, <c>false</c>.
        /// </value>
        public bool SerializeAsCamelProperty { get; set; }

        /// <summary>
        /// JsonTextWriter 的配置方法。
        /// </summary>
        public Action<JsonTextWriter> WriterConfiguration { get; set; }

        /// <summary>
        /// AggtSerializer 的配置方法。
        /// </summary>
        public Action<AggtSerializer> SerializerConfiguration { get; set; }

        private void Serialize(Type type, object value, Stream writeStream, HttpContent content)
        {
            bool handled = false;

            //对 Result 类型进行特殊的处理。
            if (value is Result)
            {
                var res = (Result)value;
                using (var jw = this.CreateJsonWriter(writeStream, content))
                {
                    this.SerializeResult(res, jw);

                    jw.Flush();
                }

                handled = true;
            }
            //对实体进行特殊的处理。
            else if (value is IDomainComponent)
            {
                using (var jw = this.CreateJsonWriter(writeStream, content))
                {
                    this.SerializeAggt(value as IDomainComponent, jw);

                    handled = true;
                }
            }

            if (!handled)
            {
                SerializeByJsonSerializer(value, writeStream, content);
            }
        }

        /// <summary>
        /// 使用指定的 JsonTextWriter 来序列化 Result 类型。
        /// </summary>
        /// <param name="res"></param>
        /// <param name="jw"></param>
        protected virtual void SerializeResult(Result res, JsonTextWriter jw)
        {
            jw.WriteStartObject();
            jw.WritePropertyName(this.SerializeAsCamelProperty ? "success" : "Success");
            jw.WriteValue(res.Success);
            jw.WritePropertyName(this.SerializeAsCamelProperty ? "message" : "Message");
            jw.WriteValue(res.Message);
            jw.WritePropertyName(this.SerializeAsCamelProperty ? "statusCode" : "StatusCode");
            jw.WriteValue(res.StatusCode);

            jw.WritePropertyName(this.SerializeAsCamelProperty ? "data" : "Data");
            if (res.Data is IDomainComponent)
            {
                this.SerializeAggt(res.Data as IDomainComponent, jw);
            }
            else
            {
                this.SerializeByJsonSerializer(res.Data, jw);
            }

            jw.WriteEndObject();
        }

        /// <summary>
        /// 使用指定的 JsonTextWriter 来序列化 IDomainComponent 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="jw"></param>
        protected virtual void SerializeAggt(IDomainComponent value, JsonTextWriter jw)
        {
            var serializer = new AggtSerializer();
            serializer.UseCamelProperty = this.SerializeAsCamelProperty;
            if (this.SerializerConfiguration != null)
            {
                this.SerializerConfiguration(serializer);
            }
            serializer.Serialize(value, jw);
        }

        private async Task<object> Deserialize(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            var idContent = content as IdFromRouteHttpContent;
            if (idContent != null) content = idContent.Raw;

            var headers = (content == null) ? null : content.Headers;
            if (headers != null && headers.ContentLength == 0L)
            {
                return MediaTypeFormatter.GetDefaultValueForType(type);
            }

            object result = null;
            try
            {
                //对实体的属性进行特殊的处理。
                if (typeof(IDomainComponent).IsAssignableFrom(type))
                {
                    var json = await content.ReadAsStringAsync();

                    var deserializer = new AggtDeserializer();
                    if (type.IsSubclassOf(typeof(Entity)))
                    {
                        var jObject = JObject.Parse(json);
                        if (idContent != null)
                        {
                            result = deserializer.DeserializeEntity(type, jObject, idContent.Id);
                        }
                        else
                        {
                            result = deserializer.DeserializeEntity(type, jObject);
                        }
                    }
                    else
                    {
                        result = deserializer.Deserialize(type, json);
                    }
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

        private JsonTextWriter CreateJsonWriter(Stream writeStream, HttpContent content)
        {
            var writer = new StreamWriter(writeStream, this.SelectCharacterEncoding((content == null) ? null : content.Headers));
            var jw = new JsonTextWriter(writer);
            if (this.WriterConfiguration != null) { this.WriterConfiguration(jw); }
            return jw;
        }

        #region //CommonQueryCriteria

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
                SerializeByJsonSerializer(value, jsonTextWriter);
                jsonTextWriter.Flush();
            }
        }

        /// <summary>
        /// 使用 <see cref="JsonSerializer"/> 来进行序列化。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="jsonTextWriter"></param>
        protected void SerializeByJsonSerializer(object value, JsonTextWriter jsonTextWriter)
        {
            JsonSerializer jsonSerializer = JsonSerializer.Create();
            if (this.SerializeAsCamelProperty)
            {
                jsonSerializer.ContractResolver = new CamelCasePropertyNamesContractResolver();
            }

            jsonSerializer.Serialize(jsonTextWriter, value);
        }

        #endregion
    }
}