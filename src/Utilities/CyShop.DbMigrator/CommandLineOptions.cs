namespace CyShop.DbMigrator;

public class CommandLineOptions
{
    public bool Override { get; init; }

    public static CommandLineOptions Parse(string[] args)
    {
        return new CommandLineOptions
        {
            Override = args.Any(a => a is "--override" or "-o")
        };
    }
}
