/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120310
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120310
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Specialized;
using System.Collections;
using Newtonsoft.Json;

namespace Rafy.Web.Json
{
    /// <summary>
    /// 一个简单的 json 序列化基类。（不支持反序列化）
    /// </summary>
    [Serializable]
    public abstract class JsonModel
    {
        /// <summary>
        /// 返回当前对象对应的 json 字符串
        /// </summary>
        /// <returns></returns>
        public string ToJsonString()
        {
            return LiteJsonWriter.Convert(this);
        }

        /// <summary>
        /// 子类重写此方法来实现 json 序列化的自定义。
        /// </summary>
        /// <param name="json"></param>
        protected abstract void ToJson(LiteJsonWriter json);

        internal void ToJsonInternal(LiteJsonWriter json, JsonWriter writer)
        {
            writer.WriteStartObject();

            this.ToJson(json);

            writer.WriteEndObject();
        }

        /// <summary>
        /// Writes all properties by reflection.
        /// </summary>
        /// <param name="json">The json writer.</param>
        protected void WriteAllProperties(LiteJsonWriter json)
        {
            var properties = this.GetType().GetProperties();
            for (int i = 0, c = properties.Length; i < c; i++)
            {
                var property = properties[i];
                var value = property.GetValue(this, null);
                if (value is IEnumerable<JsonModel>)
                {
                    json.WriteProperty(property.Name, value as IEnumerable<JsonModel>);
                }
                else
                {
                    json.WriteProperty(property.Name, value);
                }
            }
        }
    }
}