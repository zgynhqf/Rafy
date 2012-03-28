using System;

namespace OEA.Threading
{
    /// <summary>
    /// 任意开始，一起结束
    /// </summary>
    public interface IAsyncMultiActions
    {
        /// <summary>
        /// 最后一个异步任务执行完毕后
        /// </summary>
        event EventHandler LastActionEnded;
        /// <summary>
        /// 第一个异步任务开始执行时
        /// </summary>
        event EventHandler FirstActionStarted;

        /// <summary>
        /// 调用此方法时，任务直接进入调度队列中
        /// </summary>
        /// <param name="action"></param>
        void Execute(Action action);
    }
}
