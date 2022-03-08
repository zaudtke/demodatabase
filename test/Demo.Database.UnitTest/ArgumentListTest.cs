using Demo.Database;
using Xunit;

namespace Demo.Database.UnitTest;
public class ArgumentListTest
{
	[Theory(DisplayName = "ArgumentList.IsHelpCommand Returns True/False correctly based on arguments")]
	[InlineData(false,new string[]{""})]
	[InlineData(false,new string[]{"  "})]
	[InlineData(false,new string[]{"", "  "})]
	[InlineData(false,new string[]{"nothelp", "--nh", "--halp"})]
	[InlineData(false,new string[]{"-h"})]
	[InlineData(false,new string[]{"-help"})]
	[InlineData(false,new string[]{"?h"})]
	[InlineData(false,new string[]{"?help"})]
	[InlineData(true,new string[]{"--h"})]
	[InlineData(true,new string[]{"--H"})]
	[InlineData(true,new string[]{"--help"})]
	[InlineData(true,new string[]{"--Help"})]
	[InlineData(true,new string[]{"--HELP"})]
	[InlineData(true,new string[]{"--h", "--otherflag"})]
	[InlineData(true,new string[]{"--otherflag", "--h"})]
	[InlineData(true,new string[]{"--help", "--otherflag"})]
	[InlineData(true,new string[]{"otherflag", "--help"})]
	[InlineData(true,new string[]{"--HELP", "--otherflag"})]
	[InlineData(true,new string[]{"otherflag", "--HELP"})]
	public void ArgumentList_IsHelpCommand_Returns_CorrectValue(bool expected, string[] args)
	{
		var sut = ArgumentList.IsHelpCommand(args);
		Assert.Equal(expected, sut);
	}
}
