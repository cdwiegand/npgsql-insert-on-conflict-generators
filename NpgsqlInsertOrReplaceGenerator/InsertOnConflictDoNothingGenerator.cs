#pragma warning disable EF1001 // Internal EF Core API usage.
using System.Text;
using Microsoft.EntityFrameworkCore.Update;
using Npgsql.EntityFrameworkCore.PostgreSQL.Update.Internal;

namespace NpgsqlInsertOnConflictGenerators;

public class InsertOnConflictDoNothingGenerator : NpgsqlUpdateSqlGenerator, IUpdateSqlGenerator
{
    public InsertOnConflictDoNothingGenerator(UpdateSqlGeneratorDependencies dependencies) : base(dependencies)
    {
    }

    protected override void AppendInsertCommand(StringBuilder commandStringBuilder, string name, string? schema, IReadOnlyList<IColumnModification> writeOperations,
        IReadOnlyList<IColumnModification> readOperations, bool overridingSystemValue)
    {
        AppendInsertCommandHeader(commandStringBuilder, name, schema, writeOperations);

        if (overridingSystemValue)
        {
            commandStringBuilder.AppendLine().Append("OVERRIDING SYSTEM VALUE");
        }

        AppendValuesHeader(commandStringBuilder, writeOperations);
        AppendValues(commandStringBuilder, name, schema, writeOperations);

        // start of ON CONFLICT clause
        commandStringBuilder.AppendLine().Append("ON CONFLICT DO NOTHING");
        // end of ON CONFLICT clause

        AppendReturningClause(commandStringBuilder, readOperations);
        commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);
    }
}