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

Ext.define('Oea.autoUI.ViewFactory', {
    extend: 'Ext.util.Observable',
    constructor: function () {
        this.addEvents('commandCreated');
    },

    //ListView
    createListView: function (block) {
        var view = new Oea.view.ListView(block);

        var store = this._createStore(block);

        var grid = null;
        if (block.isTree) {
            grid = this._createTreeListControl(block.gridConfig, store, view);
        }
        else {
            if (block.groupBy) { block.gridConfig.groupBy = block.groupBy; }

            grid = this._createListControl(block.gridConfig, store, view);
        }

        view._setControl(grid);

        return view;
    },
    _createStore: function (block) {
        var storeCfg = block.storeConfig;
        if (block.groupBy) { storeCfg.groupField = block.groupBy; }

        var store = Oea.data.EntityRepository.createStore({
            model: block.model,
            isTree: block.isTree,
            storeConfig: storeCfg
        });

        return store;
    },
    _createTreeListControl: function (gridConfig, store, view) {
        var config = {
            rootVisible: false,
            minHeight: 100,
            //            useArrows: true,
            border: 0,
            store: store
        };

        var c = Oea.findFirst(gridConfig.columns, function (c) { return c.editor != null; });
        var isEditing = c != null;
        if (isEditing) {
            var editingPlugin = Ext.create('Ext.grid.plugin.RowEditing', {
                pluginId: 'editingPlugin',
                clicksToEdit: 2,
                saveBtnText: '确定',
                cancelBtnText: '取消'
            });
            Ext.apply(config, {
                viewConfig: {
                    plugins: { ptype: 'treeviewdragdrop' }
                },
                plugins: [editingPlugin]
            });
        }

        Ext.merge(config, gridConfig);

        this._createCommands(config, view);

        var treeGrid = Ext.create('Ext.tree.Panel', config);
        treeGrid.getSelectionModel().setSelectionMode('multi');

        return treeGrid;
    },
    _createListControl: function (gridConfig, store, view) {
        var c = Oea.findFirst(gridConfig.columns, function (c) { return c.editor != null; });
        var isEditing = c != null;

        var config = {
            store: store,
            border: 0,
            minHeight: 200
        };

        //超过10000就不用分页了。
        if (store.pageSize < 10000) {
            view._pagingBar = Ext.create('Ext.toolbar.Paging', {
                store: store,
                displayInfo: true,
                dock: 'bottom'
            });

            config.dockedItems = [view._pagingBar];
        }

        if (isEditing) {
            var editingPlugin = Ext.create('Ext.grid.plugin.RowEditing', {
                pluginId: 'editingPlugin',
                clicksToEdit: 2,
                saveBtnText: '确定',
                cancelBtnText: '取消'
            });
            config.plugins = [editingPlugin];
        }

        Ext.merge(config, gridConfig);

        if (config.groupBy) {
            var groupingFeature = Ext.create('Ext.grid.feature.Grouping', {
                //                enableGroupingMenu: false,
                //                groupByText: '用该字段分组',
                //                showGroupsText: '显示分组',
                hideGroupedHeader: true,
                groupHeaderTpl: '{name}：({rows.length})'
            });
            config.features = [groupingFeature];
        }

        this._createCommands(config, view);

        var grid = Ext.create('Ext.grid.Panel', config);
        grid.getSelectionModel().setSelectionMode('multi');

        return grid;
    },

    createConditionView: function (block) {
        var view = new Oea.view.ConditionView(block);

        var formCfg = Ext.merge({}, block.formConfig);
        if (!formCfg.tbar) formCfg.tbar = [];
        formCfg.tbar.push({
            "text": "查询",
            "command": "Oea.cmd.ExecuteQuery"
        });

        var form = this._createEditForm(formCfg, view);

        view._setControl(form);
        view.attachNewEntity();

        return view;
    },

    createNavigationView: function (block) {
        //暂时只支持 ConditionView
        return this.createConditionView(block);
    },

    //DetailView
    createDetailView: function (block, entity) {
        var view = new Oea.view.DetailView(block);

        var formCfg = Ext.merge({ bodyPadding: 10 }, block.formConfig);

        var form = this._createEditForm(formCfg, view);

        view._setControl(form);

        if (entity) { view.setCurrent(entity); }

        return view;
    },
    _createEditForm: function (formCfg, view) {
        this._createCommands(formCfg, view);

        var formPanel = Ext.create('Ext.form.Panel', formCfg);

        return formPanel;
    },

    _createCommands: function (panelCfg, view) {
        if (panelCfg.tbar) {
            var me = this;
            Oea.each(panelCfg.tbar, function (ti) { me._setHandler(ti, view); });
        }
    },
    _setHandler: function (tbarItemCfg, view) {
        /// <summary>
        /// 为某个工具栏项生成客户端命令，并添加事件处理函数
        /// </summary>
        /// <param name="tbarItemCfg"></param>
        /// <param name="view"></param>

        tbarItemCfg.id = view._getCmdControlId(tbarItemCfg.command);

        var cmd = Ext.create(tbarItemCfg.command, { meta: tbarItemCfg });

        //在创建 cmd 的过程中，可能会修改 meta 中的数据以生成界面
        cmd._modifyMeta(tbarItemCfg);

        tbarItemCfg.handler = function () { cmd.tryExecute(view, this); };

        view._addCmd(tbarItemCfg.command, cmd);

        this.fireEvent('commandCreated', { command: cmd });
    }
});