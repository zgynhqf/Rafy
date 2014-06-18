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

Ext.define('Rafy', {
    singleton: true,

    //internal const
    _IdPropertyName: 'Id',
    _plugins: [],
    //internal
    _isDebugging: true,

    //--------------------------------------  异常 -------------------------------------
    error: function (name) {
        name = name || "";
        alert(name);
        throw new Error(name);
    },
    notSupport: function (name) {
        name = name || "";
        this.error("这个方法暂时不被支持：" + name);
    },
    notImplement: function (name) {
        name = name || "";
        this.error("这个方法没有被实现：" + name);
    },
    markAbstract: function (name) {
        this.notImplement(name);
    },
    emptyArgument: function (name) {
        var funcName = arguments.callee.caller.$name;
        this.error(funcName + " 方法中的参数：" + name + " 不能为空。");
    },

    //--------------------------------------  服务 -------------------------------------
    invokeService: function (op) {
        /// <summary>
        /// 调用指定的服务
        /// </summary>
        /// <param name="op">
        /// svc: 必选，字符串表示的服务名称。
        /// svcInput: 可选，服务参数对应的 json 对象。
        /// callback: 可选，回调。
        /// async: true。
        /// </param>
        var o = Rafy.svc.ServiceInvoker;
        return o.invoke.apply(o, arguments);
    },

    //--------------------------------------  数组扩展方法 -------------------------------------
    each: function (array, fn) {
        /// <summary>
        /// 遍历数组的每一个元素，应用指定的方法
        /// </summary>
        /// <param name="array">需要遍历的数组、或者 Store、TreeStore、MixedCollection 对象</param>
        /// <param name="fn">
        /// 一个回调函数，参数是每一个元素，如果该方法返回 false，则终止整个循环。
        /// </param>
        /// <returns>如果在某个元素时终止了整个循环，则返回这个元素，否则返回 null。</returns>
        if (array) {
            //树型数据集，使用深度递归遍历
            if (array instanceof Ext.data.TreeStore) {
                //跳过 root，直接遍历第一层子结点。
                var root = array.getRootNode();
                for (var i = 0; i < root.childNodes.length; i++) {
                    var stopped = this._eachInTreeNode(root.childNodes[i], fn);
                    if (stopped) return stopped;
                }
            }
            else if (array.isStore || array instanceof Ext.util.AbstractMixedCollection) {
                var c = array.getCount();
                for (var i = 0; i < c; i++) {
                    var item = array.getAt(i);
                    var res = fn(item);
                    if (res === false) return item;
                }
            }
            else {
                for (var i = 0; i < array.length; i++) {
                    var item = array[i];
                    var res = fn(item);
                    if (res === false) return item;
                }
            }
        }
        return null;
    },
    _eachInTreeNode: function (node, fn) {
        /// <summary>
        /// 深度递归遍历树型数据集
        /// </summary>
        /// <param name="store"></param>
        /// <param name="fn"></param>

        var res = fn(node);
        if (res === false) return node;

        //递归遍历结点的子结点。
        if (!node.isLeaf()) {
            for (var j = 0; j < node.childNodes.length; j++) {
                var stopped = this._eachInTreeNode(node.childNodes[i], fn);
                if (stopped) return stopped;
            }
        }

        return null;
    },
    findFirst: function (array, filter) {
        /// <summary>
        /// 遍历数组的每一个元素，找到指定的项
        /// </summary>
        /// <param name="array">array,Store,MixedCollection</param>
        /// <param name="filter">bool function(item)，返回真表示找到想要的结果。</param>
        /// <returns></returns>
        return this.each(array, function (i) {
            if (filter(i)) {
                return false;
            }
        });
    },
    first: function (array, filter) {
        /// <summary>
        /// 遍历数组的每一个元素，找到指定的项。
        /// 如果没有找到，则抛出异常。
        /// </summary>
        /// <param name="array">array,Store,MixedCollection</param>
        /// <param name="filter">bool function(item)</param>
        /// <returns></returns>
        var r = this.findFirst(array, filter);
        if (r == null) throw new Error("没有找到对应的元素。");
        return r;
    },
    select: function (array, selector) {
        /// <summary>
        /// 遍历数组的每一个元素，应用指定的方法来生成新的数组
        /// </summary>
        /// <param name="array">需要遍历的数组、或者 Store、MixedCollection 对象</param>
        /// <param name="selector">
        /// 一个回调函数，参数是每一个元素，返回选择的结果。
        /// </param>
        /// <returns>新的数组</returns>

        var res = [];

        this.each(array, function (item) {
            res.push(selector(item));
        });

        return res;
    },
    sum: function (array, numSelector) {
        /// <summary>
        /// 遍历数组的每一个元素，应用指定的方法来生成新的数组
        /// </summary>
        /// <param name="array">需要遍历的数组、或者 Store、MixedCollection 对象</param>
        /// <param name="selector">
        /// 一个回调函数，参数是每一个元素，返回 number 类型的数据。
        /// </param>
        /// <returns>最终的和</returns>
        var sum = Ext.Array.sum(Rafy.select(array, numSelector));
        return sum;
    },

    //-------------------------------------  插件 -------------------------------------
    definePlugin: function (name, data) {
        /// <summary>
        /// 定义一个插件类
        /// </summary>
        /// <param name="name">插件类型的名称</param>
        /// <param name="data">类型相关定义</param>
        var plugin = Ext.define(name, data);
        this._plugins.push(plugin);
    },
    _getPlugins: function () {
        /// <summary>
        /// internal
        /// 获取所有的插件的集合。
        /// </summary>
        /// <returns type=""></returns>
        return this._plugins;
    },

    isDebugging: function () {
        return this._isDebugging;
    },

    //-------------------------------------  实体定义 -------------------------------------
    _dm: function (model, entityConfig) {
        /// <summary>
        /// internal
        /// define model
        /// 定义一个实体类型。
        /// </summary>
        /// <param name="model"></param>
        /// <param name="entityConfig"></param>
        Ext.apply(entityConfig, {
            "extend": "Rafy.data.Entity",
            "proxy": {
                "url": "/Rafy_EntityDataPortal.ashx?type=" + model,
                "reader": {
                    "type": "json",
                    "root": "entities"
                },
                "type": "ajax"
            }
        });
        //if (entityConfig.isTree) {
        //    Ext.Array.insert(entityConfig.fields, 0, [{
        //        "name": "TreeIndex",
        //        "defaultValue": "",
        //        "type": "string"
        //    }, {
        //        "name": "TreePId",
        //        "defaultValue": 0,
        //        "type": "int"
        //    }]);
        //}

        //为所有字段生成 get、set 方法。
        this.each(entityConfig.fields, function (f) {
            var name = f.name;
            entityConfig['set' + name] = function (value) {
                this.set(name, value);
            }
            entityConfig['get' + name] = function () {
                return this.get(name);
            }
        });

        Ext.define(model, entityConfig, function () {
            //所有的实体类型上，都添加一个 isTree 属性，用于判断实体类是否为树型实体。
            this.isTree = !!entityConfig.isTree;
        });
    },
    getModel: function (model) {
        /// <summary>
        /// 获取指定的实体类型。
        /// </summary>
        /// <param name="model">实体类型名称或者实体类型本身。</param>
        /// <returns type=""></returns>
        return Ext.ModelMgr.getModel(model);
    },
    getModelName: function (model) {
        /// <summary>
        /// 获取指定实体模型的类型名称。
        /// </summary>
        /// <param name="model"></param>
        /// <returns type=""></returns>
        if (Ext.isString(model)) {
            return model;
        }
        return Ext.getClassName(model);
    },

    //-------------------------------------  命令定义 -------------------------------------
    _commands: [],
    defineCommand: function (cmdName, members) {
        /// <summary>
        /// 定义一个命令类型。
        /// </summary>
        /// <param name="cmdName">命令的名称。</param>
        /// <param name="members">
        /// meta: 按钮的元数据。目前主要用于配置界面上的按钮。
        /// </param>

        var o = Rafy.cmd.CommandManager;
        return o.defineCommand.apply(o, arguments);
    },

    //-------------------------------------  语言 -------------------------------------
    translate: function (raw) {
        /// <summary>
        /// 使用多国语言引擎，翻译并收集某个固定的字符串。
        /// </summary>
        /// <param name="raw"></param>
        /// <returns type="String"></returns>

        //to do:暂时没有实现多国语言的收集与翻译。
        return raw;
    }
});