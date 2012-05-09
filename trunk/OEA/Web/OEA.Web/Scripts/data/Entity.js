Ext.define('Oea.data.Entity', {
    extend: 'Ext.data.Model',
    idProperty: 'Id',
    constructor: function () {
        this.callParent(arguments);

        this.addEvents('propertyChanged');
    },
    fields: [
        { name: 'Id', type: 'int' }
    ],
    proxy: {
        type: 'ajax',
        url: 'OEA_EntityDataPortal.ashx',
        reader: {
            type: 'json',
            root: 'entities',
            totalProperty: 'totalCount'
        }
    },
    //override
    set: function (property, value) {
        this.callParent(arguments);

        if (arguments.length > 1 || !Ext.isObject(property)) {
            this.onPropertyChanged({ property: property, value: value });
        }
    },
    //protected virtual
    onPropertyChanged: function (e) {
        this.fireEvent('propertyChanged', e);
    }
});