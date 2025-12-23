# Usage

```
services.AddDbContext<PitStorageEfDbContext>(optionsBuilder => {
    optionsBuilder
        .UseNpgsql(conn)
        .ReplaceService<IUpdateSqlGenerator, InsertOnKeysConflictReplaceGenerator>();
});
```