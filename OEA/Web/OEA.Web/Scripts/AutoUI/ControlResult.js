Ext.define('Oea.autoUI.ControlResult', {
    constructor: function (view, control) {
        this._view = view;
        this._control = control;
    },
    getView: function () { return this._view; },
    getControl: function () { return this._control; }
});