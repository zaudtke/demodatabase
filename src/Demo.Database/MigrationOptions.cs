using Microsoft.Extensions.Configuration;

namespace Demo.Database;

public static class MigrationOptions
{
	public static IReadOnlyDictionary<string, string> ValidOptions = new Dictionary<string, string>
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

	public static (Configuration Configuration, List<string> Errors) BuildConfiguration(string[] arguments, IConfigurationRoot config)
	{
		bool help = false;
		bool sql = false;
		bool psql = false;
		bool migrate = false;
		bool idempotent = false;
		bool dataload = false;
		DatabaseServerType dbType = DatabaseServerType.SqlServer;
		string connectionString = "";
		List<string> errors = new();

		for (var index = 1; index < arguments.Length; index++)
		{
			switch (arguments[index].ToLower())
			{
				case "-h":
				case "-?":
				case "--help":
					help = true;
					break;
				case "--sql":
					sql = true;
					dbType = DatabaseServerType.SqlServer;
					break;
				case "--psql":
					psql = true;
					dbType = DatabaseServerType.PostgreSQL;
					break;
				case "--migrate":
					migrate = true;
					break;
				case "--idempotent":
					idempotent = true;
					break;
				case "--dataload":
					dataload = true;
					break;
				case "--all":
					migrate = true;
					idempotent = true;
					dataload = true;
					break;
				default:
					errors.Add($"{arguments[index]} is not a valid option.");
					break;
			}
		}
		if (!help)
		{
			switch ((sql, psql))
			{
				case (true, true):
					errors.Add("Only 1 Database type can be migrated at at time.  Choose --sql or --psql");
					break;
				case (false, false):
					errors.Add("A Database type is required.  Choose --sql or --psql");
					break;
				case (true, false):
					connectionString = config.GetConnectionString("SqlServer");
					break;
				case (false, true):
					connectionString = config.GetConnectionString("PostgreSQL`");
					break;
			}
			if (string.IsNullOrEmpty(connectionString))
			{
				errors.Add("ConnectionString missing, please add to User Secrets.");
			}
		}
		var database = new Database(dbType, connectionString);
		var configuration = new Configuration(database, help, migrate, idempotent, dataload);
		return (configuration, errors);

	}
}


