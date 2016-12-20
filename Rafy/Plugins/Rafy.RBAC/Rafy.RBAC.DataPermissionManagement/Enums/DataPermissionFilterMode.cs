/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20161220
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20161220 11:40
 * 
*******************************************************/


namespace Rafy.RBAC.DataPermissionManagement
{
    /// <summary>
    /// 数据权限过滤模型
    /// </summary>
    public enum DataPermissionFilterMode
    {
        /// <summary>
        /// 当前用户
        /// </summary>
        CurrentUser = 1,
        /// <summary>
        /// 当前组织
        /// </summary>
        CurrentOrg = 2,
        /// <summary>
        ///当前组织及
        /// </summary>
        CurrentOrgAndLower = 3,
        /// <summary>
        /// 自定义
        /// </summary>
        Custom = 4
    }
}
