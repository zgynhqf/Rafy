Ext.define('Oea.cmd.ExecuteQuery', {
    extend: 'Oea.cmd.Command',
    execute: function (view) {
        view.executeQuery();
    }
});