using Microsoft.Extensions.Configuration;
using Demo.Database;

var arguments = Environment.GetCommandLineArgs();
var configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

var (migrationConfig, errors) = MigrationOptions.BuildConfiguration(arguments, configuration);

if(errors.Any())
	return Output.DisplayConfigErrors(errors);

if(migrationConfig.Help)
	return Output.DisplayHelp();

return 0;
