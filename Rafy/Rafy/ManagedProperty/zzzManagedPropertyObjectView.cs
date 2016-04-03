//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.ComponentModel;
//using Rafy.ManagedProperty;

//namespace Rafy.ManagedProperty
//{
//    public class ManagedPropertyLogicalView : INotifyPropertyChanged, ICustomTypeDescriptor
//    {
//        private ManagedPropertyObject _coreObject;

//        public ManagedPropertyLogicalView(ManagedPropertyObject coreObject)
//        {
//            this._coreObject = coreObject;
//        }

//        internal ManagedPropertyObject CoreObject
//        {
//            get { return this._coreObject; }
//        }

//        public event PropertyChangedEventHandler PropertyChanged
//        {
//            add { this._coreObject.PropertyChanged += value; }
//            remove { this._coreObject.PropertyChanged -= value; }
//        }

//        #region ICustomTypeDescriptor Members

//        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
//        {
//            return ManagedPropertyDescriptor.GetProperties(this._coreObject);
//        }

//        //以下实现复制自 DataRowView

//        AttributeCollection ICustomTypeDescriptor.GetAttributes()
//        {
//            return new AttributeCollection(null);
//        }
//        string ICustomTypeDescriptor.GetClassName()
//        {
//            return null;
//        }
//        string ICustomTypeDescriptor.GetComponentName()
//        {
//            return null;
//        }
//        TypeConverter ICustomTypeDescriptor.GetConverter()
//        {
//            return null;
//        }
//        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
//        {
//            return null;
//        }
//        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
//        {
//            return null;
//        }
//        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
//        {
//            return null;
//        }
//        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
//        {
//            return new EventDescriptorCollection(null);
//        }
//        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
//        {
//            return new EventDescriptorCollection(null);
//        }
//        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
//        {
//            return ((ICustomTypeDescriptor)this).GetProperties(null);
//        }
//        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
//        {
//            return this;
//        }

//        #endregion
//    }
//}