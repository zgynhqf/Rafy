/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20220226
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20220226 19:29
 * 
*******************************************************/


using Rafy.DataPortal;
using System;
using System.Runtime.Serialization;

namespace Rafy.Grpc
{
    /// <summary>
    /// Grpc 调用请求对象。
    /// </summary>
    [Serializable]
    internal class GrpcCallRequest
    {
        public object Instance;
        public string Method;
        public object[] Arguments;
        public DataPortalContext Context;
    }
}