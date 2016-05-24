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
        public bool SerializeAsCamelProperty { get; set; } = true;

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
                    this.SerializeDomainComponent(value as IDomainComponent, jw);

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
                this.SerializeDomainComponent(res.Data as IDomainComponent, jw);
            }
            else
            {
                this.SerializeByJsonSerializer(res.Data, jw);
            }

            jw.WriteEndObject();
        }

        /// <summary>
        /// 使用指定的 JsonTextWriter 来序列化 <see cref="IDomainComponent"/> 类型。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="jw"></param>
        protected virtual void SerializeDomainComponent(IDomainComponent value, JsonTextWriter jw)
        {
            var serializer = CreateSerializer();
            serializer.UseCamelProperty = this.SerializeAsCamelProperty;
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

                    result = DeserializeDomainComponent(type, idContent != null ? idContent.Id : null, json);
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

        /// <summary>
        /// 反序列化 <see cref="IDomainComponent"/> 类型。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        protected virtual IDomainComponent DeserializeDomainComponent(Type type, string id, string json)
        {
            IDomainComponent result;
            var deserializer = CreateDeserializer();
            if (type.IsSubclassOf(typeof(Entity)))
            {
                var jObject = JObject.Parse(json);
                if (id != null)
                {
                    result = deserializer.DeserializeEntity(type, jObject, id);
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

            return result;
        }

        /// <summary>
        /// 创建一个 <see cref="AggtSerializer"/> 的对象。
        /// </summary>
        /// <returns></returns>
        protected virtual AggtSerializer CreateSerializer()
        {
            return new AggtSerializer();
        }

        /// <summary>
        /// 创建一个 <see cref="AggtDeserializer"/> 的对象。
        /// </summary>
        /// <returns></returns>
        private static AggtDeserializer CreateDeserializer()
        {
            return new AggtDeserializer();
        }

        /// <summary>
        /// 创建一个用于序列化的 JsonTextWriter。
        /// </summary>
        /// <param name="writeStream"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        protected virtual JsonTextWriter CreateJsonWriter(Stream writeStream, HttpContent content)
        {
            var writer = new StreamWriter(writeStream, this.SelectCharacterEncoding((content == null) ? null : content.Headers));
            var jw = new JsonTextWriter(writer);
            return jw;
        }

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