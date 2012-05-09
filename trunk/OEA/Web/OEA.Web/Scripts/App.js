Ext.define('Oea.App', {
    singleton: true,
    extend: 'Ext.util.Observable',
    _modules: null,
    constructor: function () {
        this._modules = new Oea.ModuleCollection();

        this.addEvents(
            'loginSuccessed',
            'loginFailed',
            'startupCompleted',
            'mainWindowLoaded',
            'moduleCreated',
            'exit'
            );
    },

    getModules: function () { return this._modules; },

    showModule: function (opt) {
        /// <summary>
        /// 显示某个模块
        /// </summary>
        /// <param name="opt">
        /// module：系统中唯一的模块名称。
        /// container: null，类型为 Ext.Container。生成的模块应该放在这个容器中。如果未指定，则直接显示在 Body 中。
        /// </param>
        var me = this;
        opt = Ext.apply({ isAggt: true }, opt);

        var module = this._modules.getByLabel(opt.module);

        //如果已经定义了客户端的模版类型 WebTemplateType，则直接构造客户端模板，生成界面。
        if (module.templateType) {
            var uiTemplate = Ext.create(module.templateType);
            var ui = uiTemplate.createUI(module.model);

            this._showInContainer(opt.container, ui.getControl());

            this.fireEvent('moduleCreated', { moduleUI: ui, moduleName: module.keyLabel });
        }
        else {
            //根据模块到服务器查找对象的界面元数据并生成。
            var me = this;
            var callBack = this._createCallback(opt);
            Oea.AutoUI.getMeta({
                module: opt.module,
                callback: function (meta) {
                    var moduleUI = callBack(meta);//moduleUI 是一个 ControlResult

                    me.fireEvent('moduleCreated', { moduleUI: moduleUI, moduleName: opt.module });
                }
            });
        }
    },

    showModel: function (opt) {
        /// <summary>
        /// 显示某个模块
        /// </summary>
        /// <param name="opt">
        /// model: 当未指定 module 时，
        /// container: null(Ext.Container)。生成的模块应该放在这个容器中。如果未指定，则直接显示在 Body 中。
        /// viewName: '', 如果使用某个实体类的扩展视图，则这个参数指定扩展视图的名称。
        /// isAggt: true。是否显示聚合模型。
        /// </param>
        opt = Ext.apply({
            isAggt: true,
            viewName: ''
        }, opt);

        var callBack = this._createCallback(opt);
        Oea.AutoUI.getMeta({
            model: opt.model,
            viewName: opt.viewName,
            isAggt: opt.isAggt,
            callback: callBack
        });
    },
    _createCallback: function (opt) {
        /// <summary>
        /// 创建一个处理异常获取到的界面元数据的回调函数。
        /// 此函数主要用于界面生成
        /// </summary>
        /// <param name="opt"></param>
        var me = this;
        if (!opt.isAggt) {
            return function (meta) {
                var listView = Oea.AutoUI.createListView(meta);

                listView.loadData();

                me._showInContainer(opt.container, listView.getControl());

                return listView;
            };
        }
        else {
            return function (meta) {
                var controlResult = Oea.AutoUI.generateAggtControl(meta);

                //如果没有导航/查询面板，则发动一次查询。
                if (meta.surrounders.length == 0) {
                    var listView = controlResult.getView();
                    if (listView.isListView) { listView.loadData(); }
                }

                me._showInContainer(opt.container, controlResult.getControl());

                return controlResult;
            };
        }
    },

    _showInContainer: function (container, control) {
        //如果未指定容器，则直接显示在 Body 中。
        if (!container) {
            container = Ext.widget('viewport', {
                border: 0,
                layout: 'fit',
                autoScroll: true
            });
        }

        //生成的聚合界面可能比较大，所以需要显示“加载中”
        container.setLoading(true);
        setTimeout(function () {
            container.add(control);
            container.setLoading(false);
        }, 10);
    },

    startup: function () {
        /// <summary>
        /// 启动整个客户端应用程序
        /// </summary>

        this._initPlugins();

        this.fireEvent('loginSuccessed');
        //this.fireEvent('loginFailed');
        this.fireEvent('startupCompleted');
        this.fireEvent('mainWindowLoaded');
    },
    _initPlugins: function () {
        var me = this;
        //这里的 plugin 就不需要排序了，因为 js 输出的顺序是在服务端排序过的。
        var plugins = Oea._getPlugins();
        for (var i = 0; i < plugins.length; i++) {
            //构造实例，并替换到数组中
            var pluginType = plugins[i];
            var p = Ext.create(pluginType);
            plugins[i] = p;

            if (p.init) { p.init(me); }
        }
    }
});