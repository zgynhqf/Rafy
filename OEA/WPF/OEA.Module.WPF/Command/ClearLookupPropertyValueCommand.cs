/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100331
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100331
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using OEA.Editors;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using OEA.Module.WPF.Editors;
using OEA.WPF;
using OEA.WPF.Command;
using SimpleCsla.Core;


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