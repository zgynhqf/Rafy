本程序集主要用于兼容旧的 IPropertyQuery 代码。
保留所有基于 IPropertyQuery 的接口；内部实现时，把 IPropertyQuery 转换为 IQuery 对象，再调用新的 QueryList(IQuery) 接口来完成。