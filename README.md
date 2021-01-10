# DeltaX.LinSql

## Description
This repo is simple **SQL** query generator using **Poco** class. The Poco can be configured using Attributes or not.

Use `TableQueryFactory` for configure all **Poco** class.

Use `QueryBuilder` for easy queries using Lambda Expression.

 
## Example configure Poco Class to SQL table

This sample configure columns:
 - Id as identity / primary key field
 - FullName as read/write field (called UserFullName on sql table) 
 - Update as read only field
 - Active as read/write field
```
var queryFactory = TableQueryFactory.GetInstance(); 
if (!queryFactory.IsConfiguredTable<User>())
{
    queryFactory.ConfigureTable<User>("user", cfg =>
    {
        cfg.AddColumn(c => c.Id, null, true, true);
        cfg.AddColumn(c => c.FullName, "UserFullName");
        cfg.AddColumn(c => c.Updated, p => { p.IgnoreInsert = true; p.IgnoreUpdate = true; });
        cfg.AddColumn(c => c.Active); 
    });
}
```

## Examples of `DeltaX.LinSql.Table`

These examples are usefull on Repository patter, see: Example: `DapperRepositoryBase.cs` for more information.

### Insert Entity and read identity primary key

```
public Task<Tkey> InsertAsync<TEntity, Tkey>(TEntity item, IEnumerable<string> fieldsToInsert = null)
    where TEntity : class
{
    var query = queryFactory.GetInsertQuery<TEntity>(fieldsToInsert);
    query += "; " + queryFactory.DialectQuery.IdentityQueryFormatSql;
    return db.ExecuteScalarAsync<Tkey>(query, item);
}
```

### Delete Entity
```
public Task DeleteAsync<TEntity>(TEntity entity)
    where TEntity : class
{
    var query = queryFactory.GetDeleteQuery<TEntity>();
    return db.ExecuteAsync(query, entity);
}
```

### Get Entity
```
public Task<TEntity> GetAsync<TEntity>(string whereClause, object param)
            where TEntity : class
{
    var query = queryFactory.GetSingleQuery<TEntity>(whereClause); 
    return db.QueryFirstOrDefaultAsync<TEntity>(query, param);
}
```

### Get GetPagedList Entity
```
var query = queryFactory.GetPagedListQuery<TEntity>(skipCount, rowsPerPage, whereClause, orderByClause); 
return db.QueryAsync<TEntity>(query, param);
```

### Update Entity
```
var query = queryFactory.GetUpdateQuery<TEntity>(null, fieldsToSet);
return db.ExecuteAsync(query, entity);

```

**NOTE:** please, see `DeltaX.LinSql.Table.UniTest` project for all uses case.


## Examples of `DeltaX.LinSql.Query`

### Update (Set specific field) with Join
```
IDbConnection db = ... 
(var sql, var param) = new QueryBuilder<Poco>()
    .Join<Poco2>((t1, t2) => t1.Id == t2.Id)
    .Where((t1, t2) => t1.Active && t2.Active && t2.Id == 123)
    .Set((t1) => t1.Name, "Nada")
    .GetSqlParameters();

int affectedRows = db.ExecuteAsync(sql, param).Result; 

```

### Update Entity 
```
IDbConnection db = ... 
var poco = new Poco { Id = 1, Active = true, Name = "Alfredo" };
(var sql, var param) = new QueryBuilder<Poco>()
    .Update(poco)
    .GetSqlParameters();

int affectedRows = db.ExecuteAsync(sql, param).Result;

```

### Select all fields and all rows
```
IDbConnection db = ... 
(var sql, var param) = new QueryBuilder<Poco>()
    .Where(t => t.Active)
    .SelectAll()
    .GetSqlParameters();

IEnumerable<Poco> result = db.QueryAsync<Poco>(sql, param).Result;

```


**NOTE:** please, see `DeltaX.LinSql.Query.UniTest` project for all uses case.
 

## Install 

- Go to https://www.nuget.org/packages/DeltaX_LinSql_Table/ and https://www.nuget.org/packages/DeltaX_LinSql_Query/ 
for install via nuget.
 
- Dowlnload last release [v1.0.2]


;)
