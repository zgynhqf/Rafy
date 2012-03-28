using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.MetaModel
{
    public enum Direction
    {
        /// <summary>
        /// 水平
        /// </summary>
        Horizontal,

        /// <summary>
        /// 垂直。 细表默认和主表垂直显示
        /// </summary>
        Vertical
    }

    /// <summary>
    /// 选择模式：多选/单选。
    /// </summary>
    public enum ReferenceSelectionMode
    {
        /// <summary>
        /// 单选模式
        /// </summary>
        Single,

        /// <summary>
        /// 多选模式
        /// </summary>
        Multiple
    }

    /// <summary>
    /// 可见性指示器的类型
    /// </summary>
    public enum VisiblityType
    {
        AlwaysShow,

        AlwaysHide,

        /// <summary>
        /// 动态表示是否需要检测动态属性来获取可见性
        /// </summary>
        Dynamic,
    }
}