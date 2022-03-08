using Demo.Database;

var arguments = Environment.GetCommandLineArgs();

if(ArgumentList.IsHelpCommand(arguments))
{
	DisplayHelp();
	return 0;
}


return 0;

// Helper Functions
void DisplayHelp()
{
	Console.WriteLine("\r\nAvailable Commands");
	foreach(var kvp in ArgumentList.Arguments)
	{
		Console.WriteLine($"{kvp.Key}\t{kvp.Value}");
	}
	Console.WriteLine();
}
