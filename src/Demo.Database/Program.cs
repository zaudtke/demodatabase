using Demo.Database;
using Microsoft.Extensions.Configuration;

var arguments = Environment.GetCommandLineArgs();
var configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

var deploymentOptions = DeploymentOptions.BuildConfiguration(arguments, configuration);

return deploymentOptions switch
{
	(null, var errors) => Output.DisplayConfigErrors(errors),
	var (deploymentConfig, _) when deploymentConfig.Help => Output.DisplayHelp(),
	var (deploymentConfig, _) => Deployment.Build(deploymentConfig).Run()
};
