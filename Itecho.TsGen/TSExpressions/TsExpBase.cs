namespace Itecho.TsGen.TSExpressions;

/// <summary>
/// The base for all TS expressions
/// There are two derived classes TSStandaloneExp and TSExp 
/// </summary>
public abstract class TsExpBase
{
    public abstract void Write(TsCodeGenerator gen);
}