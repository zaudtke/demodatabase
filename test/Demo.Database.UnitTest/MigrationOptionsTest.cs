using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Demo.Database.UnitTest;
public class MigrationOptionsTest
{
	[Theory(DisplayName = "MigrationOptions.BuildConfiguration Returns Error List correctly based on invalid arguments")]
	[InlineData(true, new string[] { "app.dll", "--sql", "--psql" }, new string[] { "Only 1 Database type can be migrated at at time.  Choose --sql or --psql." })]
	[InlineData(true, new string[] { "app.dll", "--migrate" }, new string[] { "A Database type is required.  Choose --sql or --psql." })]
	[InlineData(false, new string[] { "app.dll", "--sql" }, new string[] { "ConnectionStrings:SqlServer missing, please add to User Secrets." })]
	[InlineData(false, new string[] { "app.dll", "--psql" }, new string[] { "ConnectionStrings:PostgreSQL missing, please add to User Secrets." })]
	[InlineData(true, new string[] { "app.dll", "--psql", "--migrate", "--invalid", "--teardown" }, new string[] { "--invalid is not a valid option.", "--teardown is not a valid option." })]
	[InlineData(true, new string[] { "app.dll", "--psql", "--drop", "--migrate" }, new string[] { "--drop can not be run with other options." })]
	public void MigrationOptions_BuildConfiguration_Returns_ListOfErrors_When_Invalid_Arguments_Passed_In(bool includeConnections, string[] args, string[] expected)
	{
		var configRoot = GivenConfigurationRoot(includeConnections, includeConnections);
		var (config, errors) = MigrationOptions.BuildConfiguration(args, configRoot);

		Assert.Null(config);
		Assert.NotNull(errors);
		Assert.Equal(expected.ToList(), errors);
	}

	[Theory(DisplayName = "MigrationOptions.BuildConfiguration Returns valid configuration with Help = True when valid Help option passed in.")]
	[InlineData("")]
	[InlineData("executing/path/app.dll")]
	[InlineData("executing/path/app.dll", "-?")]
	[InlineData("executing/path/app.dll", "-h")]
	[InlineData("executing/path/app.dll", "-H")]
	[InlineData("executing/path/app.dll", "--help")]
	[InlineData("executing/path/app.dll", "--Help")]
	[InlineData("executing/path/app.dll", "--HELP")]
	[InlineData("executing/path/app.dll", "--heLP")]
	[InlineData("executing/path/app.dll", "--help", "--migrate")]
	[InlineData("executing/path/app.dll", "--migrate", "--help")]
	public void MigrationOptions_BuildConfiguration_Returns_ValidConfiguration_With_Help_True_When_Valid_HelpOption(params string[] args)
	{
		var configRoot = GivenConfigurationRoot(false, false);
		var (config, errors) = MigrationOptions.BuildConfiguration(args, configRoot);

		Assert.NotNull(config);
		Assert.True(config!.Help); // ! means don't warn me about possible null.
		Assert.NotNull(errors);
		Assert.Empty(errors);
	}

