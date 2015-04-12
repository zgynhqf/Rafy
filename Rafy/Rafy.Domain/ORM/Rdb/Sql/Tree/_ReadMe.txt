Rafy.Domain.ORM.Tree：本命名空间下的所有类表示了 Sql 的语法树。
Rafy.Domain.ORM.TreeGenerator：本命名空间下的类可以为 Sql 语法树生成对应的 Sql 语句。

两个命名空间都不公开，只由内部生成 Sql 使用。将来，可以考虑公开，给上层开发人员提供生成跨数据库的 Sql 语句的功能。

本质上，这两个命名空间下的类，都不依赖 Rafy 中的其它组件，也不是 ORM 关系映射功能。它们只是提供了一种非常灵活的 Sql 语句生成方式。