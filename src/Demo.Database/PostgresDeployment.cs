using DbUp;
using DbUp.Helpers;

namespace Demo.Database;

public class PostgresDeployment : IDeployment
{
	private readonly DeploymentConfiguration _deploymentConfiguration;

	public PostgresDeployment(DeploymentConfiguration deploymentConfiguration)
	{
		_deploymentConfiguration = deploymentConfiguration;
	}
	public int Run()
	{
		Output.Display("\r\nDeploying Postgres Changes...");
		try
		{
			Output.Display("Ensuring Database Exists....");
			EnsureDatabase.For
				.PostgresqlDatabase(_deploymentConfiguration.Database.ConnectionString);
			Output.Display("\r\nDatabaseExists.");

			if (_deploymentConfiguration.RunMigrations)
			{
				Output.Display("Running Deployment Scripts....");
				var migrationEngine = DeployChanges.To
					.PostgresqlDatabase(_deploymentConfiguration.Database.ConnectionString)
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

			if (_deploymentConfiguration.RunIdempotent)
			{
				Output.Display("Running Idempotent Scripts....");
				var migrationEngine = DeployChanges.To
					.PostgresqlDatabase(_deploymentConfiguration.Database.ConnectionString)
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

			if (_deploymentConfiguration.RunDataLoad)
			{
				Output.Display("Running Dataload Scripts....");
				var migrationEngine = DeployChanges.To
					.PostgresqlDatabase(_deploymentConfiguration.Database.ConnectionString)
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
