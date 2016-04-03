/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130514
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130514 09:43
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Rafy
{
    /// <summary>
    /// 一个可被锁定的集合。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SealableCollection<T> : Collection<T>
    {
        private bool _sealed;

        /// <summary>
        /// 是否已经被锁定。
        /// </summary>
        public bool IsSealed
        {
            get { return _sealed; }
        }

        /// <summary>
        /// 锁定本集合。
        /// </summary>
        public void Seal()
        {
            this._sealed = true;
        }

        /// <summary>
        /// 子类可调用此方法来取消本集合的锁定状态。
        /// </summary>
        protected void Unseal()
        {
            _sealed = false;
        }

        protected override void ClearItems()
        {
            this.CheckSealed();

            base.ClearItems();
        }

        protected override void InsertItem(int index, T item)
        {
            this.CheckSealed();

            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            this.CheckSealed();

            base.RemoveItem(index);
        }

        protected override void SetItem(int index, T item)
        {
            this.CheckSealed();

            base.SetItem(index, item);
        }

        private void CheckSealed()
        {
            if (this._sealed) throw new InvalidOperationException("该集合已经被锁定，不能更改。");
        }
    }
}