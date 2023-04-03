//using Microsoft.AspNetCore.Mvc.Formatters;
//using Microsoft.Net.Http.Headers;
//using Newtonsoft.Json.Linq;
//using Rafy.Domain;
//using Rafy.Domain.Serialization.Json;
//using System.Reflection.Metadata;
//using System.Text;

//namespace Rafy.Web.Http
//{
//    /// <summary>
//    /// 用法：
//    /// var builder = WebApplication.CreateBuilder(args);
//    /// builder.Services.AddControllersWithViews(options =>
//    /// {
//    ///     options.InputFormatters.Insert(0, new EntityInputFormatter());
//    /// });
//    /// 
//    /// https://learn.microsoft.com/en-us/aspnet/core/web-api/advanced/custom-formatters?view=aspnetcore-7.0
//    /// </summary>
//    public class EntityInputFormatter : TextInputFormatter
//    {
//        public EntityInputFormatter()
//        {
//            this.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
//            base.SupportedEncodings.Add(Encoding.UTF8);
//            base.SupportedEncodings.Add(Encoding.Unicode);
//        }

//        protected override bool CanReadType(Type type)
//        {
//            return typeof(IDomainComponent).IsAssignableFrom(type);
//        }

//        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
//        {
//            var httpContext = context.HttpContext;

//            var logger = httpContext.RequestServices.GetRequiredService<ILogger<EntityInputFormatter>>();

//            var headers = httpContext.Request.Headers;
//            if (headers == null || headers.ContentLength > 0L)
//            {
//                using var reader = new StreamReader(httpContext.Request.Body, encoding);
//                {
//                    try
//                    {
//                        var json = await reader.ReadToEndAsync();

//                        var res = DeserializeDomainComponent(context.ModelType, json);
//                        if (res != null)
//                        {
//                            return InputFormatterResult.Success(res);
//                        }
//                    }
//                    catch (Exception exception)
//                    {
//                        if (logger != null)
//                        {
//                            logger.LogError(string.Empty, exception);
//                        }
//                    }
//                }
//            }

//            return InputFormatterResult.Failure();
//        }

//        /// <summary>
//        /// 反序列化 <see cref="IDomainComponent"/> 类型。
//        /// </summary>
//        /// <param name="type"></param>
//        /// <param name="json"></param>
//        /// <returns></returns>
//        protected virtual IDomainComponent DeserializeDomainComponent(Type type, string json)
//        {
//            IDomainComponent result;
//            var deserializer = CreateDeserializer();
//            if (type.IsSubclassOf(typeof(Entity)))
//            {
//                var jObject = JObject.Parse(json);
//                result = deserializer.DeserializeEntity(type, jObject);
//            }
//            else
//            {
//                result = deserializer.Deserialize(type, json);
//            }

//            return result;
//        }

//        /// <summary>
//        /// 创建一个 <see cref="AggtDeserializer"/> 的对象。
//        /// </summary>
//        /// <returns></returns>
//        protected virtual AggtDeserializer CreateDeserializer()
//        {
//            var serializer = new AggtDeserializer();
//            serializer.UpdatedEntityCreationMode = UpdatedEntityCreationMode.RequeryFromRepository;
//            return serializer;
//        }
//    }
//}
