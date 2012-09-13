using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Reflection;
using System.Windows.Controls;

namespace OEA.Module.WPF
{
    public static class WPFExtension
    {
        #region Trees

        /// <summary>
        /// Returns the first visual child from parent based on T
        /// </summary>        
        public static T GetVisualChild<T>(this Visual parent) where T : Visual
        {
            T child = default(T);

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }

            return child;
        }

        /// <summary>
        /// 从指定元素往可视树上方查找，找到第一个指定类型的元素时，把该元素返回。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="child"></param>
        /// <returns></returns>
        public static T GetVisualParent<T>(this DependencyObject child) where T : Visual
        {
            var parent = VisualTreeHelper.GetParent(child);
            if (parent == null) return null;
            if (parent is T) return parent as T;

            return parent.GetVisualParent<T>();
        }

        /// <summary>
        /// 从指定元素往可视树上方查找，找到第一个指定类型的元素时，把该元素返回。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="child"></param>
        /// <returns></returns>
        public static T GetLogicalParent<T>(this DependencyObject child) where T : Visual
        {
            var parent = LogicalTreeHelper.GetParent(child);
            if (parent == null) return null;
            if (parent is T) return parent as T;

            return parent.GetLogicalParent<T>();
        }

        /// <summary>
        /// 获取某个指定元素的可视树根对象。
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        public static DependencyObject GetVisualRoot(this DependencyObject child)
        {
            var parent = child;
            var root = parent;
            while (parent != null)
            {
                root = parent;
                parent = VisualTreeHelper.GetParent(parent);
            }

            return root;
        }

        /// <summary>
        /// 把某一个元素从逻辑树中移除，使用另一个新的元素在原来的位置中替换它。
        /// </summary>
        /// <param name="oldElement"></param>
        /// <param name="newElement"></param>
        public static void ReplaceInParent(this FrameworkElement oldElement, FrameworkElement newElement)
        {
            oldElement.RemoveFromParent(false);

            SetLastParent(newElement, GetLastParent(oldElement));
            SetLastIndexInParent(newElement, GetLastIndexInParent(oldElement));

            newElement.AttachToLastParent();
        }

        /// <summary>
        /// 把某个元素从逻辑树中移除
        /// </summary>
        /// <param name="element"></param>
        public static void RemoveFromParent(this FrameworkElement element, bool recur = true)
        {
            if (element == null) throw new ArgumentNullException("element");

            if (element.Parent == null) return;

            SetLastParent(element, element.Parent as FrameworkElement);

            var panel = element.Parent as Panel;
            if (panel != null)
            {
                SetLastIndexInParent(element, panel.Children.IndexOf(element));
                panel.Children.Remove(element);
                if (recur)
                {
                    if (panel.Children.Count == 0)
                    {
                        panel.RemoveFromParent(true);
                    }
                }
                return;
            }

            var itemsControl = element.Parent as ItemsControl;
            if (itemsControl != null)
            {
                SetLastIndexInParent(element, itemsControl.Items.IndexOf(element));
                itemsControl.Items.Remove(element);
                if (recur)
                {
                    if (itemsControl.Items.Count == 0)
                    {
                        itemsControl.RemoveFromParent(true);
                    }
                }
                return;
            }

            var contentControl = element.Parent as ContentControl;
            if (contentControl != null)
            {
                contentControl.Content = null;
                if (recur)
                {
                    contentControl.RemoveFromParent(recur);
                }
                return;
            }

            throw new NotSupportedException();
        }

        public static void AttachToLastParent(this FrameworkElement element)
        {
            if (element == null) throw new ArgumentNullException("element");

            if (element.Parent != null) return;

            var parent = GetLastParent(element);
            if (parent == null) return;

            AttachToParent(element, parent);

            parent.AttachToLastParent();
        }

