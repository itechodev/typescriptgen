using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace typescriptgen
{
    [Generator]
    public class TypescriptIncrementalGenerator : IIncrementalGenerator
    {
        private static bool IsSyntaxTargetForGeneration(SyntaxNode node) => node is ClassDeclarationSyntax;


        private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        {
            // we know the node is a ClassDeclarationSyntax thanks to IsSyntaxTargetForGeneration
            return context.Node as ClassDeclarationSyntax;
        }

        // Initialize is called by the host exactly once, regardless of the number of further compilations that may occur.
        // Rather than a dedicated Execute method, an Incremental Generator instead defines an immutable execution pipeline as part of initialization
        // The Initialize method receives an instance of IncrementalGeneratorInitializationContext which is used by the generator to define a set of transformations.
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var classDeclarations = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, token) => IsSyntaxTargetForGeneration(node),
                transform: static (context, token) => GetSemanticTargetForGeneration(context)
            ).Where(c => c is not null);

            var compilationAndClasses
                = context.CompilationProvider.Combine(classDeclarations.Collect());

            context.RegisterSourceOutput(compilationAndClasses,
                static (spc, source) => Execute(source.Left, source.Right, spc));
        }

        private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax?> classes,
            SourceProductionContext context)
        {
            if (classes.IsDefaultOrEmpty)
            {
                return;
            }

            foreach (var classSyntax in classes)
            {
                if (classSyntax is null)
                    continue;
                
                var filename = classSyntax.Identifier.Value + ".g.cs";
                context.AddSource(filename,  SourceText.From(GenerateClassSyntax(classSyntax), Encoding.UTF8));
            }
        }
        
        private static string GenerateClassSyntax(ClassDeclarationSyntax classSyntax)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"export interface {classSyntax.Identifier.Text} {{");
            foreach (var property in classSyntax.Members)
            {
                if (property is PropertyDeclarationSyntax propertyDeclarationSyntax)
                {
                    sb.AppendLine(
                        $"    {propertyDeclarationSyntax.Identifier.Text}: {propertyDeclarationSyntax.Type};");
                }
            }

            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}