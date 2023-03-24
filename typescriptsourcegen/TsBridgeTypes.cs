namespace typescriptgen;

public class TsController
{
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<TsMethod> Methods { get; set; } = new();
}

public class TsMethod
{
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public TsType ReturnType { get; set; } = new();
    public List<TsParameter> Parameters { get; set; } = new();
}

public enum TsParameterBinding
{
    Undefined,
    Query,
    Body,
    Path,
    Header
}

public class TsParameter
{
    public string Name { get; set; } = string.Empty;
    public TsParameterBinding Binding { get; set; }
}