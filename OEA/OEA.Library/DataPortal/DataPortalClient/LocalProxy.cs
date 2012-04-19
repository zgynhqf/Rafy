/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120419
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120419
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Server;
using OEA.Library;
using OEA.Utils;

namespace OEA.DataPortalClient
{
    /// <summary>
    /// 当机版本的访问代理类。
    /// </summary>
    public class LocalProxy : IDataPortalProxy
    {
        public bool IsServerRemote
        {
            get { return false; }
        }

        public DataPortalResult Fetch(Type objectType, object criteria, DataPortalContext context)
        {
            var server = new DataPortalFacade();
            return server.Fetch(objectType, criteria, context);
        }

        public DataPortalResult Update(object obj, DataPortalContext context)
        {
            //当机访问时，需要克隆一个新的对象，
            //这样，在底层 Update 更改 obj 时，不会影响上层的实体。
            //同时，这个被修改的实体也会被返回给上层。
            obj = ObjectCloner.Clone(obj);

            var server = new DataPortalFacade();
            return server.Update(obj, context);
        }
    }
}
