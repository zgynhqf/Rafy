using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace Rafy
{
    /// <summary>
    /// A simple class to deal with xml serialize
    /// This class is thread safe
    /// </summary>
    public class XmlFormatter : IStateFormatter
    {
        private XmlSerializer _serializer;

        #region Constructor

        /// <summary>
        /// Construct a XmlFormatter to format a specific type
        /// </summary>
        /// <param name="t"></param>
        public XmlFormatter(Type t)
        {
            if (t == null) { throw new ArgumentNullException(); }

            this._serializer = new XmlSerializer(t);
        }

        #endregion

        /// <summary>
        /// Serialize a object of the specific type to a xml document.
        /// </summary>
        /// <param name="x">The object to be serialized.</param>
        /// <returns></returns>
        public string Serialize(object x)
        {
            StringWriter w = new StringWriter();
            this._serializer.Serialize(w, x);
            return w.ToString();
        }

        /// <summary>
        /// Deserialize the xml to the original obecjt
        /// </summary>
        /// <param name="xml">xml document</param>
        /// <returns></returns>
        public object Deserialize(string xml)
        {
            StringReader sr = new StringReader(xml);
            return this._serializer.Deserialize(sr);
        }
    }
}
