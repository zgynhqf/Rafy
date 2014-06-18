Ext.define('JXC.PurchaseOrderItemExt', {
    override: 'JXC.PurchaseOrderItem',
    _onPropertyChanged: function (e) {
        //如果数量或者单价改变，则汇总父对象
        if (e.property == "Amount" || e.property == "RawPrice") {
            this.set("View_TotalPrice", this.get("Amount") * this.get("RawPrice"));
        }

        this.callParent(arguments);
    }
});