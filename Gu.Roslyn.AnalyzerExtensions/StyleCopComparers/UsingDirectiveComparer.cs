namespace Gu.Roslyn.AnalyzerExtensions.StyleCopComparers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <inheritdoc />
    public sealed class UsingDirectiveComparer : IComparer<UsingDirectiveSyntax>
    {
        /// <summary> The default instance. </summary>
        public static readonly UsingDirectiveComparer Default = new UsingDirectiveComparer();
        private static readonly StringComparer OrdinalIgnoreCase = StringComparer.OrdinalIgnoreCase;

        /// <summary>Compares two nodes and returns a value indicating whether one is less than, equal to, or greater than the other according to StyleCop.</summary>
        /// <returns>A signed integer that indicates if the node should be before the other according to StyleCop.</returns>
        /// <param name="x">The first node to compare.</param>
        /// <param name="y">The second node to compare.</param>
        public static int Compare(UsingDirectiveSyntax x, UsingDirectiveSyntax y)
        {
            if (TryGetRoot(x.Name, out var xn) &&
                TryGetRoot(y.Name, out var yn))
            {
                var xText = xn.Identifier.ValueText;
                var yText = yn.Identifier.ValueText;
                if (xText != yText)
                {
                    if (xText == "System")
                    {
                        return -1;
                    }

                    if (yText == "System")
                    {
                        return 1;
                    }

                    return OrdinalIgnoreCase.Compare(xText, yText);
                }

                return CompareRecursive(xn.Parent as QualifiedNameSyntax, yn.Parent as QualifiedNameSyntax);
            }

            return 0;
        }

        /// <summary>
        /// Returns true for:
        /// namespace Foo and using Foo;
        /// namespace Foo.Bar and using Foo;
        /// </summary>
        /// <param name="namespaceDeclarationSyntax">The <see cref="NamespaceDeclarationSyntax"/>.</param>
        /// <param name="usingDirective">The <see cref="UsingDirectiveSyntax"/>.</param>
        /// <returns>True if the using directive is not needed.</returns>
        public static bool IsSameOrContained(NamespaceDeclarationSyntax namespaceDeclarationSyntax, UsingDirectiveSyntax usingDirective)
        {
            if (TryGetRoot(namespaceDeclarationSyntax.Name, out var nameSpaceName) &&
                TryGetRoot(usingDirective.Name, out var usingName))
            {
                if (nameSpaceName.Identifier.ValueText != usingName.Identifier.ValueText)
                {
                    return false;
                }

                var namespaceParent = nameSpaceName.Parent as QualifiedNameSyntax;
                var usingParent = usingName.Parent as QualifiedNameSyntax;
                while (true)
                {
                    if (usingParent == null)
                    {
                        return true;
                    }

                    if (namespaceParent == null ||
                        namespaceParent.Right.Identifier.ValueText != usingParent.Right.Identifier.ValueText)
                    {
                        return false;
                    }

                    namespaceParent = namespaceParent.Parent as QualifiedNameSyntax;
                    usingParent = usingParent.Parent as QualifiedNameSyntax;
                }
            }

            return false;
        }

        /// <inheritdoc />
        int IComparer<UsingDirectiveSyntax>.Compare(UsingDirectiveSyntax x, UsingDirectiveSyntax y) => Compare(x, y);

        private static bool TryGetRoot(NameSyntax uds, out SimpleNameSyntax name)
        {
            if (uds is SimpleNameSyntax simpleName)
            {
                name = simpleName;
                return true;
            }

            if (uds is QualifiedNameSyntax qns)
            {
                return TryGetRoot(qns.Left, out name);
            }

            name = null;
            return false;
        }

        private static int CompareRecursive(QualifiedNameSyntax xqn, QualifiedNameSyntax yqn)
        {
            if (xqn == null && yqn == null)
            {
                return 0;
            }

            if (xqn == null)
            {
                return -1;
            }

            if (yqn == null)
            {
                return 1;
            }

            var compare = OrdinalIgnoreCase.Compare(xqn.Right.Identifier.ValueText, yqn.Right.Identifier.ValueText);
            if (compare == 0)
            {
                return CompareRecursive(xqn.Parent as QualifiedNameSyntax, yqn.Parent as QualifiedNameSyntax);
            }

            return compare;
        }
    }
}
