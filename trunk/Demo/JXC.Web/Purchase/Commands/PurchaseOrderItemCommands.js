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

Rafy.defineCommand('Jxc.AddPurchaseOrderItem', {
    extend: 'Jxc.SelectProductCommand',
    meta: { text: "选择商品", group: "edit" },

    //proected override
    onItemSelected: function (product, newItem) {
        newItem.set('RawPrice', product.get('CaiGouDanjia'));
    }
});