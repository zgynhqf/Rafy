除了开发者常用的 Linq 查询，Rafy 框架还提供了 Sql 语法树的方式来进行查询。  
这种查询方式下，开发者不需要直接编写真正的 Sql 语句，而是转而使用一套中间 Sql 语法树对象。这隔离了与具体数据库的耦合，使得开发者编写的查询可以跨越多种不同的数据库运行，甚至可以在非关系型数据库中运行。同时，框架还结合托管属性，提供了方便开发者使用的 API，并尽量保持与传统 Sql 相近的语法，使得开发者可以快速理解并编写。  

##快速示例
SqlTree 查询是直接以一种类似于 Sql 语法的格式，并结合实体托管属性 IManagedProperty 来进行查询的查询模式。如下：

```cs
[RepositoryQuery]
public virtual ChapterList GetBy(string name, PagingInfo pi)
{
    var f = QueryFactory.Instance;

    var t = f.Table<Chapter>();

    var q = f.Query(
        selection: t.Star(),//查询所有列
        from: t,//要查询的实体的表
        where: t.Column(Chapter.NameProperty).Contains(name)//where 条件,
        orderBy: new List<IOrderBy> {//排序
            f.OrderBy(source.Column(Chapter.NameProperty), OrderDirection.Ascending)
        }
    );

    return (ChapterList)this.QueryData(q, pi);
}
```

可以看到，SqlTree 语法非常简单：
 -  通过 QueryFactory.Instance 类型的单例对象来定义整个 SqlTree 查询对象。

 -  查询中使用的是实体类型(Chapter)和实体的托管属性(Chapter.NameProperty)来定义表和字段。
    ​                     
    更多的查询语法示例，见本节后面的[更多示例]。

##使用场景
当您处于以下场景时，需要使用 SqlTree 查询：
 - Linq 查询无法支持的一些场景。  
     Linq 查询目前只支持有限的一些操作符的解析，以及不太复杂的关系的分析。所以当您的查询较为复杂，已经无法使用 Linq 查询来实现时，可以考虑使用 SqlTree 查询。 
 - 需要更精确地控制 Sql 语句。  
     如果想要更加精确地控制最终生成的 Sql 语句，也需要使用 SqlTree。  
       例如，Linq 查询中需要两个实体有确切的实体关系才会最终生成 Join 语句；但是 SqlTree 则与 Sql 语句无异，开发者可以随意将两个实体对应的表进行 Join 操作。
 - 需要更好的性能。  
     SqlTree 查询是 Rafy 框架查询数据（表格、实体）的核心实现。在框架底层，Linq 查询也都是完全是基于 SqlTree 查询来实现的。当开发者在使用 Linq 查询时，编译器其实是生成一组对象来表示一棵表达式树，而 Rafy 框架会解析这棵树，生成更加底层的 SqlTree 对象，才交给执行引擎去生成真正的 Sql 语句并最终执行。所以，直接使用 SqlTree 则节约了表达式树的生成（大量反射与对象）与解析的性能消耗。  
       同样，Rafy 没有象 Hibernate 框架定义一套新的基于字符串的查询语法（如 hql），也是因为开发者编写 hql，不但无法得到编译时的语法支持，而且性能上也需要消耗对 hql 进行解析并生成 SqlTree，不如直接使用更直接的 SqlTree。  
       当然，Rafy 在 SqlTree 的基础上再推出 Linq 查询的原因，是因为 SqlTree 本身需要一定的学习周期才能使用，而开发者则更熟悉使用 Linq 语法进行查询，基本可以认为是上手即用，所以支持 Linq 查询可以简化大部分的简单开发场景。
 - 希望编写更通用的查询。  
     仓库基类 EntityRepository 中自带的 GetAll、GetById 等方法，都是面向所有实体类型的非常通用的查询。对于基于 Rafy 的上层框架的开发者而言，除了直接使用这些自带的通用查询，很多时候是需要自行编写一些类似的通用查询。  
       Linq 的 Labmda 语法中的属性表达式(e.Name)需要绑定到具体的实体类型(Book e)，这导致了必须使用反射去生成表达式树，才能编写通用查询。但是，SqlTree 的语法是基于托管属性框架的，它不需要使用确切的实体属性表达式，只需要使用托管属性的运行时对象 IManagedProperty 即可(Book.NameProperty)。这使得开发人员可以更加方便地编写通用查询。例如，仓库基类 EntityRepository 中的所有查询方法，都是直接通过使用实体的托管属性来实现的，例如：GetById、GetByParentId、GetAll 等。
 - 可以为扩展属性编写查询。  
     由于扩展属性写在额外的程序集插件中的，所以当无法通过 Linq 表达式进行查询。这时就不得不通过托管属性 IManagedProperty 来定义 SqlTree 完成查询了。  
       关于扩展属性，参见：[实体属性扩展](../../../领域实体框架\插件级扩展\实体属性扩展.html)。
 - 支持多个数据库。  
     上述的场景中，其实还可以直接编写 Sql 语句来进行查询。但是这样就很难保证开发者编写的 Sql 语句能够在多个数据库上能够正确运行。
 - 查询需要支持仓库数据层的扩展点。  
     由于 Rafy 的查询核心都是基于 SqlTree 来实现的，所以内部的所有扩展点都是要依赖 SqlTree的。如果开发者直接编写 Sql 语句来查询，那么这些许多的扩展点都将无效，无法对开发者编写的这条 Sql 语句进行扩展。  
       例如：当使用[幽灵插件](../../../插件\幽灵插件.html)对所有幽灵数据进行自动过滤时，如果开发者使用手工编写的 Sql 语法进行查询，那么自动过滤功能无效，需要开发者自己进行幽灵数据的过滤。

