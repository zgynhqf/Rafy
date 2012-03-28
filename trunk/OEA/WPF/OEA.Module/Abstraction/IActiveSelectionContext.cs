using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace OEA
{
    public interface IActiveSelectionContext : ISelectionContext
    {
        /// <summary>
        /// 选择所有元素。
        /// </summary>
        void SelectAll();
    }
}
