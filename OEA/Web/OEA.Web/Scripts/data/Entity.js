/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：201201
 * 说明：
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 201201
 * 
*******************************************************/

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
    ////override
    //get: function () {
    //    var value = this.callParent(arguments);
    //    return value;
    //},
    //override
    set: function (property, value) {
        this.callParent(arguments);

        if (arguments.length > 1 || !Ext.isObject(property)) {
            this.onPropertyChanged({ property: property, value: value, entity: this });
        }
    },
    //protected virtual
    onPropertyChanged: function (e) {
        this.fireEvent('propertyChanged', e);
    }
});