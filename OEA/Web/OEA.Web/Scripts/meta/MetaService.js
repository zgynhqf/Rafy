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

Ext.define('Oea.meta.MetaService', {
    singleton: true,
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
        ///     OEA.Web.ClientMetaModel.ClientAggtMeta
        /// </param>
        var url = this._url(op);
        var aOp = {
            url: url,
            async: op.async,
            success: function (response, opts) {
                var meta = Ext.decode(response.responseText);

                meta.model = op.model;

                op.callback(meta);
            },
            failure: function (response, opts) { }
        };
        Ext.Ajax.request(aOp);
    },
    _url: function (op) {
        var res = Ext.String.format("/OEA_MetaModelPortal.ashx?type={0}", op.model);
        if (op.module) { res += "&module=" + encodeURIComponent(op.module); }
        if (op.viewName) { res += "&viewName=" + encodeURIComponent(op.viewName); }
        if (op.templateType) { res += "&templateType=" + encodeURIComponent(op.templateType); }
        if (op.isAggt) { res += "&isAggt=1"; }
        if (op.isReadonly) { res += "&isReadonly=1"; }
        if (op.ignoreCommands) { res += "&ignoreCommands=1"; }
        if (op.isDetail) { res += "&isDetail=1"; }
        else if (op.isLookup) { res += "&isLookup=1"; }
        return res;
    }
});