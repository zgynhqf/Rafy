using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Reflection;

using OEA.Module.WPF.Controls;
using System.Windows.Controls;

namespace OEA.Module.WPF
{
    public static class WPFExtension
    {
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

        public static T GetVisualParent<T>(this DependencyObject child) where T : Visual
        {
            var v = VisualTreeHelper.GetParent(child);
            if (v == null) return null;
            if (v is T) return v as T;

            return v.GetVisualParent<T>();
        }

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

        public static T GetLogicalParent<T>(this DependencyObject child) where T : Visual
        {
            var v = LogicalTreeHelper.GetParent(child);
            if (v == null) return null;
            if (v is T) return v as T;

            return v.GetLogicalParent<T>();
        }

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

        /// <summary>
        /// Sets the value of the <paramref name="property"/> only if it hasn't been explicitely set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="property">The property.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static bool SetIfDefault<T>(this DependencyObject obj, DependencyProperty property, T value)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            if (property == null) throw new ArgumentNullException("property");
            if (!property.PropertyType.IsAssignableFrom(typeof(T))) { throw new ArgumentException(""); }

            if (DependencyPropertyHelper.GetValueSource(obj, property).BaseValueSource == BaseValueSource.Default)
            {
                obj.SetValue(property, value);

                return true;
            }

            return false;
        }

        /// <summary>
        /// 找到最上层的IFrameTemplate
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static IEntityWindow GetWorkspaceWindow(this FrameworkElement element)
        {
            DependencyObject currentElement = element;

            while (currentElement != null)
            {
                var win = currentElement as IEntityWindow;
                if (win != null) return win;

                currentElement = LogicalTreeHelper.GetParent(currentElement) ??
                    VisualTreeHelper.GetParent(currentElement);
            }

            return null;
        }

        /// <summary>
        /// 获取某个 view 所在的 工作区页签
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public static IEntityWindow GetWorkspaceWindow(this ObjectView view)
        {
            return (view.Control as FrameworkElement).GetWorkspaceWindow();
        }

        /// <summary>
        /// 把某一个元素从逻辑树中移除，使用另一个新的元素在原来的位置中替换它。
        /// </summary>
        /// <param name="oldElement"></param>
        /// <param name="newElement"></param>
        public static void ReplaceInParent(this FrameworkElement oldElement, FrameworkElement newElement)
        {
            oldElement.RemoveFromParent(false);

            WPFMeta.SetLastParent(newElement, WPFMeta.GetLastParent(oldElement));
            WPFMeta.SetLastIndexInParent(newElement, WPFMeta.GetLastIndexInParent(oldElement));

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

            WPFMeta.SetLastParent(element, element.Parent as FrameworkElement);

            var panel = element.Parent as Panel;
            if (panel != null)
            {
                WPFMeta.SetLastIndexInParent(element, panel.Children.IndexOf(element));
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
                WPFMeta.SetLastIndexInParent(element, itemsControl.Items.IndexOf(element));
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

            var parent = WPFMeta.GetLastParent(element);
            if (parent == null) return;

            AttachToParent(element, parent);

            parent.AttachToLastParent();
        }

        public static void AttachToParent(this FrameworkElement element, DependencyObject parent)
        {
            var panel = parent as Panel;
            if (panel != null)
            {
                int index = WPFMeta.GetLastIndexInParent(element);
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
                int index = WPFMeta.GetLastIndexInParent(element);
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

        internal static FrameworkElement LoadContent(this FrameworkElementFactory visualTree)
        {
            var template = new ControlTemplate()
            {
                VisualTree = visualTree
            };
            template.Seal();
            return template.LoadContent() as FrameworkElement;
        }
    }
}
