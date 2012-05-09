Ext.define('Jxc.PurchaseOrderModule', {
    extend: 'Jxc.ConditionQueryModule',
    constructor: function (meta) {
        this.callParent(arguments);

        this.setEntityType('JXC.PurchaseOrder');
        this.setServerTemplate('JXC.WPF.PurchaseOrderModule, JXC');
    }
    //,
    //onUIGenerated: function (ui) {
    //    var me = this;

    //    var lv = ui.getView();
    //    var c = lv.findCmd(Jxc.AddBill);

    //    this.callParent(arguments);
    //}
});