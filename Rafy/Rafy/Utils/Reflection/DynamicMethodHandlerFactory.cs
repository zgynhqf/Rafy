#if NET45
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Rafy.Reflection
{
    /// <summary>
    /// Delegate for a dynamic constructor method.
    /// </summary>
    public delegate object DynamicCtorDelegate();
    /// <summary>
    /// Delegate for a dynamic method.
    /// </summary>
    /// <param name="target">
    /// Object containg method to invoke.
    /// </param>
    /// <param name="args">
    /// Parameters passed to method.
    /// </param>
    public delegate object DynamicMethodDelegate(object target, object[] args);
    /// <summary>
    /// Delegate for getting a value.
    /// </summary>
    /// <param name="target">Target object.</param>
    /// <returns></returns>
    public delegate object DynamicMemberGetDelegate(object target);
    /// <summary>
    /// Delegate for setting a value.
    /// </summary>
    /// <param name="target">Target object.</param>
    /// <param name="arg">Argument value.</param>
    public delegate void DynamicMemberSetDelegate(object target, object arg);

    internal static class DynamicMethodHandlerFactory
    {
        public static DynamicMethodDelegate CreateMethod(MethodInfo method)
        {
            ParameterInfo[] pi = method.GetParameters();

            DynamicMethod dm = new DynamicMethod("dm", typeof(object),
                new Type[] { typeof(object), typeof(object[]) },
                typeof(DynamicMethodHandlerFactory), true);

            ILGenerator il = dm.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);

            for (int index = 0; index < pi.Length; index++)
            {
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldc_I4, index);

                Type parameterType = pi[index].ParameterType;
                if (parameterType.IsByRef)
                {
                    parameterType = parameterType.GetElementType();
                    if (parameterType.IsValueType)
                    {
                        il.Emit(OpCodes.Ldelem_Ref);
                        il.Emit(OpCodes.Unbox, parameterType);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldelema, parameterType);
                    }
                }
                else
                {
                    il.Emit(OpCodes.Ldelem_Ref);

                    if (parameterType.IsValueType)
                    {
                        il.Emit(OpCodes.Unbox, parameterType);
                        il.Emit(OpCodes.Ldobj, parameterType);
                    }
                }
            }

            if ((method.IsAbstract || method.IsVirtual)
                && !method.IsFinal && !method.DeclaringType.IsSealed)
            {
                il.Emit(OpCodes.Callvirt, method);
            }
            else
            {
                il.Emit(OpCodes.Call, method);
            }

            if (method.ReturnType == typeof(void))
            {
                il.Emit(OpCodes.Ldnull);
            }
            else if (method.ReturnType.IsValueType)
            {
                il.Emit(OpCodes.Box, method.ReturnType);
            }
            il.Emit(OpCodes.Ret);

            return (DynamicMethodDelegate)dm.CreateDelegate(typeof(DynamicMethodDelegate));
        }

        public static DynamicMemberGetDelegate CreatePropertyGetter(PropertyInfo property)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            if (!property.CanRead) return null;

            MethodInfo getMethod = property.GetGetMethod();
            if (getMethod == null)   //maybe is private
                getMethod = property.GetGetMethod(true);

            DynamicMethod dm = new DynamicMethod("propg", typeof(object),
                new Type[] { typeof(object) },
                property.DeclaringType, true);

            ILGenerator il = dm.GetILGenerator();

            if (!getMethod.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.EmitCall(OpCodes.Callvirt, getMethod, null);
            }
            else
                il.EmitCall(OpCodes.Call, getMethod, null);

            if (property.PropertyType.IsValueType)
                il.Emit(OpCodes.Box, property.PropertyType);

            il.Emit(OpCodes.Ret);

            return (DynamicMemberGetDelegate)dm.CreateDelegate(typeof(DynamicMemberGetDelegate));
        }

        public static DynamicMemberSetDelegate CreatePropertySetter(PropertyInfo property)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            if (!property.CanWrite) return null;

            MethodInfo setMethod = property.GetSetMethod();
            if (setMethod == null)   //maybe is private
                setMethod = property.GetSetMethod(true);

            DynamicMethod dm = new DynamicMethod("props", null,
                new Type[] { typeof(object), typeof(object) },
                property.DeclaringType, true);

            ILGenerator il = dm.GetILGenerator();

            if (!setMethod.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
            }
            il.Emit(OpCodes.Ldarg_1);

            EmitCastToReference(il, property.PropertyType);
            if (!setMethod.IsStatic && !property.DeclaringType.IsValueType)
            {
                il.EmitCall(OpCodes.Callvirt, setMethod, null);
            }
            else
                il.EmitCall(OpCodes.Call, setMethod, null);

            il.Emit(OpCodes.Ret);

            return (DynamicMemberSetDelegate)dm.CreateDelegate(typeof(DynamicMemberSetDelegate));
        }

        public static DynamicMemberGetDelegate CreateFieldGetter(FieldInfo field)
        {
            if (field == null)
                throw new ArgumentNullException("field");

            DynamicMethod dm = new DynamicMethod("fldg", typeof(object),
                new Type[] { typeof(object) },
                field.DeclaringType, true);

            ILGenerator il = dm.GetILGenerator();

            if (!field.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);

                EmitCastToReference(il, field.DeclaringType);  //to handle struct object

                il.Emit(OpCodes.Ldfld, field);
            }
            else
                il.Emit(OpCodes.Ldsfld, field);

            if (field.FieldType.IsValueType)
                il.Emit(OpCodes.Box, field.FieldType);

            il.Emit(OpCodes.Ret);

            return (DynamicMemberGetDelegate)dm.CreateDelegate(typeof(DynamicMemberGetDelegate));
        }

        public static DynamicMemberSetDelegate CreateFieldSetter(FieldInfo field)
        {
            if (field == null)
                throw new ArgumentNullException("field");

            DynamicMethod dm = new DynamicMethod("flds", null,
                new Type[] { typeof(object), typeof(object) },
                field.DeclaringType, true);

            ILGenerator il = dm.GetILGenerator();

            if (!field.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
            }
            il.Emit(OpCodes.Ldarg_1);

            EmitCastToReference(il, field.FieldType);

            if (!field.IsStatic)
                il.Emit(OpCodes.Stfld, field);
            else
                il.Emit(OpCodes.Stsfld, field);
            il.Emit(OpCodes.Ret);

            return (DynamicMemberSetDelegate)dm.CreateDelegate(typeof(DynamicMemberSetDelegate));
        }

        private static void EmitCastToReference(ILGenerator il, Type type)
        {
            if (type.IsValueType)
                il.Emit(OpCodes.Unbox_Any, type);
            else
                il.Emit(OpCodes.Castclass, type);
        }

    }
}
#endif

