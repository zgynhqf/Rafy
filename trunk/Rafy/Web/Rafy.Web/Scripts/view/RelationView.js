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

Ext.define('Rafy.view.RelationView', {
    statics: {
        list: 'list',
        detail: 'detail',
        navigation: 'navigation',
        condition: 'condition',
        result: 'result'
    },
    constructor: function (name, target) {
        this._owner = null;//internal
        this._name = name;
        this._target = target;
    },
    getOwner: function () { return this._owner; },
    getName: function () { return this._name; },
    getTarget: function () { return this._target; }
});