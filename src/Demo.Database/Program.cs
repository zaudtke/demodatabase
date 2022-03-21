using Demo.Database;
using Microsoft.Extensions.Configuration;

var arguments = Environment.GetCommandLineArgs();
var configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

var migrationOptions = MigrationOptions.BuildConfiguration(arguments, configuration);

return migrationOptions switch
{
	(null, var errors) => Output.DisplayConfigErrors(errors),
	var (migrationConfig, _) when migrationConfig.Help => Output.DisplayHelp(),
	var (migrationConfig, _) => Deployment.Build(migrationConfig).Run()
};
