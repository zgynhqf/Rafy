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

Rafy.defineCommand('Jxc.ShowBill', {
    meta: { text: "查看", group: "view" },

    _template: '',

    constructor: function () {
        this.callParent(arguments);

        this._template = new Jxc.ReadonlyBillTemplate();
    },

    canExecute: function (listView) {
        return listView.getCurrent() != null;
    },

    execute: function (listView, source) {
        var ui = this._template.createUI(listView.getModel());
        var view = ui.getView();
        var item = listView.getCurrent();
        view.setCurrent(item);

        var win = Rafy.Window.show({
            title: '查看' + listView.getMeta().label,
            items: ui.getControl(), animateTarget: source
        });
    },
    getTemplate: function () { return this._template; },
    setTemplate: function (value) { this._template = value; }
});