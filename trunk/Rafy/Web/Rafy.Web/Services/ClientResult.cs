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
using Rafy;
using Rafy.Web.Json;

namespace Rafy.Web.ClientMetaModel
{
    /// <summary>
    /// 客户端常用的结果返回值
    /// </summary>
    [Serializable]
    public class ClientResult : JsonModel
    {
        public const string SuccessProperty = "Success";
        public const string MessageProperty = "Message";

        /// <summary>
        /// 是否操作成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 操作的友好提示信息
        /// </summary>
        public string Message { get; set; }

        protected override void ToJson(LiteJsonWriter json)
        {
            json.WriteProperty(SuccessProperty, Success.Box());
            json.WritePropertyIf(MessageProperty, Message);
        }
    }
}