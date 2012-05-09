Ext.define('Jxc.SelectProductCommand', {
    extend: 'Oea.cmd.Command',
    config: { meta: { text: "选择商品", group: "edit" } },

    _template: null,

    constructor: function () {
        this.callParent(arguments);

        this._template = new Jxc.ProductModule();
        this._template.on('blocksDefined', function (e) {
            var gc = e.blocks.mainBlock.gridConfig;
            gc.selType = 'checkboxmodel';
            gc.selModel = { checkOnly: true };

            e.blocks.children = [];
        });
        this._template.on('uiGenerated', function (e) {
            e.ui.getView().loadData();
        });
    },
    canExecute: function (view) {
        return this.isParentSelected(view);
    },

    execute: function (listView, source) {
        var me = this;
        var ui = this._template.createUI();

        Oea.Window.show({
            title: "选择商品", items: ui.getControl(),
            callback: function (btn) {
                if (btn == "确定") {
                    Oea.each(ui.getView().getSelection(), function (product) {
                        var newItem = listView.addNew();

                        newItem.set('ProductId', product.getId());
                        newItem.set('PurchaseOrderId_Label', product.get('MingCheng'));
                        newItem.set('View_ProductName', product.get('MingCheng'));
                        newItem.set('View_ProductCategoryName', product.get('ProductCategoryId_Label'));
                        newItem.set('View_Specification', product.get('GuiGe'));

                        me.onItemSelected(product, newItem);

                        ////以下代码不能刷新界面，不知道为何？
                        //var o = {
                        //    ProductId: product.getId(),
                        //    PurchaseOrderId_Label: product.get('MingCheng'),
                        //    View_ProductName: product.get('MingCheng'),
                        //    View_ProductCategoryName: product.get('ProductCategoryId_Label'),
                        //    View_Specification: product.get('GuiGe')
                        //};
                        //newItem.set(o);
                    });
                }
            }
        });
    },
    //protected virtual
    onItemSelected: function (product, newItem) { }
});