using DbUp;
using DbUp.Helpers;
using DbUp.Postgresql;
namespace Demo.Database;


public interface IMigration
{
	int Run();
}

public static class Migration
{
	public static IMigration Build(Configuration config)
	{
		return config.Database.Type switch
		{
			DatabaseServerType.Postgres => new SqlMigration(config),
			DatabaseServerType.SqlServer => new PostgresMigration(config),
			_ => throw new Exception("Invalid Database Server Type")
		};
	}
}

public class SqlMigration : IMigration
{
	private readonly Configuration _configuration;

	public SqlMigration(Configuration configuration)
	{
		_configuration = configuration;
	}
	public int Run()
	{
		Output.Display("\r\nDeploying Sql Server Changes...");
		try
		{
			Output.Display("Ensuring Database Exists....");
			EnsureDatabase.For
				.SqlDatabase(_configuration.Database.ConnectionString);
			Output.Display("\r\nDatabaseExists.");

			if (_configuration.RunMigrations)
			{
				Output.Display("Running Migration Scripts....");
				var migrationEngine = DeployChanges.To
					.SqlDatabase(_configuration.Database.ConnectionString)
					.WithScriptsFromFileSystem("../../../../scripts/sqlserver/migrations/")
					.LogToConsole()
					.Build();

				var migrationResult = migrationEngine.PerformUpgrade();
				if (!migrationResult.Successful)
				{
					Output.Display($"{migrationResult.Error}");
				}
				
				Output.Display("Migration Scripts Successfully Deployed....");
			}
			
			if (_configuration.RunIdempotent)
			{
				Output.Display("Running Idempotent Scripts....");
				var migrationEngine = DeployChanges.To
					.SqlDatabase(_configuration.Database.ConnectionString)
					.WithScriptsFromFileSystem("../../../../scripts/sqlserver/idempotent/")
					.LogToConsole()
					.JournalTo(new NullJournal())
					.Build();

				var migrationResult = migrationEngine.PerformUpgrade();
				if (!migrationResult.Successful)
				{
					Output.Display($"{migrationResult.Error}");
				}
				
				Output.Display("Idempotent Scripts Successfully Deployed....");
			}
			
			if (_configuration.RunDataLoad)
			{
				Output.Display("Running Dataload Scripts....");
				var migrationEngine = DeployChanges.To
					.SqlDatabase(_configuration.Database.ConnectionString)
					.WithScriptsFromFileSystem("../../../../scripts/sqlserver/dataload/")
					.LogToConsole()
					.JournalTo(new NullJournal())
					.Build();

				var migrationResult = migrationEngine.PerformUpgrade();
				if (!migrationResult.Successful)
				{
					Output.Display($"{migrationResult.Error}");
				}
				
				Output.Display("Dataload Scripts Successfully Deployed....");
			}

			Output.Display("Deployment to Sql Server Complete.");
			return 0;
		}
		catch (Exception e)
		{
			Output.Display($"Fatal Exception running Migration: {e}");
			return -1;
		}
	}
}

public class PostgresMigration : IMigration
{
	private readonly Configuration _configuration;

	public PostgresMigration(Configuration configuration)
	{
		_configuration = configuration;
	}
	public int Run()
	{
		Output.Display("\r\nDeploying Postgres Changes...");
		try
		{
			Output.Display("Ensuring Database Exists....");
			EnsureDatabase.For
				.PostgresqlDatabase(_configuration.Database.ConnectionString);
			Output.Display("\r\nDatabaseExists.");

			if (_configuration.RunMigrations)
			{
				Output.Display("Running Migration Scripts....");
				var migrationEngine = DeployChanges.To
					.PostgresqlDatabase(_configuration.Database.ConnectionString)
					.WithScriptsFromFileSystem("../../../../scripts/postgres/migrations/")
					.LogToConsole()
					.Build();

				var migrationResult = migrationEngine.PerformUpgrade();
				if (!migrationResult.Successful)
				{
					Output.Display($"{migrationResult.Error}");
				}
				
				Output.Display("Migration Scripts Successfully Deployed....");
			}
			
			if (_configuration.RunIdempotent)
			{
				Output.Display("Running Idempotent Scripts....");
				var migrationEngine = DeployChanges.To
					.PostgresqlDatabase(_configuration.Database.ConnectionString)
					.WithScriptsFromFileSystem("../../../../scripts/postgres/idempotent/")
					.LogToConsole()
					.JournalTo(new NullJournal())
					.Build();

				var migrationResult = migrationEngine.PerformUpgrade();
				if (!migrationResult.Successful)
				{
					Output.Display($"{migrationResult.Error}");
				}
				
				Output.Display("Idempotent Scripts Successfully Deployed....");
			}
			
			if (_configuration.RunDataLoad)
			{
				Output.Display("Running Dataload Scripts....");
				var migrationEngine = DeployChanges.To
					.PostgresqlDatabase(_configuration.Database.ConnectionString)
					.WithScriptsFromFileSystem("../../../../scripts/postgres/dataload/")
					.LogToConsole()
					.JournalTo(new NullJournal())
					.Build();

				var migrationResult = migrationEngine.PerformUpgrade();
				if (!migrationResult.Successful)
				{
					Output.Display($"{migrationResult.Error}");
				}
				
				Output.Display("Dataload Scripts Successfully Deployed....");
			}

			Output.Display("Deployment to Postgres Complete.");
			return 0;
		}
		catch (Exception e)
		{
			Output.Display($"Fatal Exception running Migration: {e}");
			return -1;
		}
	}
}
