Ext.define('Jxc.AddPurchaseOrderItem', {
    extend: 'Jxc.SelectProductCommand',
    config: { meta: { text: "选择商品", group: "edit" } },

    //proected override
    onItemSelected: function (product, newItem) {
        newItem.set('RawPrice', product.get('CaiGouDanjia'));
    }
});