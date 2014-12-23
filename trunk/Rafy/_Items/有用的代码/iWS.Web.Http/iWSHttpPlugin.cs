/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20141217
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20141217 16:38
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Rafy.ComponentModel;

namespace iWS.Web.Http
{
    /// <summary>
    /// 加这个 Plugin 是因为使用了 RepositoryODataQueryExtension。
    /// </summary>
    public class iWSHttpPlugin : DomainPlugin
    {
        public override void Initialize(IApp app)
        {
        }
    }
}