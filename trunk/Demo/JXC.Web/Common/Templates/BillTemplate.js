Ext.define('Jxc.BillTemplate', {
    extend: 'Rafy.UITemplate',
    constructor: function (meta) {
        this.callParent(arguments);

        this.setServerTemplate('JXC.WPF.Templates.BillTemplate, JXC');
    }
});

//rafy:commandEnd

Ext.define('Jxc.ReadonlyBillTemplate', {
    extend: 'Rafy.UITemplate',
    constructor: function (meta) {
        this.callParent(arguments);

        this.setServerTemplate('JXC.WPF.Templates.ReadonlyBillTemplate, JXC');
    }
    ////以下代码用于帮助调试
    //,
    //_onUIGenerated: function (ui) {
    //    //无法解决的问题：
    //    //采购订单添加及查看页面，先切换到附件页签，再切换回订单项页签时，总是莫名其妙地显示 Loading，原因不详????
    //    var c = ui.getView().findChild('JXC.PurchaseOrderItem');
    //    if (c != null) {
    //        //c.getData().on('beforeload', function (store, op, opt) {
    //        //    alert('beforeloadbeforeload');
    //        //});
    //        //c.on('dataChanged', function (e) {
    //        //    if (e.value) {
    //        //        e.value.on('beforeload', function (store, op, opt) {
    //        //            alert('beforeloadbeforeload');
    //        //        });
    //        //    }
    //        //});
    //        var g = c.getControl();
    //        var s = g.setLoading;
    //        g.setLoading = function () {
    //            alert('beforeloadbeforeload');
    //            s.apply(g, arguments);
    //        };
    //        g.setLoading(true);
    //    }
    //}
});
