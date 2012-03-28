using System;
using System.Collections.Generic;
using OEA.Module.View;



namespace OEA
{
    /// <summary>
    /// 可以附加一个控件在这个对象上
    /// </summary>
    public interface IControlWrapper
    {
        /// <summary>
        /// 被包含的控件，如果为null，表示还没有控件包含进来。
        /// </summary>
        object Control { get; }
    }
}
