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

Rafy.defineCommand('Rafy.rbac.cmd.LookupSelectAddEntity', {
    extend: 'Rafy.cmd.LookupSelectAdd',
    targetRefProperty: null,
    _onSelected: function (listView, selection) {
        /// <summary>
        /// protected override
        /// </summary>
        /// <param name="listView"></param>
        /// <param name="selection"></param>
        if (selection.length > 0) {
            var store = listView.getData();
            var pName = this.targetRefProperty;

            var itemsAdded = [];

            //只需要把选中对象的 id 赋值到新对象上即可。
            for (var i = 0; i < selection.length; i++) {
                var selected = selection[i];
                var id = selected.getId();
                var index = store.findExact(pName, id);
                if (index < 0) {
                    var item = listView.addNew();
                    item.set(pName, id);
                    this._onItemAdded(item, selected);

                    itemsAdded.push(item);
                }
            }

            //选中刚添加的所有对象。
            var sm = listView.getSelectionModel();
            sm.deselectAll();
            sm.select(itemsAdded);

            //不主动保存，由用户点击根对象的保存按钮来进行组合对象的保存。
            //listView.save(function (res) {
            //    if (res.Success) {
            //        Ext.Msg.alert('提示', '保存成功！');
            //    }
            //    else {
            //        Ext.Msg.alert('保存失败', res.Message);
            //    }
            //});
        }
    },
    _onItemAdded: function (item, selected) {
        /// <summary>
        /// internal abstract
        /// </summary>
        /// <param name="item"></param>
        /// <param name="selected"></param>

        Rafy.markAbstract('_onItemAdded');
    }
});

//rafy:commandEnd

Rafy.defineCommand('Rafy.rbac.cmd.LookupSelectAddOrgPosition', {
    extend: 'Rafy.rbac.cmd.LookupSelectAddEntity',
    constructor: function () {
        this.callParent(arguments);

        this.viewName = "只显示编码名称";
        this.targetRefProperty = "PositionId";
    },
    _onItemAdded: function (item, selected) {
        /// <summary>
        /// internal override
        /// </summary>
        /// <param name="item"></param>
        /// <param name="selected"></param>
        item.set('View_Code', selected.get('Code'));
        item.set('View_Name', selected.get('Name'));
    }
});

//rafy:commandEnd

Rafy.defineCommand('Rafy.rbac.cmd.LookupSelectAddOrgPositionUser', {
    extend: 'Rafy.rbac.cmd.LookupSelectAddEntity',
    constructor: function () {
        this.callParent(arguments);

        this.targetRefProperty = "UserId";
    },
    _onItemAdded: function (item, selected) {
        /// <summary>
        /// internal override
        /// </summary>
        /// <param name="item"></param>
        /// <param name="selected"></param>
        item.set('View_Code', selected.get('Code'));
        item.set('View_Name', selected.get('Name'));
    }
});