using Microsoft.Extensions.Configuration;

namespace Demo.Database;

public static class MigrationOptions
{
	public static readonly IReadOnlyDictionary<string, string> ValidOptions = new Dictionary<string, string>
	{
		{"-h", "Show command line help."},
		{"--help", "Show command line help."},
		{"-?", "Show command line help."},
		{"--sql", "Run the migrations against Sql Server."},
		{"--psql", "Run the migrations against PostgreSQL."},
		{"--migrate", "Run migration scripts."},
		{"--idempotent", "Run idempotent scripts."},
		{"--dataload", "Run data load scripts."},
		{"--all", "Run all scripts (migrate, idempotent, dataload)."},
		{"--drop", "Drop Demo Database."}
	};

	public static (Configuration? Configuration, List<string> Errors) BuildConfiguration(string[] arguments, IConfigurationRoot config)
	{
		var help = false;
		var sql = false;
		var psql = false;
		var migrate = false;
		var idempotent = false;
		var dataload = false;
		var drop = false;
		var dbType = DatabaseServerType.SqlServer;
		var connectionString = "";
		var errors = new List<string>();

		var argCount = arguments.Length;
		if (argCount <= 1)
		{
			// Argument 0 is the Executing App when using Top Level Statements
			help = true;
		}

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
					dbType = DatabaseServerType.Postgres;
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
				case "--drop":
					drop = true;
					break;
				default:
					errors.Add($"{arguments[index]} is not a valid option.");
					break;
			}
		}
		if (!help)
		{
			switch (sql, psql)
			{
				case (true, true):
					errors.Add("Only 1 Database type can be migrated at at time.  Choose --sql or --psql.");
					connectionString = "<NotAbleToSet>";
					break;
				case (false, false):
					errors.Add("A Database type is required.  Choose --sql or --psql.");
					connectionString = "<NotAbleToSet>";
					break;
				case (true, false):
					connectionString = config.GetConnectionString("SqlServer");
					if (string.IsNullOrEmpty(connectionString))
						errors.Add("ConnectionStrings:SqlServer missing, please add to User Secrets.");
					break;
				case (false, true):
					connectionString = config.GetConnectionString("PostgreSQL");
					if (string.IsNullOrEmpty(connectionString))
						errors.Add("ConnectionStrings:PostgreSQL missing, please add to User Secrets.");
					break;
			}
		}

		if (drop && (migrate || idempotent || dataload))
		{
			errors.Add("--drop can not be run with other options.");
		}

		if (errors.Any())
		{
			return (null, errors);
		}

		var database = new Database(dbType, connectionString);
		var configuration = new Configuration(database, help, migrate, idempotent, dataload, drop);

		return (configuration, errors); // errors will be empty list
	}
}


