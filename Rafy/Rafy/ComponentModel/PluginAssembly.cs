/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110331
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100331
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Rafy.MetaModel;

namespace Rafy.ComponentModel
{
    /// <summary>
    /// 表示某一个插件程序集
    /// </summary>
    [DebuggerDisplay("{Assembly.FullName}")]
    public class PluginAssembly
    {
        public PluginAssembly(Assembly assembly, IPlugin instance)
        {
            this.Instance = instance;
            this.Assembly = assembly;
        }

        /// <summary>
        /// 程序集当中的插件对象。
        /// 如果插件中没有定义，则此属性为 null。
        /// </summary>
        public IPlugin Instance { get; private set; }

        /// <summary>
        /// 程序集本身
        /// </summary>
        public Assembly Assembly { get; private set; }

        /// <summary>
        /// 本属性表示插件在所有插件中的启动索引号。
        /// 索引号表示了插件的启动优先级，索引号越小，越先被启动。
        /// 该优先级的计算方式为：
        /// 
        /// 1. 所有 DomainPlugin 的索引号全部少于所有的 UIPlugin 的索引号；
        /// 2. 接着按照 SetupLevel 进行排序，越小的 SetupLevel 对应越小的索引号。
        /// 3. 对于 SetupLevel 相同的插件，则根据引用关系对插件进行排序，引用其它插件越少的插件，对应的索引号更小。
        /// </summary>
        public int SetupIndex { get; internal set; }
    }

    internal class EmptyPlugin : IPlugin
    {
        private Assembly _assembly;

        public EmptyPlugin(Assembly assembly)
        {
            _assembly = assembly;
        }

        /// <summary>
        /// 插件对应的程序集。
        /// </summary>
        public Assembly Assembly
        {
            get { return _assembly; }
        }

        int IPlugin.SetupLevel { get { return ReuseLevel.Main; } }

        public void Initialize(IApp app) { }
    }
}