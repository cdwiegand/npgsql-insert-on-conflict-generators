using System.Collections;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using Npgsql.EntityFrameworkCore.PostgreSQL.Update.Internal;
using NpgsqlInsertOnConflictGenerators;
using NpgsqlTypes;

namespace NpgsqlInsertGeneratorTests
{
    [TestClass]
    public class InsertOnConflictDoNothingGeneratorTests
    {
        [TestMethod]
        public void Test()
        {
            var updateDeps = new RelationalSqlGenerationHelperDependencies();
            var mappingSource = new NpgsqlTypeMappingSource(
                new TypeMappingSourceDependencies(
                    new ValueConverterSelector(new ValueConverterSelectorDependencies()),
                    new JsonValueReaderWriterSource(new JsonValueReaderWriterSourceDependencies()),
                    []
                ),
                new RelationalTypeMappingSourceDependencies([]),
                new NpgsqlSqlGenerationHelper(updateDeps),
                new NpgsqlSingletonOptions()
            );
            UpdateSqlGeneratorDependencies updateGen = new UpdateSqlGeneratorDependencies(
                new NpgsqlSqlGenerationHelper(updateDeps), mappingSource);
            InsertOnConflictDoNothingGenerator gen = new(updateGen);

            StringBuilder sb = new();
            var cs = new ConventionSet();
            var model = new Model(cs);
            var relModel = new RelationalModel(model);
            var typeBase = new EntityType(typeof(TestModel), model, true, ConfigurationSource.Convention);
            var table = new Table("test_table", "test_schema", relModel);
            var mapString = new NpgsqlStringTypeMapping("varchar", NpgsqlDbType.Varchar);
            var mapInt = new IntTypeMapping("int");

            // column 0
            var prop0 = new Property("Id", typeof(string), null, null, typeBase, ConfigurationSource.Convention, null);
            var icol0 = new Column("id", "int", table, mapInt);
            table.PrimaryKey = new UniqueConstraint("test_table_pk", table, [icol0]);
            var colModP0 = new ColumnModificationParameters(icol0, null, 1, prop0, mapInt, false, true, true,
                false, true, false);

            // column 1
            var prop1 = new Property("TestCol", typeof(string), null, null, typeBase, ConfigurationSource.Convention, null);
            var icol1 = new Column("test_col", "varchar", table, mapString);
            var colModP1 = new ColumnModificationParameters(icol1, null, "newValue", prop1, mapString, false, true, false,
                false, true, true);

            var comparer0 = new Comparer(CultureInfo.InvariantCulture);
            IComparer<IUpdateEntry> entryComparer = new EntryCurrentValueComparer(prop0, comparer0);
            ModificationCommandParameters pars = new ModificationCommandParameters(table, true, true, entryComparer);
            INonTrackedModificationCommand cmd = new NpgsqlModificationCommand(pars);
            cmd.AddColumnModification(colModP0);
            cmd.AddColumnModification(colModP1);
            gen.AppendInsertOperation(sb, cmd, 0);

            string sql = sb.ToString().Replace("\r\n", " ").Replace("\n", " ").Trim();
            Assert.Contains(" ON CONFLICT ", sql);
            Assert.AreEqual(
                "INSERT INTO test_schema.test_table (id, test_col) VALUES (1, 'newValue') ON CONFLICT DO NOTHING;",
                sql);
        }
    }
}
