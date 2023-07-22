using Microsoft.AspNetCore.Mvc;

namespace Itecho.TsGen.TSExpressions;

public abstract class InterpolatedSegment
{
    public abstract void Write(TsCodeGenerator gen);
}

public class ExpSegment : InterpolatedSegment
{
    public TsExp Expression { get; }

    public ExpSegment(TsExp expression)
    {
        Expression = expression;
    }

    public override void Write(TsCodeGenerator gen)
    {
        gen.Write("${");
        Expression.Write(gen);
        gen.Write("}");
    }
}

public class StringSegment : InterpolatedSegment
{
    private string Value { get; }

    public StringSegment(string value)
    {
        Value = value;
    }

    public override void Write(TsCodeGenerator gen)
    {
        gen.Write(Value);
    }
}

public class InterpolatedStringExp : TsExp
{
    public InterpolatedSegment[] Segments { get; }

    public new static StringSegment String(string value)
    {
        return new StringSegment(value);
    }
    
    public static ExpSegment Expression(TsExp expression)
    {
        return new ExpSegment(expression);
    }
    
    public InterpolatedStringExp(InterpolatedSegment[] segments)
    {
        Segments = segments;
    }

    public override void Write(TsCodeGenerator gen)
    {
        gen.Write("`");
        foreach (var segment in Segments)
        {
            segment.Write(gen);
        }

        gen.Write("`");
    }
}