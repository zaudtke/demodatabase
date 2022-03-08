

using System.Collections.Generic;

using System.Linq;
namespace Demo.Database;
public static class ArgumentList
{
	private static Dictionary<string,string> _validArguments = new Dictionary<string, string>
	{
		{"--h", "Display available commands"},
		{"--help", "Display available commands"},
		{"--sql", "Run the migrations against Sql Seerver"},
		{"--psql", "Run the migrations against PostgreSQL"}
	};

	public static IReadOnlyDictionary<string,string> Arguments => _validArguments;

	public static bool IsHelpCommand(string[] args)
	{
		if (args.Any(a => a.Equals("--h", StringComparison.InvariantCultureIgnoreCase)
			|| a.Equals("--help", StringComparison.InvariantCultureIgnoreCase)))
		{
			return true;
		}
		
		return false;
	}
}

public class MigrationCommand
{
	private List<string> _errors = new ();

	public MigrationCommand(DatabaseServerType type)
	{
		DatabaseServerType = type;
	}

	public DatabaseServerType DatabaseServerType {get; private set;}
	public bool IsValid => _errors.Any();
	public IEnumerable<string> Errors => _errors;
}

public enum DatabaseServerType
{
	SqlServer = 0,
	PostgreSQL = 1
}
