using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Threading
{
    /// <summary>
    /// 这个只有准备好了任务后，再一起开始
    /// </summary>
    public interface IParallelActions
    {
        /// <summary>
        /// 可同时运行的最大线程数。
        /// </summary>
        int MaxThreadCount { get; set; }

        /// <summary>
        /// 准备需要执行的“非主任务”
        /// </summary>
        /// <param name="action"></param>
        void Prepare(Action action);

        /// <summary>
        /// 把所有的action清空。
        /// </summary>
        void Clear();

        /// <summary>
        /// 执行所有Action。
        /// 执行完毕，函数返回。
        /// </summary>
        void RunAll();
    }
}
