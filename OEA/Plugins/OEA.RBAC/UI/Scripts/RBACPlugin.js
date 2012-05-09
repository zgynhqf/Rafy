Oea.definePlugin('Oea.RBAC', {
    init: function (app) {
        this._logSystem(app);
        //app.on('startupCompleted', function () {
        //    Ext.Msg.alert('mainWindowLoaded');
        //});
        //app.on('mainWindowLoaded', function () {
        //    Ext.Msg.alert('mainWindowLoaded');
        //});
    },
    _logSystem: function (app) {
    	/// <summary>
    	/// 以 AOP 的方式来记录系统日志。
    	/// </summary>
        /// <param name="app"></param>

        this._logCommands();

        //to do: log open module
        this._logModules(app);
    },
    _logModules: function (app) {
        var me = this;
        app.on('moduleCreated', function (e) {
            me._logAsync({
                title: '打开模块：' + e.moduleName,
                type: 'OpenModule',
                ModuleName: e.moduleName
            });
        });
    },
    _logCommands: function (app) {
        var me = this;

        //临时所有命令的执行事件，以添加系统日志
        Oea.AutoUI.viewFactory.on('commandCreated', function (e) {

            //条件面板中的“查询”按钮并不是生成的，所以没有这个方法，需要过滤。
            var cmd = e.command;
            if (cmd.getMeta) {
                var cmdName = cmd.getMeta().text;
                cmd.on('executed', function (e) {
                    var model = e.view.getMeta().label;

                    me._logAsync({
                        title: '执行命令完成：' + cmdName,
                        type: 'Command',
                        friendlyContent: Ext.String.format("对象：{0}", model),
                        coderContent: Ext.String.format('类型名：{0}\r\n命令名称：{1}', model, cmdName)
                    });
                });
                cmd.on('executeFailed', function (e) {
                    var model = e.view.getMeta().label;

                    me._logAsync({
                        title: '执行命令失败：' + cmdName,
                        type: 'Command',
                        friendlyContent: Ext.String.format("对象：{0}", model),
                        coderContent: Ext.String.format('类型名：{0}\r\n命令名称：{1}\r\n发生异常：{2}', model, cmdName, e.exception.message)
                    });
                });
            }
        });
    },
    _logAsync: function (opt) {
        Oea.invokeSvc({
            svc: 'OEA.RBAC.LogService',
            svcParams: {
                Title: opt.title,
                Type: opt.type,
                CoderContent: opt.coderContent,
                //以下可选
                FriendlyContent: opt.friendlyContent,
                ModuleName: opt.moduleName,
                EntityId: opt.entityId
            }
        });
    }
});
