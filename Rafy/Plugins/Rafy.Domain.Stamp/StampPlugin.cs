/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20151207
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20151207 15:07
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.ComponentModel;

namespace Rafy.Domain.Stamp
{
    /// <summary>
    /// <para>实体的跟踪戳插件。                                                                                       </para>
    /// <para>本插件为实体添加“创建时间”、“最后更新时间”、“创建人”、“最后更新人”等跟踪戳属性，并自动在数据层维护这些属性。        </para>
    /// <para>使用方法：                                                                                            </para>
    /// <para>1.映射实体的所有属性到数据库。                                                                          </para>
    /// <para>2.在用户登录成功后，需要设置 <see cref="RafyEnvironment.Principal"/> 的值。                             </para>
    /// </summary>
    public class StampPlugin : DomainPlugin
    {
        public override void Initialize(IApp app)
        {
            DataSaver.SubmitInterceptors.Add(typeof(StampSubmitInterceptor));
        }
    }
}