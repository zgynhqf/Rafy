/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：201201
 * 说明：客户端命令
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 201201
 * 
*******************************************************/

Ext.define('Rafy.cmd.CommandManager', {
    singleton: true,

    _commands : [],
    
    defineCommand: function (cmdName, members) {
        /// <summary>
        /// 定义一个命令类型。
        /// </summary>
        /// <param name="cmdName">命令的名称。</param>
        /// <param name="members">
        /// meta: 按钮的元数据。目前主要用于配置界面上的按钮。
        /// </param>

        //如果没有定义基类，那么基类是 Rafy.cmd.Command。
        Ext.applyIf(members, {
            extend: 'Rafy.cmd.Command'
        });

        //如果 meta 是直接定义在 members 中的，则应该把它移动到 config 中。
        var meta = members.meta;
        if (meta) {
            delete members.meta;
            if (!members.config) {
                members.config = {};
            }
            members.config.meta = meta;
        }

        var cmdClass = Ext.define(cmdName, members);

        this._commands.push(cmdClass);

        return cmdClass;
    },
    getCommandClasses: function () {
    	/// <summary>
    	/// 获取所有命令类型的集合
    	/// </summary>
    	/// <returns type=""></returns>
        return this._commands;
    },
    getCommands: function () {
        var res = [];

        Rafy.each(this._commands, function (c) {
            res.push(Ext.getClassName(c));
        });

        return res;
    }
});