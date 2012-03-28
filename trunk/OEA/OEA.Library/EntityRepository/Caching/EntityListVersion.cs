/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111207
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111207
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.Library.Caching
{
    /// <summary>
    /// 逻辑上实体列表的版本号。
    /// </summary>
    public class EntityListVersion
    {
        public Type ClassRegion { get; set; }

        public Type ScopeClass { get; set; }

        public string ScopeId { get; set; }

        public DateTime Value { get; set; }

        /// <summary>
        /// 此属性需要依赖注入
        /// </summary>
        public static IEntityListVersionRepository Repository { get; private set; }

        public static void SetProvider(IEntityListVersionRepository provider)
        {
            Repository = provider;
        }
    }

    /// <summary>
    /// 实体列表版本号的仓库。
    /// （目前使用领域模型中的ScopeVersion进行存储。）
    /// </summary>
    public interface IEntityListVersionRepository
    {
        /// <summary>
        /// 获取指定的列表范围，如果不存储，则构造一个新的。
        /// </summary>
        /// <param name="region"></param>
        /// <param name="scopeClass"></param>
        /// <param name="scopeId"></param>
        /// <returns></returns>
        EntityListVersion GetOrNew(Type region, Type scopeClass, string scopeId);

        /// <summary>
        /// 添加一个指定的范围版本号
        /// </summary>
        /// <param name="region"></param>
        /// <param name="scopeClass"></param>
        /// <param name="scopeId"></param>
        /// <returns></returns>
        EntityListVersion Add(Type region, Type scopeClass, string scopeId);

        /// <summary>
        /// 获取一个指定的范围版本号
        /// </summary>
        /// <param name="region"></param>
        /// <param name="scopeClass"></param>
        /// <param name="scopeId"></param>
        /// <returns></returns>
        EntityListVersion Get(Type region, Type scopeClass, string scopeId);

        /// <summary>
        /// 清空所有版本号，所有缓存都会过期。
        /// </summary>
        void Clear();

        /// <summary>
        /// 通知整个表被改变
        /// </summary>
        /// <param name="region"></param>
        void UpdateVersion(Type region);

        /// <summary>
        /// 更新指定的范围的版本号
        /// </summary>
        /// <param name="region"></param>
        /// <param name="scopeClass"></param>
        /// <param name="scopeId"></param>
        void UpdateVersion(Type region, Type scopeClass, string scopeId);

        /// <summary>
        /// 如果要进行批量的更新，请先调用此方法。
        /// </summary>
        /// <returns></returns>
        IDisposable BeginBillSave();

        void EndBillSave();
    }
}
