namespace Gu.Roslyn.AnalyzerExtensions.Tests.SyntaxTreeTests
{
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public partial class ExpressionSyntaxExtTests
    {
        public class IsExecutedBefore
        {
            [Test]
            public void InnerBeforeOuter()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        private readonly int i;
        public Foo()
        {
            i = 1;
        }
    }
}");
                var outer = syntaxTree.FindAssignmentExpression("i = 1");
                var inner = syntaxTree.FindLiteralExpression("1");
                Assert.AreEqual(ExecutedBefore.Yes, inner.IsExecutedBefore(outer));
                Assert.AreEqual(ExecutedBefore.No, outer.IsExecutedBefore(inner));
            }

            [TestCase("1", "2", ExecutedBefore.Yes)]
            [TestCase("2", "1", ExecutedBefore.No)]
            public void And(string firstInt, string otherInt, ExecutedBefore expected)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public static bool Get(int a, int b) => a == 1 && b == 2;
    }
}");
                var first = syntaxTree.FindLiteralExpression(firstInt);
                var other = syntaxTree.FindLiteralExpression(otherInt);
                Assert.AreEqual(expected, first.IsExecutedBefore(other));
            }

            [TestCase("1", "2", ExecutedBefore.Yes)]
            [TestCase("2", "1", ExecutedBefore.No)]
            public void Or(string firstInt, string otherInt, ExecutedBefore expected)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public static bool Get(int a, int b) => a == 1 || b == 2;
    }
}");
                var first = syntaxTree.FindLiteralExpression(firstInt);
                var other = syntaxTree.FindLiteralExpression(otherInt);
                Assert.AreEqual(expected, first.IsExecutedBefore(other));
            }

            [TestCase("1", "2", ExecutedBefore.Yes)]
            [TestCase("2", "1", ExecutedBefore.No)]
            public void LambdaLocal(string firstInt, string otherInt, ExecutedBefore expected)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public Foo()
        {
            this.E += (_, __) =>
            {
                var a = 1;
                a = 2;
            };
        }

        public event EventHandler E;
    }
}");
                var first = syntaxTree.FindLiteralExpression(firstInt);
                Assert.AreEqual(expected, first.IsExecutedBefore(syntaxTree.FindLiteralExpression(otherInt)));
                Assert.AreEqual(expected, first.IsExecutedBefore(syntaxTree.FindStatement(otherInt)));
            }

            [TestCase("1", "2", ExecutedBefore.Yes)]
            [TestCase("2", "1", ExecutedBefore.No)]
            [TestCase("1", "3", ExecutedBefore.Yes)]
            [TestCase("3", "1", ExecutedBefore.No)]
            [TestCase("2", "3", ExecutedBefore.Maybe)]
            [TestCase("3", "2", ExecutedBefore.Maybe)]
            [TestCase("4", "5", ExecutedBefore.Yes)]
            [TestCase("5", "4", ExecutedBefore.No)]
            [TestCase("3", "4", ExecutedBefore.Maybe)]
            [TestCase("4", "3", ExecutedBefore.Maybe)]
            public void LambdaLocalClosure(string firstInt, string otherInt, ExecutedBefore expected)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public Foo()
        {
            var a = 1;
            this.E += (_, __) => a = 3;
            this.E += (_, __) =>
            {
                a = 4;
                a = 5;
            };
            a = 2;
        }

        public event EventHandler E;
    }
}");
                var first = syntaxTree.FindLiteralExpression(firstInt);
                var other = syntaxTree.FindLiteralExpression(otherInt);
                Assert.AreEqual(expected, first.IsExecutedBefore(other));
            }

            [TestCase("1", "2", ExecutedBefore.Yes)]
            [TestCase("2", "1", ExecutedBefore.No)]
            [TestCase("1", "3", ExecutedBefore.Yes)]
            [TestCase("3", "1", ExecutedBefore.No)]
            [TestCase("2", "3", ExecutedBefore.Maybe)]
            [TestCase("3", "2", ExecutedBefore.Maybe)]
            [TestCase("4", "5", ExecutedBefore.Yes)]
            [TestCase("5", "4", ExecutedBefore.No)]
            [TestCase("3", "4", ExecutedBefore.Maybe)]
            [TestCase("4", "3", ExecutedBefore.Maybe)]
            public void LambdaParameterClosure(string firstInt, string otherInt, ExecutedBefore expected)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
    using System;

    public class Foo
    {
        public Foo(int a)
        {
            a = 1;
            this.E += (_, __) => a = 3;
            this.E += (_, __) =>
            {
                a = 4;
                a = 5;
            };
            a = 2;
        }

        public event EventHandler E;
    }
}");
                var first = syntaxTree.FindLiteralExpression(firstInt);
                var other = syntaxTree.FindLiteralExpression(otherInt);
                Assert.AreEqual(expected, first.IsExecutedBefore(other));
            }
        }
    }
}
