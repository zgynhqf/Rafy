Ext.define('Jxc.ProductModule', {
    extend: 'Rafy.ModuleRuntime',
    _onUIGenerated: function (ui) {
        var me = this;

        var lv = ui.getView();
        lv.on('itemCreated', function (e) {
            var code = Jxc.AutoCodeHelper.generateCode(me.getModel());
            e.item.setBianMa(code);
        });

        lv.loadData();

        this.callParent(arguments);
    }
});