using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Runtime.Serialization;
using System.Reflection;
using Rafy.Reflection;

namespace Rafy
{
    /// <summary>
    /// Object containing the serialization
    /// data for a specific object.
    /// </summary>
    [DataContract(Name = "SC")]
    public class SerializationInfoContainer
    {
        #region 字段

        private Dictionary<string, int> _children = new Dictionary<string, int>();

        private Dictionary<string, object> _values = new Dictionary<string, object>();

        #endregion

        public SerializationInfoContainer(int referenceId)
        {
            this.ReferenceId = referenceId;
        }

        #region DataMembers

        /// <summary>
        /// Reference number for this object.
        /// </summary>
        [DataMember(Name = "R")]
        public int ReferenceId { get; set; }

        /// <summary>
        /// Assembly-qualified type name of the
        /// object being serialized.
        /// </summary>
        [DataMember(Name = "T")]
        public string TypeName { get; set; }

        /// <summary>
        /// Dictionary containing child reference data.
        /// </summary>
        [DataMember(Name = "C")]
        public Dictionary<string, int> References
        {
            get { return _children; }
            set { _children = value; }
        }

        /// <summary>
        /// Dictionary containg field data.
        /// </summary>
        [DataMember(Name = "S")]
        public Dictionary<string, object> States
        {
            get { return _values; }
            set { _values = value; }
        }

        #endregion

        #region GetState / AddState

        /// <summary>
        /// Adds a value to the serialization stream.
        /// </summary>
        /// <param name="name">
        /// Name of the field.
        /// </param>
        /// <param name="state">
        /// 注意：只能是系统自带的类型
        /// Value of the field.
        /// </param>
        public void AddState(string name, object state)
        {
            if (state != null)
            {
                var stateType = state.GetType();
                if (stateType.IsEnum)
                {
                    state = (int)state;
                }
                else if (stateType == typeof(bool))
                {
                    state = ((bool)state) ? 1 : 0;
                }
                else if (stateType == typeof(DateTime))
                {
                    state = ((DateTime)state).ToString();
                }
            }

            this._values.Add(name, state);
        }

        /// <summary>
        /// Gets a value from the list of fields.
        /// </summary>
        /// <typeparam name="T">
        /// Type to which the value should be coerced.
        /// </typeparam>
        /// <param name="name">
        /// Name of the field.
        /// </param>
        /// <returns></returns>
        public T GetState<T>(string name)
        {
            var value = this.GetFieldValue(name, typeof(T));
            if (value != null) return (T)value;

            return default(T);
        }

        public object GetState(string name, Type targetType)
        {
            var value = this.GetFieldValue(name, targetType);
            if (value != null) return value;

            //defaultValue
            return TypeHelper.GetDefaultValue(targetType);
        }

        private object GetFieldValue(string name, Type targetType)
        {
            object value = null;
            if (this._values.TryGetValue(name, out value))
            {
                if (value != null)
                {
                    return TypeHelper.CoerceValue(targetType, value.GetType(), value);
                }
            }

            return null;
        }

        #endregion

        #region AddDelegate / GetDelegate

        /// <summary>
        /// Adds a child to the list of child references.
        /// </summary>
        /// <param name="name">Name of the field.</param>
        /// <param name="action">The action.</param>
        /// <exception cref="System.NotSupportedException">实例对象的代理方法暂时不支持被序列化！</exception>
        public void AddDelegate(string name, Delegate action)
        {
            //由于性能考虑，实例对象的代理方法暂时不支持被序列化！
            if (action.Target != null) throw new NotSupportedException("实例对象的代理方法暂时不支持被序列化！");

            var info = new DelegateInfo
            {
                DeclaringType = action.Method.DeclaringType.AssemblyQualifiedName,
                MethodName = action.Method.Name
            };

            this.AddState(name, info);
        }

        public TDelegate GetDelegate<TDelegate>(string name)
            where TDelegate : class
        {
            var info = this.GetState<DelegateInfo>(name);
            if (info == null) return null;

            var declaringType = Type.GetType(info.DeclaringType);
            var method = declaringType.GetMethod(info.MethodName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            var result = Delegate.CreateDelegate(typeof(TDelegate), method) as TDelegate;

            return result;
        }

        #endregion

        #region AddRef / GetRef

        /// <summary>
        /// Adds a child to the list of child references.
        /// </summary>
        /// <param name="name">
        /// Name of the field.
        /// </param>
        /// <param name="referenceId">
        /// Reference id for the child object.
        /// </param>
        public void AddRef(string name, int referenceId)
        {
            this._children.Add(name, referenceId);
        }

        public int GetRef(string name)
        {
            return this._children[name];
        }

        #endregion

        /// <summary>
        /// value type, string, object
        /// </summary>
        /// <param name="stateType"></param>
        /// <returns></returns>
        internal static bool IsState(Type stateType)
        {
            return stateType.IsValueType ||
                stateType == typeof(object) || stateType == typeof(string);
        }

        //internal bool IsSystemRef(Type stateType)
        //{
        //    return !stateType.IsValueType &&
        //        stateType.Namespace.StartsWith("System");
        //}
    }

    [DataContract(Name = "DI")]
    internal class DelegateInfo
    {
        [DataMember(Name = "T")]
        public string DeclaringType { get; set; }

        [DataMember(Name = "M")]
        public string MethodName { get; set; }
    }
}