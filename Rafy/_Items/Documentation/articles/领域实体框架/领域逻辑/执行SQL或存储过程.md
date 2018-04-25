有时候，开发者不想通过实体来操作数据库，而是希望通过 SQL 语句或存储过程来直接访问数据库。Rafy 也提供了一组 API 来方便实现这类需求。  

##IDbAccesser 接口
为了尽量屏蔽各数据库中 SQL 语句参数的不同标识，同时也为了使开发者更简单地实现参数化的查询。Rafy 中提供了接口来方便开发者使用。接口定义如下：

```cs
/// <summary>
/// A db accesser which can use formatted sql to communicate with data base.
/// </summary>
public interface IDbAccesser : IDisposable
{
/// <summary>
/// The underlying db connection
/// </summary>
IDbConnection Connection { get; }

/// <summary>
/// 数据连接结构
/// </summary>
DbConnectionSchema ConnectionSchema { get; }

/// <summary>
/// Gets a raw accesser which is oriented to raw sql and <c>IDbDataParameter</c>。
/// </summary>
IRawDbAccesser RawAccesser { get; }

/// <summary>
/// Execute a sql which is not a database procudure, return rows effected.
/// </summary>
/// <param name="formattedSql">a formatted sql which format looks like the parameter of String.Format</param>
/// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
/// <returns>The number of rows effected.</returns>
int ExecuteText(string formattedSql, params object[] parameters);

/// <summary>
/// Execute the sql, and return the element of first row and first column, ignore the other values.
/// </summary>
/// <param name="formattedSql">a formatted sql which format looks like the parameter of String.Format</param>
/// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
/// <returns>DBNull or value object.</returns>
object QueryValue(string formattedSql, params object[] parameters);

/// <summary>
/// Query out some data from database.
/// </summary>
/// <param name="formattedSql">a formatted sql which format looks like the parameter of String.Format</param>
/// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
/// <returns></returns>
IDataReader QueryDataReader(string formattedSql, params object[] parameters);

/// <summary>
/// Query out some data from database.
/// </summary>
/// <param name="formattedSql">a formatted sql which format looks like the parameter of String.Format</param>
/// <param name="closeConnection">Indicates whether to close the corresponding connection when the reader is closed?</param>
/// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
/// <returns></returns>
IDataReader QueryDataReader(string formattedSql, bool closeConnection, params object[] parameters);

/// <summary>
/// Query out a row from database.
/// If there is not any records, return null.
/// </summary>
/// <param name="formattedSql">a formatted sql which format looks like the parameter of String.Format</param>
/// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
/// <returns></returns>
DataRow QueryDataRow(string formattedSql, params object[] parameters);

/// <summary>
/// Query out a DataTable object from database by the specific sql.
/// </summary>
/// <param name="formattedSql">a formatted sql which format looks like the parameter of String.Format</param>
/// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
/// <returns></returns>
DataTable QueryDataTable(string formattedSql, params object[] parameters);

/// <summary>
/// Query out a row from database.
/// If there is not any records, return null.
/// </summary>
/// <param name="formattedSql">a formatted sql which format looks like the parameter of String.Format</param>
/// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
/// <returns></returns>
LiteDataRow QueryLiteDataRow(string formattedSql, params object[] parameters);

/// <summary>
/// Query out a DataTable object from database by the specific sql.
/// </summary>
/// <param name="formattedSql">a formatted sql which format looks like the parameter of String.Format</param>
/// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
/// <returns></returns>
LiteDataTable QueryLiteDataTable(string formattedSql, params object[] parameters);
}
```

该接口使用类似于 String.Format 中的字符串格式来表达 SQL 中的参数。并在连接不同的数据库时，生成相应数据库对应的参数格式。  
具体使用方法如下：  
执行查询代码示例：

```cs
var bookRepo = RF.ResolveInstance<BookRepository>();
using (var dba = DbAccesserFactory.Create(bookRepo))
{
DataTable table = dba.QueryDataTable("SELECT * FROM Books WHERE id > {0}", 0);
}
```

执行非查询代码示例：
```cs
var bookRepo = RF.ResolveInstance<BookRepository>();
using (var dba = DbAccesserFactory.Create(bookRepo))
{
int linesAffected = dba.ExecuteText("DELETE FROM Books WHERE id > {0}", 0);
}
```

另外，DbAccesserFactory 中也提供了不通过仓库对象，而直接使用‘数据库连接的配置名’进行构建的方法，例如：
```cs
using (var dba = DbAccesserFactory.Create("JXC"))
{
int linesAffected = dba.ExecuteText("DELETE FROM Books WHERE id > {0}", 0);
}
```

参数过多时，则依次按顺序传入即可：

```cs
            using (var dba = DbAccesserFactory.Create(bookRepo))
            {
                for (int i = 0; i < 10; i++)
                {
                    dba.ExecuteText(
                    "INSERT INTO Book (Author,BookCategoryId,BookLocId,Code,Content,Name,Price,Publisher) VALUES ({0},{1},{2},{3},{4},{5},{6},{7})",
                    string.Empty,
                    null,
                    null,
                    string.Empty,
                    string.Empty,
                    i.ToString(),
                    null,
                    string.Empty
                    );
                }
            }
```


##IRawDbAccesser 接口
由于 IDbAccesser 接口封装了 SQL 语句中参数对应不同数据库中名称的变化，同时也更方便开发者使用，所以一般情况下，都推荐使用该接口。但是，IDbAccesser 接口并不支持存储过程的调用。另外，有时开发者希望自己来构建原生的 SQL 语句和参数，这时，就需要用到 IRawDbAccesser 接口了。（接口定义过长，这里不再贴出。）  
该接口的使用方法与 IDbAccesser 类似，不同的地方在于 SQL 中需要传入特定数据库的参数名，并且参数需要自行构造，例如：

```cs

            using (var dba = DbAccesserFactory.Create(bookRepo))
            {
                for (int i = 0; i < 10; i++)
                {
                    dba.RawAccesser.ExecuteText(
                        "INSERT INTO Book (Author,BookCategoryId,BookLocId,Code,Content,Name,Price,Publisher,Id) VALUES ('', NULL, NULL, '', '', :p0, NULL, '', :p1)",
                        dba.RawAccesser.ParameterFactory.CreateParameter("p0", i),
                        dba.RawAccesser.ParameterFactory.CreateParameter("p1", i)
                        );
                }
            }
                    
```

另外，接口也可以使用存储过程了，例如：

```cs
            using (var dba = DbAccesserFactory.Create(bookRepo))
            {
                for (int i = 0; i < 10; i++)
                {
                    dba.RawAccesser.ExecuteProcedure(
                        "InsertBookProcedure",
                        dba.RawAccesser.ParameterFactory.CreateParameter("p0", i),
                        dba.RawAccesser.ParameterFactory.CreateParameter("p1", i)
                        );
                }
            }                 
```

