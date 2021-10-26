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

Rafy.defineCommand('Rafy.rbac.cmd.ChoosePermissions', {
    meta: { text: "设置权限" },

    _moduleView: null,//模块视图
    _operationView: null,//操作视图
    _curPositionId: null,//当前正在授权的岗位的 Id

    canExecute: function (view) {
        return view.getCurrent() != null;
    },
    execute: function (listView, source) {
        var me = this;
        me._curPositionId = listView.getCurrent().getId();

        //以下代码连续生成两个视图，拼接后显示。
        Rafy.AutoUI.getMeta({
            model: 'Rafy.RBAC.Old.ModuleAC',
            ignoreCommands: true,
            callback: function (block) {
                //模块视图
                Rafy.each(block.gridConfig.columns, function (c) { c.width = 200; });
                me._moduleView = Rafy.AutoUI.createListView(block);
                me._moduleView.loadData();

                Rafy.AutoUI.getMeta({
                    model: 'Rafy.RBAC.Old.OperationAC',
                    ignoreCommands: true,
                    callback: function (oacBlock) {
                        //勾选的操作列表视图
                        var gridCfg = oacBlock.gridConfig;
                        Rafy.each(gridCfg.columns, function (c) { c.width = 400; });
                        gridCfg.selType = 'checkboxmodel';
                        gridCfg.selModel = { checkOnly: true };
                        me._operationView = Rafy.AutoUI.createListView(oacBlock);

                        var ui = me._createUI();
                        me._popupWin(ui, source);

                        me._moduleView.on('currentChanged', function (e) { me._onModuleSelected(); });
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
        var win = Rafy.Window.show({
            title: '设置权限'.t(),
            width: 800, height: 500,
            animateTarget: source, items: ui,
            buttons: ['保存模块'.t(), '关闭'.t()],
            callback: function (btn) {
                if (btn == '保存模块'.t()) {
                    me._save();
                    return false;
                }
            }
        });
    },
    _onModuleSelected: function () {
        /// <summary>
        /// 模块列表选择后，右边的操作列表显示该模块中所有可用的操作。
        /// 数据则直接从聚合对象父子关系中获得。
        /// </summary>
        var me = this;
        var current = me._moduleView.getCurrent();
        var list = current.OperationACList();
        me._operationView.setData(list);
        me._reloadOperations();
    },
    _reloadOperations: function () {
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

        var c = Ext.create('Rafy.RBAC.Old.OperationAC_GetDenyListCriteria', {
            ModuleACId: me._moduleView.getCurrent().getId(),
            OrgPositionId: me._curPositionId
        });

        Rafy.data.EntityRepository.query({
            model: me._operationView.getMeta().model,
            criteria: c,
            callback: function (records) {
                var sm = me._operationView.getControl().getSelectionModel();
                sm.selectAll();

                var list = me._operationView.getData();
                for (var i = 0; i < list.getCount() ; i++) {
                    var item = list.getAt(i);
                    var exsit = Rafy.findFirst(records, function (r) {
                        return r.getId() == item.getId();
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
        Rafy.each(all, function (item) {
            var e = Rafy.findFirst(selection, function (i2) { return i2 == item; });
            if (e == null) {
                if (ids != '') { ids += ','; }
                ids += item.get("Id");
            }
        });

        //调用服务保存结果
        var me = this;
        Rafy.invokeService({
            svc: 'Rafy.RBAC.Old.DenyOperationService',
            svcInput: {
                DenyOperationIds: ids,
                ModuleId: this._moduleView.getCurrent().get("Id"),
                OrgPositionId: this._curPositionId
            },
            callback: function (res) {
                Ext.Msg.alert('提示', res.Message);
                //if (res.Success) {
                //    me._reloadOperations();
                //}
                //else {
                //    Ext.Msg.alert('提示', res.Message);
                //}
            }
        });
    }
});