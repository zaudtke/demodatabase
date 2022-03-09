namespace Demo.Database;

public static class MigrationOptions
{
	private static Dictionary<string, string> _validOptions = new Dictionary<string, string>
	{
		{"-h", "Show command line help."},
		{"--help", "Show command line help."},
		{"-?", "Show command line help."},
		{"--sql", "Run the migrations against Sql Seerver."},
		{"--psql", "Run the migrations against PostgreSQL."},
		{"--migrate", "Run migratoin scripts."},
		{"--idempotent", "Run idempotent scripts."},
		{"--dataload", "Run data load scripts."},
		{"--all", "Run all scripts (migrate, idempotent, dataload)."}
	};

	public static void DisplayHelp()
	{
		// Flip Key and Value to group aliases together based on Value aka Description
		var output = _validOptions.GroupBy(x => x.Value)
			.ToDictionary(x => x.Key, x => x.Select(i => i.Key).ToList());

		Console.WriteLine("\r\nDescripton:\r\n   Run Scripts to update Demo Database.\r\n\r\nOptions:");
		foreach (var kvp in output)
		{
			Console.WriteLine($"{kvp.Value.Aggregate((x, y) => $"{x}, {y}"),-20}{kvp.Key}");
		}
	}

	public static bool IsHelpOption(string[] args)
	{
		if (args.Any(a => a.Equals("-h", StringComparison.InvariantCultureIgnoreCase)
			|| a.Equals("--help", StringComparison.InvariantCultureIgnoreCase)
			|| a.Equals("-?")))
		{
			return true;
		}

		return false;
	}
}


