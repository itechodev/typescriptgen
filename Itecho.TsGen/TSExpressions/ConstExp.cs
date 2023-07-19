using Itecho.TsGen.TsTypes;

namespace Itecho.TsGen.TSExpressions;

public class ConstExp : TsExp
{
    public object? Value { get; }
    public TsPrimitive.TsPrimitiveType Type { get; }

    public ConstExp(object? value, TsPrimitive.TsPrimitiveType type)
    {
        Value = value;
        Type = type;
    }

    public ConstExp(string? value) : this(value, TsPrimitive.TsPrimitiveType.String)
    {
    }

    public ConstExp(double value) : this(value, TsPrimitive.TsPrimitiveType.Number)
    {
    }

    public ConstExp(int value) : this(value, TsPrimitive.TsPrimitiveType.Number)
    {
    }

    public ConstExp(bool value) : this(value, TsPrimitive.TsPrimitiveType.Boolean)
    {
    }

    public ConstExp(DateTime value) : this(value, TsPrimitive.TsPrimitiveType.Date)
    {
    }

    public override void Write(TsCodeGenerator gen)
    {
        throw new NotImplementedException();
    }
}