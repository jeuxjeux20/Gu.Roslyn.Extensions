namespace Gu.Roslyn.AnalyzerExtensions
{
    using System;
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

    public class NamedTypeSymbolComparer : IEqualityComparer<INamedTypeSymbol>
    {
        public static readonly NamedTypeSymbolComparer Default = new NamedTypeSymbolComparer();

        private NamedTypeSymbolComparer()
        {
        }

        public static bool Equals(INamedTypeSymbol x, INamedTypeSymbol y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x == null ||
                y == null)
            {
                return false;
            }

            if (x.MetadataName != y.MetadataName ||
                x.ContainingNamespace?.MetadataName != y.ContainingNamespace?.MetadataName ||
                x.Arity != y.Arity)
            {
                return false;
            }

            for (var i = 0; i < x.Arity; i++)
            {
                if (!TypeSymbolComparer.Equals(x.TypeArguments[i], y.TypeArguments[i]))
                {
                    return false;
                }
            }

            return true;
        }

        //// ReSharper disable once UnusedMember.Global
        //// ReSharper disable UnusedParameter.Global
        [Obsolete("Should only be called with arguments of type INamedTypeSymbol.", error: true)]
#pragma warning disable SA1313 // Parameter names must begin with lower-case letter
        public static new bool Equals(object _, object __) => throw new InvalidOperationException("This is hidden so that it is not called by accident.");
#pragma warning restore SA1313 // Parameter names must begin with lower-case letter
        //// ReSharper restore UnusedParameter.Global

        /// <inheritdoc />
        bool IEqualityComparer<INamedTypeSymbol>.Equals(INamedTypeSymbol x, INamedTypeSymbol y) => Equals(x, y);

        /// <inheritdoc/>
        public int GetHashCode(INamedTypeSymbol obj)
        {
            return obj?.MetadataName.GetHashCode() ?? 0;
        }
    }
}
