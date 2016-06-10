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
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace Rafy.Reflection
{
    /// <summary>
    /// 本类提供了使用 Emit 方式来高速调用字段、属性、方法的一些方法。
    /// 所有调用过的被生成的方法，都会被存储起来，以方便再次调用。
    /// </summary>
    public static class MethodCaller
    {
        #region 缓存操作

        /// <summary>
        /// Gets the store count.
        /// </summary>
        /// <value>
        /// The store count.
        /// </value>
        public static int StoreCount
        {
            get { return _memberCache.Count + _methodCache.Count; }
        }

        /// <summary>
        /// Clears the store.
        /// </summary>
        public static void ClearStore()
        {
            _memberCache.Clear();
            _methodCache.Clear();
        }

        #endregion

        #region 访问属性

        private const BindingFlags propertyFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        private const BindingFlags fieldFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private static readonly Dictionary<MethodCacheKey, DynamicMemberHandle> _memberCache = new Dictionary<MethodCacheKey, DynamicMemberHandle>();

        /// <summary>
        /// Invokes a property getter using dynamic
        /// method invocation.
        /// </summary>
        /// <param name="obj">Target object.</param>
        /// <param name="property">Property to invoke.</param>
        /// <returns></returns>
        public static object CallPropertyGetter(object obj, string property)
        {
            var mh = GetCachedProperty(obj.GetType(), property);
            return mh.DynamicMemberGet(obj);
        }

        /// <summary>
        /// Invokes a property setter using dynamic
        /// method invocation.
        /// </summary>
        /// <param name="obj">Target object.</param>
        /// <param name="property">Property to invoke.</param>
        /// <param name="value">New value for property.</param>
        public static void CallPropertySetter(object obj, string property, object value)
        {
            var mh = GetCachedProperty(obj.GetType(), property);
            mh.DynamicMemberSet(obj, value);
        }

        internal static DynamicMemberHandle GetCachedProperty(Type objectType, string propertyName)
        {
            var key = new MethodCacheKey(objectType, propertyName, new Type[] { typeof(object) });
            DynamicMemberHandle mh = null;
            if (!_memberCache.TryGetValue(key, out mh))
            {
                lock (_memberCache)
                {
                    if (!_memberCache.TryGetValue(key, out mh))
                    {
                        PropertyInfo info = objectType.GetProperty(propertyName, propertyFlags);
                        if (info == null)
                            throw new InvalidOperationException(
                              string.Format("Member not found on object ({0})", propertyName));
                        mh = new DynamicMemberHandle(info);
                        _memberCache.Add(key, mh);
                    }
                }
            }
            return mh;
        }

        internal static DynamicMemberHandle GetCachedField(Type objectType, string fieldName)
        {
            var key = new MethodCacheKey(objectType, fieldName, new Type[] { typeof(object) });
            DynamicMemberHandle mh = null;
            if (!_memberCache.TryGetValue(key, out mh))
            {
                lock (_memberCache)
                {
                    if (!_memberCache.TryGetValue(key, out mh))
                    {
                        FieldInfo info = objectType.GetField(fieldName, fieldFlags);
                        if (info == null)
                            throw new InvalidOperationException(
                              string.Format("Resources.MemberNotFoundException:{0}", fieldName));
                        mh = new DynamicMemberHandle(info);
                        _memberCache.Add(key, mh);
                    }
                }
            }
            return mh;
        }

        #endregion

        #region 访问方法

        private const BindingFlags allLevelFlags = BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private const BindingFlags oneLevelFlags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private const BindingFlags ctorFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static Dictionary<MethodCacheKey, DynamicMethodHandle> _methodCache = new Dictionary<MethodCacheKey, DynamicMethodHandle>();

        /// <summary>
        /// 使用反射动态调用一个方法。
        /// 如果指定对象已经实现了这个方法，则直接调用，否则直接返回。
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="method">The method.</param>
        /// <param name="parameters">
        /// 方法的参数数组。
        /// 注意，为了能尽量正确地找到方法，数组中的每一个元素应该尽量不是 null。如果有参数传入 null 时，可以传入参数的类型来代替。
        /// 在有任意一个参数的类型未指定的情况下，会对方法进行模糊匹配，同时不再对找到的结果方法 Emit 生成方法并缓存。
        /// </param>
        /// <param name="result">如果成功调用，则返回方法的返回值。</param>
        /// <returns>
        /// 返回是否在目标对象上成功调用了该方法。
        /// </returns>
        public static bool CallMethodIfImplemented(object obj, string method, object[] parameters, out object result)
        {
            return CallMethodCore(obj, method, parameters, false, out result);
        }

        /// <summary>
        /// Uses reflection to dynamically invoke a method,
        /// throwing an exception if it is not
        /// implemented on the target object.
        /// </summary>
        /// <param name="obj">
        /// Object containing method.
        /// </param>
        /// <param name="method">
        /// Name of the method.
        /// </param>
        /// <param name="parameters">
        /// Parameters to pass to method.
        /// </param>
        public static object CallMethod(object obj, string method, params object[] parameters)
        {
            object result = null;
            CallMethodCore(obj, method, parameters, true, out result);
            return result;
        }

        /// <summary>
        /// Uses reflection to dynamically invoke a method,
        /// throwing an exception if it is not
        /// implemented on the target object.
        /// </summary>
        /// <param name="obj">
        /// Object containing method.
        /// </param>
        /// <param name="info">
        /// MethodInfo for the method.
        /// </param>
        /// <param name="parameters">
        /// Parameters to pass to method.
        /// </param>
        public static object CallMethod(object obj, MethodInfo info, params object[] parameters)
        {
            var mh = GetCachedMethod(info);
            if (mh == null || mh.DynamicMethod == null) throw new InvalidProgramException(info.Name + " 方法没有实现。");
            return CallMethod(obj, mh, parameters);
        }

        /// <summary>
        /// Uses reflection to dynamically invoke a method,
        /// throwing an exception if it is not implemented
        /// on the target object.
        /// </summary>
        /// <param name="obj">
        /// Object containing method.
        /// </param>
        /// <param name="methodHandle">
        /// MethodHandle for the method.
        /// </param>
        /// <param name="parameters">
        /// Parameters to pass to method.
        /// </param>
        private static object CallMethod(object obj, DynamicMethodHandle methodHandle, params object[] parameters)
        {
            var method = methodHandle.DynamicMethod;

            object[] inParams = parameters == null ? new object[] { null } : parameters;
            //如果最后一个参数是一个 param 数组，则尝试把参数动态添加到这个数组中。
            if (methodHandle.HasFinalArrayParam)
            {
                bool rebuildParameters = true;

                //四种情况：实参比形参少一个；实参多于形参；
                //实参与形参相等，且最后一个参数是数组的元素；
                //实参与形参相等，且最后一个参数就是数组；
                //其中，只有最后一种情况下，不需要重新构造一个新的参数列表。
                var pCount = methodHandle.MethodParamsLength;
                if (parameters.Length == pCount)
                {
                    var last = parameters[pCount - 1];
                    if (last is Array)
                    {
                        rebuildParameters = false;
                    }
                }

                if (rebuildParameters)
                {
                    //构造一个全新的参数列表集合，并把新集合中除最后一个位置外的元素全部从 imParams 拷贝。
                    object[] newParameterList = new object[pCount];
                    Array.Copy(inParams, newParameterList, pCount - 1);

                    //计算出剩余的所有参数的个数，并构造一个数组，把剩余的所有参数都拷贝进这个数组中。
                    var paramArrayLength = inParams.Length - (pCount - 1);
                    var paramArray = Array.CreateInstance(methodHandle.FinalArrayElementType, paramArrayLength);
                    Array.Copy(inParams, pCount - 1, paramArray, 0, paramArrayLength);
                    newParameterList[newParameterList.Length - 1] = paramArray;

                    //修改为使用新的集合。
                    inParams = newParameterList;
                }
            }

            var result = methodHandle.DynamicMethod(obj, inParams);

            return result;
        }

        /// <summary>
        /// 使用反射动态调用一个方法。
        /// 如果指定对象已经实现了这个方法，则直接调用，否则直接返回。
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="method">The method.</param>
        /// <param name="parameters">方法的参数数组。
        /// 注意，为了能尽量正确地找到方法，数组中的每一个元素应该尽量不是 null。如果有参数传入 null 时，可以传入参数的类型来代替。
        /// 在有任意一个参数的类型未指定的情况下，会对方法进行模糊匹配。</param>
        /// <param name="errorIfNotFound">如果没有找到对应的方法，是否需要抛出异常。</param>
        /// <param name="result">如果成功调用，则返回方法的返回值。</param>
        /// <returns>
        /// 返回是否在目标对象上成功调用了该方法。
        /// </returns>
        /// <exception cref="System.InvalidProgramException"></exception>
        private static bool CallMethodCore(object obj, string method, object[] parameters, bool errorIfNotFound, out object result)
        {
            result = null;

            //计算是否在 parameters 数组中存在未知参数类型的参数。同时也修正所有以 Type 传输的参数为 null。
            bool hasNullParameterType = false;
            var pTypes = new Type[parameters.Length];
            for (int i = 0, c = parameters.Length; i < c; i++)
            {
                var p = parameters[i];
                if (p == null)
                {
                    hasNullParameterType = true;
                }
                else if (p is NullParameter)
                {
                    pTypes[i] = (p as NullParameter).ParameterType;

                    //如果传输过来的值是一个 NullParameter，表明实参是一个 null 值。
                    //同时，修正 parameters 中的值为原来的 null。
                    parameters[i] = null;
                }
                else
                {
                    pTypes[i] = p.GetType();
                }
            }

            bool found = false;
            List<MethodInfo> multiFound = null;
            var objType = obj.GetType();

            //如果没有 Null 参数，则找到缓存的高速方法进行调用。
            if (!hasNullParameterType)
            {
                var mh = GetCachedMethod(objType, method, pTypes);
                if (mh != null && mh.DynamicMethod != null)
                {
                    found = true;
                    result = CallMethod(obj, mh, parameters);
                }
            }
            else
            {
                //如果有 Null 参数，那么则需要使用模糊查找方法的算法来找到方法并进行调用。
                var methods = FindMethodsOnNullParam(objType, method, pTypes);
                if (methods.Count == 1)
                {
                    var mh = GetCachedMethod(methods[0]);
                    if (mh != null && mh.DynamicMethod != null)
                    {
                        found = true;
                        result = CallMethod(obj, mh, parameters);
                    }
                }
                else if (methods.Count > 1)
                {
                    multiFound = methods;
                }
            }

            #region 抛出没有找到可用的类型的异常。

            if (errorIfNotFound)
            {
                if (multiFound != null)
                {
                    var error = new StringBuilder();
                    error.AppendFormat("在类型 {0} 中找到多个匹配的方法：", objType);
                    foreach (var m in multiFound)
                    {
                        error.AppendLine(m.ToString());
                    }

                    throw new InvalidProgramException(error.ToString());
                }
                else if (!found)
                {
                    var error = new StringBuilder();
                    error.AppendFormat("在类型 {0} 中没有找到方法：{1}(", objType, method);
                    for (int i = 0, c = pTypes.Length; i < c; i++)
                    {
                        if (i > 0) error.Append(',');
                        var t = pTypes[i];
                        error.Append(t != null ? pTypes[i].Name : "null");
                    }
                    error.Append(")。");

                    throw new InvalidProgramException(error.ToString());
                }
            }

            #endregion

            return found;
        }

        private static DynamicMethodHandle GetCachedMethod(MethodInfo info)
        {
            var key = new MethodCacheKey(info);
            DynamicMethodHandle mh = null;
            if (!_methodCache.TryGetValue(key, out mh))
            {
                lock (_methodCache)
                {
                    if (!_methodCache.TryGetValue(key, out mh))
                    {
                        mh = new DynamicMethodHandle(info);
                        _methodCache.Add(key, mh);
                    }
                }
            }
            return mh;
        }

        private static DynamicMethodHandle GetCachedMethod(Type objType, string method, params Type[] parameters)
        {
            var key = new MethodCacheKey(objType, method, parameters);
            DynamicMethodHandle mh = null;
            if (!_methodCache.TryGetValue(key, out mh))
            {
                MethodInfo info = GetMethod(objType, method, parameters);
                if (info != null)
                {
                    lock (_methodCache)
                    {
                        if (!_methodCache.TryGetValue(key, out mh))
                        {
                            mh = new DynamicMethodHandle(info);
                            _methodCache.Add(key, mh);
                        }
                    }
                }
            }
            return mh;
        }

        /// <summary>
        /// 如果参数列表中存在 null 类型时，使用模糊的匹配方式。
        /// </summary>
        /// <param name="objType"></param>
        /// <param name="methodName"></param>
        /// <param name="paramters"></param>
        /// <returns></returns>
        private static List<MethodInfo> FindMethodsOnNullParam(Type objType, string methodName, Type[] paramters)
        {
            var res = new List<MethodInfo>();

            var type = objType;
            while (type != null)
            {
                var methods = type.GetMethods(oneLevelFlags);
                for (int i = 0, c = methods.Length; i < c; i++)
                {
                    var method = methods[i];
                    if (method.Name == methodName)
                    {
                        #region 如果方法是虚方法，且结果集中已经有该虚方法的重载方法时，不再检测该方法。

                        //方法是虚方法，而且现在正在检测的类型 type 是 objType 的某个基类。
                        if (method.IsVirtual && type != objType)
                        {
                            bool overrideExisted = false;
                            for (int j = 0, c2 = res.Count; j < c2; j++)
                            {
                                var existed = res[j];
                                if (existed.GetBaseDefinition() == method.GetBaseDefinition())
                                {
                                    overrideExisted = true;
                                    continue;
                                }
                            }
                            if (overrideExisted)
                            {
                                continue;
                            }
                        }

                        #endregion

                        var targetParameters = method.GetParameters();
                        var isParamArray = IsParamArray(targetParameters);
                        if (!isParamArray)
                        {
                            //必须匹配每一个参数的类型。
                            if (targetParameters.Length == paramters.Length)
                            {
                                //检测每一个参数都必须兼容
                                bool matched = true;
                                for (int j = 0, c2 = paramters.Length; j < c2; j++)
                                {
                                    if (!IsParamTypeMatched(paramters[j], targetParameters[j].ParameterType)) { matched = false; }
                                }

                                //所有参数都兼容，则返回该方法。
                                if (matched) { res.Add(method); }
                            }
                        }
                        else
                        {
                            if (paramters.Length >= targetParameters.Length - 1)
                            {
                                bool matched = true;

                                //先检测前面的每一个参数
                                for (int j = 0, c2 = targetParameters.Length - 1; j < c2; j++)
                                {
                                    if (!IsParamTypeMatched(paramters[j], targetParameters[j].ParameterType)) { matched = false; }
                                }

                                //然后再检测多余的参数是否与数组元素的类型兼容。
                                if (matched)
                                {
                                    var elementType = targetParameters[targetParameters.Length - 1].ParameterType.GetElementType();
                                    for (int z = targetParameters.Length - 1, c3 = paramters.Length; z < c3; z++)
                                    {
                                        if (!IsParamTypeMatched(paramters[z], targetParameters[z].ParameterType)) { matched = false; }
                                    }
                                }

                                //所有参数都兼容，则返回该方法。
                                if (matched) { res.Add(method); }
                            }
                        }
                    }
                }

                type = type.BaseType;
            }

            return res;
        }

        /// <summary>
        /// 判断指定的实参是否与方法中的某形参匹配。
        /// </summary>
        /// <param name="parameterType">实参类型。</param>
        /// <param name="targetType">形参的类型。</param>
        /// <returns></returns>
        private static bool IsParamTypeMatched(Type parameterType, Type targetType)
        {
            if (parameterType == null)
            {
                return targetType.IsClass || TypeHelper.IsNullable(targetType);
            }

            return targetType.IsAssignableFrom(parameterType) ||
                !parameterType.IsClass && TypeHelper.IgnoreNullable(targetType).IsAssignableFrom(parameterType);
        }

        #endregion

        #region 查找方法的逻辑

        /// <summary>
        /// Uses reflection to locate a matching method
        /// on the target object.
        /// </summary>
        /// <param name="objectType">Type of object containing method.</param>
        /// <param name="method">Name of the method.</param>
        /// <param name="parameters">Parameters to pass to method.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidProgramException"></exception>
        private static MethodInfo GetMethod(Type objectType, string method, params Type[] parameters)
        {
            // try to find a strongly typed match

            // first see if there's a matching method
            // where all params match types
            var result = FindMethod(objectType, method, parameters);

            //不再支持模糊匹配。
            //if (result == null)
            //{
            //    // no match found - so look for any method
            //    // with the right number of parameters
            //    try
            //    {
            //        result = FindMethod(objectType, method, parameters.Length);
            //    }
            //    catch (AmbiguousMatchException)
            //    {
            //        // we have multiple methods matching by name and parameter count
            //        result = FindMethodUsingFuzzyMatching(objectType, method, parameters);
            //    }
            //}

            //if (result == null)
            //{
            //    result = objectType.GetMethod(method, allLevelFlags);
            //}

            return result;
        }

        private static MethodInfo FindMethodUsingFuzzyMatching(Type objectType, string method, Type[] parameters)
        {
            //从 objectType 开始，向它的基类方向不断地寻找。
            int parameterCount = parameters.Length;
            Type currentType = objectType;
            do
            {
                //获取当前类型的所有方法。
                MethodInfo[] methods = currentType.GetMethods(oneLevelFlags);

                //查找一个有数组类型参数的方法来接收所有参数。
                for (int i = 0, c = methods.Length; i < c; i++)
                {
                    var target = methods[i];
                    if (target.Name == method)
                    {
                        var targetParams = target.GetParameters();
                        var pCount = targetParams.Length;
                        if (pCount > 0)
                        {
                            //判断是否可以被一个拥有数组类型参数的方法所接收。
                            if (parameterCount >= pCount - 1)
                            {
                                if (IsLastArray(targetParams))
                                {
                                    return target;
                                }
                            }
                        }
                    }
                }

                //直接使用参数的个数来简单匹配方法。
                for (int i = 0, c = methods.Length; i < c; i++)
                {
                    var target = methods[i];
                    if (target.Name == method && target.GetParameters().Length == parameterCount)
                    {
                        return target;
                    }
                }

                currentType = currentType.BaseType;
            } while (currentType != null);

            return null;
        }

        /// <summary>
        /// 查找指定类型上对应参数类型的指定方法。
        /// 从子类到基类逐个检查。同时，也检测私有的方法。
        /// </summary>
        /// <param name="objectType">包含这个方法的类型。</param>
        /// <param name="method">方法名。</param>
        /// <param name="types">方法的所有参数类型。</param>
        private static MethodInfo FindMethod(Type objectType, string method, Type[] types)
        {
            MethodInfo info = null;
            do
            {
                // find for a strongly typed match
                info = objectType.GetMethod(method, oneLevelFlags, null, types, null);
                if (info != null) { break; }

                objectType = objectType.BaseType;
            } while (objectType != null);

            return info;

            //以下方案不可用。这是因为不能查找到基类中字义的私有的方法。
            //*********************** 代码块解释 *********************************
            // * 
            // * 应该在整个继承层次中查询方法。否则会出现以下情况：
            // * 子类中一旦编写了参数类型为 object 类型的方法时，父类的所有其它类型的方法都找不到了。
            // * 
            //**********************************************************************/
            //return objectType.GetMethod(method, allLevelFlags, null, types, null);
        }

        /// <summary>
        /// Returns information about the specified
        /// method, finding the method based purely
        /// on the method name and number of parameters.
        /// </summary>
        /// <param name="objectType">
        /// Type of object containing method.
        /// </param>
        /// <param name="method">
        /// Name of the method.
        /// </param>
        /// <param name="parameterCount">
        /// Number of parameters to pass to method.
        /// </param>
        private static MethodInfo FindMethod(Type objectType, string method, int parameterCount)
        {
            // walk up the inheritance hierarchy looking
            // for a method with the right number of
            // parameters
            MethodInfo result = null;
            Type currentType = objectType;
            do
            {
                MethodInfo info = currentType.GetMethod(method, oneLevelFlags);
                if (info != null)
                {
                    var infoParams = info.GetParameters();
                    var pCount = infoParams.Length;
                    if (pCount > 0 && IsLastArray(infoParams))
                    {
                        // last param is a param array or only param is an array
                        if (parameterCount >= pCount - 1)
                        {
                            // got a match so use it
                            result = info;
                            break;
                        }
                    }
                    else if (pCount == parameterCount)
                    {
                        // got a match so use it
                        result = info;
                        break;
                    }
                }
                currentType = currentType.BaseType;
            } while (currentType != null);

            return result;
        }

        /// <summary>
        /// 判断某个方法的形参列表中是否拥有一个可接受多个实际参数的数组类型参数：
        /// 方法只有一个数组类型的参数，或者方法的形参列表中的最后一个参数被标记为数组型。
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        internal static bool IsLastArray(ParameterInfo[] parameters)
        {
            var l = parameters.Length;
            return l > 0 &&
                (l == 1 && parameters[0].ParameterType.IsArray || IsParamArray(parameters));
        }

        /// <summary>
        /// 判断某个方法的形参列表中的最后一个参数被标记为数组型(params)。
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        private static bool IsParamArray(ParameterInfo[] parameters)
        {
            var l = parameters.Length;
            return l > 0 && parameters[l - 1].GetCustomAttributes(typeof(ParamArrayAttribute), true).Length > 0;
        }

        #endregion

        ///// <summary>
        ///// Gets a property type descriptor by name.
        ///// </summary>
        ///// <param name="t">Type of object containing the property.</param>
        ///// <param name="propertyName">Name of the property.</param>
        //public static PropertyDescriptor GetPropertyDescriptor(Type t, string propertyName)
        //{
        //    var propertyDescriptors = TypeDescriptor.GetProperties(t);
        //    PropertyDescriptor result = null;
        //    foreach (PropertyDescriptor desc in propertyDescriptors)
        //        if (desc.Name == propertyName)
        //        {
        //            result = desc;
        //            break;
        //        }
        //    return result;
        //}
    }

    /// <summary>
    /// 表示一个空参数。
    /// </summary>
    [Serializable]
    internal class NullParameter
    {
        public Type ParameterType { get; set; }
    }
}