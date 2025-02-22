namespace Gu.Roslyn.AnalyzerExtensions.Tests.SyntaxTreeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public class DocumentationCommentTriviaSyntaxExtensionsTests
    {
        [Test]
        public void Summary()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    public class Foo
    {
        /// <summary>
        /// The identity function.
        /// </summary>
        /// <typeparam name=""T"">The type</typeparam>
        /// <param name=""i"">The value to return.</param>
        /// <returns><paramref name=""i""/></returns>
        public T Id<T>(T i) => i;
    }
}");
            var method = syntaxTree.FindMethodDeclaration("Id");
            Assert.AreEqual(true, method.TryGetDocumentationComment(out var comment));
            Assert.AreEqual(true, comment.TryGetSummary(out var summary));
            CodeAssert.AreEqual("<summary>\r\n        /// The identity function.\r\n        /// </summary>", summary.ToFullString());
        }

        [Test]
        public void SummaryWithPragma()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    public class Foo
    {
#pragma warning disable WPF0013 // CLR accessor for attached property must match registered type.
        /// <summary>
        /// The identity function.
        /// </summary>
        /// <typeparam name=""T"">The type</typeparam>
        /// <param name=""i"">The value to return.</param>
        /// <returns><paramref name=""i""/></returns>
        public T Id<T>(T i) => i;
    }
}");
            var method = syntaxTree.FindMethodDeclaration("Id");
            Assert.AreEqual(true, method.TryGetDocumentationComment(out var comment));
            Assert.AreEqual(true, comment.TryGetSummary(out var summary));
            CodeAssert.AreEqual("<summary>\r\n        /// The identity function.\r\n        /// </summary>", summary.ToFullString());
        }

        [Test]
        public void TypeParam()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    public class Foo
    {
        /// <summary>
        /// The identity function.
        /// </summary>
        /// <typeparam name=""T"">The type</typeparam>
        /// <param name=""i"">The value to return.</param>
        /// <returns><paramref name=""i""/></returns>
        public T Id<T>(T i) => i;
        }
}");
            var method = syntaxTree.FindMethodDeclaration("Id");
            Assert.AreEqual(true, method.TryGetDocumentationComment(out var comment));
            Assert.AreEqual(true, comment.TryGetTypeParam("T", out var summary));
            Assert.AreEqual("<typeparam name=\"T\">The type</typeparam>", summary.ToFullString());
            Assert.AreEqual(false, comment.TryGetTypeParam("MISSING", out _));
        }

        [Test]
        public void Param()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    public class Foo
    {
        /// <summary>
        /// The identity function.
        /// </summary>
        /// <typeparam name=""T"">The type</typeparam>
        /// <param name=""i"">The value to return.</param>
        /// <returns><paramref name=""i""/></returns>
        public T Id<T>(T i) => i;
        }
}");
            var method = syntaxTree.FindMethodDeclaration("Id");
            Assert.AreEqual(true, method.TryGetDocumentationComment(out var comment));
            Assert.AreEqual(true, comment.TryGetParam("i", out var summary));
            Assert.AreEqual("<param name=\"i\">The value to return.</param>", summary.ToFullString());
            Assert.AreEqual(false, comment.TryGetParam("missing", out _));
        }

        [Test]
        public void Returns()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    public class Foo
    {
        /// <summary>
        /// The identity function.
        /// </summary>
        /// <typeparam name=""T"">The type</typeparam>
        /// <param name=""i"">The value to return.</param>
        /// <returns><paramref name=""i""/></returns>
        public T Id<T>(T i) => i;
    }
}");
            var method = syntaxTree.FindMethodDeclaration("Id");
            Assert.AreEqual(true, method.TryGetDocumentationComment(out var comment));
            Assert.AreEqual(true, comment.TryGetReturns(out var summary));
            Assert.AreEqual("<returns><paramref name=\"i\"/></returns>", summary.ToFullString());
        }
    }
}
