/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20210912
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20210912 04:29
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Domain.EntityPhantom
{
    class PhantomSubmitInterceptor : ISubmitInterceptor
    {
        public int SubmitInterceptorIndex { get; set; }

        public void Submit(SubmitArgs e, ISubmitInterceptorLink link)
        {
            if (e.Action == SubmitAction.Delete)
            {
                var dp = e.DataProvider;
                if (dp.Repository.EntityMeta.IsPhantomEnabled && !PhantomContext.DeleteRealData.Value)
                {
                    var entity = e.Entity;

                    //将整个组合的状态都设置好。
                    foreach (var item in CompositionEnumerator.Create(entity))
                    {
                        (item.GetRepository() as EntityRepository).LoadAllChildren(item);
                        item.SetIsPhantom(true);
                    }

                    //然后调用 DataSaver 来保存整个组合的数据。（不直接使用 Sql 语句，因为这里并不一定是使用 RDb 作为持久层。）
                    e.Action = SubmitAction.Update;

                    //link.InvokeNext(this, e);

                    //////由于删除后的实体的状态会变为‘New’，所以需要把这个字段的值重置。
                    ////entity.ResetProperty(EntityPhantomExtension.IsPhantomProperty);

                    ////已经调用数据层逻辑，退出。
                    //return;
                }
            }

            link.InvokeNext(this, e);
        }
    }
}
