namespace Itecho.TsGen;

public static class ProcessArguments
{
    public class Args
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Value { get; set; } = new();
    }

    public static List<Args> Process(IEnumerable<string> list)
    {
        var ret = new List<Args>();
        var enumerator = list.GetEnumerator();
        Args? it = null;
        while (enumerator.MoveNext())
        {
            var current = enumerator.Current;
            if (current.StartsWith("-"))
            {
                it = new Args
                {
                    Name = current[1..]
                };
                ret.Add(it);
                continue;
            }
            if (it != null)
            {
                it.Value.Add(current);
            }
        }

        return ret;
    }
}