Ext.define('Oea.view.RelationView', {
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