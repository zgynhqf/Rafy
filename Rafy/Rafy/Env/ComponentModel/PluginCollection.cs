﻿/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130115 15:19
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130115 15:19
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Rafy.ComponentModel
{
    /// <summary>
    /// 使用代码添加的插件程序集。
    /// 只是以插件机制加载，但本质上是必需的，并非插件。
    /// </summary>
    public class PluginCollection : Collection<IPlugin>
    {
        private Dictionary<string, IPlugin> _nameCache = new Dictionary<string, IPlugin>();

        public IPlugin Find(Assembly assembly)
        {
            return this.Find(assembly.GetName().Name);
            //for (int i = 0, c = this.Count; i < c; i++)
            //{
            //    var item = this[i];
            //    if (item.Assembly == assembly)
            //    {
            //        return item;
            //    }
            //}
            //return null;
        }

        public IPlugin Find(Type pluginType)
        {
            return this.Find(pluginType.Assembly);
            //return this.FirstOrDefault(p => pluginType.IsInstanceOfType(p));
        }

        public IPlugin Find(string assemblyName)
        {
            if (_nameCache.TryGetValue(assemblyName, out var value))
            {
                return value;
            }
            return null;
        }

        private void OnItemAdded(IPlugin item)
        {
            _nameCache[item.Assembly.GetName().Name] = item;
        }

        #region Lockable

        private bool _isLocked;

        /// <summary>
        /// 表示当前的插件集合是否已经被锁定（不可再修改）。
        /// </summary>
        public bool IsLocked
        {
            get { return _isLocked; }
        }

        protected override void ClearItems()
        {
            this.EnsureWritable();

            base.ClearItems();
        }

        protected override void RemoveItem(int index)
        {
            this.EnsureWritable();

            base.RemoveItem(index);
        }

        protected override void InsertItem(int index, IPlugin item)
        {
            if (!this.ContainsType(item))
            {
                this.EnsureWritable();

                base.InsertItem(index, item);
                this.OnItemAdded(item);
            }
        }

        protected override void SetItem(int index, IPlugin item)
        {
            if (!this.ContainsType(item))
            {
                this.EnsureWritable();

                base.SetItem(index, item);
                this.OnItemAdded(item);
            }
        }

        internal void Lock()
        {
            _isLocked = true;
        }

        internal void Unlock()
        {
            _isLocked = false;
        }

        private void EnsureWritable()
        {
            if (_isLocked) throw new InvalidOperationException("集合不可再变更。");
        }

        private bool ContainsType(IPlugin plugin)
        {
            return this.Any(p => p.GetType() == plugin.GetType());
        }

        #endregion
    }
}