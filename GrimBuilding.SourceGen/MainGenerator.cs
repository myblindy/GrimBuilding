using Microsoft.CodeAnalysis;
using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GrimBuilding.SourceGen
{
    [Generator]
    public class MainGenerator : ISourceGenerator
    {
        DataTable Query(string dbPath, FormattableString query, params object[] parameters)
        {
            using var conn = new SqliteConnection($"Data Source={dbPath}");
            conn.Open();

            var paramIdx = 0;
            using var cmd = new SqliteCommand(Regex.Replace(query.Format, @"\{[^}]+\}", e => $"@p{paramIdx++}"), conn);
            cmd.Parameters.AddRange(parameters.Select((val, idx) => new SqliteParameter($"@p{idx}", val)));

            using var reader = cmd.ExecuteReader();
            var dt = new DataTable();
            dt.Load(reader);
            return dt;
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var dbPath = context.AdditionalFiles.FirstOrDefault(w => Path.GetFileName(w.Path).Equals("data.db", StringComparison.OrdinalIgnoreCase))?.Path;
            if (dbPath is null) return;

            var sb = new StringBuilder();

            sb.AppendLine(@"
namespace GrimBuilding.Common.Support {
public abstract class BasePlayerSkill
{
    public abstract string Name { get; }
    public abstract string Description { get; }
}");

            using (var dt = Query(dbPath, $"select name, description from playerskills"))
            {
                foreach (DataRow row in dt.Rows)
                    sb.AppendLine($@"
public class {row["name"]}PlayerSkill : BasePlayerSkill
{{
    public override string Name => ""{row["name"]}"";
    public override string Description => ""{row["description"]}"";
}}");
            }

            sb.Append("}");
            context.AddSource("Generated.cs", sb.ToString());
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            SQLitePCL.Batteries.Init();
        }
    }
}
