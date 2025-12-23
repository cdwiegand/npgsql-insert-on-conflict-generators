#pragma warning disable EF1001 // Internal EF Core API usage.
using System.Text;
using Microsoft.EntityFrameworkCore.Update;
using Npgsql.EntityFrameworkCore.PostgreSQL.Update.Internal;

namespace NpgsqlInsertOnConflictGenerators;

public class InsertOnKeysConflictReplaceGenerator : NpgsqlUpdateSqlGenerator, IUpdateSqlGenerator
{
  public InsertOnKeysConflictReplaceGenerator(UpdateSqlGeneratorDependencies dependencies) : base(dependencies)
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
        commandStringBuilder.AppendLine().Append("ON CONFLICT (");
        bool isFirstKey = true;
        foreach (var key in writeOperations.Where(p => p.IsKey))
        {
            if (isFirstKey)
            {
                isFirstKey = false;
            }
            else
            {
                commandStringBuilder.Append(", ");
            }
            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, key.ColumnName);
        }
        commandStringBuilder.Append(") DO UPDATE SET ");

        bool isFirstField = true;
        foreach (var field in writeOperations.Where(p => !p.IsKey))
        {
            if (isFirstField)
            {
                isFirstField = false;
            }
            else
            {
                commandStringBuilder.Append(", ");
            }

            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, field.ColumnName);
            commandStringBuilder.Append(" = EXCLUDED.");
            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, field.ColumnName);
        }
        // end of ON CONFLICT clause

        AppendReturningClause(commandStringBuilder, readOperations);
        commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);
    }
}