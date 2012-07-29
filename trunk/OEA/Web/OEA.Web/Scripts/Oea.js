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

Ext.define('Oea', {
    singleton: true,

    _IdPropertyName: 'Id',
    _plugins: [],
    //internal
    _isDebugging: true,

    notSupport: function (name) {
        name = name || "";
        alert("这个方法暂时不被支持。" + name);
        throw new Error("这个方法暂时不被支持。" + name);
    },
    notImplement: function (name) {
        name = name || "";
        alert("这个方法没有被实现。" + name);
        throw new Error("这个方法没有被实现。" + name);
    },
    markAbstract: function (name) {
        this.notImplement(name);
    },
    invokeSvc: function (op) {
        /// <summary>
        /// 调用指定的服务
        /// </summary>
        /// <param name="op">
        /// svc: 必选，字符串表示的服务名称。
        /// svcParams: 可选，服务参数对应的 json 对象。
        /// callback: 可选，回调。
        /// async: true。
        /// </param>
        var o = Oea.svc.ServiceInvoker;
        return o.invoke.apply(o, arguments);
    },
    //------------------------------------- 数组扩展方法 -------------------------------------
    each: function (array, fn) {
        /// <summary>
        /// 遍历数组的每一个元素，应用指定的方法
        /// </summary>
        /// <param name="array">需要遍历的数组、或者 Store、MixedCollection 对象</param>
        /// <param name="fn">
        /// 一个回调函数，参数是每一个元素，如果该方法返回 false，则终止整个循环。
        /// </param>
        /// <returns></returns>
        if (array) {
            if (array.isStore || array instanceof Ext.util.AbstractMixedCollection) {
                var c = array.getCount();
                for (var i = 0; i < c; i++) {
                    var item = array.getAt(i);
                    var res = fn(item);
                    if (res === false) return false;
                }
            }
            else {
                for (var i = 0; i < array.length; i++) {
                    var item = array[i];
                    var res = fn(item);
                    if (res === false) return false;
                }
            }
        }
        return true;
    },
    findFirst: function (array, filter) {
    	/// <summary>
        /// 遍历数组的每一个元素，找到指定的项
    	/// </summary>
    	/// <param name="array">array,Store,MixedCollection</param>
    	/// <param name="filter">bool function(item)</param>
    	/// <returns></returns>
        var res = null;

        this.each(array, function (i) {
            if (filter(i)) {
                res = i;
                return false;
            }
        });

        return res;
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
        var sum = Ext.Array.sum(Oea.select(array, numSelector));
        return sum;
    },

    definePlugin: function (name, data) {
        /// <summary>
        /// 定义一个插件类
        /// </summary>
        /// <param name="name">插件类型的名称</param>
        /// <param name="data">类型相关定义</param>
        var plugin = Ext.define(name, data);
        this._plugins.push(plugin);
    },
    //internal
    _getPlugins: function () {
        return this._plugins;
    },

    isDebugging: function () {
        return this._isDebugging;
    },

    defineEntity: function (model, entityConfig) {
        Ext.apply(entityConfig, {
            "extend": "Oea.data.Entity",
            "proxy": {
                "url": "/OEA_EntityDataPortal.ashx?type=" + model,
                "reader": {
                    "type": "json",
                    "root": "entities"
                },
                "type": "ajax"
            }
        });
        Ext.define(model, entityConfig);
    }
});