/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：201201
 * 说明：客户端应用程序定义
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 201201
 * 
*******************************************************/

Ext.define('Rafy.App', {
    singleton: true,
    extend: 'Ext.util.Observable',

    constructor: function () {
        this.callParent(arguments);

        this._modules = new Rafy.ModuleCollection();

        this.addEvents(
            'loginSuccessed',
            'loginFailed',
            'startupCompleted',
            'moduleCreated',
            'exit'
            );
    },

    //-------------------------------------  模块操作 -------------------------------------
    _modules: null,//模块列表
    _workspace: null,//工作区
    _currentModule: null,//最后一次显示的模块对象。

    getModules: function () {
        /// <summary>
        /// 获取所有模块的列表。
        /// </summary>
        /// <returns type="Rafy.ModuleCollection"></returns>
        return this._modules;
    },
    showModule: function (opt) {
        /// <summary>
        /// 创建并显示某个模块
        /// </summary>
        /// <param name="opt">
        /// 字符串或配置对象。
        /// 
        /// module：系统中唯一的模块或名称。
        /// //container: null，类型为 Ext.Container。生成的模块应该放在这个容器中。如果未指定，则直接显示在 Body 中。
        /// </param>
        /// <returns type="">返回模块对象。</returns>

        //参数处理。找到对应的模块对象。
        if (!opt) { Rafy.emptyArgument('opt') }
        if (Ext.isString(opt) || opt.keyLabel) {
            opt = { module: opt };
        }
        var module = opt.module;
        if (Ext.isString(module)) {
            module = this._modules.getByKey(module);
        }
        if (!module) {
            Rafy.error("没有找到对应的模块：" + opt.module);
        }

        //如果该模块已经存在，则设置该模块为当前模块，并返回。
        var ws = this.getWorkspace();
        if (ws) {
            var modules = ws.getModules();
            var exists = Rafy.findFirst(modules, function (m) { return m == module; });
            if (exists) {
                ws.setCurrentModule(module);
                return;
            }
        }

        //为模块生成模块控件
        var ui = this._createModuleControl(module);

        //如果指定了 Workspace，则把生成的模块控件加入到 Workspace 中；否则就显示在指定的 container 中。
        if (ws) {
            ws._addModule(module.keyLabel, ui.getControl());
        }
        else {
            this._showInContainer(opt.container, ui.getControl());
        }

        //生成完毕，发生事件。
        this._onModuleCreated(ui, module.keyLabel);
        this._currentModule = module;

        return module;
    },
    getCurrentModule: function () {
        /// <summary>
        /// 返回当前显示的模块对象。
        /// </summary>
        /// <returns type=""></returns>
        return this._currentModule;
    },
    getWorkspace: function () {
        /// <summary>
        /// 返回工作区对象。如果返回 null，表示没有工作区对象，应用程序不能承载多个模块。
        /// 
        /// 工作区对象有以下方法：
        /// module[] getModules() //返回当前已经打开的所有模块的集合。
        /// setCurrentModule(module) //可设置当前模块。
        /// removeModule(module) //移除某个已经打开的模块。
        /// </summary>
        /// <returns type="Object"></returns>
        return this._workspace;
    },
    setWorkspaceProvider: function (provider) {
        /// <summary>
        /// 设置模块的提供器。
        /// 本方法只能调用一次。
        /// </summary>
        /// <param name="provider">
        /// provider 需要实现以下几个方法，所有的参数都是字符串。
        /// module[] getModules()
        /// setCurrentModule(module)
        /// addModule(module, moduleControl)
        /// removeModule(module)
        /// </param>
        var v = provider;
        if (!v.getModules || !v.setCurrentModule || !v.addModule || !v.removeModule) {
            Rafy.notSupport("provider 必须支持以下方法：getModules、setCurrentModule、addModule、removeModule");
        }

        var app = this;

        app._workspace = {
            _provider: v,
            getModules: function () {
                var res = [];
                //获取字符串数组
                var moduleNames = this._provider.getModules();

                //转换为模块数组。
                var modules = Rafy.App.getModules();
                Rafy.each(moduleNames, function (name) {
                    var module = modules.getByKey(name);
                    if (module) {
                        res.push(module);
                    }
                });

                return res;
            },
            getCurrentModule: function () {
                /// <summary>
                /// 返回当前工作区中打开的模块。
                /// </summary>
                /// <returns type="Object"></returns>
                return app._currentModule;
            },
            setCurrentModule: function (value) {
                /// <summary>
                /// 设置工作区的当前模块。
                /// </summary>
                /// <param name="value">字符串或模块对象。</param>
                /// <returns type=""></returns>
                value = this._toModuleItem(value);

                app._currentModule = value;

                value = this._toModuleLabel(value);

                return this._provider.setCurrentModule(value.keyLabel);
            },
            removeModule: function (module) {
                /// <summary>
                /// 移除指定的模块
                /// </summary>
                /// <param name="module"></param>
                /// <returns type=""></returns>
                module = this._toModuleLabel(module);

                return this._provider.removeModule(module);
            },
            _addModule: function (module, control) {
                return this._provider.addModule(module, control);
            },
            _toModuleItem: function (module) {
                if (Ext.isString(module)) { module = app.getModules().getByKey(module); }
                return module;
            },
            _toModuleLabel: function (module) {
                if (!Ext.isString(module)) { module = module.keyLabel; }
                return module;
            }
        };
    },
    _createModuleControl: function (module) {
        /// <summary>
        /// 生成某个模块的控件
        /// </summary>
        /// <param name="module">客户端模块对象。</param>
        /// <returns type="">返回模块对应的控件对象。</returns>

        //根据模块到服务器查找对象的界面元数据并生成。
        var meta = null;
        Rafy.AutoUI.getMeta({
            async: false,
            module: module.keyLabel,
            callback: function (res) { meta = res; }
        });

        var runtime = module.clientRuntime;

        var ui = this._createAggtControl(meta, !runtime);//ControlResult;

        //如果已经定义了客户端的模版类型 clientRuntime，则为运行时选入生成的界面。
        if (runtime) {
            var clientRuntime = Ext.create(runtime);
            clientRuntime._setMeta(meta);
            clientRuntime._notifyUIGenerated(ui);
        }

        return ui;
    },
    _onModuleCreated: function (moduleUI, moduleName) {
        this.fireEvent('moduleCreated', { moduleUI: moduleUI, moduleName: moduleName });
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

        var me = this;
        Rafy.AutoUI.getMeta({
            model: opt.model,
            viewName: opt.viewName,
            isAggt: opt.isAggt,
            callback: function (meta) {
                if (!opt.isAggt) {
                    var listView = Rafy.AutoUI.createListView(meta);
                    //最后一次创建的根视图，用于调试。
                    window.$view = listView;

                    listView.loadData();

                    me._showInContainer(opt.container, listView.getControl());
                }
                else {
                    var ui = me._createAggtControl(meta, true);

                    me._showInContainer(opt.container, ui.getControl());
                }
            }
        });
    },

    _showInContainer: function (container, control) {
        //如果未指定容器，则直接显示在 Body 中。
        if (!container) {
            container = Ext.widget('viewport', {
                border: 0,
                layout: 'fit',//如果不添加 fit，则模块直接在页面中显示时，无法显示。例如：http://localhost:8000/default/module/?module=部门权限管理
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
    _createAggtControl: function (meta, autoLoad) {
        /// <summary>
        /// 生成组合界面控件
        /// </summary>
        /// <param name="meta">服务端返回的组合界面元数据</param>
        /// <returns type=""></returns>
        var ui = Rafy.AutoUI.generateAggtControl(meta);

        var listView = ui.getView();
        //最后一次创建的根视图，用于调试。
        window.$view = listView;

        //如果没有导航/查询面板，则发动一次查询。
        if (autoLoad && !meta.surrounders) {
            if (listView.isListView) { listView.loadData(); }
        }

        return ui;
    },

    //-------------------------------------  启动 -------------------------------------
    startup: function () {
        /// <summary>
        /// 启动整个客户端应用程序
        /// </summary>

        this._initPlugins();

        this.fireEvent('loginSuccessed');
        //this.fireEvent('loginFailed');
        this.fireEvent('startupCompleted');
    },
    _initPlugins: function () {
        var me = this;
        //这里的 plugin 就不需要排序了，因为 js 输出的顺序是在服务端排序过的。
        var plugins = Rafy._getPlugins();
        for (var i = 0; i < plugins.length; i++) {
            //构造实例，并替换到数组中
            var pluginType = plugins[i];
            var p = Ext.create(pluginType);
            plugins[i] = p;

            if (p.init) { p.init(me); }
        }
    }
});