/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110513
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110513
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.WPF
{
    /// <summary>
    /// 过程辨别器。
    /// </summary>
    /// <typeparam name="TProcessSource">应该是一个枚举类型，表示所有可能的过程。</typeparam>
    public class ProcessSourceIndicator<TProcessSource> : IDisposable
        where TProcessSource : struct
    {
        private int _enterCount = 0;

        private bool _lastEnterSuccess = false;

        private TProcessSource _currentProcess;

        /// <summary>
        /// 当前的过程
        /// </summary>
        public TProcessSource CurrentProcess
        {
            get { return this._currentProcess; }
        }

        /// <summary>
        /// 最后一次调用 TryEnterProcess 方法是否成功进入目标过程。
        /// </summary>
        public bool Success
        {
            get { return this._lastEnterSuccess; }
        }

        /// <summary>
        /// 尝试进入目标过程。
        /// 
        /// 使用本方法后，可以使用 Success 属性来检测是否成功。
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        public IDisposable TryEnterProcess(TProcessSource process)
        {
            this._lastEnterSuccess = false;

            if (this._enterCount == 0)
            {
                this._currentProcess = process;
                this._lastEnterSuccess = true;
            }

            this._enterCount++;

            return this;
        }

        /// <summary>
        /// 判断当前是否正处于目标流程中。
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        public bool Is(TProcessSource process)
        {
            return this._currentProcess.Equals(process);
        }

        void IDisposable.Dispose()
        {
            this._enterCount--;

            if (this._enterCount == 0) { this._currentProcess = default(TProcessSource); }
        }
    }
}