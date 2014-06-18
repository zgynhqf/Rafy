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

Ext.define('Rafy.autoUI.ControlResult', {
    constructor: function (view, control) {
        this._view = view;
        this._control = control;
    },
    getView: function () { return this._view; },
    getControl: function () { return this._control; }
});