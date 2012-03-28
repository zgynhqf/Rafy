/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110713
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110713
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using SimpleCsla.Wpf;
using System.Windows.Threading;
using OEA.Module.WPF;
using System.Windows;

namespace OEA
{
    /// <summary>
    /// 在 CslaDataProvider 的基础上增加了一个更加通用的数据获取方案器。
    /// </summary>
    public class OEADataProvider : CslaDataProvider
    {
        /// <summary>
        /// 数据获取器
        /// </summary>
        public Func<object> DataProducer { get; set; }

        protected override QueryRequest CreateRequest()
        {
            if (this.DataProducer == null) { return base.CreateRequest(); }

            return new OEAQueryRequest()
            {
                DataProducer = this.DataProducer
            };
        }

        protected override QueryResult DoQueryCore(QueryRequest request)
        {
            var req = request as OEAQueryRequest;

            if (req == null) { return base.DoQueryCore(request); }
            else return DoCommonQuery(req);
        }

        private static QueryResult DoCommonQuery(OEAQueryRequest req)
        {
            var result = new QueryResult();

            try
            {
                result.Data = req.DataProducer();
            }
            catch (Exception e)
            {
                result.Exception = e;
            }

            return result;
        }

        private class OEAQueryRequest : CslaDataProvider.QueryRequest
        {
            public Func<object> DataProducer { get; set; }
        }

        /// <summary>
        /// 在CslaDataProvider的基础上增加了： 错误处理。
        /// </summary>
        /// <param name="newData"></param>
        /// <param name="error"></param>
        /// <param name="completionWork"></param>
        /// <param name="callbackArguments"></param>
        protected override void OnQueryFinished(object newData, Exception error, DispatcherOperationCallback completionWork, object callbackArguments)
        {
            base.OnQueryFinished(newData, error, completionWork, callbackArguments);
            if (null != error)
            {
                Logger.LogError("CSLADataProvider获取数据报错", error);
                Action<Exception> action = e => e.ManageException();
                Application.Current.Dispatcher.Invoke(action, error);
            }
        }
    }
}