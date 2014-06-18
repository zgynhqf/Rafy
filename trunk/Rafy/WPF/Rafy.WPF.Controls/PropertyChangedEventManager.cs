/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121115 19:46
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121115 19:46
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace Rafy.WPF.Controls
{
    public class PropertyChangedEventManager : WeakEventManager
    {
        #region SingleTon

        private static PropertyChangedEventManager CurrentManager
        {
            get
            {
                var mgrType = typeof(PropertyChangedEventManager);
                var mgr = WeakEventManager.GetCurrentManager(mgrType) as PropertyChangedEventManager;
                if (mgr == null)
                {
                    mgr = new PropertyChangedEventManager();
                    WeakEventManager.SetCurrentManager(mgrType, mgr);
                }
                return mgr;
            }
        }

        private PropertyChangedEventManager() { }

        #endregion

        #region API

        public static void AddListener(INotifyPropertyChanged source, IWeakEventListener listener)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            if (listener == null) { throw new ArgumentNullException("listener"); }

            PropertyChangedEventManager.CurrentManager.ProtectedAddListener(source, listener);
        }

        public static void RemoveListener(INotifyPropertyChanged source, IWeakEventListener listener)
        {
            if (source == null) { throw new ArgumentNullException("source"); }
            if (listener == null) { throw new ArgumentNullException("listener"); }

            PropertyChangedEventManager.CurrentManager.ProtectedRemoveListener(source, listener);
        }

        #endregion

        #region 实现基类方法

        protected override void StartListening(object source)
        {
            var eventSource = (INotifyPropertyChanged)source;
            eventSource.PropertyChanged += new PropertyChangedEventHandler(this.OnPropertyChanged);
        }

        protected override void StopListening(object source)
        {
            var eventSource = (INotifyPropertyChanged)source;
            eventSource.PropertyChanged -= new PropertyChangedEventHandler(this.OnPropertyChanged);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            base.DeliverEvent(sender, args);
        }

        #endregion
    }
}