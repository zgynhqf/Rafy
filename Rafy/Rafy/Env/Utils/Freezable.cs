/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110314
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100314
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using Rafy;
using Rafy.Reflection;

namespace Rafy.Utils
{
    /// <summary>
    /// 一个可以被冻结的对象
    /// </summary>
    public abstract class Freezable : Extendable
    {
        #region Freezable

        private bool _frozen;

        /// <summary>
        /// 返回当前的对象是否已经被冻结了。
        /// 子类可以在对状态进行更新时检查此属性。
        /// </summary>
        public bool IsFrozen
        {
            get
            {
                return this._frozen;
            }
        }

        /// <summary>
        /// 冻结本对象。
        /// 冻结后，对象变为不可变对象。
        /// </summary>
        public void Freeze()
        {
            if (!this._frozen)
            {
                this._frozen = true;

                this.OnFrozen();
            }
        }

        /// <summary>
        /// 调用此方法保证本对象还没有被冻结。否则会抛出异常。
        /// </summary>
        protected void CheckUnFrozen()
        {
            if (this._frozen) throw new InvalidOperationException("本对象已经被冻结，不可再改变！");
        }

        protected void SetValue<T>(ref T field, T value)
        {
            this.CheckUnFrozen();

            field = value;
        }

        /// <summary>
        /// 当对象被冻结时发生。
        /// </summary>
        protected virtual void OnFrozen()
        {
            foreach (var field in this.EnumerateAllFileds())
            {
                if (field.HasMarked<UnAutoFreezeAttribute>()) { continue; }

                var value = field.GetValue(this);
                var fValue = value as Freezable;
                if (fValue != null)
                {
                    fValue.Freeze();
                }
                else
                {
                    var collection = value as IEnumerable;
                    if (collection != null)
                    {
                        foreach (var item in collection)
                        {
                            var child = item as Freezable;
                            if (child != null)
                            {
                                child.Freeze();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 调用所有孩子的Freeze方法
        /// </summary>
        /// <param name="children"></param>
        protected static void FreezeChildren(IList children)
        {
            for (int i = 0, c = children.Count; i < c; i++)
            {
                var child = children[i] as Freezable;
                if (child == null) throw new InvalidOperationException("集合内所有对象必须继承自Freeable。");
                child.Freeze();
            }
        }

        /// <summary>
        /// 调用所有孩子的Freeze方法
        /// </summary>
        /// <param name="children"></param>
        protected static void FreezeChildren(IEnumerable children)
        {
            foreach (var item in children)
            {
                var child = item as Freezable;
                if (child == null) throw new InvalidOperationException("集合内所有对象必须继承自Freeable。");
                child.Freeze();
            }
        }

        /// <summary>
        /// 调用所有孩子的Freeze方法
        /// </summary>
        /// <param name="children"></param>
        protected static void FreezeChildren(params Freezable[] children)
        {
            foreach (var child in children)
            {
                if (child == null) throw new InvalidOperationException("集合内所有对象必须继承自Freeable。");
                child.Freeze();
            }
        }

        #endregion

        #region Clone

        protected Freezable Clone(FreezableCloneOptions option)
        {
            Freezable target = null;

            if (!option.CopiedPairs.TryGetValue(this, out target))
            {
                target = Activator.CreateInstance(this.GetType()) as Freezable;
                option.CopiedPairs[this] = target;
                target.CloneValues(this, option);
            }

            return target;
        }

        /// <summary>
        /// 子类重写此方法来实现更复杂的复制功能。
        /// 默认使用反射进行对象的拷贝。
        /// 
        /// 注意：
        /// 集合字段，需要直接声明为 <![CDATA[IList<T>]]> 类型才能进行拷贝！！！
        /// </summary>
        /// <param name="target"></param>
        /// <param name="option"></param>
        protected virtual void CloneValues(Freezable target, FreezableCloneOptions option)
        {
            var isFreeze = this._frozen;
            this._frozen = false;

            var allfields = this.EnumerateAllFileds();
            foreach (var field in allfields)
            {
                if (field.HasMarked<NonSerializedAttribute>()) { continue; }

                var value = field.GetValue(target);

                //如果是引用对象，则进行深拷贝。
                var freezable = value as Freezable;
                if (freezable != null)
                {
                    if (option.DeepCloneRef)
                    {
                        value = freezable.Clone(option);
                    }
                }

                //如果是子对象集合，则进行递归拷贝。
                if (value is IList && option.CloneChildren)
                {
                    var fieldValue = field.GetValue(this);
                    if (fieldValue != null)
                    {
                        var myChildren = fieldValue as IList;
                        if (myChildren == null) throw new NotSupportedException("不支持不能转换为 IList 的字段" + field.Name);
                        if (!myChildren.IsReadOnly)
                        {
                            foreach (var item in value as IEnumerable)
                            {
                                var freeable = item as Freezable;
                                if (freeable != null)
                                {
                                    var newChild = freeable.Clone(option);
                                    myChildren.Add(newChild);
                                }
                            }
                        }
                    }
                    continue;
                }

                field.SetValue(this, value);
            }

            this._frozen = isFreeze;
        }

        private IEnumerable<FieldInfo> EnumerateAllFileds()
        {
            var type = this.GetType();
            while (type != typeof(Freezable) && type != null)
            {
                foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    yield return field;
                }

                type = type.BaseType;
            }
        }

        #endregion

        protected override void OnExtendedPropertyChanging(string property, object value)
        {
            this.CheckUnFrozen();

            base.OnExtendedPropertyChanging(property, value);
        }
    }

    /// <summary>
    /// ready for extend
    /// </summary>
    public class FreezableCloneOptions
    {
        /// <summary>
        /// 此数据用于防止循环引用对象时，进行重复的拷贝而导航溢出。
        /// </summary>
        internal Dictionary<Freezable, Freezable> CopiedPairs = new Dictionary<Freezable, Freezable>();

        public bool DeepCloneRef { get; set; }

        public bool CloneChildren { get; set; }
    }

    /// <summary>
    /// 标记此标记的所有字段都不会在冻结时，自动被 Freezable 基类冻结上。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class UnAutoFreezeAttribute : Attribute { }
}