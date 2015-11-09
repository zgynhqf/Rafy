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

Ext.define('Rafy.control.ComboList', {
    extend: 'Ext.form.field.ComboBox',
    //extend: 'Rafy.control.ComboBoxTest',
    alias: 'widget.combolist',

    //override default config
    editable: false,
    //doAutoSelect 方法中会调用 picker 的一些方法：getNode、hightlightItem 等，
    //而这些方法并没有在 grid、treeGrid 中实现。
    autoSelect: false,
    pageSize: 10,
    matchFieldWidth: false,

    //config
    model: '',
    dataSourceProperty: '',

    //private fields
    _view: null,
    _refProperty: '',
    _refId: '',
    _isTree: false,

    //refProperty 的初始化放在 initComponent 中时，如果在 List 中下拉编辑时，是获取不到 Name 的。
    //    initComponent: function () {
    //        var me = this;
    //        me.callParent();
    //
    //        me._refProperty = me.getName().replace("_Display", "");
    //    },

    createPicker: function () {
        /// <summary>
        /// override
        /// 重写父类构造控件方法，生成列表控件
        /// </summary>
        /// <returns></returns>
        var meta = null;
        Rafy.AutoUI.getMeta({
            async: false, //同步
            model: this.model, isLookup: true, isReadonly: true, ignoreCommands: true,
            callback: function (res) { meta = res; }
        });

        this._isTree = Rafy.getModel(this.model).isTree;

        Ext.applyIf(meta.gridConfig, {
            floating: true,
            hidden: true,
            minWidth: 250,
            ownerCt: this.up('[floating]')
        });
        if (this._isTree) {
            meta.gridConfig.useArrows = true;
        }
        Ext.apply(meta.storeConfig, { pageSize: this.pageSize });

        var v = Rafy.AutoUI.createListView(meta);
        this._view = v;

        //重新设置数据源，这时，picker 还没有值，所以不会造成重复绑定。
        this.bindStore(v.getData());

        var grid = v.getControl();

        this.mon(grid.getView(), {
            itemclick: this.onItemClick,
            refresh: this.onListRefresh,
            scope: this
        });
        this.mon(grid.getSelectionModel(), {
            'beforeselect': this.onBeforeSelect,
            'beforedeselect': this.onBeforeDeselect,
            'selectionchange': this.onListSelectionChange,
            scope: this
        });

        return grid;
    },

    //override
    setValue: function (value, doSelect) {
        /// <summary>
        /// 在设置的同时，把选择项的 Id 也获取到，并存储下来。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="doSelect"></param>
        this.callParent(arguments);

        var ls = this.lastSelection;
        if (ls.length > 0) {
            this._refId = ls[0].getId();
        }
        else {
            this._refId = '';
        }
    },

    //------------------------------------- 重写以下方法处理 Tree 的兼容。（TreeStore 没有的一些方法的问题。） -------------------------------------
    //override
    findRecord: function (field, value) {
        /// <summary>
        /// 在树型状态下，暂时不支持查找某一行数据。
        /// 这会导致展开时无法直接根据数据选择相应的行。
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        if (this._isTree) { return false; }

        return this.callParent(arguments);
    },
    //override
    loadPage: function (pageNum) {
        var me = this;

        //如果是树型表格控件，则不支持分页
        if (me._isTree) {
            //在加载第一页时，表示需要进行数据的初始化，树型 Store 开始加载数据。
            if (pageNum == 1) {
                me._view.loadData();
            }

            return false;
        }

        return me.callParent(arguments);
    },
    //override
    expand: function () {
        this.callParent(arguments);

        //如果使用的是自定义数据源，由于已经强制其使用本地模式，
        //但是又希望每次展开都刷新数据，所以这里在 expand 方法中再次重新设置数据源。
        this._trySetCustomDataSource();

        //树型控件的滚动条有问题，需要手动设置它的高度。
        if (this._isTree) {
            var tree = this._view.getControl();
            if (!tree.height) { tree.setHeight(250); }
        }
    },

    _trySetCustomDataSource: function () {
        /// <summary>
        /// 尝试使用自定义数据源。
        /// </summary>

        //如果使用自定义数据源，则调用服务获取自定义数据。
        var dsp = this.dataSourceProperty;
        if (!dsp) { return; }

        var form = this.up('form').getForm();;
        var e = form.getRecord();
        if (!e) return;

        var dataSource = null;

        //复制一个临时对象，并把当前输入的值都设置到该实体上，
        //然后向服务端传输数据，并调用它的属性获取列表数据源。
        e = e.copy();
        form.updateRecord(e);

        var dto = Rafy.data.Serializer.serialize(e, false);

        Rafy.invokeService({
            async: false,//同步调用
            svc: 'Rafy.Web.GetCustomDataSourceService',
            svcInput: {
                Entity: dto,
                DataSourceProperty: dsp
            },
            callback: function (res) { dataSource = res.DataSource; }
        });

        //下面的 loadRawData 方法使用本地数据源，
        //它只会发生 onLoad 事件，不会发生 onBeforeLoad 事件。所以这里需要手动调用一下此事件处理。
        this.onBeforeLoad();

        //由于修改 invokeService 添加自动反序列化数据为实体集合的功能后，下面的代码不可用。
        //这里先直接把数据使用 setData 设置到 view 中。具体是否发现问题，还有待测试。
        this._view.setData(dataSource);
        ////只有放在 bindStore 后面，this.onLoad 事件处理函数才会发生。
        ////var data = this._view.getData();
        ////data.loadRawData(dataSource);
    },

    //------------------------------------- 重写以下两个方法，在获取数据时把 引用实体的 id 也返回。 -------------------------------------
    //override
    getModelData: function () {
        var data = this.callParent(arguments);
        this._addRefId(data);
        return data;
    },
    //override
    getSubmitData: function () {
        var data = this.callParent(arguments);
        this._addRefId(data);
        return data;
    },
    _addRefId: function (data) {
        if (this._refId) {
            if (!this._refProperty) this._refProperty = this.getName().replace("_Display", "");

            data[this._refProperty] = this._refId;
        }
    }
});