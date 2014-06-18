/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121011 10:54
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121011 10:54
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;

namespace Rafy.WPF.Controls
{
    internal class InternalCollectionChangedEventManager : WeakEventManager
    {
        #region SingleTon

        private static InternalCollectionChangedEventManager CurrentManager
        {
            get
            {
                var mgrType = typeof(InternalCollectionChangedEventManager);
                var mgr = WeakEventManager.GetCurrentManager(mgrType) as InternalCollectionChangedEventManager;
                if (mgr == null)
                {
                    mgr = new InternalCollectionChangedEventManager();
                    WeakEventManager.SetCurrentManager(mgrType, mgr);
                }
                return mgr;
            }
        }

        private InternalCollectionChangedEventManager() { }

        #endregion

        #region 方便的方法

        public static void AddListener(TreeGridColumnCollection source, IWeakEventListener listener)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (listener == null)
            {
                throw new ArgumentNullException("listener");
            }
            InternalCollectionChangedEventManager.CurrentManager.ProtectedAddListener(source, listener);
        }

        public static void RemoveListener(TreeGridColumnCollection source, IWeakEventListener listener)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (listener == null)
            {
                throw new ArgumentNullException("listener");
            }
            InternalCollectionChangedEventManager.CurrentManager.ProtectedRemoveListener(source, listener);
        }

        //public static void AddHandler(TreeGridColumnCollection source, EventHandler<NotifyCollectionChangedEventArgs> handler)
        //{
        //    if (handler == null)
        //    {
        //        throw new ArgumentNullException("handler");
        //    }
        //    InternalCollectionChangedEventManager.CurrentManager.ProtectedAddHandler(source, handler);
        //}

        //public static void RemoveHandler(TreeGridColumnCollection source, EventHandler<NotifyCollectionChangedEventArgs> handler)
        //{
        //    if (handler == null)
        //    {
        //        throw new ArgumentNullException("handler");
        //    }
        //    InternalCollectionChangedEventManager.CurrentManager.ProtectedRemoveHandler(source, handler);
        //}

        #endregion

        #region 实现基类方法

        protected override void StartListening(object source)
        {
            var gridViewColumnCollection = (TreeGridColumnCollection)source;
            gridViewColumnCollection.InternalCollectionChanged += new NotifyCollectionChangedEventHandler(this.OnCollectionChanged);
        }

        protected override void StopListening(object source)
        {
            var gridViewColumnCollection = (TreeGridColumnCollection)source;
            gridViewColumnCollection.InternalCollectionChanged -= new NotifyCollectionChangedEventHandler(this.OnCollectionChanged);
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            base.DeliverEvent(sender, args);
        }

        #endregion
    }
}