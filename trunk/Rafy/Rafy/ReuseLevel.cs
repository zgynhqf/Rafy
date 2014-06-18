using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy
{
    /// <summary>
    /// 产品的 “721” 重用级别
    /// </summary>
    public static class ReuseLevel
    {
        /// <summary>
        /// 主版本（7）
        /// </summary>
        public const int Main = 100;

        /// <summary>
        /// 部分版本（2）
        /// </summary>
        public const int Part = 200;

        /// <summary>
        /// 客户化版本（1）
        /// </summary>
        public const int Customized = 300;
    }

    /// <summary>
    /// Rafy 中的其它插件启动级别。
    /// </summary>
    public static class PluginSetupLevel
    {
        /// <summary>
        /// Rafy 系统级别的重用级别，一般不要使用。
        /// </summary>
        public const int System = -1;
    }
}
