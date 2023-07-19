namespace Itecho.TsGen.TSExpressions;

/// <summary>
/// Top level expression which cannot be nested
/// such as comments, import, export, interface
/// most top level expression does contain other expressions
/// </summary>
public abstract class TsStandaloneExp : TsExpBase
{
}