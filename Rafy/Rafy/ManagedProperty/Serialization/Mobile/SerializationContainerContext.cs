/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111110
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111110
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Serialization.Mobile
{
    public class SerializationContainerContext : ISerializationContext
    {
        private SerializationInfoContainer _core;

        private MobileFormatter _refFormatter;

        public SerializationContainerContext(SerializationInfoContainer core, MobileFormatter refFormatter)
        {
            this._core = core;
            this._refFormatter = refFormatter;
        }

        /// <summary>
        /// 是否正在序列/反序列化 State
        /// </summary>
        public bool IsProcessingState { get; set; }

        public MobileFormatter RefFormatter
        {
            get { return this._refFormatter; }
            internal set { this._refFormatter = value; }
        }

        public SerializationInfoContainer Container
        {
            get { return this._core; }
            internal set { this._core = value; }
        }

        public Dictionary<string, int> References
        {
            get { return this._core.References; }
        }

        public Dictionary<string, object> States
        {
            get { return this._core.States; }
        }

        public void AddState(string name, object state)
        {
            this._core.AddState(name, state);
        }

        public T GetState<T>(string name)
        {
            return this._core.GetState<T>(name);
        }

        public object GetState(string name, Type targetType)
        {
            return this._core.GetState(name, targetType);
        }

        public void AddDelegate(string name, Delegate action)
        {
            this._core.AddDelegate(name, action);
        }

        public TDelegate GetDelegate<TDelegate>(string name)
            where TDelegate : class
        {
            return this._core.GetDelegate<TDelegate>(name);
        }

        public void AddRef(string name, object reference)
        {
            var refId = this._refFormatter.SerialzeRef(reference);
            this._core.AddRef(name, refId);
        }

        public IMobileObject GetRef(string name)
        {
            var refId = this._core.GetRef(name);
            return this._refFormatter.GetObject(refId);
        }

        public T GetRef<T>(string name) where T : class
        {
            return this.GetRef(name) as T;
        }

        public bool IsState(Type stateType)
        {
            return SerializationInfoContainer.IsState(stateType);
        }
    }
}