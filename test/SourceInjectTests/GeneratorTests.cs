using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using SourceInject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace SourceInjectTests
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
namespace WebApp
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
            attributeCode.ShouldNotBeNull();
            extensionCode.ShouldNotBeNull();

            const string expectedAttributeCode = @"// <auto-generated />
[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
internal class AddServiceAttribute : System.Attribute
{
}
";
            const string expectedExtensionCode = @"// <auto-generated />
using Microsoft.Extensions.DependencyInjection;

namespace WebApp
{
    public static class GeneratedServicesExtension
    {
        public static void AddServicesToDI(this IServiceCollection services)
        {
        }
    }
}
";
            attributeCode.ShouldBe(expectedAttributeCode);
            extensionCode.ShouldBe(expectedExtensionCode);
        }

        [Fact]
        public void GeneratedCodeWithOneService()
        {
            var source = @"
using Microsoft.Extensions.DependencyInjection;
namespace WebApp
{
    class C
    {
        void M(IServiceCollection services)
        {
             services.AddServicesToDI();
        }
    }
    [AddService]
    class MyService
    {
    }
}";
            var (_, extensionCode) = GetGeneratedOutput(source);
            extensionCode.ShouldNotBeNull();

            const string expectedExtensionCode = @"// <auto-generated />
using Microsoft.Extensions.DependencyInjection;

namespace WebApp
{
    public static class GeneratedServicesExtension
    {
        public static void AddServicesToDI(this IServiceCollection services)
        {
            services.AddScoped<WebApp.MyService>();
        }
    }
}
";
            extensionCode.ShouldBe(expectedExtensionCode);
        }

        [Fact]
        public void GeneratedCodeWithTwoServices()
        {
            var source = @"
using Microsoft.Extensions.DependencyInjection;
namespace WebApp
{
    class C
    {
        void M(IServiceCollection services)
        {
             services.AddServicesToDI();
        }
    }
    [AddService]
    class MyService1
    {
    }
    [AddService]
    class MyService2
    {
    }
}";
            var (_, extensionCode) = GetGeneratedOutput(source);
            extensionCode.ShouldNotBeNull();

            const string expectedExtensionCode = @"// <auto-generated />
using Microsoft.Extensions.DependencyInjection;

namespace WebApp
{
    public static class GeneratedServicesExtension
    {
        public static void AddServicesToDI(this IServiceCollection services)
        {
            services.AddScoped<WebApp.MyService1>();
            services.AddScoped<WebApp.MyService2>();
        }
    }
}
";
            extensionCode.ShouldBe(expectedExtensionCode);
        }

        [Fact]
        public void GeneratedCodeWithDifferentNamespace()
        {
            var source = @"
using Microsoft.Extensions.DependencyInjection;
namespace MyNamespace
{
    class C
    {
        void M(IServiceCollection services)
        {
             services.AddServicesToDI();
        }
    }
    [AddService]
    class MyService
    {
    }
}";
            var (_, extensionCode) = GetGeneratedOutput(source);
            extensionCode.ShouldNotBeNull();

            const string expectedExtensionCode = @"// <auto-generated />
using Microsoft.Extensions.DependencyInjection;

namespace MyNamespace
{
    public static class GeneratedServicesExtension
    {
        public static void AddServicesToDI(this IServiceCollection services)
        {
            services.AddScoped<MyNamespace.MyService>();
        }
    }
}
";
            expectedExtensionCode.ShouldBe(extensionCode);
        }

        [Fact]
        public void GeneratedCodeWithoutNamespace()
        {
            var source = @"
using Microsoft.Extensions.DependencyInjection;
class C
{
    void M(IServiceCollection services)
    {
         services.AddServicesToDI();
    }
}
[AddService]
class MyService
{
}";
            var (_, extensionCode) = GetGeneratedOutput(source);
            extensionCode.ShouldNotBeNull();

            const string expectedExtensionCode = @"// <auto-generated />
using Microsoft.Extensions.DependencyInjection;

public static class GeneratedServicesExtension
{
    public static void AddServicesToDI(this IServiceCollection services)
    {
        services.AddScoped<MyService>();
    }
}
";
            extensionCode.ShouldBe(expectedExtensionCode);
        }

        [Fact]
        public void GeneratedCodeWithTopLevelStatement()
        {
            var source = @"
using Microsoft.Extensions.DependencyInjection;
var services = new ServiceCollection();
services.AddServicesToDI();
var serviceProvider = services.BuildServiceProvider();
[AddService]
class MyService
{
}";
            var (_, extensionCode) = GetGeneratedOutput(source, true);
            extensionCode.ShouldNotBeNull();

            const string expectedExtensionCode = @"// <auto-generated />
using Microsoft.Extensions.DependencyInjection;

public static class GeneratedServicesExtension
{
    public static void AddServicesToDI(this IServiceCollection services)
    {
        services.AddScoped<MyService>();
    }
}
";
            extensionCode.ShouldBe(expectedExtensionCode);
        }

        private (string, string) GetGeneratedOutput(string source, bool executable = false)
        {
            var outputCompilation = CreateCompilation(source, executable);
            var trees = outputCompilation.SyntaxTrees.Reverse().Take(2).Reverse().ToList();
            foreach (var tree in trees)
            {
                output.WriteLine(Path.GetFileName(tree.FilePath) + ":");
                output.WriteLine(tree.ToString());
            }
            return (trees.First().ToString(), trees[1].ToString());
        }

        private static Compilation CreateCompilation(string source, bool executable)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);

            var references = new List<MetadataReference>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                if (!assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
                    references.Add(MetadataReference.CreateFromFile(assembly.Location));

            var compilation = CSharpCompilation.Create("foo",
                                                       new SyntaxTree[] { syntaxTree },
                                                       references,
                                                       new CSharpCompilationOptions(executable ? OutputKind.ConsoleApplication : OutputKind.DynamicallyLinkedLibrary));

            var generator = new Generator();

            var driver = CSharpGeneratorDriver.Create(generator);
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generateDiagnostics);

            var compileDiagnostics = outputCompilation.GetDiagnostics();
            compileDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error).ShouldBeFalse("Failed: " + compileDiagnostics.FirstOrDefault()?.GetMessage());

            generateDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error).ShouldBeFalse("Failed: " + generateDiagnostics.FirstOrDefault()?.GetMessage());
            return outputCompilation;
        }
    }
}