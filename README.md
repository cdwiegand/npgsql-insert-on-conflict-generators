# Reason

This package exists to solve a problem I saw where Entity Framework doesn't have the concept of an UPSERT. While normal use cases would have you pull and explicitly push (either as new or existing) entities, you may have a need to just "push" an entity, letting the database server do the upsert itself, or skip the new record if it already exists, without the extra query to determine prior existence.

**Note that using either class (`InsertOnConflictDoNothingGenerator` or `InsertOnKeysConflictReplaceGenerator`) applies to the entire DbContext, as there is no way to indicate when calling `AddAsync` that you want to perform an upsert call.**

## Insert replacing based on the same key on an existing record

``` C#
services.AddDbContext<PitStorageEfDbContext>(optionsBuilder => {
    optionsBuilder
        .UseNpgsql(conn)
        .ReplaceService<IUpdateSqlGenerator, InsertOnKeysConflictReplaceGenerator>();
        // Add/AddAsync will now replace existing values based on using the keys defined on the model
});
```

## Insert skipping if record already exists by key

``` C#
services.AddDbContext<PitStorageEfDbContext>(optionsBuilder => {
    optionsBuilder
        .UseNpgsql(conn)
        .ReplaceService<IUpdateSqlGenerator, InsertOnConflictDoNothingGenerator>();
        // Add/AddAsync will now skip an INSERT if the record already exists using the keys defined on the model
});
```
