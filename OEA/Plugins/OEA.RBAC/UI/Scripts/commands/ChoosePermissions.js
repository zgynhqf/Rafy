Ext.define('Oea.rbac.cmd.ChoosePermissions', {
    extend: 'Oea.cmd.Command',
    config: {
        meta: { text: "设置权限" }
    },
    _moduleView: null,
    _operationView: null,
    _curPositionId: null,
    canExecute: function (view) {
        return view.getCurrent() != null;
    },
    execute: function (listView, source) {
        var me = this;
        me._curPositionId = listView.getCurrent().get("Id");
        Oea.AutoUI.getMeta({
            model: 'OEA.RBAC.ModuleAC',
            ignoreCommands: true,
            callback: function (block) {
                //模块视图
                me._moduleView = Oea.AutoUI.createListView(block);
                me._moduleView.loadData();

                Oea.AutoUI.getMeta({
                    model: 'OEA.RBAC.OperationAC',
                    ignoreCommands: true,
                    callback: function (oacBlock) {
                        //勾选的操作列表视图
                        var gridCfg = oacBlock.gridConfig;
                        gridCfg.selType = 'checkboxmodel';
                        gridCfg.selModel = { checkOnly: true };
                        me._operationView = Oea.AutoUI.createListView(oacBlock);

                        var ui = me._createUI();
                        me._popupWin(ui, source);

                        me._moduleView.on('currentChanged', function () { me._onModuleSelectedChanged(); });
                    }
                });
            }
        });
    },
    _createUI: function () {
        var me = this;
        var ui = Ext.widget('container', {
            border: 0,
            layout: 'border',
            items: [{
                region: 'west',
                width: 200,
                border: 0,
                split: true,
                layout: 'fit',
                items: me._moduleView.getControl()
            }, {
                region: 'center',
                border: 0,
                autoScroll: true,
                layout: 'fit',
                items: me._operationView.getControl()
            }]
        });
        return ui;
    },
    _popupWin: function (ui, source) {
        var me = this;
        var win = Oea.Window.show({
            title: '设置权限',
            width: 600, height: 400,
            animateTarget: source, items: ui,
            buttons: '确定,关闭',
            callback: function (btn) {
                if (btn == '确定') {
                    me._confirm();
                }
            }
        });
    },
    _onModuleSelectedChanged: function () {
        var me = this;
        var current = me._moduleView.getCurrent();
        var list = current.OperationACList();
        me._operationView.setData(list);
        me._operationView.loadData({
            callback: function () {
                me._initSelection();
            }
        });
    },
    _initSelection: function () {
        var me = this;
        var sm = me._operationView.getControl().getSelectionModel();
        sm.selectAll();
    },
    _confirm: function () {
        Ext.Msg.alert("确定。");
    }
});