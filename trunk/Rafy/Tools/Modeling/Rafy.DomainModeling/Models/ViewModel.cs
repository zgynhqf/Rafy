/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130412 17:18
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130412 17:18
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Rafy;

namespace Rafy.DomainModeling.Models
{
    /// <summary>
    /// odml 文档模型中的基类。
    /// </summary>
    public abstract class DocumentViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 本元素对应的文档对象。
        /// </summary>
        internal ODMLDocument Document;

        /// <summary>
        /// 指定当前的子集合。
        /// 用于同步每一个子元素的 Document 字段。
        /// </summary>
        /// <param name="children"></param>
        protected void AddChildren(INotifyChangedCollection children)
        {
            children.CollectionChanged += children_CollectionChanged;

            foreach (DocumentViewModel child in children)
            {
                child.Document = Document;
            }
        }

        void children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.Document != null)
            {
                var oldItems = e.OldItems ?? (sender as INotifyChangedCollection).PopClearedItems();
                if (oldItems != null)
                {
                    foreach (DocumentViewModel item in oldItems)
                    {
                        item.Document = null;
                    }
                }

                if (e.NewItems != null)
                {
                    foreach (DocumentViewModel item in e.NewItems)
                    {
                        item.Document = this.Document;
                    }
                }
                this.Document.OnChanged();
            }
        }

        /// <summary>
        /// 任何属性变更时的事件。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));

            if (Document != null)
            {
                Document.OnChanged();
            }
        }
    }
}