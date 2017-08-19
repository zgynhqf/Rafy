/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：201101
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 201001
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using System.Threading;

namespace Rafy.Utils
{
    /// <summary>
    /// 一个简单的 Emit 上下文。
    /// 目前只是提供了一个基本的 ModuleBuilder。
    /// </summary>
    internal class EmitContext
    {
        public static readonly EmitContext Instance = new EmitContext();

        private EmitContext() { }

        private ModuleBuilder _module;

        private object _moduleLock = new object();

        /// <summary>
        /// 获取动态的模块，所有的类都生成在这个模块中。
        /// </summary>
        /// <returns></returns>
        public ModuleBuilder GetDynamicModule()
        {
            if (this._module == null)
            {
                lock (this._moduleLock)
                {
                    if (this._module == null)
                    {
                        AppDomain myDomain = Thread.GetDomain();
                        var myAsmName = new AssemblyName("EmitContext_DynamicAssembly");
                        var myAsmBuilder = myDomain.DefineDynamicAssembly(myAsmName, AssemblyBuilderAccess.Run);
                        this._module = myAsmBuilder.DefineDynamicModule("EmitContext_DynamicModule");
                    }
                }
            }

            return this._module;
        }
    }
}