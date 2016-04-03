/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111110
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111110
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.ManagedProperty
{
    /// <summary>
    /// 一个可以被冻结的对象
    /// </summary>
    public abstract class FreezableMeta
    {
        private bool _frozen;

        /// <summary>
        /// 返回当前的对象是否已经被冻结了。
        /// 子类可以在对状态进行更新时检查此属性。
        /// </summary>
        public bool IsFrozen
        {
            get { return this._frozen; }
        }

        /// <summary>
        /// 冻结本对象。
        /// 冻结后，对象变为不可变对象。
        /// </summary>
        internal void Freeze()
        {
            if (!this._frozen)
            {
                this._frozen = true;
            }
        }

        /// <summary>
        /// for unit test
        /// </summary>
        internal void Unfreeze()
        {
            this._frozen = false;
        }

        /// <summary>
        /// 调用此方法保证本对象还没有被冻结。否则会抛出异常。
        /// </summary>
        protected void CheckUnFrozen()
        {
            if (this._frozen) throw new InvalidOperationException("本对象已经被冻结，不可再改变！");
        }
    }
}