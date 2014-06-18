/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120220
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120220
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.CodeDom.Compiler;
using System.Collections;
using Newtonsoft.Json;

namespace Rafy.Web.Json
{
    /// <summary>
    /// 轻量级的 Json Writer
    /// </summary>
    public class LiteJsonWriter
    {
        /// <summary>
        /// Converts the specified model to json.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        internal static string Convert(JsonModel model)
        {
            var converter = new LiteJsonWriter();

            using (var jsonWriter = new StringWriter())
            {
                converter._writer = new JsonTextWriter(jsonWriter)
                {
                    QuoteName = false,
                };
                if (RafyEnvironment.IsDebuggingEnabled)
                {
                    converter._writer.Formatting = Formatting.Indented;
                }

                //使用 JsonSerializer 会通过反射把对象的所有属性进行序列化，这时，不能对这个过程进行控制。
                //var serializer = new JsonSerializer();
                //serializer.Serialize(converter._writer, model);

                converter.WriteJsonModel(model);

                var json = jsonWriter.ToString();

                return json;
            }
        }

        private JsonWriter _writer;

        private void WriteJsonModel(JsonModel model)
        {
            model.ToJsonInternal(this, _writer);
        }

        /// <summary>
        /// Writes the property if value is not null.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void WritePropertyIf(string name, object value)
        {
            if (value == null) { return; }

            //如果数组中没有元素，则直接返回。
            var array = value as IEnumerable<JsonModel>;
            if (array != null && !array.Any()) { return; }

            this.WriteProperty(name, value);
        }

        /// <summary>
        /// Writes the property.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void WriteProperty(string name, object value)
        {
            _writer.WritePropertyName(name);
            if (value is IEnumerable<JsonModel>)
            {
                this.WriteChildren(value as IEnumerable<JsonModel>);
            }
            else if (value is JsonModel)
            {
                this.WriteJsonModel(value as JsonModel);
            }
            else
            {
                this.WriteValue(value);
            }
        }

        private void WriteChildren(IEnumerable<JsonModel> children)
        {
            _writer.WriteStartArray();

            foreach (var child in children)
            {
                this.WriteJsonModel(child);
            }

            _writer.WriteEndArray();
        }

        private void WriteValue(object value)
        {
            //if (value is Enum || value is Guid)
            //{
            //    //字符串需要处理转义字符。
            //    var strValue = value.ToString().Replace("\"", "\\\"")
            //        .Replace("\r", "\\r")
            //        .Replace("\n", "\\n");

            //    //并添加引号。
            //    _writer.WriteValue(strValue);
            //    return;
            //}

            if (value is Array)
            {
                //if (value is byte[])
                //{
                //    return string.Format("'bytes[{0}]'", (value as Array).Length);
                //}

                _writer.WriteStartArray();
                foreach (var item in value as Array)
                {
                    _writer.WriteValue(item);
                }
                _writer.WriteEndArray();
                return;
            }

            //由于时间类型在 json.net 中序列化时，是使用格式："\/Date(1378278235827+0800)\/"，
            //而 ExtJs 无法将这个字符串转换为客户端实体的属性，所以这里特殊处理一下，
            //直接返回时间的字符串表示类型。
            if (value is DateTime)
            {
                value = value.ToString();
            }

            _writer.WriteValue(value);
        }

        //public static string CommonToJson(object model)
        //{
        //    using (var buffer = new MemoryStream())
        //    {
        //        var knownTypes = new Type[] { typeof(List<int>), typeof(byte[]), typeof(DateTimeOffset), typeof(DateTime) };
        //        var serializer = new DataContractJsonSerializer(typeof(object), knownTypes);
        //        serializer.WriteObject(buffer, model);

        //        var bytes = buffer.ToArray();
        //        var json = Encoding.UTF8.GetString(bytes);

        //        json = Regex.Replace(json, @"\\/", "/");
        //        MatchEvaluator me = match =>
        //        {
        //            string sRet = string.Empty;

        //            try
        //            {
        //                System.DateTime dt = new DateTime(1970, 1, 1);
        //                dt = dt.AddMilliseconds(long.Parse(match.Groups["ms"].Value));
        //                dt = dt.ToLocalTime();
        //                sRet = dt.ToString("yyyy-MM-dd HH:mm:ss");
        //            }
        //            catch { }

        //            return sRet;
        //        };
        //        json = Regex.Replace(json, @"/Date\((?<ms>\d+)[\+\-]\d+\)/", me);

        //        return json;
        //    }
        //}
    }
}