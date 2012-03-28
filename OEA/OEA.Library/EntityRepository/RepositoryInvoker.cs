/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110215
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100215
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.Library
{
    public static class RepositoryInvoker<TEntity>
    {
        public static Entity Get(object param)
        {
            return Get(new object[] { param });
        }

        public static EntityList GetList()
        {
            return GetList(new object[0]);
        }

        public static EntityList GetList(object param)
        {
            return GetList(new object[] { param });
        }

        public static EntityList GetList(object p1, object p2)
        {
            return GetList(new object[] { p1, p2 });
        }

        public static EntityList GetList(object p1, object p2, object p3)
        {
            return GetList(new object[] { p1, p2, p3 });
        }

        public static EntityList GetList(object p1, object p2, object p3, object p4)
        {
            return GetList(new object[] { p1, p2, p3, p4 });
        }

        public static EntityList GetList(object p1, object p2, object p3, object p4, object p5)
        {
            return GetList(new object[] { p1, p2, p3, p4, p5 });
        }

        private static Entity Get(params object[] param)
        {
            var repository = RF.Create(typeof(TEntity));
            return repository.Get(param);
        }

        private static EntityList GetList(params object[] param)
        {
            var repository = RF.Create(typeof(TEntity));
            return repository.GetListImplicitly(param);
        }
    }
}
