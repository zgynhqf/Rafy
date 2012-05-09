Ext.define('Jxc.ProductModule', {
    extend: 'Oea.UITemplate',
    constructor: function (meta) {
        this.callParent(arguments);

        this.setEntityType('JXC.Product');
        this.setServerTemplate('JXC.WPF.ProductModule, JXC');
    },
    onUIGenerated: function (ui) {
        var me = this;

        var lv = ui.getView();
        lv.on('itemCreated', function (e) {
            var code = Jxc.AutoCodeHelper.generateCode(me.getEntityType());
            e.item.set('BianMa', code);
        });

        this.callParent(arguments);
    }
});