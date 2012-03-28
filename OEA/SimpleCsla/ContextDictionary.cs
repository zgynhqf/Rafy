using System;
using System.Net;
using System.Collections.Generic;
using SimpleCsla.Serialization.Mobile;
using System.Collections.Specialized;

namespace SimpleCsla.Core
{
    /// <summary>
    /// Dictionary type that is serializable
    /// with the MobileFormatter.
    /// </summary>
    [Serializable()]
    public class ContextDictionary : HybridDictionary
    {
    }
}
