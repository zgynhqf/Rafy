/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130416 23:09
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130416 23:09
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Rafy.ManagedProperty
{
    partial class ManagedPropertyObject
    {
        /// <summary>
        /// ManagedPropertyObject 使用这个类型在 Debugger 中显示字段列表。
        /// </summary>
        internal class ManagedPropertyObjectTypeProxy
        {
            private ManagedPropertyObject _mpo;

            public ManagedPropertyObjectTypeProxy(ManagedPropertyObject mpo)
            {
                _mpo = mpo;
            }

            public List<ManagedPropertyField> CompiledProperties
            {
                get { return _mpo._compiledFields.ToList(); }
            }

            public List<ManagedPropertyField> ExtensionProperties
            {
                get
                {
                    return _mpo.PropertiesContainer.GetAvailableProperties()
                        .Where(mp => mp.IsExtension)
                        .Select(mp => new ManagedPropertyField
                        {
                            _property = mp,
                            _value = _mpo.GetProperty(mp)
                        }).ToList();
                }
            }

            public List<ManagedPropertyField> ReadOnlyProperties
            {
                get
                {
                    return _mpo.PropertiesContainer.GetAvailableProperties()
                        .Where(mp => mp.IsReadOnly)
                        .Select(mp => new ManagedPropertyField
                        {
                            _property = mp,
                            _value = _mpo.GetProperty(mp)
                        }).ToList();
                }
            }

            public List<ManagedPropertyField> RuntimeProperties
            {
                get
                {
                    if (_mpo._runtimeFields == null)
                    {
                        return new List<ManagedPropertyField>();
                    }
                    return _mpo._runtimeFields.Select(kv => kv.Value).ToList();
                }
            }

            public Dictionary<string, object> DynamicProperties
            {
                get
                {
                    return _mpo._dynamics;
                }
            }
        }
    }
}
