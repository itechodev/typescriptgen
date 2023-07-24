namespace Itecho.TsGen.TsTypes
{
    /// <summary>
    /// base of all TS types
    /// </summary>
    public abstract class TsType
    {
        public static TsBuildInType BuildIn(BuildInType name) => new(name);

        public static TsGenericReference GenericReference(TsType referencedType, params TsType[] parameters) =>
            new(referencedType, parameters);

        public static TsPrimitive Primitive(TsPrimitive.TsPrimitiveType type) => new(type);
    }
}