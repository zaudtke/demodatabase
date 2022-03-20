namespace Demo.Database;

public record Configuration(Database Database, bool Help, bool RunMigrations, bool RunIdempotent, bool RunDataLoad, bool DropDatabase);

public record Database(DatabaseServerType Type, string ConnectionString);
public enum DatabaseServerType
{
	Postgres = 0,
	SqlServer = 1,
}
