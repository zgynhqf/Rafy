/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20170315
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20170315 13:41
 * 
*******************************************************/

using System;
using Rafy.DataTableMigration.Contexts;
using Rafy.Domain;

namespace Rafy.DataTableMigration.Services
{
    /// <summary>
    /// 定义一个数据归档的服务。
    /// </summary>
    public interface IDataTableMigrationService : IProgress<DataTableMigrationEventArgs>
    {
        /// <summary>
        /// 当数据归档状态发生变化时引发此事件。
        /// </summary>
        event EventHandler<DataTableMigrationEventArgs> ProgressChanged;

        /// <summary>
        /// 执行数据归档。
        /// </summary>
        void ExecuteArchivingData();

        /// <summary>
        /// 存储到历史表。
        /// </summary>
        /// <param name="repository">表示当前 <see cref="component"/> 对应的仓库。</param>
        /// <param name="component">表示一个领域对象。</param>
        /// <param name="isSupportTree"> 是否是树形实体</param>
        void SaveToHistory(IRepository repository, EntityList component, bool isSupportTree);

        /// <summary>
        /// 从原始表中移除聚合。
        /// </summary>
        /// <param name="repository">表示当前 <see cref="entityList" /> 对应的仓库。</param>
        /// <param name="entityList">表示一个领域对象的集合。</param>
        void RemoveOriginData(IRepository repository, EntityList entityList);
    }
}