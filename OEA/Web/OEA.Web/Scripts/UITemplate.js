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

Ext.define('Oea.UITemplate', {
    extend: 'Ext.util.Observable',

    _entityType: null,
    _serverTemplateType: null,

    constructor: function (meta) {
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
        /// value 是该类在服务端的 AssemblyQuanifyName，如："OEA.Library.Audit.AuditItem, OEA.RBAC"
        /// </param>
        this._serverTemplateType = value;
    },
    getEntityType: function () { return this._entityType; },
    setEntityType: function (value) { this._entityType = value; },

    createUI: function (entityType) {
        /// <summary>
        /// 为当前的实体类型生成 ui 界面。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns type="Oea.autoUI.ControlResult"></returns>

        //如果指定了 entityType，则更新本模板当前使用的实体类型
        if (entityType) { this._entityType = entityType; }

        var blocks = this.defineBlocks();
        var ui = this.createUICore(blocks);
        this.onUIGenerated(ui);
        return ui;
    },
    defineBlocks: function () {
        /// <summary>
        /// 获取本模板定义的聚合块。
        /// </summary>
        /// <returns type="OEA.Web.ClientMetaModel.ClientAggtMeta"></returns>
        var blocks = this.defineBlocksCore();

        this.onBlockDefined(blocks);

        return blocks;
    },

    //protected virtual
    defineBlocksCore: function () {
        /// <summary>
        /// 子类可以重写此方法来实现本地定义聚合块。
        ///
        /// 默认使用同步的方式，获取服务器上定义的块定义。
        /// </summary>
        /// <returns></returns>

        var blocks = null;

        Oea.AutoUI.getMeta({
            async: false,
            model: this._entityType,
            templateType: this._serverTemplateType,
            isAggt: true,
            callback: function (result) { blocks = result; }
        });

        return blocks;
    },
    //protected virtual
    onBlockDefined: function (blocks) {
        this.fireEvent('blocksDefined', { blocks: blocks });
    },
    //protected virtual
    createUICore: function (blocks) {
        return Oea.AutoUI.generateAggtControl(blocks);
    },
    //protected virtual
    onUIGenerated: function (ui) {
        this.fireEvent('uiGenerated', { ui: ui });
    }
});