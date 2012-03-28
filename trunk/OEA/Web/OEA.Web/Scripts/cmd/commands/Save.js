Ext.define('Oea.cmd.Save', {
    extend: 'Oea.cmd.Command',
    config: {
        meta: { text: "保存" }
    },
    execute: function (listView) {
        listView.save(function (res) {
            if (res.success) {
//                if (listView.getIsTree()) {
//                    setTimeout(function () { listView.expandSelection(); }, 200);
//                }
                //                Ext.Msg.alert('提示', '保存成功！');
            }
            else {
                Ext.Msg.alert('保存失败', res.msg);
            }
        });
    }
});