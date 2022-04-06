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

Rafy.defineCommand('Jxc.AddPurchaseOrder', {
    extend: 'Jxc.AddBill',
    meta: { text: "添加采购订单", group: "edit" },

    constructor: function () {
        this.callParent(arguments);

        this.setSvc('JXC.AddPurchaseOrderService');

        this._attachCollectionBehavior();
    },

    //protected override
    onItemCreated: function (item) {
        var model = this.getOwnerView().getModel();
        item.set('Code', Jxc.AutoCodeHelper.generateCode(model));
        item.setDate(new Date());
        item.setPlanStorageInDate(new Date());
    },

    _attachCollectionBehavior: function () {
        /// <summary>
        /// 在生成后的界面中，加入以下行为：点击总金额这个属性编辑器时，汇总总金额
        /// </summary>
        var me = this;
        this.getTemplate().on('uiGenerated', function (e) {
            setInterval(function () {
                me._collectTotalMoney(e.ui.getView());
            }, 200);
        });
    },
    _collectTotalMoney: function (view) {
        /// <summary>
        /// 汇总总金额
        /// </summary>
        /// <param name="view"></param>

        var isFormClosed = !view.getControl().getForm().monitor;
        if (isFormClosed) return;

        var po = view.updateEntity();

        var list = po.PurchaseOrderItemList();
        var sum = Rafy.sum(list, function (i) {
            var RawPrice = i.getRawPrice();
            var Amount = i.getAmount();
            return RawPrice * Amount;
        });
        po.setTotalMoney(sum);

        view.updateControl();
    }
});