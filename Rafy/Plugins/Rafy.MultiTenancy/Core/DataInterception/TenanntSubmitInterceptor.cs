/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20210912
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20210912 04:30
 * 
*******************************************************/

using Rafy.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.MultiTenancy.Core.DataInterception
{
    public class TenanntSubmitInterceptor : ISubmitInterceptor
    {
        public int SubmitInterceptorIndex { get; set; }

        public void Submit(SubmitArgs e, ISubmitInterceptorLink link)
        {
            if (e.Action == SubmitAction.Insert)
            {
                if (e.DataProvider.Repository.EntityMeta.GetIsMultiTenancyEnabled())
                {
                    TenantAwareEntityExtension.SetTenantId(
                        e.Entity,
                        MultiTenancyUtility.GetTenantId()
                    );
                }
            }

            link.InvokeNext(this, e);
        }
    }
}