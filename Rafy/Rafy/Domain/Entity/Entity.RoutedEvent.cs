using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain
{
    //路由事件
    public abstract partial class Entity
    {
        /// <summary>
        /// 发生某个路由事件
        /// 子类重写此方法以实现监听路由事件。
        /// 
        /// 注意：子类在重写时，调用基类方法就表示继续路由。一般在最后才调用基类的方法。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnRoutedEvent(object sender, EntityRoutedEventArgs e)
        {
            this.Route(sender, e);
        }

        /// <summary>
        /// 触发某个路由事件
        /// </summary>
        /// <param name="indicator"></param>
        /// <param name="args"></param>
        protected void RaiseRoutedEvent(EntityRoutedEvent indicator, object args)
        {
            var arg = new EntityRoutedEventArgs
            {
                Source = this,
                Event = indicator,
                Args = args
            };

            this.Route(this, arg);
        }

        internal void RouteByList(object sender, EntityRoutedEventArgs e)
        {
            this.OnRoutedEvent(sender, e);
        }

        private void Route(object sender, EntityRoutedEventArgs e)
        {
            if (e.Handled) return;

            switch (e.Event.Type)
            {
                case EntityRoutedEventType.BubbleToParent:

                    var parent = (this as IEntity).FindParentEntity();
                    if (parent != null)
                    {
                        parent.OnRoutedEvent(sender, e);
                    }

                    break;
                case EntityRoutedEventType.BubbleToTreeParent:

                    if (_treeParent != null)
                    {
                        _treeParent.OnRoutedEvent(sender, e);
                    }

                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// LogicalView 路由事件的参数
    /// </summary>
    public class EntityRoutedEventArgs : EventArgs
    {
        internal EntityRoutedEventArgs() { }

        /// <summary>
        /// 事件源头的实体
        /// </summary>
        public IDomainComponent Source { get; internal set; }

        /// <summary>
        /// 发生的事件标记
        /// </summary>
        public EntityRoutedEvent Event { get; internal set; }

        /// <summary>
        /// 事件参数
        /// </summary>
        public object Args { get; internal set; }

        public bool Handled { get; set; }
    }

    /// <summary>
    /// Entity 路由事件的标记
    /// </summary>
    public class EntityRoutedEvent
    {
        private EntityRoutedEvent() { }

        public EntityRoutedEventType Type { get; private set; }

        public static EntityRoutedEvent Register(EntityRoutedEventType type)
        {
            return new EntityRoutedEvent()
            {
                Type = type
            };
        }
    }

    public enum EntityRoutedEventType
    {
        BubbleToParent,
        BubbleToTreeParent
    }
}