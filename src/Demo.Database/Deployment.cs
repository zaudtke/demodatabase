using DbUp;
using DbUp.Helpers;

namespace Demo.Database;


public interface IDeployment
{
	int Run();
}

public static class Deployment
{
	public static IDeployment Build(DeploymentConfiguration config)
	{
		return config.Database.Type switch
		{
			DatabaseServerType.Postgres => new SqlDeployment(config),
			DatabaseServerType.SqlServer => new PostgresDeployment(config),
			_ => throw new Exception("Invalid Database Server Type")
		};
	}
}




