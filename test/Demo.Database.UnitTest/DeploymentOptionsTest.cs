﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Demo.Database.UnitTest;
public class DeploymentOptionsTest
{
	[Theory(DisplayName = "DeploymentOptions.BuildConfiguration Returns Error List correctly based on invalid arguments")]
	[InlineData(true, new string[] { "app.dll", "--sql", "--psql" }, new string[] { "Only 1 Database type can be deployed at at time.  Choose --sql or --psql." })]
	[InlineData(true, new string[] { "app.dll", "--migrate" }, new string[] { "A Database type is required.  Choose --sql or --psql." })]
	[InlineData(false, new string[] { "app.dll", "--sql" }, new string[] { "ConnectionStrings:SqlServer missing, please add to User Secrets." })]
	[InlineData(false, new string[] { "app.dll", "--psql" }, new string[] { "ConnectionStrings:Postgres missing, please add to User Secrets." })]
	[InlineData(true, new string[] { "app.dll", "--psql", "--migrate", "--invalid", "--teardown" }, new string[] { "--invalid is not a valid option.", "--teardown is not a valid option." })]
	public void DeploymentOptions_BuildConfiguration_Returns_ListOfErrors_When_Invalid_Arguments_Passed_In(bool includeConnections, string[] args, string[] expected)
	{
		var configRoot = GivenConfigurationRoot(includeConnections, includeConnections);
		var (config, errors) = DeploymentOptions.BuildConfiguration(args, configRoot);

		Assert.Null(config);
		Assert.NotNull(errors);
		Assert.Equal(expected.ToList(), errors);
	}

	[Theory(DisplayName = "DeploymentOptions.BuildConfiguration Returns valid configuration with Help = True when valid Help option passed in.")]
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
	public void DeploymentOptions_BuildConfiguration_Returns_ValidConfiguration_With_Help_True_When_Valid_HelpOption(params string[] args)
	{
		var configRoot = GivenConfigurationRoot(false, false);
		var (config, errors) = DeploymentOptions.BuildConfiguration(args, configRoot);

		Assert.NotNull(config);
		Assert.True(config!.Help); // ! means don't warn me about possible null.
		Assert.NotNull(errors);
		Assert.Empty(errors);
	}

	[Theory(DisplayName = "DeploymentOptions.BuildConfiguration Returns valid configuration")]
	[InlineData(new string[] { "app.dll", "--psql" }, 0, "PostgresConnectionString", false, false, false)]
	[InlineData(new string[] { "app.dll", "--psql", "--migrate" }, 0, "PostgresConnectionString", true, false, false)]
	[InlineData(new string[] { "app.dll", "--psql", "--idempotent" }, 0, "PostgresConnectionString", false, true, false)]
	[InlineData(new string[] { "app.dll", "--psql", "--dataload" }, 0, "PostgresConnectionString", false, false, true)]
	[InlineData(new string[] { "app.dll", "--psql", "--migrate", "--idempotent" }, 0, "PostgresConnectionString", true, true, false)]
	[InlineData(new string[] { "app.dll", "--psql", "--migrate", "--dataload" }, 0, "PostgresConnectionString", true, false, true)]
	[InlineData(new string[] { "app.dll", "--psql", "--idempotent", "--dataload" }, 0, "PostgresConnectionString", false, true, true)]
	[InlineData(new string[] { "app.dll", "--psql", "--migrate", "--idempotent", "--dataload" }, 0, "PostgresConnectionString", true, true, true)]
	[InlineData(new string[] { "app.dll", "--psql", "--all" }, 0, "PostgresConnectionString", true, true, true)]
	[InlineData(new string[] { "app.dll", "--sql" }, 1, "SqlConnectionString", false, false, false)]
	[InlineData(new string[] { "app.dll", "--sql", "--migrate" }, 1, "SqlConnectionString", true, false, false)]
	[InlineData(new string[] { "app.dll", "--sql", "--idempotent" }, 1, "SqlConnectionString", false, true, false)]
	[InlineData(new string[] { "app.dll", "--sql", "--dataload" }, 1, "SqlConnectionString", false, false, true)]
	[InlineData(new string[] { "app.dll", "--sql", "--migrate", "--idempotent" }, 1, "SqlConnectionString", true, true, false)]
	[InlineData(new string[] { "app.dll", "--sql", "--migrate", "--dataload" }, 1, "SqlConnectionString", true, false, true)]
	[InlineData(new string[] { "app.dll", "--sql", "--idempotent", "--dataload" }, 1, "SqlConnectionString", false, true, true)]
	[InlineData(new string[] { "app.dll", "--sql", "--migrate", "--idempotent", "--dataload" }, 1, "SqlConnectionString", true, true, true)]
	[InlineData(new string[] { "app.dll", "--sql", "--all" }, 1, "SqlConnectionString", true, true, true)]
	public void DeploymentOptions_BuildConfiguration_Succeeds(string[] args, int expectedDbType, string expectedConnectionString, bool expectedRunMigrations, bool expectedRunIdempotent, bool expectedRunDataload)
	{
		var configRoot = GivenConfigurationRoot(true, true);
		var expectedConfig = GivenConfiguration(expectedDbType, expectedConnectionString, expectedRunMigrations,
			expectedRunIdempotent, expectedRunDataload);
		var (config, errors) = DeploymentOptions.BuildConfiguration(args, configRoot);

		Assert.NotNull(errors);
		Assert.Empty(errors);

		Assert.NotNull(config);
		Assert.Equal(expectedConfig.Database.Type, config!.Database.Type);
		Assert.Equal(expectedConfig.Database.ConnectionString, config!.Database.ConnectionString);
		Assert.Equal(expectedConfig.Help, config!.Help);
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
			connectionStrings.Add("ConnectionStrings:Postgres", "PostgresConnectionString");

		return new ConfigurationBuilder()
			.AddInMemoryCollection(connectionStrings)
			.Build();
	}

	private static DeploymentConfiguration GivenConfiguration(int dbType, string connectionString, bool runMigrations, bool runIdempotent, bool runDataload)
	{
		var database = new Database((DatabaseServerType)dbType, connectionString);
		return new DeploymentConfiguration(database, false, runMigrations, runIdempotent, runDataload);
	}
}
