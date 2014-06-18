/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130403 19:08
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130403 19:08
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using Rafy;
using System.Linq;
using System.Text;
using Rafy.DomainModeling.Models.Xml;
using System.Collections.Specialized;

namespace Rafy.DomainModeling.Models
{
    /// <summary>
    /// 表示一个可以被绘制并保存的建模文档。
    /// 
    /// ODML:Rafy Domain Makeup Lanuage
    /// </summary>
    public class ODMLDocument : DocumentViewModel
    {
        private NotifyChangedCollection<EntityTypeElement> _entityTypes = new NotifyChangedCollection<EntityTypeElement>();
        private NotifyChangedCollection<EnumElement> _enumTypes = new NotifyChangedCollection<EnumElement>();
        private NotifyChangedCollection<ConnectionElement> _connections = new NotifyChangedCollection<ConnectionElement>();

        public ODMLDocument()
        {
            this.Document = this;
            AddChildren(_entityTypes);
            AddChildren(_enumTypes);
            AddChildren(_connections);
        }

        private bool _HideNonsenceLabels;
        /// <summary>
        /// 隐藏同名连接标题
        /// </summary>
        public bool HideNonsenceLabels
        {
            get { return this._HideNonsenceLabels; }
            set
            {
                if (_HideNonsenceLabels != value)
                {
                    _HideNonsenceLabels = value;
                    this.OnPropertyChanged("HideNonsenceLabels");

                    foreach (var connection in this._connections)
                    {
                        ResetConnectionsLabelHidden(connection);
                    }
                }
            }
        }

        private void ResetConnectionsLabelHidden(ConnectionElement connection)
        {
            connection.LabelVisible = true;

            if (_HideNonsenceLabels)
            {
                if (connection.ConnectionType != ConnectionType.Composition && connection.ConnectionType != ConnectionType.Aggregation)
                {
                    BlockElement to = _entityTypes.FirstOrDefault(t => t.FullName == connection.To);
                    if (to == null) to = _enumTypes.FirstOrDefault(t => t.FullName == connection.To);

                    var hasNonsenceLabel = to.Name == connection.Label;
                    if (hasNonsenceLabel)
                    {
                        connection.LabelVisible = false;
                    }
                }
            }
        }

        /// <summary>
        /// 所有的实体类型
        /// </summary>
        public NotifyChangedCollection<EntityTypeElement> EntityTypes
        {
            get { return _entityTypes; }
        }

        /// <summary>
        /// 所有的枚举类型
        /// </summary>
        public NotifyChangedCollection<EnumElement> EnumTypes
        {
            get { return _enumTypes; }
        }

        /// <summary>
        /// 所有显示的连接。
        /// </summary>
        public NotifyChangedCollection<ConnectionElement> Connections
        {
            get { return _connections; }
        }

        /// <summary>
        /// 如果文档中任何一个部分变更，则发生此事件。
        /// </summary>
        public event EventHandler Changed;

        internal void OnChanged()
        {
            var handler = this.Changed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// 通过类型全名称查找某个实体元素。
        /// </summary>
        /// <param name="fullName"></param>
        /// <returns></returns>
        public EntityTypeElement FindEntityType(string fullName)
        {
            return this._entityTypes.FirstOrDefault(el => el.FullName == fullName);
        }

        /// <summary>
        /// 通过类型全名称查找某个枚举元素。
        /// </summary>
        /// <param name="fullName"></param>
        /// <returns></returns>
        public EnumElement FindEnumType(string fullName)
        {
            return this._enumTypes.FirstOrDefault(el => el.FullName == fullName);
        }

        /// <summary>
        /// 通过主键查找连接元素。
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public ConnectionElement FindConnection(string from, string to, string label)
        {
            return this._connections.FirstOrDefault(el => el.From == from && el.To == to && el.Label == label);
        }

        /// <summary>
        /// 通过主键查找连接元素。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ConnectionElement FindConnection(IConnectionKey key)
        {
            //return this._connections.FirstOrDefault(el => el.From == key.From && el.To == key.To && el.Label == key.Label);
            return this.FindConnection(key.From, key.To, key.Label);
        }

        #region Serialization

        /// <summary>
        /// 序列化本文档对象到 xml 文件。
        /// </summary>
        /// <returns></returns>
        public string ToXml()
        {
            var xmlDoc = DocumentXml.ConvertToXmlDoc(this);
            var xml = xmlDoc.ToXml();
            return xml;
        }

        /// <summary>
        /// 反序列化 xml 为一个 odml 文档对象。
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static ODMLDocument Parse(string xml)
        {
            var xmlDoc = DocumentXml.FromXml(xml);
            var doc = xmlDoc.Restore();
            return doc;
        }

        #endregion
    }
}