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

                        me._moduleView.on('currentChanged', function (e) { me._onModuleSelectedChanged(); });
                    }
                });
            }
        });
    },
    _createUI: function () {
        var me = this;
        //左右划分两块
        var ui = Ext.widget('container', {
            border: 0,
            layout: 'border',
            items: [{
                region: 'west', width: 200, border: 0, split: true, layout: 'fit',
                items: me._moduleView.getControl()
            }, {
                region: 'center', border: 0, layout: 'fit',
                items: me._operationView.getControl()
            }]
        });
        return ui;
    },
    _popupWin: function (ui, source) {
        var me = this;
        var win = Oea.Window.show({
            title: '设置权限',
            width: 800, height: 500,
            animateTarget: source, items: ui,
            buttons: '保存模块,关闭',
            callback: function (btn) {
                if (btn == '保存模块') {
                    me._save();
                    return false;
                }
            }
        });
    },
    _onModuleSelectedChanged: function () {
        /// <summary>
        /// 模块列表选择后，右边的操作列表显示该模块中所有可用的操作。
        /// 数据则直接从聚合对象父子关系中获得。
        /// </summary>
        var me = this;
        var current = me._moduleView.getCurrent();
        var list = current.OperationACList();
        me._operationView.setData(list);
        me._realoadOperations();
    },
    _realoadOperations: function () {
        /// <summary>
        /// 刷新右边的操作列表数据
        /// </summary>
        var me = this;
        me._operationView.loadData({
            callback: function () {
                me._initSelection();
            }
        });
    },
    _initSelection: function () {
        /// <summary>
        /// 根据数据库中的数据，初始化所有的 CheckBox 选择框。（没有禁用的选择上，禁用的反选。）
        /// </summary>
        var me = this;

        var c = Ext.create('OEA.RBAC.OperationAC_GetDenyListCriteria', {
            ModuleACId: me._moduleView.getCurrent().get("Id"),
            OrgPositionId: me._curPositionId
        });

        Oea.data.EntityRepository.query({
            model: me._operationView.getMeta().model,
            criteria: c,
            callback: function (records) {
                var sm = me._operationView.getControl().getSelectionModel();
                sm.selectAll();

                var list = me._operationView.getData();
                for (var i = 0; i < list.getCount() ; i++) {
                    var item = list.getAt(i);
                    var exsit = Oea.findFirst(records, function (r) {
                        return r.get("Id") == item.get("Id");
                    });
                    if (exsit) {
                        sm.deselect(item);
                    }
                }
            }
        });
    },
    _save: function () {
        /// <summary>
        /// 保存选择的操作列表。
        /// </summary>

        //对于没有选中的数据，序列化它们的 Id 列表。
        var ids = '';
        var selection = this._operationView.getSelection();
        var all = this._operationView.getData();
        Oea.each(all, function (item) {
            var e = Oea.findFirst(selection, function (i2) { return i2 == item; });
            if (e == null) {
                if (ids != '') { ids += ','; }
                ids += item.get("Id");
            }
        });

        //调用服务保存结果
        var me = this;
        Oea.invokeSvc({
            svc: 'OEA.RBAC.DenyOperationService',
            svcParams: {
                DenyOperationIds: ids,
                ModuleId: this._moduleView.getCurrent().get("Id"),
                OrgPositionId: this._curPositionId
            },
            callback: function (res) {
                Ext.Msg.alert('提示', res.msg);
                //if (res.success) {
                //    me._realoadOperations();
                //}
                //else {
                //    Ext.Msg.alert('提示', res.msg);
                //}
            }
        });
    }
});