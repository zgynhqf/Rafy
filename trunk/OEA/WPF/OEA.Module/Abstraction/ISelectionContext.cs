using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using OEA.Library;

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
        IList<Entity> SelectedEntities { get; }

        event EventHandler<SelectedEntityChangedEventArgs> SelectedItemChanged;
    }
}
