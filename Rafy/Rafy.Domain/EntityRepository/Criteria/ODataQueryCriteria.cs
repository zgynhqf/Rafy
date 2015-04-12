/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20141216
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20141216 13:45
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rafy.Domain
{
    /// <summary>
    /// 一个简单的支持 OData 协议的查询器。
    /// OData 语法简介：
    /// http://www.cnblogs.com/PurpleTide/archive/2010/12/21/1912299.html
    /// http://www.cnblogs.com/PurpleTide/archive/2010/12/21/1912395.html
    /// 
    /// 支持的操作符：
    /// 只支持以下 OData 操作符：
    ///     $orderby、$filter、$inlinecount、$expand
    /// 新操作符：
    ///     $pageNumber：从 1 开始的页码；
    ///     $pageSize：一页中的数据量。
    /// $expand 说明：
    ///     除了可以需要贪婪加载的属性列表，还可以指定属性名为 TreeChildren，表示贪婪加载树节点的所有子节点。
    /// $filter 说明：
    ///     * 支持的对比操作符：eq,ne,lt,le,gt,ge。
    ///     * 支持字符串的模糊匹配操作符：contains、startsWith、endsWith、notContains、notStartsWith、notEndsWith。
    ///     * 对时间类型进行比较时，直接使用字符串来表示时间值，如：CreateTime lt '2014-12-18 10:30'。
    ///     * Or 与 And 没有优先级之分。
    ///     * 可以使用空值 null。注意：null 表示空值，而 'null' 则表示字符串值。
    ///     示例（详见源码单元测试）：
    ///         NickName eq 'huqf'
    ///         NickName eq 'huqf' and UserName eq 'huqf'
    ///         NickName eq 'huqf' or UserName eq 'huqf' and ActiveTimeStamp lt '2014-12-17 19:00'
    ///         NickName eq 'huqf' and UserName eq 'huqf' or ActiveTimeStamp lt '2014-12-17 19:00'
    ///         ActiveTimeStamp lt '2014-12-17 19:00' and (NickName eq 'huqf' or UserName eq 'huqf')
    ///         (NickName eq 'huqf' or UserName eq 'huqf') and (Email eq 'email' or Present eq 'persent')
    ///         (NickName eq 'huqf' or UserName eq 'huqf')
    ///         (NickName eq 'huqf')
    ///         (ActiveTimeStamp lt '2014-12-17 19:00') and (((NickName eq 'huqf' or UserName eq 'huqf')))
    ///         (NickName eq 'huqf' or (Email eq 'email' or Present eq 'persent')) and UserName eq 'huqf'
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
    }
}