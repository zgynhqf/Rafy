/*******************************************************
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
    public class PluginTable : Collection<Assembly>
    {
        private static PluginTable _domainLibraries = new PluginTable();

        private static PluginTable _uiLibraries = new PluginTable();

        /// <summary>
        /// 已经添加的实体插件程序集。
        /// </summary>
        public static PluginTable DomainLibraries
        {
            get { return _domainLibraries; }
        }

        /// <summary>
        /// 已经添加的模块插件程序集。
        /// </summary>
        public static PluginTable UILibraries
        {
            get { return _uiLibraries; }
        }

        /// <summary>
        /// 直接添加一个插件对应的程序集引用。
        /// </summary>
        /// <typeparam name="TPlugin"></typeparam>
        public void AddPlugin<TPlugin>()
            where TPlugin : IPlugin
        {
            this.Add(typeof(TPlugin).Assembly);
        }

        private bool _locked;

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

        protected override void InsertItem(int index, Assembly item)
        {
            if (!this.Contains(item))
            {
                this.EnsureWritable();

                base.InsertItem(index, item);
            }
        }

        protected override void SetItem(int index, Assembly item)
        {
            if (!this.Contains(item))
            {
                this.EnsureWritable();

                base.SetItem(index, item);
            }
        }

        internal void Lock()
        {
            this._locked = true;
        }

        private void EnsureWritable()
        {
            if (this._locked) throw new InvalidOperationException("集合不可再变更。");
        }
    }
}