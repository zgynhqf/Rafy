
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rafy.Domain
{
    /// <summary>
    /// <para>一个简单的支持 OData 协议的查询器。</para>
    /// <para>
    /// OData 语法简介：
    /// http://www.cnblogs.com/PurpleTide/archive/2010/12/21/1912299.html
    /// http://www.cnblogs.com/PurpleTide/archive/2010/12/21/1912395.html
    /// </para>
    /// <para>支持的操作符：</para>
    /// <para>只支持以下 OData 操作符：</para>
    /// <para>    $orderby、$filter、$inlinecount、$expand                                                                       </para>
    /// <para>新操作符：                                                                                                         </para>
    /// <para>    $pageNumber：从 1 开始的页码；                                                                                  </para>
    /// <para>    $pageSize：一页中的数据量。                                                                                     </para>
    /// <para>$expand 说明：                                                                                                     </para>
    /// <para>    除了可以需要贪婪加载的属性列表，还可以指定属性名为 TreeChildren，表示贪婪加载树节点的所有子节点。                     </para>
    /// <para>$filter 说明：                                                                                                      </para>
    /// <para>    * 支持的对比操作符：eq,ne,lt,le,gt,ge。                                                                          </para>
    /// <para>    * 支持字符串的模糊匹配操作符：contains、startsWith、endsWith、notContains、notStartsWith、notEndsWith。            </para>
    /// <para>    * 对时间类型进行比较时，直接使用字符串来表示时间值，如：CreateTime lt '2014-12-18 10:30'。                          </para>
    /// <para>    * Or 与 And 没有优先级之分。                                                                                     </para>
    /// <para>    * 可以使用空值 null。注意：null 表示空值，而 'null' 则表示字符串值。                                                </para>
    /// <para>    * （另外，不支持对集合进行对比的操作 in 和 notIn，这两个操作需要转换为 A eq 1 or A eq 2 or A eq 3                    </para>
    /// <para>    * 特殊字符 如 ‘  “  \ 需要加转义\\                                                                                                                  </para>
    /// <para>    示例（详见源码单元测试）：                                                                                        </para>
    /// <para>        NickName eq 'huqf'                                                                                          </para>
    /// <para>        NickName eq 'hu\'qf'  or   NickName eq 'hu\"qf'   or   NickName eq 'hu\\qf'                                                                                        </para>
    /// <para>        NickName eq 'huqf' and UserName eq 'huqf'                                                                   </para>
    /// <para>        NickName eq 'huqf' or UserName eq 'huqf' and ActiveTimeStamp lt '2014-12-17 19:00'                          </para>
    /// <para>        NickName eq 'huqf' and UserName eq 'huqf' or ActiveTimeStamp lt '2014-12-17 19:00'                          </para>
    /// <para>        ActiveTimeStamp lt '2014-12-17 19:00' and (NickName eq 'huqf' or UserName eq 'huqf')                        </para>
    /// <para>        (NickName eq 'huqf' or UserName eq 'huqf') and (Email eq 'email' or Present eq 'persent')                   </para>
    /// <para>        (NickName eq 'huqf' or UserName eq 'huqf')                                                                  </para>
    /// <para>        (NickName eq 'huqf')                                                                                        </para>
    /// <para>        (ActiveTimeStamp lt '2014-12-17 19:00') and (((NickName eq 'huqf' or UserName eq 'huqf')))                  </para>
    /// <para>        (NickName eq 'huqf' or (Email eq 'email' or Present eq 'persent')) and UserName eq 'huqf'                   </para>
    /// </summary>
    [Serializable]
    public class ODataQueryCriteria : Criteria
    {
        /// <summary>
        /// 根据该属性进行排序。
        /// </summary>
        public string OrderBy { get; set; }

        /// <summary>
        /// 过滤器。
        /// 支持 OData 的六个操作符。
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// EagerLoadProperties
        /// </summary>
        public string Expand { get; set; }

        /// <summary>
        /// 如果某次查询结果是一棵完整的子树，那么必须设置此参数为 true，才可以把整个树标记为完整加载。
        /// </summary>
        public bool MarkTreeFullLoaded { get; set; }
    }
}