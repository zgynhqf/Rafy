/*******************************************************
 * 
 * 作者：CSLA
 * 创建日期：2009
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 周金根 2009
 * 
*******************************************************/

using System;
using System.Reflection;

namespace Rafy.Reflection
{
    internal class MethodCacheKey
    {
        internal Type Type;
        internal string MethodName;
        internal Type[] ParamTypes;
        private int _hashKey;

        public MethodCacheKey(MethodInfo methodInfo)
        {
            this.Type = methodInfo.DeclaringType;
            this.MethodName = methodInfo.Name;
            var parameters = methodInfo.GetParameters();
            this.ParamTypes = new Type[parameters.Length];

            _hashKey = this.Type.GetHashCode();
            _hashKey = _hashKey ^ this.MethodName.GetHashCode();
            for (int i = 0, c = parameters.Length; i < c; i++)
            {
                var item = parameters[i].ParameterType;
                _hashKey = _hashKey ^ item.GetHashCode();
                this.ParamTypes[i] = item;
            }
        }

        public MethodCacheKey(Type type, string methodName, Type[] paramTypes)
        {
            this.Type = type;
            this.MethodName = methodName;
            this.ParamTypes = paramTypes;

            _hashKey = type.GetHashCode();
            _hashKey = _hashKey ^ methodName.GetHashCode();
            for (int i = 0, c = paramTypes.Length; i < c; i++)
            {
                var item = paramTypes[i];
                _hashKey = _hashKey ^ item.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            var key = obj as MethodCacheKey;
            return key != null &&
                key.Type == this.Type &&
                key.MethodName == this.MethodName &&
                ArrayEquals(key.ParamTypes, this.ParamTypes);
        }

        private bool ArrayEquals(Type[] a1, Type[] a2)
        {
            if (a1.Length != a2.Length)
                return false;

            for (int pos = 0; pos < a1.Length; pos++)
                if (a1[pos] != a2[pos])
                    return false;
            return true;
        }

        public override int GetHashCode()
        {
            return _hashKey;
        }
    }
}