	[Theory(DisplayName = "MigrationOptions.BuildConfiguration Returns valid configuration")]
	[InlineData(new string[] { "app.dll", "--psql" }, 0, "PostgreSQLConnectionString", false, false, false, false)]
	[InlineData(new string[] { "app.dll", "--psql", "--migrate" }, 0, "PostgreSQLConnectionString", true, false, false, false)]
	[InlineData(new string[] { "app.dll", "--psql", "--idempotent" }, 0, "PostgreSQLConnectionString", false, true, false, false)]
	[InlineData(new string[] { "app.dll", "--psql", "--dataload" }, 0, "PostgreSQLConnectionString", false, false, true, false)]
	[InlineData(new string[] { "app.dll", "--psql", "--migrate", "--idempotent" }, 0, "PostgreSQLConnectionString", true, true, false, false)]
	[InlineData(new string[] { "app.dll", "--psql", "--migrate", "--dataload" }, 0, "PostgreSQLConnectionString", true, false, true, false)]
	[InlineData(new string[] { "app.dll", "--psql", "--idempotent", "--dataload" }, 0, "PostgreSQLConnectionString", false, true, true, false)]
	[InlineData(new string[] { "app.dll", "--psql", "--migrate", "--idempotent", "--dataload" }, 0, "PostgreSQLConnectionString", true, true, true, false)]
	[InlineData(new string[] { "app.dll", "--psql", "--all" }, 0, "PostgreSQLConnectionString", true, true, true, false)]
	[InlineData(new string[] { "app.dll", "--psql", "--drop" }, 0, "PostgreSQLConnectionString", false, false, false, true)]
	[InlineData(new string[] { "app.dll", "--sql" }, 1, "SqlConnectionString", false, false, false, false)]
	[InlineData(new string[] { "app.dll", "--sql", "--migrate" }, 1, "SqlConnectionString", true, false, false, false)]
	[InlineData(new string[] { "app.dll", "--sql", "--idempotent" }, 1, "SqlConnectionString", false, true, false, false)]
	[InlineData(new string[] { "app.dll", "--sql", "--dataload" }, 1, "SqlConnectionString", false, false, true, false)]
	[InlineData(new string[] { "app.dll", "--sql", "--migrate", "--idempotent" }, 1, "SqlConnectionString", true, true, false, false)]
	[InlineData(new string[] { "app.dll", "--sql", "--migrate", "--dataload" }, 1, "SqlConnectionString", true, false, true, false)]
	[InlineData(new string[] { "app.dll", "--sql", "--idempotent", "--dataload" }, 1, "SqlConnectionString", false, true, true, false)]
	[InlineData(new string[] { "app.dll", "--sql", "--migrate", "--idempotent", "--dataload" }, 1, "SqlConnectionString", true, true, true, false)]
	[InlineData(new string[] { "app.dll", "--sql", "--all" }, 1, "SqlConnectionString", true, true, true, false)]
	[InlineData(new string[] { "app.dll", "--sql", "--drop" }, 1, "SqlConnectionString", false, false, false, true)]
	public void MigrationOptions_BuildConfiguration_Succeeds(string[] args, int expectedDbType, string expectedConnectionString, bool expectedRunMigrations, bool expectedRunIdempotent, bool expectedRunDataload, bool expectedDropDatabase)
	{
		var configRoot = GivenConfigurationRoot(true, true);
		var expectedConfig = GivenConfiguration(expectedDbType, expectedConnectionString, expectedRunMigrations,
			expectedRunIdempotent, expectedRunDataload, expectedDropDatabase);
		var (config, errors) = MigrationOptions.BuildConfiguration(args, configRoot);

		Assert.NotNull(errors);
		Assert.Empty(errors);

		Assert.NotNull(config);
		Assert.Equal(expectedConfig.Database.Type, config!.Database.Type);
		Assert.Equal(expectedConfig.Database.ConnectionString, config!.Database.ConnectionString);
		Assert.Equal(expectedConfig.Help, config!.Help);
		Assert.Equal(expectedConfig.DropDatabase, config!.DropDatabase);
		Assert.Equal(expectedConfig.RunIdempotent, config!.RunIdempotent);
		Assert.Equal(expectedConfig.RunMigrations, config!.RunMigrations);
		Assert.Equal(expectedConfig.RunDataLoad, config!.RunDataLoad);

	}


	private static IConfigurationRoot GivenConfigurationRoot(bool includeSqlConnection, bool includePostgresConnection)
	{
		var connectionStrings = new Dictionary<string, string>();

		if (includeSqlConnection)
			connectionStrings.Add("ConnectionStrings:SqlServer", "SqlConnectionString");

		if (includePostgresConnection)
			connectionStrings.Add("ConnectionStrings:PostgreSQL", "PostgreSQLConnectionString");

		return new ConfigurationBuilder()
			.AddInMemoryCollection(connectionStrings)
			.Build();
	}

	private static Configuration GivenConfiguration(int dbType, string connectionString, bool runMigrations, bool runIdempotent, bool runDataload, bool dropDatabase)
	{
		var database = new Database((DatabaseServerType)dbType, connectionString);
		return new Configuration(database, false, runMigrations, runIdempotent, runDataload, dropDatabase);
	}
}
