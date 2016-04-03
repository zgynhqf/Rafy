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

namespace Rafy.Domain.Caching
{
    /// <summary>
    /// 逻辑上实体列表的版本号。
    /// 用于客户端缓存。
    /// </summary>
    public class EntityListVersion
    {
        /// <summary>
        /// 属性哪个实体类的范围
        /// </summary>
        public Type ClassRegion { get; set; }

        /// <summary>
        /// 是否基于某个父类型来进行分开缓存。
        /// </summary>
        public Type ScopeClass { get; set; }

        /// <summary>
        /// 如果是基于父类型来缓存，则这个属性表示当前列表对应的父对象的 Id。
        /// </summary>
        public string ScopeId { get; set; }

        /// <summary>
        /// 当前这个列表缓存的时间戳。
        /// </summary>
        public DateTime Value { get; set; }
    }

    /// <summary>
    /// 实体列表版本号的仓库。
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
        IDisposable BatchSaveScope();

        ///// <summary>
        ///// 在 BeginBillSave 的范围内，调用此方法后，才会把批量更新信息提交到服务端。
        ///// </summary>
        //void SubmitBillSave();
    }
}
