/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121031 17:01
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121031 17:01
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;

namespace Rafy.WPF.Controls
{
    public sealed class StaticResourceKey : ResourceKey
    {
        private string _key;

        private Type _type;

        public StaticResourceKey(Type type, string key)
        {
            this._type = type;
            this._key = key;
        }

        public string Key
        {
            get { return this._key; }
        }

        public Type Type
        {
            get { return this._type; }
        }

        public override Assembly Assembly
        {
            get { return this._type.Assembly; }
        }
    }
}
