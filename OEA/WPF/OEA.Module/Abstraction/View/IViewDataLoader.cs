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
    public interface IViewDataLoader : IAsyncDataContext
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
        /// 根据指定参数查询出View使用的对象
        /// </summary>
        /// <param name="getListParam"></param>
        void GetObject(params object[] getListParam);

        void GetObject(string getListMethod, params object[] getListParam);

        /// <summary>
        /// 根据指定参数查询出View使用的对象
        /// </summary>
        /// <param name="getListParam"></param>
        void GetObjectAsync(params object[] getListParam);

        void GetObjectAsync(string getListMethod, params object[] getListParam);

        /// <summary>
        /// 使用自定义数据提供函数来进行异步数据加载
        /// </summary>
        /// <param name="dataProvider"></param>
        void GetObjectAsync(Func<object> dataProvider);

        object[] LastQueryParam { get; }
    }
}
