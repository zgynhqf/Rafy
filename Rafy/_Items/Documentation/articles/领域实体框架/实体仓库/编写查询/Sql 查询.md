在对实体的编写查询时，除了使用 Linq 查询和 SqlTree 查询，还可以直接传入 Sql 语句来实现一些非常复杂的查询。例如：  

 ```cs
[RepositoryQuery]
public virtual WarehouseList GetByName(string name)
{
    FormattedSql sql =
@"SELECT Id, Code, Name 
FROM .....
INNER JOIN .....
WHERE NAME = {0}";
    sql.Parameters.Add(name);

    return (WarehouseList)(this.DataQueryer as RdbDataQueryer).QueryData(sql);
}
 ```

以上代码编写了一个自定义 Sql 字符串进行查询，这个 Sql 语句的查询结果是一个包含三个字段（Id, Code, Name）的表格。在传递给 QueryList 方法后，框架会执行这个 Sql 语句，并把返回的结果表转换为 Warehouse 实体类的集合，表中的每一行都会转换为一个 Warehouse 实体。  

上面代码中，字符串被隐式转换为一个 FormattedSql 类型的对象。

使用 FormattedSql 来格式化的 Sql 语句，可以支持在多种不同数据库上执行。它其中的每一个参数都使用大括号括住的数字来代替，最终会通过 ADO.NET 来使用参数化的查询。建议开发者多使用此类，而不是直接使用 String.Format 简单地格式化 Sql 语句。
