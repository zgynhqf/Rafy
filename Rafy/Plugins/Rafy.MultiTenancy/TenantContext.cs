/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20141222
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20141222 16:53
 * 
*******************************************************/

namespace Rafy.MultiTenancy
{
    /// <summary>
    /// 多租户上下文
    /// </summary>
    public static class TenantContext
    {
        /// <summary>
        /// 当前上下文环境中的多租户 Id。
        /// </summary>
        public static readonly AppContextItem<string> TenantId =
            new AppContextItem<string>("Rafy.MultiTenancy.TenantContext.TenantId");
    }
}