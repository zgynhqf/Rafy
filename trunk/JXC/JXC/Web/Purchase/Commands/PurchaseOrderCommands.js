Ext.define('Jxc.AddPurchaseOrder', {
    extend: 'Jxc.AddBill',
    config: { meta: { text: "添加采购订单", group: "edit" } },

    constructor: function () {
        this.callParent(arguments);

        this.setSvc('JXC.AddPurchaseOrderService');

        this._attachCollectionBehavior();
    },

    //protected override
    onItemCreated: function (item) {
        item.set('Code', Jxc.AutoCodeHelper.generateCode(this.getView().getModel()));
    },

    _attachCollectionBehavior: function () {
        /// <summary>
        /// 在生成后的界面中，加入以下行为：点击总金额这个属性编辑器时，汇总总金额
        /// </summary>
        var me = this;
        this.getTemplate().on('uiGenerated', function (e) {
            var view = e.ui.getView();
            var tmEditor = view.findEditor('TotalMoney');
            tmEditor.on('render', function () {
                tmEditor.getEl().on('mousedown', function () {
                    me._collectTotalMoney(view);
                });
            });
        });
    },
    _collectTotalMoney: function (view) {
        /// <summary>
        /// 汇总总金额
        /// </summary>
        /// <param name="view"></param>

        var po = view.updateEntity();

        var list = po.PurchaseOrderItemList();
        var sum = Oea.sum(list, function (i) {
            var RawPrice = i.get("RawPrice");
            var Amount = i.get("Amount");
            return RawPrice * Amount;
        });
        po.set("TotalMoney", sum);

        view.updateControl();
    }
});