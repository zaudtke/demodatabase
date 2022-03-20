namespace Demo.Database;

public static class Output
{
	public static int DisplayHelp()
	{
		// Flip Key and Value to group aliases together based on Value aka Description
		var output = MigrationOptions.ValidOptions.GroupBy(x => x.Value)
			.ToDictionary(x => x.Key, x => x.Select(i => i.Key).ToList());

		Console.WriteLine("\r\nDescription:\r\n   Run Scripts to update Demo Database.\r\n\r\nOptions:");
		foreach (var kvp in output)
		{
			Console.WriteLine($"{kvp.Value.Aggregate((x, y) => $"{x}, {y}"),-20}{kvp.Key}");
		}
		Console.WriteLine();
		return 0;
	}

	public static int DisplayConfigErrors(IEnumerable<string> errors)
	{
		Console.WriteLine("\r\nBuildConfiguration Errors:\r\n\r\n");
		foreach (var e in errors)
		{
			Console.WriteLine(e);
		}
		Console.WriteLine();
		return -1;
	}
}
