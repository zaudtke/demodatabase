using Demo.Database;

var arguments = Environment.GetCommandLineArgs();

if(MigrationOptions.IsHelpOption(arguments))
{
	MigrationOptions.DisplayHelp();
	return 0;
}


return 0;
