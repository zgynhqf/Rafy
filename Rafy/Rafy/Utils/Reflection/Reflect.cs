﻿using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Rafy.Reflection
{
    /// <summary>
    /// Provides strong-typed reflection of the some Type.
    /// type.
    /// </summary>
    public static class Reflect
    {
        /// <summary>
        /// Gets the method represented by the lambda expression.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns></returns>
        public static MethodInfo GetMethod<TTarget>(Expression<Action<TTarget>> method)
        {
            return GetMethodInfo(method);
        }

        /// <summary>
        /// Gets the method represented by the lambda expression.
        /// </summary>
        /// <typeparam name="TTarget">Type to reflect.</typeparam>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <param name="method">The method.</param>
        /// <returns></returns>
        public static MethodInfo GetMethod<TTarget, T1>(Expression<Action<TTarget, T1>> method)
        {
            return GetMethodInfo(method);
        }

        /// <summary>
        /// Gets the method represented by the lambda expression.
        /// </summary>
        /// <exception cref="ArgumentNullException">The <paramref name="method"/> is null.</exception>
        /// <exception cref="ArgumentException">The <paramref name="method"/> is not a lambda expression or it does not represent a method invocation.</exception>
        public static MethodInfo GetMethod<TTarget, T1, T2>(Expression<Action<TTarget, T1, T2>> method)
        {
            return GetMethodInfo(method);
        }

        /// <summary>
        /// Gets the method represented by the lambda expression.
        /// </summary>
        /// <exception cref="ArgumentNullException">The <paramref name="method"/> is null.</exception>
        /// <exception cref="ArgumentException">The <paramref name="method"/> is not a lambda expression or it does not represent a method invocation.</exception>
        public static MethodInfo GetMethod<TTarget, T1, T2, T3>(Expression<Action<TTarget, T1, T2, T3>> method)
        {
            return GetMethodInfo(method);
        }

        private static MethodInfo GetMethodInfo(Expression method)
        {
            if (method == null) throw new ArgumentNullException("method");

            var lambda = method as LambdaExpression;
            if (lambda == null) throw new ArgumentException("Not a lambda expression", "method");
            if (lambda.Body.NodeType != ExpressionType.Call) throw new ArgumentException("Not a method call", "method");

            return ((MethodCallExpression)lambda.Body).Method;
        }

        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="throwIfNotFound"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Member is not a property</exception>
        public static PropertyInfo GetProperty<TTarget>(Expression<Func<TTarget, object>> property, bool throwIfNotFound = true)
        {
            var info = GetMemberExpression(property, throwIfNotFound)?.Member as PropertyInfo;
            if (info == null && throwIfNotFound) throw new ArgumentException("Member is not a property");

            return info;
        }

        /// <summary>
        /// Gets the property represented by the lambda expression.
        /// </summary>
        /// <typeparam name="P">Type assigned to the property</typeparam>
        /// <typeparam name="TTarget">Type to reflect.</typeparam>
        /// <param name="property">Property Expression</param>
        /// <param name="throwIfNotFound"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Member is not a property</exception>
        public static PropertyInfo GetProperty<TTarget, P>(Expression<Func<TTarget, P>> property, bool throwIfNotFound = true)
        {
            var info = GetMemberExpression(property, throwIfNotFound)?.Member as PropertyInfo;
            if (info == null && throwIfNotFound) throw new ArgumentException("Member is not a property");

            return info;
        }

        /// <summary>
        /// Gets the field represented by the lambda expression.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="throwIfNotFound"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Member is not a field</exception>
        public static FieldInfo GetField<TTarget>(Expression<Func<TTarget, object>> field, bool throwIfNotFound = true)
        {
            var info = GetMemberExpression(field, throwIfNotFound)?.Member as FieldInfo;
            if (info == null) throw new ArgumentException("Member is not a field");

            return info;
        }

        public static MemberExpression GetMemberExpression(LambdaExpression lambda, bool throwIfNotFound = true)
        {
            if (lambda == null) throw new ArgumentNullException("lambda");

            MemberExpression memberExpr = null;

            // The Func<TTarget, object> we use returns an object, so first statement can be either 
            // a cast (if the field/property does not return an object) or the direct member access.
            if (lambda.Body.NodeType == ExpressionType.Convert)
            {
                // The cast is an unary expression, where the operand is the 
                // actual member access expression.
                memberExpr = ((UnaryExpression)lambda.Body).Operand as MemberExpression;
            }
            else if (lambda.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpr = lambda.Body as MemberExpression;
            }

            if (memberExpr == null)
            {
                if (throwIfNotFound)
                {
                    throw new ArgumentException("Not a member access", "member");
                }
                return null;
            }

            return memberExpr;
        }
    }
}