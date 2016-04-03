/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140629
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140629 00:20
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.ComponentModel
{
    internal class EventBus : IEventBus
    {
        private Dictionary<Type, IEventSubscribers> _subscribers = new Dictionary<Type, IEventSubscribers>();

        public void Publish(object eventModel)
        {
            if (eventModel == null) throw new ArgumentNullException("eventModel");
            PublishCore(eventModel.GetType(), eventModel);
        }

        public void Publish<TEvent>(TEvent eventModel)
        {
            if (eventModel == null) throw new ArgumentNullException("eventModel");
            PublishCore(typeof(TEvent), eventModel);
        }

        private void PublishCore(Type type, object eventModel)
        {
            IEventSubscribers subscribers = null;
            if (_subscribers.TryGetValue(type, out subscribers))
            {
                subscribers.Publish(eventModel);
            }
        }

        public void Subscribe<TEvent>(object subscriber, Action<TEvent> handler)
        {
            var type = typeof(TEvent);

            IEventSubscribers subscribers = null;
            if (!_subscribers.TryGetValue(type, out subscribers))
            {
                subscribers = new EventSubscribers<TEvent>();
                _subscribers.Add(type, subscribers);
            }

            var concrete = subscribers as EventSubscribers<TEvent>;

            concrete.Add(new EventSubscriberItem<TEvent>
            {
                Subscriber = subscriber,
                Handler = handler
            });
        }

        public void Unsubscribe<TEvent>(object subscriber)
        {
            var type = typeof(TEvent);

            IEventSubscribers subscribers = null;
            if (_subscribers.TryGetValue(type, out subscribers))
            {
                var concrete = subscribers as EventSubscribers<TEvent>;

                concrete.RemoveByOwner(subscriber);

                if (concrete.Count == 0)
                {
                    _subscribers.Remove(type);
                }
            }
        }

        public IEventSubscribers GetSubscribers<TEvent>()
        {
            var type = typeof(TEvent);

            IEventSubscribers subscribers = null;
            _subscribers.TryGetValue(type, out subscribers);

            return subscribers;
        }

        private class EventSubscribers<TEvent> : IEventSubscribers
        {
            private List<EventSubscriberItem<TEvent>> _items = new List<EventSubscriberItem<TEvent>>();

            internal void Add(EventSubscriberItem<TEvent> item)
            {
                _items.Add(item);
            }

            internal void RemoveByOwner(object subscriber)
            {
                for (int i = _items.Count - 1; i >= 0; i--)
                {
                    var item = _items[i];
                    if (item.Subscriber == subscriber)
                    {
                        _items.RemoveAt(i);
                    }
                }
            }

            public Type EventType
            {
                get { return typeof(TEvent); }
            }

            public int Count
            {
                get { return _items.Count; }
            }

            public void Publish(object eventModel)
            {
                for (int i = 0, c = _items.Count; i < c; i++)
                {
                    var item = _items[i];
                    item.Handle(eventModel);
                }
            }
        }

        class EventSubscriberItem<TEvent>
        {
            public object Subscriber;
            public Action<TEvent> Handler;

            public void Handle(object eventModel)
            {
                Handler((TEvent)eventModel);
            }
        }
    }
}