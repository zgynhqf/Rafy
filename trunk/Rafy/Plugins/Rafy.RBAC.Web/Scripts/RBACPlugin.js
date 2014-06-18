Rafy.definePlugin('Rafy.RBAC', {
    init: function (app) {
        this._logSystem(app);
        //app.on('startupCompleted', function () {
        //    Ext.Msg.alert('startupCompleted');
        //});
    },
    _logSystem: function (app) {
        /// <summary>
        /// 以 AOP 的方式来记录系统日志。
        /// </summary>
        /// <param name="app"></param>

        this._logCommands();

        this._logModules(app);
    },
    _logModules: function (app) {
        /// <summary>
        /// 记录打开模块的操作。
        /// </summary>
        /// <param name="app"></param>
        var me = this;
        app.on('moduleCreated', function (e) {
            me._logAsync({
                title: '打开模块：'.t() + e.moduleName,
                type: 'OpenModule',
                moduleName: e.moduleName
            });
        });
    },
    _logCommands: function () {
        /// <summary>
        /// 记录按钮的动作。
        /// </summary>
        var me = this;

        //临时所有命令的执行事件，以添加系统日志
        Rafy.AutoUI.viewFactory.on('commandCreated', function (e) {
            var cmd = e.command;

            //如果是查询按钮，则返回，不记录日志。
            if (Ext.getClassName(cmd) == 'Rafy.cmd.ExecuteQuery') {
                return;
            }

            //条件面板中的“查询”按钮并不是生成的，所以没有这个方法，需要过滤。
            var cmdName = cmd.getMeta().text;

            cmd.on('executed', function (e) {
                me._queryLabel(this.getOwnerView(), function (model, modelLabel) {
                    me._logAsync({
                        title: '执行命令完成：'.t() + cmdName,
                        type: 'Command',
                        friendlyContent: Ext.String.format("对象：{0}".t(), modelLabel.t()),
                        coderContent: Ext.String.format('类型名：{0}\r\n命令名称：{1}'.t(), model, cmdName)
                    });
                });
            });

            cmd.on('executeFailed', function (e) {
                me._queryLabel(this.getOwnerView(), function (model, modelLabel) {
                    me._logAsync({
                        title: '执行命令失败：'.t() + cmdName,
                        type: 'Command',
                        friendlyContent: Ext.String.format("对象：{0}".t(), modelLabel.t()),
                        coderContent: Ext.String.format('类型名：{0}\r\n命令名称：{1}\r\n发生异常：{2}'.t(), model, cmdName, e.exception.message)
                    });
                });
            });
        });
    },
    _logAsync: function (opt) {
        //如果没有指定模块的名称，那么就通过应用程序来查找当前打开的模块。
        if (!opt.moduleName) {
            var module = Rafy.App.getCurrentModule();
            if (module) {
                opt.moduleName = module.keyLabel;
            }
        }

        Rafy.invokeService({
            svc: 'Rafy.RBAC.LogService',
            svcInput: {
                Title: opt.title,
                Type: opt.type,
                CoderContent: opt.coderContent,
                //以下可选
                FriendlyContent: opt.friendlyContent,
                ModuleName: opt.moduleName,
                EntityId: opt.entityId
            }
        });
    },
    _queryLabel: function (view, callback) {
        var model = Rafy.getModelName(view.getModel());
        Rafy.invokeService({
            svc: 'Rafy.RBAC.QueryModelLabelService',
            svcInput: {
                ClientEntity: model
            },
            callback: function (res) {
                var label = res.Label;
                if (!label) label = model;
                callback(model, label);
            }
        });
    }
});
