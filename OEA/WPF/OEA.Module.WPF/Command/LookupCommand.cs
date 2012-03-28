using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.WPF.Command;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.MetaModel.Attributes;
using OEA.Editors;
using OEA.Module.WPF.Editors;
using SimpleCsla.Core;

using OEA.WPF;

using System.Collections;
using System.ComponentModel;

using System.Windows;
using OEA.Library;


namespace OEA.Module.WPF.Command
{
    [Command(Label = "清除属性值")]
    public class ClearLookupPropertyValueCommand : ClientCommand<PropertyEditor>
    {
        public override void Execute(PropertyEditor editor)
        {
            //清空Object
            string propertyName = editor.PropertyViewInfo.ReferenceViewInfo.RefEntityProperty;
            if (string.IsNullOrEmpty(propertyName) == false)
            {
                var entity = editor.Context.CurrentObject;
                if (entity != null)
                {
                    entity.SetPropertyValue(propertyName, null);
                }
            }

            //清空key
            editor.PropertyValue = null;
        }
    }
}