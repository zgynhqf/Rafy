///*******************************************************
// * 
// * 作者：胡庆访
// * 创建时间：20130403 19:09
// * 说明：此文件只包含一个类，具体内容见类型注释。
// * 运行环境：.NET 4.0
// * 版本号：1.0.0
// * 
// * 历史记录：
// * 创建文件 胡庆访 20130403 19:09
// * 
//*******************************************************/

//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.Linq;
//using System.Text;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using Rafy.DomainModeling.Controls;
//using Rafy.DomainModeling.Models.Xml;

//namespace Rafy.DomainModeling.Models
//{
//    /// <summary>
//    /// 不采用绑定时的文档加载器。
//    /// </summary>
//    internal static class DocumentHelper
//    {
//        internal static void LoadDocument(ModelingDesigner designer, ODMLDocument document)
//        {
//            //清空当前所有项。
//            Clear(designer);

//            var items = designer.Blocks;
//            foreach (var type in document.EntityTypes)
//            {
//                var control = CreateEntityTypeControl(type);

//                items.Add(control);
//            }

//            foreach (var type in document.EnumTypes)
//            {
//                var control = CreateEnumControl(type);

//                items.Add(control);
//            }

//            foreach (var conEl in document.Connections)
//            {
//                //一些多余的连接，需要去除。
//                if (string.IsNullOrEmpty(conEl.From) || string.IsNullOrEmpty(conEl.To)) { continue; }
//                if (items.Find(conEl.From) == null || items.Find(conEl.To) == null) { continue; }

//                var connection = new BlockRelation
//                {
//                    FromBlock = conEl.From,
//                    ToBlock = conEl.To,
//                    Label = conEl.Label,
//                    ConnectionType = conEl.ConnectionType,
//                    FromPointPos = conEl.FromPointPos,
//                    ToPointPos = conEl.ToPointPos,
//                };

//                designer.Relations.Add(connection);
//            }
//        }

//        private static void Clear(ModelingDesigner designer)
//        {
//            //designer.Items.Clear();//此 API 无用。
//            var items = designer.Blocks;
//            while (items.Count > 0) { items.RemoveAt(0); }
//            var connections = designer.Relations;
//            while (connections.Count > 0) { connections.RemoveAt(0); }
//        }

//        private static EntityTypeControl CreateEntityTypeControl(EntityTypeElement type)
//        {
//            var control = new EntityTypeControl();

//            control.TypeFullName = type.FullName;
//            control.Label = type.Label;
//            control.Title = type.Name;
//            control.Left = type.Left;
//            control.Top = type.Top;
//            if (type.Width > 0) control.Width = type.Width;
//            if (type.Height > 0) control.Height = type.Height;
//            control.HideDetails = type.HideProperties;

//            foreach (var propertyEl in type.Properties)
//            {
//                var property = new Property
//                {
//                    PropertyName = propertyEl.Name,
//                    PropertyType = propertyEl.PropertyType,
//                    Label = propertyEl.Label,
//                };

//                control.Items.Add(property);
//            }

//            return control;
//        }

//        private static EnumControl CreateEnumControl(EnumElement type)
//        {
//            var control = new EnumControl();

//            control.Label = type.Label;
//            control.Title = type.Name;
//            control.Left = type.Left;
//            control.Top = type.Top;
//            if (type.Width > 0) control.Width = type.Width;
//            if (type.Height > 0) control.Height = type.Height;

//            foreach (var enumItem in type.Items)
//            {
//                var item = new EnumItem
//                {
//                    ItemName = enumItem.Name,
//                    Label = enumItem.Label,
//                    Value = enumItem.Value,
//                };

//                control.Items.Add(item);
//            }

//            return control;
//        }

//        internal static ODMLDocument GetDocument(ModelingDesigner designer)
//        {
//            var doc = new ODMLDocument();

//            foreach (var control in designer.Blocks.OfType<EntityTypeControl>())
//            {
//                var type = CreateEntityTypeElement(control);
//                doc.EntityTypes.Add(type);
//            }

//            foreach (var control in designer.Blocks.OfType<EnumControl>())
//            {
//                var type = CreateEnumElement(control);
//                doc.EnumTypes.Add(type);
//            }

//            foreach (var conControl in designer.Relations)
//            {
//                conControl.ReadDataFromVisual();

//                var connection = new ConnectionElement(conControl.FromBlock, conControl.ToBlock)
//                {
//                    Label = conControl.Label,
//                    ConnectionType = conControl.ConnectionType,
//                    FromPointPos = conControl.FromPointPos,
//                    ToPointPos = conControl.ToPointPos,
//                };

//                doc.Connections.Add(connection);
//            }

//            return doc;
//        }

//        private static EntityTypeElement CreateEntityTypeElement(EntityTypeControl control)
//        {
//            var type = new EntityTypeElement(control.Title);

//            type.Label = control.Label;
//            type.FullName = control.TypeFullName;
//            type.HideProperties = control.HideDetails;
//            type.Left = control.Left;
//            type.Top = control.Top;
//            if (!double.IsNaN(control.Width)) type.Width = control.Width;
//            if (!double.IsNaN(control.Height)) type.Height = control.Height;

//            foreach (var propertyControl in control.Items)
//            {
//                var property = new PropertyElement(propertyControl.PropertyName)
//                {
//                    PropertyType = propertyControl.PropertyType,
//                    Label = propertyControl.Label,
//                };

//                type.Properties.Add(property);
//            }

//            return type;
//        }

//        private static EnumElement CreateEnumElement(EnumControl control)
//        {
//            var type = new EnumElement(control.Title);

//            type.Left = control.Left;
//            type.Top = control.Top;
//            if (!double.IsNaN(control.Width)) type.Width = control.Width;
//            if (!double.IsNaN(control.Height)) type.Height = control.Height;

//            foreach (var enumItemControl in control.Items)
//            {
//                var item = new EnumItemElement(enumItemControl.ItemName)
//                {
//                    Label = enumItemControl.Label,
//                    Value = enumItemControl.Value,
//                };

//                type.Items.Add(item);
//            }

//            return type;
//        }
//    }
//}