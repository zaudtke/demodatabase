using DbUp;
using DbUp.Helpers;

namespace Demo.Database;

public class PostgresDeployment : IDeployment
{
	private readonly Configuration _configuration;

	public PostgresDeployment(Configuration configuration)
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
				Output.Display("Running Deployment Scripts....");
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
				
				Output.Display("Deployment Scripts Successfully Deployed....");
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
			Output.Display($"Fatal Exception running Deployment: {e}");
			return -1;
		}
	}
}
