namespace Gu.Roslyn.AnalyzerExtensions.Tests.SyntaxTreeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public partial class MemberDeclarationSyntaxExtensionsTests
    {
        public class TryGetDocumentationComment
        {
            [Test]
            public void Class()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    /// <summary>
    /// The Foo
    /// </summary>
    public class Foo
    {
    }
}");
                var classDeclaration = syntaxTree.FindClassDeclaration("Foo");
                Assert.AreEqual(true, classDeclaration.TryGetDocumentationComment(out var result));
                var expected = "/// <summary>\r\n    /// The Foo\r\n    /// </summary>\r\n";
                CodeAssert.AreEqual(expected, result.ToFullString());
            }

            [Test]
            public void Constructor()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    public class Foo
    {
        /// <summary> Initializes a new instance of the <see cref=""Foo""/> class. </summary>
        public Foo()
        {
        }
    }
}");
                var ctor = syntaxTree.FindConstructorDeclaration("Foo");
                Assert.AreEqual(true, ctor.TryGetDocumentationComment(out var result));
                var expected = "/// <summary> Initializes a new instance of the <see cref=\"Foo\"/> class. </summary>\r\n";
                CodeAssert.AreEqual(expected, result.ToFullString());
            }

            [Test]
            public void Property()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    public class Foo
    {
        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public int Value { get; set; }
    }
}");
                var property = syntaxTree.FindPropertyDeclaration("Value");
                Assert.AreEqual(true, property.TryGetDocumentationComment(out var result));
                var expected = "/// <summary>\r\n" +
                               "        /// Gets or sets the value\r\n" +
                               "        /// </summary>\r\n";
                CodeAssert.AreEqual(expected, result.ToFullString());
            }

            [Test]
            public void Method()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    public class Foo
    {
        /// <summary>
        /// The identity function.
        /// </summary>
        /// <param name=""i"">The value to return.</param>
        /// <returns><paramref name=""i""/></returns>
        public int Id(int i) => i;
    }
}");
                var method = syntaxTree.FindMethodDeclaration("Id");
                Assert.AreEqual(true, method.TryGetDocumentationComment(out var result));
                var expected = "/// <summary>\r\n" +
                               "        /// The identity function.\r\n" +
                               "        /// </summary>\r\n" +
                               "        /// <param name=\"i\">The value to return.</param>\r\n" +
                               "        /// <returns><paramref name=\"i\"/></returns>\r\n";
                CodeAssert.AreEqual(expected, result.ToFullString());
            }

            [Test]
            public void ClassWithPragma()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
#pragma warning disable WPF0013 // CLR accessor for attached property must match registered type.
    /// <summary>
    /// The Foo
    /// </summary>
    public class Foo
    {
    }
}");
                var classDeclaration = syntaxTree.FindClassDeclaration("Foo");
                Assert.AreEqual(true, classDeclaration.TryGetDocumentationComment(out var result));
                var expected = "/// <summary>\r\n    /// The Foo\r\n    /// </summary>\r\n";
                CodeAssert.AreEqual(expected, result.ToFullString());
            }
        }
    }
}
