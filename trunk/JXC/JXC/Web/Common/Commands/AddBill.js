Ext.define('Jxc.AddBill', {
    extend: 'Oea.cmd.Command',
    config: { meta: { text: "添加", group: "edit" } },

    _template: '',
    _svc: '',

    constructor: function () {
        this.callParent(arguments);

        this._template = new Jxc.BillTemplate();

        this.addEvents('itemCreated');
    },

    execute: function (listView, source) {
        var ui = this._template.createUI(listView.getModel());
        var newItem = Ext.create(listView.getModel());
        this.onItemCreated(newItem);

        var view = ui.getView();
        view.setCurrent(newItem);

        var me = this;
        var win = Oea.Window.show({
            title: '添加', items: ui.getControl(), animateTarget: source,
            callback: function (btn) {
                if (btn == '确定') {
                    view.updateEntity();
                    var item = view.serializeData({
                        withUnchanged: true,
                        withChildren: true
                    });

                    var svcRes = null;
                    Oea.invokeSvc({
                        async: false, svc: me.getSvc(), svcParams: {
                            Item: item
                        },
                        callback: function (res) { svcRes = res; }
                    });

                    Ext.Msg.alert('提示', svcRes.msg);
                    return svcRes.success;
                }
            }
        });

        //win.maximize();
    },

    //配置：
    getSvc: function () {
        if (this._svc == '') Oea.notSupport("本类必须设置 Svc 属性！");
        return this._svc;
    },
    setSvc: function (value) { this._svc = value; },
    getTemplate: function () { return this._template; },
    setTemplate: function (value) { this._template = value; },

    //protected virtual
    onItemCreated: function (item) {
        this.fireEvent('itemCreated', { item: newItem });
    }
});