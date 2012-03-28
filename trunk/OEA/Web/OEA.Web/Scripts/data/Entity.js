Ext.define('Oea.data.Entity', {
    extend: 'Ext.data.Model',
    idProperty: 'Id',
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
    }
});