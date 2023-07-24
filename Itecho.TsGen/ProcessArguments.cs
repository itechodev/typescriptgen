namespace Itecho.TsGen;

public static class ProcessArguments
{
    public class Args
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public Args(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }

    public static List<Args> Process(IEnumerable<string> args)
    {
        return args.Select(arg =>
        {
            if (!arg.StartsWith("-"))
                return new Args(arg, string.Empty);

            var value = arg.Substring(1);
            var parts = value.Split("=");
            return parts.Length == 2 
                ? new Args(parts[0], parts[1]) 
                : new Args(value, string.Empty);
        }).ToList();
    }
}