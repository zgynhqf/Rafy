//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.CodeDom.Compiler;
//using System.IO;
//using Rafy.MetaModel;

//namespace Rafy.Web
//{
//    internal class EntityScriptGenerator
//    {
//        private IndentedTextWriter _writer;

//        internal EntityMeta EntityMeta { get; set; }

//        internal string Generate()
//        {
//            using (var jsonWriter = new StringWriter())
//            {
//                this._writer = new IndentedTextWriter(jsonWriter);

//                this.WriteClientType();

//                var json = jsonWriter.ToString();

//                return json;
//            }
//        }

//        private void WriteClientType()
//        {
//            this.WriteTypeBegin();
//            this.WriteFields();
//            this.WriteAssications();
//            //this.WriteTypeProxy();
//            this.WriteTypeEnd();
//        }

//        private void WriteTypeBegin()
//        {
//            var type = this.EntityMeta.EntityType;

//            this.Write("Ext.define('");
//            this.Write(type.FullName);
//            this.WriteLine("', {");
//            this._writer.Indent++;
//            this.Write("serverType: '");
//            this.Write(type.AssemblyQualifiedName);
//            this.WriteLine("',");
//            this.WriteLine("extend: 'Rafy.Entity',");
//        }

//        private void WriteFields()
//        {
//            this.WriteLine("fields: [");

//            this._writer.Indent++;
//            var properties = this.EntityMeta.EntityProperties;
//            for (int i = 0, c = properties.Count; i < c; i++)
//            {
//                var property = properties[i];
//                if (property.ReferenceInfo == null && property.Name != DBConvention.FieldName_Id)
//                {
//                    this.Write("{ name: '");
//                    this.Write(property.Name);
//                    this.Write("', type: '");
//                    this.Write(this.GetFieldClientType(property.Runtime.PropertyType));
//                    this.Write("' }");
//                    if (i != c - 1) { this.Write(","); }
//                    this.WriteLine();
//                }
//            }
//            this._writer.Indent--;

//            this.WriteLine("],");
//        }
//        private string GetFieldClientType(Type fieldType)
//        {
//            if (fieldType == typeof(string) || fieldType == typeof(Guid) || fieldType.IsEnum) { return "string"; }
//            if (fieldType == typeof(int)) { return "int"; }
//            if (fieldType == typeof(double)) { return "double"; }
//            if (fieldType == typeof(bool)) { return "boolean"; }
//            if (fieldType == typeof(DateTime)) { return "datetime"; }
//            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(Nullable<>))
//            {
//                var innerType = fieldType.GetGenericArguments()[0];
//                return this.GetFieldClientType(innerType);
//            }
//            throw new NotSupportedException();
//        }

//        private void WriteAssications()
//        {
//            this.WriteLine("associations: [");
//            this._writer.Indent++;

//            var properties = this.EntityMeta.EntityProperties;
//            var children = this.EntityMeta.ChildrenProperties;

//            for (int i = 0, c = properties.Count; i < c; i++)
//            {
//                var property = properties[i];
//                if (property.ReferenceInfo != null)
//                {
//                    this.Write("{ type: 'belongsTo', name: '");
//                    this.Write(property.ReferenceInfo.RefEntityProperty);
//                    this.Write("', model: '");
//                    this.Write(property.ReferenceInfo.RefType.FullName);
//                    this.Write("'}");

//                    if (children.Count > 0 || i != c - 1) { this.Write(","); }
//                    this.WriteLine();
//                }
//            }

//            for (int i = 0, c = children.Count; i < c; i++)
//            {
//                var child = children[i];

//                var childType = child.ChildType.EntityType;
//                var name = childType.Name + "s";
//                var model = childType.FullName;

//                this.Write("{ type: 'hasMany', name: '");
//                this.Write(name);
//                this.Write("', model: '");
//                this.Write(model);
//                this.Write("'}");

//                if (i != c - 1) { this.Write(","); }
//                this.WriteLine();
//            }

//            this._writer.Indent--;
//            this.WriteLine("]");
//        }

//        //private void WriteTypeProxy()
//        //{
//        //    this.WriteLine("proxy: {");
//        //    this._writer.Indent++;
//        //    this.WriteLine("type: 'ajax',");
//        //    this.WriteLine("reader: {");
//        //    this._writer.Indent++;
//        //    this.WriteLine("type: 'json',");
//        //    this.WriteLine("root: 'entities',");
//        //    this.WriteLine("totalProperty: 'totalCount',");
//        //    this.WriteLine("url: 'Rafy_EntityDataPortal.ashx'");
//        //    this._writer.Indent--;
//        //    this.WriteLine("}");
//        //    this._writer.Indent--;
//        //    this.WriteLine("}");
//        //}

//        private void WriteTypeEnd()
//        {
//            this._writer.Indent--;
//            this.WriteLine("});");
//        }

//        private void Write(string text)
//        {
//            this._writer.Write(text);
//        }

//        private void WriteLine(string text)
//        {
//            this._writer.WriteLine(text);
//        }

//        private void WriteLine()
//        {
//            this._writer.WriteLine();
//        }
//    }
//}
