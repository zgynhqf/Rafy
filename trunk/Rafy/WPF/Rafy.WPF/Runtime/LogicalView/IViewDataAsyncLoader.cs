/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：2010
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2010
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.WPF;

namespace Rafy.WPF
{
    /// <summary>
    /// Controller
    /// </summary>
    public interface IAsyncDataLoader : IAsyncDataContext
    {
        /// <summary>
        /// 显示的LogicalView
        /// </summary>
        LogicalView View { get; }

        /// <summary>
        /// 表示当前是否正自在异步加载的状态
        /// </summary>
        bool IsLoadingData { get; }

        /// <summary>
        /// 是否已经使用 DataLoader 查询过至少一次数据了。
        /// </summary>
        bool AnyLoaded { get; }

        /// <summary>
        /// 如果处于异步加载中，可以使用此方法停止异步加载。
        /// </summary>
        void CancelLoading();

        /// <summary>
        /// 异步加载所有数据
        /// </summary>
        void LoadDataAsync();

        /// <summary>
        /// 异步加载所有数据
        /// </summary>
        /// <param name="changedCallback"></param>
        void LoadDataAsync(Action changedCallback);

        /// <summary>
        /// 使用自定义数据提供函数来进行异步数据加载
        /// </summary>
        /// <param name="dataProvider"></param>
        void LoadDataAsync(Func<IDomainComponent> dataProvider);

        /// <summary>
        /// 使用自定义数据提供函数来进行异步数据加载
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="changedCallback"></param>
        void LoadDataAsync(Func<IDomainComponent> dataProvider, Action changedCallback);

        /// <summary>
        /// 使用上一次使用过的数据提供程序重新加载数据。
        /// 
        /// 如果还没有进行过任何加载，则抛出异常。
        /// </summary>
        void ReloadDataAsync();

        /// <summary>
        /// 使用上一次使用过的数据提供程序重新加载数据。
        /// 
        /// 如果还没有进行过任何加载，则抛出异常。
        /// </summary>
        /// <param name="changedCallback"></param>
        void ReloadDataAsync(Action changedCallback);
    }
}