##代码段
RafySDK 中提供了两个代码段，来辅助开发者生成基本的 SqlTree 查询结构：Rafy_Query、Rafy_Query_TableQueryContent。
详情见：[代码段](../../../领域实体框架\其它\代码段.html)。

##更多示例
下面将会列出一些常见的 SqlTree 查询示例。通过这些代码，您将学习到如何在各种查询需求下使用 SqlTree。
 - 基础查询：

 ```cs
[RepositoryQuery]
public virtual ChapterList GetBy(string name, PagingInfo pi)
{
    var f = QueryFactory.Instance;

    var t = f.Table<Chapter>();

    var q = f.Query(
        //selection: f.SelectAll(),//没有 selection，则默认表示查询所有列
        from: t,//要查询的实体的表
        where: t.Column(Chapter.NameProperty).Contains(name)//where 条件
    );

    return (ChapterList)this.QueryData(q, pi);
}
 ```

 - 表格数据查询：

 ```cs
[RepositoryQuery]
public virtual LiteDataTable GetBy(string name, PagingInfo pi)
{
    var f = QueryFactory.Instance;

    var t = f.Table(this);//使用当前的仓库来表示当前的表

    var q = f.Query(
        from: t,
        where: t.Column(Chapter.NameProperty).Contains(name)
    );

    return this.QueryTable(q, pi);//由查询实体变为查询数据表格，只是更换了这一行代码。
}
 ```

 - 两个列的条件进行比较：

 ```cs
var table = f.Table<Chapter>();
var q = f.Query(
    from: table,
    where: table.Column(Chapter.NameProperty).Equal(table.Column(Chapter.CodeProperty))//两个列相等
);
 ```

 - 使用 And、Or：

 ```cs
var table = f.Table(this);
var q = f.Query(
    from: table,
    where: f.And(
        table.Column(Chapter.NameProperty).Equal(name),
        f.Or(
            table.Column(Chapter.IdProperty).LessEqual(10),
            table.Column(Chapter.IdProperty).GreaterEqual(1000)
        )
    )
);
 ```

 - Join(`SerialNumberValueRepository`中的真实代码）：

 ```cs
/// <summary>
/// 获取某个规则下最新的一个值。
/// </summary>
/// <param name="autoCodeName"></param>
/// <returns></returns>
[RepositoryQuery]
public virtual SerialNumberValue GetLastValue(string autoCodeName)
{
    var f = QueryFactory.Instance;
    var t = f.Table<SerialNumberValue>();
    var t2 = f.Table<SerialNumberInfo>();
    var q = f.Query(
        from: t.Join(t2),//由于 SerialNumberValue 有一个 SerialNumberInfo 的引用属性，则在使用 Join 时，不需要给出 Join 的条件。
        where: t2.Column(SerialNumberInfo.NameProperty).Equal(autoCodeName),
        orderBy: new List<IOrderBy> { f.OrderBy(t.Column(SerialNumberValue.LastUpdatedTimeProperty), OrderDirection.Descending) }
    );

    return (SerialNumberValue)this.QueryData(q);
}
 ```

 - 使用完整的 Join：

 ```cs
var t = f.Table<SerialNumberValue>();
var t2 = f.Table<SerialNumberInfo>();
var q = f.Query(
    from: t.Join(t2, t.Column(SerialNumberValue.SerialNumberInfoIdProperty).Equal(t2.Column(SerialNumberInfo.IdProperty)), JoinType.Inner),//不但可以给出具体的 Join 条件，还可以给出 Join 类型。
    where: t2.Column(SerialNumberInfo.NameProperty).Equal(autoCodeName),
    orderBy: new List<IOrderBy> { f.OrderBy(t.Column(SerialNumberValue.LastUpdatedTimeProperty), OrderDirection.Descending) }
);
 ```

 - 嵌套子查询（Exists）：

 ```cs
var bookTable = f.Table(this);
var chapterTable = f.Table<Chapter>();
var q = f.Query(
    from: bookTable,
    where: f.Exists(f.Query(
        from: chapterTable,
        where: chapterTable.Column(Chapter.BookIdProperty).Equal(bookTable.IdColumn)
    ))
);
 ```

 - Not Exists：

 ```cs
var book = f.Table(this);
var chapter = f.Table<Chapter>();
var q = f.Query(
    from: book,
    where: f.Not(f.Exists(f.Query(
        from: chapter,
        where: f.And(
            chapter.Column(Chapter.BookIdProperty).Equal(bookTable.IdColumn),
            chapter.Column(Chapter.NameProperty).NotEqual("chapterName")
        )
    )))
);
 ```

 - Linq 查询与 SqlTree 混合使用  
     Linq 查询与 SqlTree 查询是可以混用的。您可以先使用 Linq 查询来快速构造简单的查询条件，然后再通过`IQuery ConvertToQuery(IQueryable queryable)`方法，传入 Linq 查询对象 IQueryable 并转换为 SqlTree 查询对象 IQuery，然后再修改 SqlTree 中的对象节点。

 ```cs
[RepositoryQuery]
public virtual ChapterList GetByBookName(string name)
{
    var q = this.CreateLinqQuery();
    q = q.Where(c => c.Book.Name == name);

    var tree = this.ConvertToQuery(q);//从 IQueryable 转换为 IQuery

    var f = QueryFactory.Instance;
    var t = tree.MainTable;

    tree.Where = f.And(tree.Where, t.Column(Chapter.NameProperty).Equal(name));

    return (ChapterList)this.QueryData(tree);
}
 ```

更多示例，请参照源码中单元测试的 ORMTest 中的 TableQuery 相关方法。
