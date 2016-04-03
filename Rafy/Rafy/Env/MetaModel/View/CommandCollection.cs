/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120312
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120312
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Rafy.MetaModel.View
{
    /// <summary>
    /// 命令集合
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public abstract class CommandCollection<TCommand> : Collection<TCommand>
        where TCommand : ViewMeta
    {
        #region 冻结

        protected override void ClearItems()
        {
            this.CheckUnFrozen();
            base.ClearItems();
        }

        protected override void InsertItem(int index, TCommand item)
        {
            this.OnItemAdding(item);
            this.CheckUnFrozen();
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            this.CheckUnFrozen();
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, TCommand item)
        {
            this.OnItemAdding(item);
            this.CheckUnFrozen();
            base.SetItem(index, item);
        }

        private bool _frozen;

        public void Freeze()
        {
            _frozen = true;
        }

        private void CheckUnFrozen()
        {
            if (_frozen) throw new InvalidOperationException("已经被冻结。");
        }

        private void OnItemAdding(TCommand item)
        {
            if (item == null) throw new InvalidOperationException("不能向集合中插入空元素。");
        }

        #endregion

        /// <summary>
        /// this.SortByLabel("保存","添加","取消")
        /// </summary>
        /// <param name="labels"></param>
        public void SortByLabel(params string[] labels)
        {
            throw new NotImplementedException();//huqf
        }

        /// <summary>
        /// 查找指定名称的命令。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TCommand Find(string name)
        {
            return this.FirstOrDefault(c => c.Name == name);
        }

        /// <summary>
        /// 删除指定全称的命令。
        /// </summary>
        /// <param name="names"></param>
        public void Remove(params string[] names)
        {
            this.Remove(names as IEnumerable<string>);
        }

        /// <summary>
        /// 删除指定全称的命令。
        /// </summary>
        /// <param name="names"></param>
        public void Remove(IEnumerable<string> names)
        {
            foreach (var name in names)
            {
                var c = this.Find(name);
                if (c != null) { this.Remove(c); }
            }
        }

        /// <summary>
        /// 批量添加命令集合到本集合中。
        /// </summary>
        /// <param name="commands"></param>
        public void AddRange(IEnumerable<TCommand> commands)
        {
            foreach (var cmd in commands) { this.Add(cmd); }
        }
    }
}