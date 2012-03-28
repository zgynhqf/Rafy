using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace OEA
{
    /// <summary>
    /// 选择的上下文
    /// </summary>
    public interface ISelectionContext : ICurrentObjectContext, IDataContext
    {
        /// <summary>
        /// 选中的对象集合
        /// </summary>
        IList SelectedObjects { get; }

        event EventHandler<SelectedEntityChangedEventArgs> SelectedItemChanged;
    }
}
