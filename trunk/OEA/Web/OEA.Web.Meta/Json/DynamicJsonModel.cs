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

namespace OEA.Web.Json
{
    /// <summary>
    /// 可添加动态属性的 json 序列化对象
    /// </summary>
    [Serializable]
    public class DynamicJsonModel : JsonModel
    {
        private Dictionary<string, object> _properties = new Dictionary<string, object>();

        /// <summary>
        /// 是否已经添加了任何的动态属性
        /// </summary>
        protected bool HasDynamicProperty
        {
            get { return this._properties.Count > 0; }
        }

        public void SetProperty(string name, object value)
        {
            this._properties[name] = value;
        }

        protected override void ToJson(LiteJsonWriter json)
        {
            int i = 0, c = this._properties.Count;

            foreach (var kv in this._properties)
            {
                json.WriteProperty(kv.Key, kv.Value, i == c - 1);
                i++;
            }
        }
    }
}