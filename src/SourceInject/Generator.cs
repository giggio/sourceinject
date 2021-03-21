using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceInject
{
    [Generator]
    public class Generator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            const string attribute = @"// <auto-generated />
[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
internal class AddServiceAttribute : System.Attribute
{
}
";
            context.RegisterForPostInitialization(context => context.AddSource("AddService.Generated.cs", SourceText.From(attribute, Encoding.UTF8)));
            context.RegisterForSyntaxNotifications(() => new ServicesReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var receiver = (ServicesReceiver?)context.SyntaxReceiver;
            if (receiver == null || !receiver.ClassesToRegister.Any() || !receiver.HasCallToMethod)
                return;
            var registrations = new StringBuilder();
            const string spaces = "            ";
            foreach (var clazz in receiver.ClassesToRegister)
            {
                var semanticModel = context.Compilation.GetSemanticModel(clazz.SyntaxTree);
                if (semanticModel == null)
                    continue;
                var symbol = semanticModel.GetDeclaredSymbol(clazz);
                if (symbol == null)
                    return;
                if (!symbol.GetAttributes().Any(a => a.AttributeClass?.Name == "AddServiceAttribute"))
                    continue;
                registrations.Append(spaces);
                registrations.AppendLine($"services.AddScoped<{GetFullName(symbol)}>();");
            }

            var invocationSemanticModel = context.Compilation.GetSemanticModel(receiver.InvocationSyntaxNode.SyntaxTree);
            var methodSyntax = receiver.InvocationSyntaxNode.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            var methodSymbol = methodSyntax == null ? null : invocationSemanticModel.GetDeclaredSymbol(methodSyntax);
            var code = $@"    public static class GeneratedServicesExtension
    {{
        public static void AddServicesToDI(this IServiceCollection services)
        {{
{registrations}        }}
    }}";
            if (methodSymbol == null || methodSymbol.ContainingNamespace.IsGlobalNamespace)
            {
                var newClassCodeBuilder = new StringBuilder();
                foreach (var line in code.Split(new[] { @"
" }, StringSplitOptions.None))
                {
                    if (line.Length > 4 && line.Substring(0, 4) == "    ")
                        newClassCodeBuilder.AppendLine(line.Substring(4, line.Length - 4));
                    else
                        newClassCodeBuilder.AppendLine(line);
                }
                code = newClassCodeBuilder.ToString();
            }
            else
            {
                var ns = methodSymbol.ContainingNamespace.Name.ToString();
                code = $@"namespace {ns}
{{
{code}
}}
";
            }
            code = @"// <auto-generated />
using Microsoft.Extensions.DependencyInjection;

" + code;
            context.AddSource("GeneratedServicesExtension.Generated.cs", SourceText.From(code, Encoding.UTF8));
        }

        public static string GetFullName(ISymbol symbol)
        {
            var ns = symbol.ContainingNamespace;
            var nss = new List<string>();
            while (ns != null)
            {
                if (string.IsNullOrWhiteSpace(ns.Name))
                    break;
                nss.Add(ns.Name);
                ns = ns.ContainingNamespace;
            }
            nss.Reverse();
            if (nss.Any())
                return $"{string.Join(".", nss)}.{symbol.Name}";
            return symbol.Name;
        }
    }
}


