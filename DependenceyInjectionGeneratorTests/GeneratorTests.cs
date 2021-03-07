using DependencyInjectionGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace DependenceyInjectionGeneratorTests
{
    public class GeneratorTests
    {
        private readonly ITestOutputHelper output;

        public GeneratorTests(ITestOutputHelper output) => this.output = output ?? throw new ArgumentNullException(nameof(output));

        [Fact]
        public void GeneratedCodeWithoutServicesWork()
        {
            var source = @"
using Microsoft.Extensions.DependencyInjection;
namespace SourceGeneratorWeb
{
    class C
    {
        void M(IServiceCollection services)
        {
            services.AddServicesToDI();
        }
    }
}";
            var (attributeCode, extensionCode) = GetGeneratedOutput(source);

            Assert.NotNull(attributeCode);
            Assert.NotNull(extensionCode);

            const string expectedAttributeCode = @"// <auto-generated />
[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
internal class AddServiceAttribute : System.Attribute
{
}
";
            const string expectedExtensionCode = @"// <auto-generated />
using Microsoft.Extensions.DependencyInjection;

namespace SourceGeneratorWeb
{
    public static class GeneratedServicesExtension
    {
        public static void AddServicesToDI(this IServiceCollection services)
        {
        }
    }
}
";
            Assert.Equal(expectedAttributeCode, attributeCode);
            Assert.Equal(expectedExtensionCode, extensionCode);
        }

        private (string, string) GetGeneratedOutput(string source)
        {
            var outputCompilation = CreateCompilation(source);
            var trees = outputCompilation.SyntaxTrees.Reverse().Take(2).Reverse().ToList();
            foreach (var tree in trees)
            {
                output.WriteLine(Path.GetFileName(tree.FilePath) + ":");
                output.WriteLine(tree.ToString());
            }
            return (trees.First().ToString(), trees[1].ToString());
        }

        private static Compilation CreateCompilation(string source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);

            var references = new List<MetadataReference>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                if (!assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
                    references.Add(MetadataReference.CreateFromFile(assembly.Location));
            references.Add(MetadataReference.CreateFromFile(typeof(Microsoft.Extensions.DependencyInjection.IServiceCollection).Assembly.Location));

            var compilation = CSharpCompilation.Create("foo",
                                                       new SyntaxTree[] { syntaxTree },
                                                       references,
                                                       new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var generator = new Generator();

            var driver = CSharpGeneratorDriver.Create(generator);
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generateDiagnostics);

            var compileDiagnostics = outputCompilation.GetDiagnostics();
            Assert.False(compileDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error), "Failed: " + compileDiagnostics.FirstOrDefault()?.GetMessage());

            Assert.False(generateDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error),
                         "Failed: " + generateDiagnostics.FirstOrDefault()?.GetMessage());
            return outputCompilation;
        }
    }
}
