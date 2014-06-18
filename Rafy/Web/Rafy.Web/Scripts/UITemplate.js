/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120502
 * 说明：
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120502
 * 
*******************************************************/

Ext.define('Rafy.UITemplate', {
    extend: 'Ext.util.Observable',

    _model: null,
    _serverTemplateType: null,

    constructor: function (meta) {
        this.callParent(arguments);

        this.addEvents('blocksDefined', 'uiGenerated');
    },

    getServerTemplate: function () {
        /// <summary>
        /// 设置对应的服务端模板类
        /// </summary>
        /// <returns></returns>
        return this._serverTemplateType;
    },
    setServerTemplate: function (value) {
        /// <summary>
        /// 设置对应的服务端模板类
        /// </summary>
        /// <param name="value">
        /// value 是该类在服务端的 AssemblyQuanifyName，如："Rafy.Library.Audit.AuditItem, Rafy.RBAC"
        /// </param>
        this._serverTemplateType = value;
    },
    getModel: function () {
    	/// <summary>
        /// 返回本模板对应的实体类型。
    	/// </summary>
    	/// <returns type=""></returns>
        return this._model;
    },
    setModel: function (value) {
    	/// <summary>
        /// 设置本模板对应的实体类型。
    	/// </summary>
    	/// <param name="value">可以是字符串，也可以是实体类型。</param>
        this._model = value;
    },

    createUI: function (model) {
        /// <summary>
        /// 为当前的实体类型生成 ui 界面。
        /// </summary>
        /// <param name="model"></param>
        /// <returns type="Rafy.autoUI.ControlResult"></returns>

        //如果指定了 model，则更新本模板当前使用的实体类型
        if (model) { this.setModel(model); }

        var blocks = this.defineBlocks();
        var ui = this._createUICore(blocks);
        this._onUIGenerated(ui);
        return ui;
    },
    defineBlocks: function () {
        /// <summary>
        /// 获取本模板定义的聚合块。
        /// </summary>
        /// <returns type="Rafy.Web.ClientMetaModel.ClientAggtMeta"></returns>
        var blocks = this._defineBlocksCore();

        this._onBlockDefined(blocks);

        return blocks;
    },

    _defineBlocksCore: function () {
        /// <summary>
        /// protected virtual
        /// 子类可以重写此方法来实现本地定义聚合块。
        ///
        /// 默认使用同步的方式，获取服务器上定义的块定义。
        /// </summary>
        /// <returns></returns>

        var blocks = null;

        Rafy.AutoUI.getMeta({
            async: false,
            model: Rafy.getModelName(this._model),
            templateType: this._serverTemplateType,
            isAggt: true,
            callback: function (result) { blocks = result; }
        });

        return blocks;
    },
    _onBlockDefined: function (blocks) {
    	/// <summary>
        /// protected virtual
    	/// </summary>
    	/// <param name="blocks"></param>
        this.fireEvent('blocksDefined', { blocks: blocks });
    },
    _createUICore: function (blocks) {
    	/// <summary>
        /// protected virtual
    	/// </summary>
    	/// <param name="blocks"></param>
    	/// <returns type=""></returns>
        return Rafy.AutoUI.generateAggtControl(blocks);
    },
    _onUIGenerated: function (ui) {
    	/// <summary>
        /// protected virtual
    	/// </summary>
    	/// <param name="ui"></param>
        this.fireEvent('uiGenerated', { ui: ui });
    }
});