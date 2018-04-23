namespace Gu.Roslyn.CodeFixExtensions.Tests
{
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public partial class CodeStyleTests
    {
        public class BackingFieldsAdjacent
        {
            [Test]
            public void DefaultsToStyleCop()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
public class Foo
{
    private int value1;

    public int Value1
    {
        get => this.value1;
        set => this.value1 = value;
    }
}
}");
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree });
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                Assert.AreEqual(false, CodeStyle.BackingFieldsAdjacent(semanticModel, out _));
            }

            [Test]
            public void WhenStyleCop()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
public class Foo
{
    private int value1;

    public Foo()
    {
    }

    public int Value1
    {
        get => this.value1;
        set => this.value1 = value;
    }
}
}");
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree });
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                Assert.AreEqual(false, CodeStyle.BackingFieldsAdjacent(semanticModel, out _));
            }

            [Test]
            public void WhenAdjacentNoEmptyLine()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
public class Foo
{
    public Foo()
    {
    }

    private int value1;
    public int Value1
    {
        get => this.value1;
        set => this.value1 = value;
    }
}
}");
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree });
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                Assert.AreEqual(true, CodeStyle.BackingFieldsAdjacent(semanticModel, out var newLineBetween));
                Assert.AreEqual(false, newLineBetween);
            }

            [Test]
            public void WhenAdjacentWithEmptyLine()
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandbox
{
public class Foo
{
    public Foo()
    {
    }

    private int value1;

    public int Value1
    {
        get => this.value1;
        set => this.value1 = value;
    }
}
}");
                var compilation = CSharpCompilation.Create("test", new[] { syntaxTree });
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                Assert.AreEqual(true, CodeStyle.BackingFieldsAdjacent(semanticModel, out var newLineBetween));
                Assert.AreEqual(true, newLineBetween);
            }
        }
    }
}
