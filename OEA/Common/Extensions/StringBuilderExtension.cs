using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hxy
{
    /// <summary>
    /// a extension of StringBuilder
    /// </summary>
    public static class StringBuilderExtension
    {
        /// <summary>
        /// Clear all content in this object
        /// </summary>
        /// <param name="sb"></param>
        /// <returns></returns>
        public static StringBuilder Clear(this StringBuilder sb)
        {
            sb.Remove(0, sb.Length);
            return sb;
        }
    }
}
