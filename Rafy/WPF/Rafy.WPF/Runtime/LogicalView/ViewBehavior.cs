using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Rafy.WPF
{
    /// <summary>
    /// 视图行为的基类
    /// </summary>
    public abstract class ViewBehavior
    {
        private LogicalView _view;

        /// <summary>
        /// 是否已经绑定到某个视图
        /// </summary>
        public bool IsAttached
        {
            get
            {
                return this._view != null;
            }
        }

        public LogicalView View
        {
            get
            {
                return _view;
            }
            internal set
            {
                if (value == null) throw new ArgumentNullException("value");
                if (this._view != null) throw new InvalidOperationException("一个行为只能附加到一个 View 上。");

                this._view = value;
            }
        }

        internal void Attach()
        {
            this.OnAttach();
        }

        protected abstract void OnAttach();
    }
}
