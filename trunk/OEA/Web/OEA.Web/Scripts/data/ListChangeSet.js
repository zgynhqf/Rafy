//internal
Ext.define('Oea.data.ListChangeSet', {
    statics: {
        //internal
        _getChangeSetData: function (store, isTree, withUnchanged) {
            var cs = !isTree ? new Oea.data.EntityListChangeSet(withUnchanged) : new Oea.data.TreeListChangeSet();
            var data = cs._getDataToServer(store);
            return data;
        },
        //internal
        _getItemData: function (item, isTree, withUnchanged) {
            var cs = !isTree ? new Oea.data.EntityListChangeSet(withUnchanged) : new Oea.data.TreeListChangeSet();
            var data = cs._getItemToServer(item);
            return data;
        },
        //internal
        _eachItemInData: function (changeSetData, isTree, fn) {
            if (isTree) {
                var me = this;
                Oea.each(changeSetData.roots, function (root) { me(root, fn); });
            }
            else {
                var i = changeSetData;
                Oea.each(i.toUpdate, fn) &&
                Oea.each(i.toCreate, fn) &&
                Oea.each(i.toDelete, fn) &&
                Oea.each(i.unchanged, fn);
            }
        },
        //internal
        _needUpdateToServer: function (changeSetData, isTree) {
            if (isTree) {
                return true;
                //                var root = changeSetData;
            }
            else {
                var i = changeSetData;
                return i.toUpdate && i.toUpdate.length
                    || i.toCreate && i.toCreate.length
                    || i.toDelete && i.toDelete.length
                    || i.unchanged && i.unchanged.length;
            }
        },
        //private
        _traverseTreeItem: function (item, fn) {
            //返回 false，表示不需要再迭代了
            if (fn(item) === false) { return false; }

            for (var i = 0; i < item.TreeChildren; i++) {
                if (!this._traverseTreeItem(item.TreeChildren[i], fn)) return false;
            }

            return true;
        }
    },

    //private
    _getDataToServer: function (store) { Oea.markAbstract(); },
    //private
    _getItemToServer: function (item) {
        var data = {};
        if (item.phantom) {
            data.toCreate = [item];
            this._convertNormalArray(data, 'toCreate');
        }
        else if (item.dirty) {
            data.toUpdate = [item];
            this._convertNormalArray(data, 'toUpdate');
        }
        else if (this._withUnchanged) {
            data.unchanged = [item];
            this._convertNormalArray(data, 'unchanged');
        }
        return data;
    },

    //protected
    _getPersistFields: function (item) {
        return Oea.data.EntityRepository.getPersistFields(item);
    },
    //protected
    _getPersistData: function (item) {
        var data = {};

        //属性需要可以保存，并且是大写
        var fields = Oea.data.EntityRepository.getPersistFields(item);
        Oea.each(fields, function (f) { data[f.name] = item.get(f.name); });

        return data;
    },
    //protected
    _convertNormalArray: function (data, property) {
        var me = this;
        var raw = data[property];
        if (!raw || raw.length == 0) {
            delete data[property];
            return;
        }

        var list = [];
        Oea.each(raw, function (item) {
            list.push(me._getPersistData(item));
        });
        data[property] = list;
    },
    //protected
    _convertDeleteArray: function (data) {
        var me = this;
        var raw = data.toDelete;
        if (!raw || raw.length == 0) {
            delete data.toDelete;
            return;
        }

        var list = [];
        Oea.each(raw, function (item) {
            if (!item.phantom) {
                list.push(me._getPersistData(item));
            }
        });
        data.toDelete = list;
    }
});

//internal
Ext.define('Oea.data.EntityListChangeSet', {
    extend: 'Oea.data.ListChangeSet',

    _withUnchanged: false,

    constructor: function (withUnchanged) {
        if (withUnchanged) this._withUnchanged = true;
    },

    _getDataToServer: function (store) {
        var data = {
            toCreate: store.getNewRecords(),
            toUpdate: store.getUpdatedRecords(),
            toDelete: store.getRemovedRecords()
        };

        this._convertNormalArray(data, 'toCreate');
        this._convertNormalArray(data, 'toUpdate');
        this._convertDeleteArray(data);

        if (this._withUnchanged) {
            data.unchanged = store.data.filterBy(function (item) {
                return item.phantom !== true && item.dirty !== true && item.isValid();
            }).items;
            this._convertNormalArray(data, 'unchanged');
        }

        return data;
    }
});

//internal
Ext.define('Oea.data.TreeListChangeSet', {
    extend: 'Oea.data.ListChangeSet',
    _getDataToServer: function (treeStore) {
        var result = { toDelete: treeStore.getRemovedRecords() };
        this._convertDeleteArray(result);

        //为了保证树型对象间的父子、前后关系，这里需要把客户端的整个树型列表上传到服务器端。
        var rootNode = treeStore.getRootNode();
        result.roots = this._convertNode(rootNode).TreeChildren;

        return result;
    },
    _convertNode: function (node) {
        var item = this._getPersistData(node);

        if (node.phantom) {
            item.isNew = 1;
        }

        if (!node.isLeaf()) {
            item.TreeChildren = [];
            for (var i = 0; i < node.childNodes.length; i++) {
                var child = this._convertNode(node.childNodes[i]);
                item.TreeChildren.push(child);
            }
        }

        return item;
    }

    //差异保存方案，暂留。
    //    ,_convertTreeCreateData: function (entityList, property, option) {
    //        var me = this;
    //        //树结点时，要处理树型结点之间的关系
    //        var raw = entityList[property];
    //        if (!raw || raw.length == 0) {
    //            delete entityList[property];
    //            return;
    //        }
    //        //roots 中只传输根对象
    //        var roots = [];
    //        //当前已经处理完的元素列表
    //        var added = [];
    //        //当前还没有处理的元素列表
    //        var toAdd = Ext.Array.clone(raw);
    //        Oea.each(raw, function (item) {
    //            var isParent = item.parentNode.isRoot();
    //            if (!isParent) {
    //                var parent = Oea.findFirst(raw, function (p) { return p == item.parentNode; });
    //                isParent = parent == null;
    //            }
    //            if (isParent) {
    //                var data = me._getPersistData(item);
    //                data._rawNode = item;
    //                roots.push(data);
    //                added.push(data);
    //                Ext.Array.remove(toAdd, item);
    //            }
    //        });
    //        var i = 1;
    //        while (toAdd.length > 0) {
    //            if (i++ > 1000) { Oea.notSupport("死循环异常……"); }
    //            Oea.each(toAdd, function (item) {
    //                var data = me._getPersistData(item);
    //                data._rawNode = item;
    //                var parent = Oea.findFirst(added, function (p) { return p._rawNode == item.parentNode; });
    //                if (parent) {
    //                    if (!parent.TreeChildren) parent.TreeChildren = [];
    //                    parent.TreeChildren.push(data);
    //                    added.push(data);
    //                    Ext.Array.remove(toAdd, item);
    //                    return false;
    //                }
    //            });
    //        }
    //        Oea.each(added, function (i) { delete i._rawNode; });
    //        entityList[property] = roots;
    //    }
});