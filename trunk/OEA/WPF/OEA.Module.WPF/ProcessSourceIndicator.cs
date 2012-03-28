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

namespace OEA.Module.WPF
{
    internal class ProcessSourceIndicator<TProcessSourceEnum> : IDisposable
    {
        private int _version = 0;

        public TProcessSourceEnum ProcessSource { get; private set; }

        public IDisposable EnterProcess(TProcessSourceEnum process)
        {
            if (this._version == 0) { this.ProcessSource = process; }

            this._version++;

            return this;
        }

        void IDisposable.Dispose()
        {
            this._version--;

            if (this._version == 0) { this.ProcessSource = default(TProcessSourceEnum); }
        }
    }
}
