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
        private Dictionary<Type, List<Action<object>>> _handlers = new Dictionary<Type, List<Action<object>>>();

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
            List<Action<object>> res = null;
            if (_handlers.TryGetValue(type, out res))
            {
                for (int i = 0, c = res.Count; i < c; i++)
                {
                    var action = res[i];
                    action(eventModel);
                }
            }
        }

        public void Subscribe<TEvent>(Action<TEvent> handler)
        {
            var type = typeof(TEvent);

            List<Action<object>> res = null;
            if (!_handlers.TryGetValue(type, out res))
            {
                res = new List<Action<object>>();
                _handlers.Add(type, res);
            }

            res.Add(e => handler((TEvent)e));
        }
    }
}