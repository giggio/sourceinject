using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace DependencyInjectionGenerator
{
    internal class ServicesReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> ClassesToRegister { get; } = new();
        public bool HasCallToMethod { get; private set; }

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax cds)
                ClassesToRegister.Add(cds);

            if (syntaxNode is InvocationExpressionSyntax
                {
                    Expression: MemberAccessExpressionSyntax
                    {
                        Name:
                        {
                            Identifier:
                            {
                                ValueText: "AddServicesToDI"
                            }
                        }
                    }
                })
            {
                HasCallToMethod = true;
            }
        }
    }
}