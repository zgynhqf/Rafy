由于 Rafy.Domain.ORM.Tree 命名空间中的类中的表名、列名是面向字符串的，不方便开发人员直接使用。所以设计了 Rafy.Domain.ORM.Query 命名空间下的类与接口，它们依赖于 Rafy.Domain.ORM.Tree 并面向 Entity、IManagedProperty 构建，方便开发人员直接使用的。

同时，Rafy.Domain.ORM.Query 中的所有接口，构成了面向 IManagedProperty 的新的查询语法树。

Rafy.Domain.ORM.Query.Impl 中的所有类，都继承自相应的 Sql 语法树，同时实现了 Query 中相应的接口。配以 QueryFactory 工厂，上层不但可以只使用接口、方便后期扩展，而且也使得应用的接口比较简洁。例如下面单元测试中用到的方法：


[TestMethod]
public void ORM_EntityQueryTree_ChildrenAll()
{
    var f = new QueryFactory();
    var articleSource = f.EntitySource(RF.ResolveInstance<ArticleRepository>());
    var userSource = f.EntitySource(RF.ResolveInstance<BlogUserRepository>(), "u");

    var query = f.Query(
        from: userSource,
        where: f.And(
            f.PropertyConstraint(
                userSource.Property(Entity.IdProperty),
                PropertyOperator.Greater, 0
            ),
            f.NotConstraint(f.ExistsConstraint(f.Query(
                selection: f.Literal("0"),
                from: articleSource,
                where: f.And(
                    f.PropertiesComparison(
                        articleSource.Property(Article.UserIdProperty),
                        userSource.Property(Entity.IdProperty)
                    ),
                    f.PropertyConstraint(
                        articleSource.Property(Article.IdProperty),
                        PropertyOperator.Greater, 0
                    )
                )
            )))
        )
    );

    var generator = new SqlServerSqlGenerator { AutoQuota = false };
    f.Generate(generator, query);
    var sql = generator.Sql;
    Assert.IsTrue(sql.ToString() == @"SELECT *
FROM BlogUser AS u
WHERE (u.Id > {0} AND NOT (EXISTS (
    SELECT 0
    FROM Article
    WHERE (Article.UserId = u.Id AND Article.Id > {1})
)))");
}