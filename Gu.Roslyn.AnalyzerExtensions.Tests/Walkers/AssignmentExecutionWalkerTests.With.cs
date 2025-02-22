namespace Gu.Roslyn.AnalyzerExtensions.Tests.Walkers
{
    using System.Linq;
    using System.Threading;
    using Gu.Roslyn.AnalyzerExtensions;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    using NUnit.Framework;

    public partial class AssignmentExecutionWalkerTests
    {
        public class With
        {
            [TestCase(Scope.Member)]
            [TestCase(Scope.Instance)]
            [TestCase(Scope.Recursive)]
            public void FieldCtorArg(Scope scope)
            {
                var testCode = @"
namespace RoslynSandbox
{
    internal class C
    {
        private readonly int value;

        internal C(int arg)
        {
            this.value = arg;
        }
    }
}";
                var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var value = syntaxTree.FindParameter("arg");
                var ctor = syntaxTree.FindConstructorDeclaration("C(int arg)");
                Assert.AreEqual(true, semanticModel.TryGetSymbol(value, CancellationToken.None, out var symbol));
                Assert.AreEqual(true, AssignmentExecutionWalker.FirstWith(symbol, ctor, scope, semanticModel, CancellationToken.None, out AssignmentExpressionSyntax result));
                Assert.AreEqual("this.value = arg", result.ToString());
                using (var walker = AssignmentExecutionWalker.With(symbol, ctor, scope, semanticModel, CancellationToken.None))
                {
                    Assert.AreEqual("this.value = arg", walker.Assignments.Single().ToString());
                }
            }

            [TestCase(Scope.Member)]
            [TestCase(Scope.Instance)]
            [TestCase(Scope.Recursive)]
            public void GenericFieldCtorArg(Scope scope)
            {
                var testCode = @"
namespace RoslynSandbox
{
    internal class C<T>
    {
        private readonly T value;

        internal C(T arg)
        {
            this.value = arg;
        }
    }
}";
                var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var value = syntaxTree.FindParameter("arg");
                var ctor = syntaxTree.FindConstructorDeclaration("C(T arg)");
                Assert.AreEqual(true, semanticModel.TryGetSymbol(value, CancellationToken.None, out var symbol));
                Assert.AreEqual(true, AssignmentExecutionWalker.FirstWith(symbol, ctor, scope, semanticModel, CancellationToken.None, out AssignmentExpressionSyntax result));
                Assert.AreEqual("this.value = arg", result.ToString());
                using (var walker = AssignmentExecutionWalker.With(symbol, ctor, scope, semanticModel, CancellationToken.None))
                {
                    Assert.AreEqual("this.value = arg", walker.Assignments.Single().ToString());
                }
            }

            [TestCase(Scope.Member)]
            [TestCase(Scope.Instance)]
            [TestCase(Scope.Recursive)]
            public void GenericFieldTypedCtorArg(Scope scope)
            {
                var testCode = @"
namespace RoslynSandbox
{
    internal class C<T>
    {
        private readonly T value;

        internal C(T arg)
        {
            this.value = arg;
        }

         public static C<int> Create(int n) => new C<int>(n);
    }
}";
                var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                ArgumentSyntax argument = syntaxTree.FindArgument("n");
                Assert.AreEqual(true, semanticModel.TryGetSymbol(argument.Parent.Parent, CancellationToken.None, out IMethodSymbol method));
                Assert.AreEqual(true, method.TryFindParameter(argument, out var symbol));
                var ctor = syntaxTree.FindConstructorDeclaration("C(T arg)");
                Assert.AreEqual(true, AssignmentExecutionWalker.FirstWith(symbol, ctor, scope, semanticModel, CancellationToken.None, out AssignmentExpressionSyntax result));
                Assert.AreEqual("this.value = arg", result.ToString());
                using (var walker = AssignmentExecutionWalker.With(symbol, ctor, scope, semanticModel, CancellationToken.None))
                {
                    Assert.AreEqual("this.value = arg", walker.Assignments.Single().ToString());
                }
            }

            [TestCase(Scope.Member)]
            [TestCase(Scope.Instance)]
            [TestCase(Scope.Recursive)]
            public void FieldCtorArgViaLocal(Scope scope)
            {
                var testCode = @"
namespace RoslynSandbox
{
    internal class C
    {
        private readonly int value;

        internal C(int arg)
        {
            var temp = arg;
            this.value = temp;
        }
    }
}";
                var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var ctor = syntaxTree.FindConstructorDeclaration("C(int arg)");
                var symbol = semanticModel.GetDeclaredSymbol(syntaxTree.FindParameter("int arg"), CancellationToken.None);
                Assert.AreEqual(true, AssignmentExecutionWalker.FirstWith(symbol, ctor, scope, semanticModel, CancellationToken.None, out AssignmentExpressionSyntax result));
                Assert.AreEqual("this.value = temp", result.ToString());
                using (var walker = AssignmentExecutionWalker.With(symbol, ctor, scope, semanticModel, CancellationToken.None))
                {
                    Assert.AreEqual("this.value = temp", walker.Assignments.Single().ToString());
                }
            }

            [TestCase(Scope.Member)]
            [TestCase(Scope.Instance)]
            [TestCase(Scope.Recursive)]
            public void FieldCtorArgInNested(Scope scope)
            {
                var testCode = @"
namespace RoslynSandbox
{
    using System.IO;

    internal class C
    {
        private StreamReader reader;

        internal C(Stream stream)
        {
            this.reader = new StreamReader(stream);
        }
    }
}";
                var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var value = syntaxTree.FindParameter("stream");
                var ctor = syntaxTree.FindConstructorDeclaration("C(Stream stream)");
                var symbol = semanticModel.GetDeclaredSymbol(value, CancellationToken.None);
                Assert.AreEqual(true, AssignmentExecutionWalker.FirstWith(symbol, ctor, scope, semanticModel, CancellationToken.None, out AssignmentExpressionSyntax result));
                Assert.AreEqual("this.reader = new StreamReader(stream)", result.ToString());
                using (var walker = AssignmentExecutionWalker.With(symbol, ctor, scope, semanticModel, CancellationToken.None))
                {
                    Assert.AreEqual("this.reader = new StreamReader(stream)", walker.Assignments.Single().ToString());
                }
            }

            [TestCase(Scope.Member)]
            [TestCase(Scope.Instance)]
            [TestCase(Scope.Recursive)]
            public void ChainedCtorArg(Scope scope)
            {
                var testCode = @"
namespace RoslynSandbox
{
    internal class C
    {
        private readonly int value;

        public C(int arg)
            : this(arg, 1)
        {
        }

        internal C(int chainedArg, int _)
        {
            this.value = chainedArg;
        }
    }
}";
                var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var ctor = syntaxTree.FindConstructorDeclaration("C(int arg)");
                var symbol = semanticModel.GetDeclaredSymbolSafe(syntaxTree.FindParameter("arg"), CancellationToken.None);
                if (scope != Scope.Member)
                {
                    Assert.AreEqual(true, AssignmentExecutionWalker.FirstWith(symbol, ctor, scope, semanticModel, CancellationToken.None, out var result));
                    Assert.AreEqual("this.value = chainedArg", result.ToString());
                    using (var walker = AssignmentExecutionWalker.With(symbol, ctor, scope, semanticModel, CancellationToken.None))
                    {
                        Assert.AreEqual("this.value = chainedArg", walker.Assignments.Single().ToString());
                    }
                }
                else
                {
                    Assert.AreEqual(false, AssignmentExecutionWalker.FirstWith(symbol, ctor, scope, semanticModel, CancellationToken.None, out _));
                    using (var walker = AssignmentExecutionWalker.With(symbol, ctor, scope, semanticModel, CancellationToken.None))
                    {
                        Assert.AreEqual(0, walker.Assignments.Count);
                    }
                }
            }

            [TestCase(Scope.Member)]
            [TestCase(Scope.Instance)]
            [TestCase(Scope.Recursive)]
            public void FieldWithCtorArgViaProperty(Scope scope)
            {
                var testCode = @"
namespace RoslynSandbox
{
    internal class C
    {
        private int number;

        internal C(int arg)
        {
            this.Number = arg;
        }

        public int Number
        {
            get { return this.number; }
            set { this.number = value; }
        }
    }
}";
                var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                var value = syntaxTree.FindParameter("arg");
                var ctor = syntaxTree.FindConstructorDeclaration("C(int arg)");
                var symbol = semanticModel.GetDeclaredSymbolSafe(value, CancellationToken.None);
                if (scope == Scope.Member)
                {
                    Assert.AreEqual(true, AssignmentExecutionWalker.FirstWith(symbol, ctor, scope, semanticModel, CancellationToken.None, out var result));
                    Assert.AreEqual("this.Number = arg", result.ToString());
                    using (var walker = AssignmentExecutionWalker.With(symbol, ctor, scope, semanticModel, CancellationToken.None))
                    {
                        Assert.AreEqual("this.Number = arg", string.Join(", ", walker.Assignments));
                    }
                }
                else
                {
                    Assert.AreEqual(true, AssignmentExecutionWalker.FirstWith(symbol, ctor, scope, semanticModel, CancellationToken.None, out var result));
                    Assert.AreEqual("this.number = value", result.ToString());
                    using (var walker = AssignmentExecutionWalker.With(symbol, ctor, scope, semanticModel, CancellationToken.None))
                    {
                        Assert.AreEqual("this.number = value", string.Join(", ", walker.Assignments));
                    }
                }
            }
        }
    }
}