#if NETSTANDARD2_0 || NETCOREAPP2_0
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Rafy.Reflection
{
    /// <summary>
    /// Delegate for a dynamic constructor method.
    /// </summary>
    public delegate object DynamicCtorDelegate();
    /// <summary>
    /// Delegate for a dynamic method.
    /// </summary>
    /// <param name="target">
    /// Object containg method to invoke.
    /// </param>
    /// <param name="args">
    /// Parameters passed to method.
    /// </param>
    public delegate object DynamicMethodDelegate(object target, object[] args);
    /// <summary>
    /// Delegate for getting a value.
    /// </summary>
    /// <param name="target">Target object.</param>
    /// <returns></returns>
    public delegate object DynamicMemberGetDelegate(object target);
    /// <summary>
    /// Delegate for setting a value.
    /// </summary>
    /// <param name="target">Target object.</param>
    /// <param name="arg">Argument value.</param>
    public delegate void DynamicMemberSetDelegate(object target, object arg);

    internal static class DynamicMethodHandlerFactory
    {
        internal static DynamicMethodDelegate CreateMethod(MethodInfo method)
        {
            var target = Expression.Parameter(typeof(object), "target");
            var targetCasted = Expression.Convert(target, method.DeclaringType);

            var args = Expression.Parameter(typeof(object[]), "args");
            var parameters = method.GetParameters();
            var parametersExpList = new Expression[parameters.Length];
            for (int i = 0, c = parameters.Length; i < c; i++)
            {
                var parameter = parameters[i];

                var exp = Expression.ArrayIndex(args, Expression.Constant(i));
                var pExp = Expression.Convert(exp, parameter.ParameterType);

                parametersExpList[i] = pExp;
            }

            var call = Expression.Call(targetCasted, method, parametersExpList);

            var body = Expression.Convert(call, typeof(object));

            var methodExp = Expression.Lambda<DynamicMethodDelegate>(body, target, args);

            var result = methodExp.Compile();

            return result;
        }

        internal static DynamicMemberGetDelegate CreatePropertyGetter(PropertyInfo property)
        {
            throw new NotSupportedException();
        }

        internal static DynamicMemberSetDelegate CreatePropertySetter(PropertyInfo property)
        {
            throw new NotSupportedException();
        }

        internal static DynamicMemberGetDelegate CreateFieldGetter(FieldInfo field)
        {
            throw new NotSupportedException();
        }

        internal static DynamicMemberSetDelegate CreateFieldSetter(FieldInfo field)
        {
            throw new NotSupportedException();
        }
    }

    //internal static class DynamicMethodHandlerFactory_OldEmit
    //{
    //    public static DynamicMethodDelegate CreateMethod(MethodInfo method)
    //    {
    //        ParameterInfo[] pi = method.GetParameters();

    //        DynamicMethod dm = new DynamicMethod("dm", typeof(object),
    //            new Type[] { typeof(object), typeof(object[]) },
    //            typeof(DynamicMethodHandlerFactory), true);

    //        ILGenerator il = dm.GetILGenerator();

    //        il.Emit(OpCodes.Ldarg_0);

    //        for (int index = 0; index < pi.Length; index++)
    //        {
    //            il.Emit(OpCodes.Ldarg_1);
    //            il.Emit(OpCodes.Ldc_I4, index);

    //            Type parameterType = pi[index].ParameterType;
    //            if (parameterType.IsByRef)
    //            {
    //                parameterType = parameterType.GetElementType();
    //                if (parameterType.IsValueType)
    //                {
    //                    il.Emit(OpCodes.Ldelem_Ref);
    //                    il.Emit(OpCodes.Unbox, parameterType);
    //                }
    //                else
    //                {
    //                    il.Emit(OpCodes.Ldelema, parameterType);
    //                }
    //            }
    //            else
    //            {
    //                il.Emit(OpCodes.Ldelem_Ref);

    //                if (parameterType.IsValueType)
    //                {
    //                    il.Emit(OpCodes.Unbox, parameterType);
    //                    il.Emit(OpCodes.Ldobj, parameterType);
    //                }
    //            }
    //        }

    //        if ((method.IsAbstract || method.IsVirtual)
    //            && !method.IsFinal && !method.DeclaringType.IsSealed)
    //        {
    //            il.Emit(OpCodes.Callvirt, method);
    //        }
    //        else
    //        {
    //            il.Emit(OpCodes.Call, method);
    //        }

    //        if (method.ReturnType == typeof(void))
    //        {
    //            il.Emit(OpCodes.Ldnull);
    //        }
    //        else if (method.ReturnType.IsValueType)
    //        {
    //            il.Emit(OpCodes.Box, method.ReturnType);
    //        }
    //        il.Emit(OpCodes.Ret);

    //        return (DynamicMethodDelegate)dm.CreateDelegate(typeof(DynamicMethodDelegate));
    //    }

    //    public static DynamicMemberGetDelegate CreatePropertyGetter(PropertyInfo property)
    //    {
    //        if (property == null)
    //            throw new ArgumentNullException("property");

    //        if (!property.CanRead) return null;

    //        MethodInfo getMethod = property.GetGetMethod();
    //        if (getMethod == null)   //maybe is private
    //            getMethod = property.GetGetMethod(true);

    //        DynamicMethod dm = new DynamicMethod("propg", typeof(object),
    //            new Type[] { typeof(object) },
    //            property.DeclaringType, true);

    //        ILGenerator il = dm.GetILGenerator();

    //        if (!getMethod.IsStatic)
    //        {
    //            il.Emit(OpCodes.Ldarg_0);
    //            il.EmitCall(OpCodes.Callvirt, getMethod, null);
    //        }
    //        else
    //            il.EmitCall(OpCodes.Call, getMethod, null);

    //        if (property.PropertyType.IsValueType)
    //            il.Emit(OpCodes.Box, property.PropertyType);

    //        il.Emit(OpCodes.Ret);

    //        return (DynamicMemberGetDelegate)dm.CreateDelegate(typeof(DynamicMemberGetDelegate));
    //    }

    //    public static DynamicMemberSetDelegate CreatePropertySetter(PropertyInfo property)
    //    {
    //        if (property == null)
    //            throw new ArgumentNullException("property");

    //        if (!property.CanWrite) return null;

    //        MethodInfo setMethod = property.GetSetMethod();
    //        if (setMethod == null)   //maybe is private
    //            setMethod = property.GetSetMethod(true);

    //        DynamicMethod dm = new DynamicMethod("props", null,
    //            new Type[] { typeof(object), typeof(object) },
    //            property.DeclaringType, true);

    //        ILGenerator il = dm.GetILGenerator();

    //        if (!setMethod.IsStatic)
    //        {
    //            il.Emit(OpCodes.Ldarg_0);
    //        }
    //        il.Emit(OpCodes.Ldarg_1);

    //        EmitCastToReference(il, property.PropertyType);
    //        if (!setMethod.IsStatic && !property.DeclaringType.IsValueType)
    //        {
    //            il.EmitCall(OpCodes.Callvirt, setMethod, null);
    //        }
    //        else
    //            il.EmitCall(OpCodes.Call, setMethod, null);

    //        il.Emit(OpCodes.Ret);

    //        return (DynamicMemberSetDelegate)dm.CreateDelegate(typeof(DynamicMemberSetDelegate));
    //    }

    //    public static DynamicMemberGetDelegate CreateFieldGetter(FieldInfo field)
    //    {
    //        if (field == null)
    //            throw new ArgumentNullException("field");

    //        DynamicMethod dm = new DynamicMethod("fldg", typeof(object),
    //            new Type[] { typeof(object) },
    //            field.DeclaringType, true);

    //        ILGenerator il = dm.GetILGenerator();

    //        if (!field.IsStatic)
    //        {
    //            il.Emit(OpCodes.Ldarg_0);

    //            EmitCastToReference(il, field.DeclaringType);  //to handle struct object

    //            il.Emit(OpCodes.Ldfld, field);
    //        }
    //        else
    //            il.Emit(OpCodes.Ldsfld, field);

    //        if (field.FieldType.IsValueType)
    //            il.Emit(OpCodes.Box, field.FieldType);

    //        il.Emit(OpCodes.Ret);

    //        return (DynamicMemberGetDelegate)dm.CreateDelegate(typeof(DynamicMemberGetDelegate));
    //    }

    //    public static DynamicMemberSetDelegate CreateFieldSetter(FieldInfo field)
    //    {
    //        if (field == null)
    //            throw new ArgumentNullException("field");

    //        DynamicMethod dm = new DynamicMethod("flds", null,
    //            new Type[] { typeof(object), typeof(object) },
    //            field.DeclaringType, true);

    //        ILGenerator il = dm.GetILGenerator();

    //        if (!field.IsStatic)
    //        {
    //            il.Emit(OpCodes.Ldarg_0);
    //        }
    //        il.Emit(OpCodes.Ldarg_1);

    //        EmitCastToReference(il, field.FieldType);

    //        if (!field.IsStatic)
    //            il.Emit(OpCodes.Stfld, field);
    //        else
    //            il.Emit(OpCodes.Stsfld, field);
    //        il.Emit(OpCodes.Ret);

    //        return (DynamicMemberSetDelegate)dm.CreateDelegate(typeof(DynamicMemberSetDelegate));
    //    }

    //    private static void EmitCastToReference(ILGenerator il, Type type)
    //    {
    //        if (type.IsValueType)
    //            il.Emit(OpCodes.Unbox_Any, type);
    //        else
    //            il.Emit(OpCodes.Castclass, type);
    //    }
    //}
}
#endif