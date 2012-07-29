Ext.define('Jxc.ShowBill', {
    extend: 'Oea.cmd.Command',
    config: { meta: { text: "查看", group: "view" } },

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

        var win = Oea.Window.show({
            title: '查看' + listView.getMeta().label,
            items: ui.getControl(), animateTarget: source
        });
    },
    getTemplate: function () { return this._template; },
    setTemplate: function (value) { this._template = value; }
});