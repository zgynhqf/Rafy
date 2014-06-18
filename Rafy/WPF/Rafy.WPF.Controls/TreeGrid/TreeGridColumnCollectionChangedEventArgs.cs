/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121011 11:00
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121011 11:00
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime;
using System.Text;

namespace Rafy.WPF.Controls
{
    internal class TreeGridColumnCollectionChangedEventArgs : NotifyCollectionChangedEventArgs
    {
        #region 字段

        private int _stableIndex = -1;

        private ReadOnlyCollection<TreeGridColumn> _clearedColumns;

        private TreeGridColumn _column;

        private string _propertyName;

        #endregion

        #region 属性

        internal int StableIndex
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._stableIndex;
            }
        }

        internal ReadOnlyCollection<TreeGridColumn> ClearedColumns
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._clearedColumns;
            }
        }

        internal TreeGridColumn Column
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._column;
            }
        }

        internal string PropertyName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._propertyName;
            }
        }

        #endregion

        #region 构造函数

        internal TreeGridColumnCollectionChangedEventArgs(TreeGridColumn column, string propertyName)
            : base(NotifyCollectionChangedAction.Reset)
        {
            this._column = column;
            this._propertyName = propertyName;
        }

        internal TreeGridColumnCollectionChangedEventArgs(NotifyCollectionChangedAction action, TreeGridColumn[] clearedColumns)
            : base(action)
        {
            this._clearedColumns = Array.AsReadOnly<TreeGridColumn>(clearedColumns);
        }

        internal TreeGridColumnCollectionChangedEventArgs(NotifyCollectionChangedAction action, TreeGridColumn changedItem, int index, int stableIndex)
            : base(action, changedItem, index)
        {
            this._stableIndex = stableIndex;
        }

        internal TreeGridColumnCollectionChangedEventArgs(NotifyCollectionChangedAction action, TreeGridColumn newItem, TreeGridColumn oldItem, int index, int stableIndex)
            : base(action, newItem, oldItem, index)
        {
            this._stableIndex = stableIndex;
        }

        internal TreeGridColumnCollectionChangedEventArgs(NotifyCollectionChangedAction action, TreeGridColumn changedItem, int index, int oldIndex, int stableIndex)
            : base(action, changedItem, index, oldIndex)
        {
            this._stableIndex = stableIndex;
        }

        #endregion
    }
}
