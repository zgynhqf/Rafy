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

Rafy.defineCommand('Jxc.AddBill', {
    meta: { text: "添加", group: "edit" },

    _template: '',
    _svc: '',

    constructor: function () {
        this.callParent(arguments);

        this._template = new Jxc.BillTemplate();

        this.addEvents('itemCreated');
    },

    execute: function (listView, source) {
        var model = listView.getModel();
        var ui = this._template.createUI(model);
        var newItem = Ext.create(model);
        this.onItemCreated(newItem);

        var view = ui.getView();
        view.setCurrent(newItem);

        var me = this;
        var win = Rafy.Window.show({
            title: '添加', items: ui.getControl(), animateTarget: source,
            height: document.body.clientHeight * 0.8,
            callback: function (btn) {
                if (btn == '确定') {
                    view.updateEntity();
                    var item = view.serializeData(true);

                    var svcRes = null;
                    Rafy.invokeService({
                        async: false, svc: me.getSvc(), svcInput: {
                            Item: item
                        },
                        callback: function (res) { svcRes = res; }
                    });

                    Ext.Msg.alert('提示', svcRes.Message);
                    return svcRes.Success;
                }
            }
        });

        //win.maximize();
    },

    //配置：
    getSvc: function () {
        if (this._svc == '') Rafy.notSupport("本类必须设置 Svc 属性！");
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