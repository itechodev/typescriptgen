using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace typescriptgen
{
    [Generator]
    public class EnumGenerator : IIncrementalGenerator
    {
        private static bool IsSyntaxTargetForGeneration(SyntaxNode node) => node is ClassDeclarationSyntax;

        private static EnumDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        {
            
            // we know the node is a EnumDeclarationSyntax thanks to IsSyntaxTargetForGeneration
            var enumDeclarationSyntax = (EnumDeclarationSyntax)context.Node;

            // loop through all the attributes on the method
            foreach (AttributeListSyntax attributeListSyntax in enumDeclarationSyntax.AttributeLists)
            {
                foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
                {
                    if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                    {
                        // weird, we couldn't get the symbol, ignore it
                        continue;
                    }

                    INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                    string fullName = attributeContainingTypeSymbol.ToDisplayString();

                    // Is the attribute the [EnumExtensions] attribute?
                    if (fullName == "NetEscapades.EnumGenerators.EnumExtensionsAttribute")
                    {
                        // return the enum
                        return enumDeclarationSyntax;
                    }
                }
            }

            // we didn't find the attribute we were looking for
            return null;
        }

        // Initialize is called by the host exactly once, regardless of the number of further compilations that may occur.
        // Rather than a dedicated Execute method, an Incremental Generator instead defines an immutable execution pipeline as part of initialization
        // The Initialize method receives an instance of IncrementalGeneratorInitializationContext which is used by the generator to define a set of transformations.
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, token) => IsSyntaxTargetForGeneration(node),
                transform: static (context, token) => GetSemanticTargetForGeneration(context)
            );
        }
    }
}