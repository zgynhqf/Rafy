using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.MetaModel;

using OEA.Library;
using OEA.Module;

namespace OEA
{
    /// <summary>
    /// Controller
    /// </summary>
    public interface IAsyncDataLoader : IAsyncDataContext
    {
        /// <summary>
        /// 显示的ObjectView
        /// </summary>
        ObjectView View { get; }

        /// <summary>
        /// 表示当前是否正自在异步加载的状态
        /// </summary>
        bool IsLoadingData { get; }

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
        void LoadDataAsync(Func<object> dataProvider);

        /// <summary>
        /// 使用自定义数据提供函数来进行异步数据加载
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="changedCallback"></param>
        void LoadDataAsync(Func<object> dataProvider, Action changedCallback);

        /// <summary>
        /// 使用最后一次使用的数据提供器重新加载数据。
        /// </summary>
        void ReloadDataAsync();

        /// <summary>
        /// 使用最后一次使用的数据提供器重新加载数据。
        /// </summary>
        /// <param name="changedCallback"></param>
        void ReloadDataAsync(Action changedCallback);
    }
}