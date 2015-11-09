/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150522
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150522 12:11
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Web.Http
{
    class IdFromRouteHttpContent : HttpContent
    {
        private HttpContent _raw;
        private string _id;

        public IdFromRouteHttpContent(HttpContent raw, string id)
        {
            _raw = raw;
            _id = id;
            this.Headers.ContentType = raw.Headers.ContentType;
            this.Headers.ContentLength = raw.Headers.ContentLength;
        }

        public HttpContent Raw
        {
            get { return _raw; }
        }

        public string Id
        {
            get { return _id; }
        }

        protected override Task<Stream> CreateContentReadStreamAsync()
        {
            //虽然实现了这个方法，但是 EntityAwareJsonMediaTypeFormatter 不会使用这个值。
            return Task.FromResult<Stream>(new MemoryStream());
            //var tc = new TaskCompletionSource<Stream>();
            //tc.SetException(new NotSupportedException());
            //return tc.Task;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            throw new NotImplementedException();
        }

        protected override bool TryComputeLength(out long length)
        {
            throw new NotImplementedException();
        }
    }
}
