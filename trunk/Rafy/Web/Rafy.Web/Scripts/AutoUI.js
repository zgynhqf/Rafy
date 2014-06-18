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

Ext.define('Rafy.AutoUI', {
    singleton: true,
    constructor: function () {
        this.viewFactory = new Rafy.autoUI.ViewFactory();
        this.aggtUI = new Rafy.autoUI.AggtUIGenerator(this.viewFactory);
    },
    //meta
    getMeta: function (op) {
        /// <summary>
        /// 获取指定的元数据
        /// </summary>
        /// <param name="op">
        /// module 和 model 必须指定一个。
        ///     module: '', 如果是获取某个模块的元数据，则指定此参数为模块名。
        ///     model: '', 如果获取某个实体的元数据，则这个参数表示实体类名。在实体类模式下，可以选填以下两种方式
        ///         templateType: ''，此参数只在 isAggt 为 true 时有用，表示自定义的聚合块模板类型名称。
        ///         viewName: ''，如果 isAggt 为 true，表示使用的定义的扩展聚合块名称。否则表示扩展视图名称。
        /// isAggt: false
        /// isReadonly: false
        /// ignoreComands: false
        /// isDetail: false
        /// isLookup: false
        /// async: true
        /// callback: 回调，参数如下：
        ///     Rafy.Web.ClientMetaModel.ClientAggtMeta
        /// </param>

        var o = Rafy.meta.MetaService;
        return o.getMeta.apply(o, arguments);
    },
    //ui
    createListView: function () {
        var o = this.viewFactory;
        return o.createListView.apply(o, arguments);
    },
    createDetailView: function () {
        var o = this.viewFactory;
        return o.createDetailView.apply(o, arguments);
    },
    generateAggtControl: function () {
        var o = this.aggtUI;
        return o.generateControl.apply(o, arguments);
    }
});