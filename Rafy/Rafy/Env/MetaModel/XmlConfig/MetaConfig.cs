/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120312
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120312
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Rafy.Reflection;

namespace Rafy.MetaModel.XmlConfig
{
    /// <summary>
    /// 这个类是 xml 变更配置文件中所有运行时类的基类。
    /// 
    /// 这些类需要支持 xml 的序列化和反序列化。
    /// </summary>
    public abstract class MetaConfig
    {
        /// <summary>
        /// 使用这个值的字符串都表示要主动把某个字符串值设置为 null。（默认情况下 null 表示没有更改。）
        /// </summary>
        public static readonly string NullString = "{null}";

        /// <summary>
        /// 子类重写此方法来返回当前的配置是否是有效的。
        /// 只有有效的配置类才需要序列化到 XML 文件中。
        /// </summary>
        /// <returns></returns>
        public abstract bool IsChanged();

        internal XElement Xml
        {
            get { return this.ToXml(); }
            set { this.ReadXml(value); }
        }

        /// <summary>
        /// 如果必要，子类可以重写此方法来指定 XML 元素的名称
        /// </summary>
        /// <returns></returns>
        protected virtual string GetXName()
        {
            return this.GetType().Name;
        }

        /// <summary>
        /// 子类实现此方法来指定 序列化 逻辑
        /// </summary>
        /// <returns></returns>
        protected abstract XElement ToXml();

        /// <summary>
        /// 子类实现此方法来指定 反序列化 逻辑
        /// </summary>
        /// <param name="element"></param>
        protected abstract void ReadXml(XElement element);

        /// <summary>
        /// 创建一个当前对象对应的 XML 元素
        /// </summary>
        /// <returns></returns>
        protected XElement CreateElement()
        {
            return new XElement(this.GetXName());
        }

        protected static void SetAttribute(XElement element, string attriName, object value)
        {
            if (value != null) { element.Add(new XAttribute(attriName, value)); }
        }

        protected static void ReadAttribute<T>(XElement element, string attriName, Action<T> ifValue)
        {
            var a = element.Attribute(attriName);
            if (a != null)
            {
                var value = TypeHelper.CoerceValue<T>(a.Value);
                ifValue(value);
                //if (typeof(T) == typeof(string))
                //{
                //    var value = a.Value;
                //    if (value == NullString) value = null;
                //    ifValue((T)(object)value);
                //}
                //else
                //{
                //    var value = TypeHelper.CoerceValue<T>(a.Value);
                //    ifValue(value);
                //}
            }
        }

        //protected virtual XElement ToXml()
        //{
        //    var res = new XElement(this.GetXName());

        //    var properties = this.GetType().GetProperties();
        //    for (int i = 0, c = properties.Length; i < c; i++)
        //    {
        //        var property = properties[i];
        //        var value = property.GetValue(this, null);
        //        if (value is IEnumerable<EVMXmlChangeModel>)
        //        {
        //            var childElement = new XElement(property.Name);
        //            foreach (var child in value as IEnumerable<EVMXmlChangeModel>)
        //            {
        //                var element = child.ToXml();
        //                childElement.Add(element);
        //            }
        //            res.Add(childElement);
        //        }
        //        else if (value is EVMXmlChangeModel)
        //        {
        //            var childElement = new XElement(property.Name);

        //            var element = (value as EVMXmlChangeModel).ToXml();
        //            childElement.Add(element);

        //            res.Add(childElement);
        //        }
        //        else
        //        {
        //            var childElement = new XAttribute(property.Name, value);
        //            res.Add(childElement);
        //        }
        //    }

        //    return res;
        //}

        //protected virtual void ReadXml(XElement element)
        //{
        //    //var properties = this.GetType().GetProperties();

        //    //foreach (var attri in element.Attributes())
        //    //{
        //    //    var property = properties.First(p => p.Name == attri.Name);
        //    //    var value = TypeHelper.CoerceValue(property.PropertyType, attri.Value);
        //    //    property.SetValue(this, value, null);
        //    //}

        //    //foreach (var child in element.Elements())
        //    //{
        //    //    var property = properties.First(p => p.Name == child.Name);
        //    //    if(typeof(IEnumerable<XmlModel>)  is )
        //    //    var c = Activator.CreateInstance(property);

        //    //    var value = TypeHelper.CoerceValue(property.PropertyType, attri.Value);
        //    //    property.SetValue(this, value, null);
        //    //}

        //    //for (int i = 0, c = properties.Length; i < c; i++)
        //    //{
        //    //    var property = properties[i];
        //    //    var value = property.GetValue(this, null);
        //    //    if (value is IEnumerable<XmlModel>)
        //    //    {
        //    //        var childElement = new XElement(property.Name);
        //    //        foreach (var child in value as IEnumerable<XmlModel>)
        //    //        {
        //    //            var element = child.ToXml();
        //    //            childElement.Add(element);
        //    //        }
        //    //        res.Add(childElement);
        //    //    }
        //    //    else if (value is XmlModel)
        //    //    {
        //    //        var childElement = new XElement(property.Name);

        //    //        var element = (value as XmlModel).ToXml();
        //    //        childElement.Add(element);

        //    //        res.Add(childElement);
        //    //    }
        //    //    else
        //    //    {
        //    //        var childElement = new XAttribute(property.Name, value);
        //    //        res.Add(childElement);
        //    //    }
        //    //}

        //    //return res;
        //}
    }
}