        public static void AttachToParent(this FrameworkElement element, DependencyObject parent)
        {
            var panel = parent as Panel;
            if (panel != null)
            {
                int index = GetLastIndexInParent(element);
                if (index < panel.Children.Count)
                {
                    panel.Children.Insert(index, element);
                }
                else
                {
                    panel.Children.Add(element);
                }
                return;
            }

            var itemsControl = parent as ItemsControl;
            if (itemsControl != null)
            {
                int index = GetLastIndexInParent(element);
                if (index < panel.Children.Count)
                {
                    itemsControl.Items.Insert(index, element);
                }
                else
                {
                    itemsControl.Items.Add(element);
                }
                return;
            }

            var contentControl = parent as ContentControl;
            if (contentControl != null)
            {
                contentControl.Content = element;
                return;
            }

            throw new NotSupportedException();
        }

        #endregion

        #region GetDependencyProperty

        /// <summary>
        /// Retrieves a <see cref="DependencyProperty"/> using reflection.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static DependencyProperty GetDependencyProperty(this DependencyObject obj, string propertyName)
        {
            DependencyProperty prop = null;

            if (obj != null)
            {
                prop = GetDependencyProperty(obj.GetType(), propertyName);
            }

            return prop;
        }

        /// <summary>
        /// Gets the dependency property according to its name.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        private static DependencyProperty GetDependencyProperty(Type type, string propertyName)
        {
            DependencyProperty prop = null;

            if (type != null)
            {
                FieldInfo fieldInfo = type.GetField(propertyName + "Property",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

                if (fieldInfo != null)
                {
                    prop = fieldInfo.GetValue(null) as DependencyProperty;
                }
            }

            return prop;
        }

        #endregion

        /// <summary>
        /// Sets the value of the <paramref name="property"/> only if it hasn't been explicitely set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="property">The property.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static bool SetIfNonLocal<T>(this DependencyObject obj, DependencyProperty property, T value)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            if (property == null) throw new ArgumentNullException("property");

            if (DependencyPropertyHelper.GetValueSource(obj, property).BaseValueSource != BaseValueSource.Local)
            {
                obj.SetValue(property, value);

                return true;
            }

            return false;
        }

        internal static FrameworkElement LoadContent(this FrameworkElementFactory visualTree)
        {
            var template = new ControlTemplate()
            {
                VisualTree = visualTree
            };
            template.Seal();
            return template.LoadContent() as FrameworkElement;
        }

        #region ListenLayoutUpdatedOnce

        /// <summary>
        /// 只监听目标对象的一次 LayoutUpdated 事件。
        /// </summary>
        /// <param name="element"></param>
        /// <param name="handler"></param>
        public static void ListenLayoutUpdatedOnce(this UIElement element, EventHandler handler)
        {
            new ListenLayoutUpdatedOnceHelper
            {
                element = element,
                handler = handler
            }.Listen();
        }

        private class ListenLayoutUpdatedOnceHelper
        {
            public UIElement element;
            public EventHandler handler;

            public void Listen()
            {
                element.LayoutUpdated += element_LayoutUpdated;
            }

            void element_LayoutUpdated(object sender, EventArgs e)
            {
                element.LayoutUpdated -= element_LayoutUpdated;
                handler(sender, e);
            }
        }

        #endregion

        #region LastParent

        public static readonly DependencyProperty LastParentProperty =
            DependencyProperty.RegisterAttached("LastParent", typeof(FrameworkElement), typeof(WPFExtension));

        public static void SetLastParent(FrameworkElement d, FrameworkElement value)
        {
            d.SetValue(LastParentProperty, value);
        }

        public static FrameworkElement GetLastParent(FrameworkElement d)
        {
            return d.GetValue(LastParentProperty) as FrameworkElement;
        }

        #endregion

        #region LastIndexInParent

        public static readonly DependencyProperty LastIndexInParentProperty =
            DependencyProperty.RegisterAttached("LastIndexInParent", typeof(int), typeof(WPFExtension));

        public static void SetLastIndexInParent(FrameworkElement d, int value)
        {
            d.SetValue(LastIndexInParentProperty, value);
        }

        public static int GetLastIndexInParent(FrameworkElement d)
        {
            return (int)d.GetValue(LastIndexInParentProperty);
        }

        #endregion
    }
}
