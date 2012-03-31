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
using SimpleCsla;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using OEA.Web.ClientMetaModel;
using OEA.Web.Json;

namespace OEA.Web.Services
{
    /// <summary>
    /// 跨 C/S，B/S 的服务基类
    /// </summary>
    [Serializable]
    public abstract class JsonService : ServiceBase
    {
        protected override sealed void DataPortal_Execute()
        {
            this.Execute();
        }

        /// <summary>
        /// 子类重写此方法实现具体的业务逻辑
        /// </summary>
        protected abstract void Execute();

        public JsonService Invoke()
        {
            return DataPortal.Execute(this);
        }
    }
}