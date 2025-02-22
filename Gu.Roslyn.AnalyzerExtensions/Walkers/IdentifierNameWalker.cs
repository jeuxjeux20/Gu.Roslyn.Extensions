namespace Gu.Roslyn.AnalyzerExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Get all <see cref="IdentifierNameSyntax"/> in the scope.
    /// </summary>
    public sealed class IdentifierNameWalker : PooledWalker<IdentifierNameWalker>
    {
        private readonly List<IdentifierNameSyntax> identifierNames = new List<IdentifierNameSyntax>();

        private IdentifierNameWalker()
        {
        }

        /// <summary>
        /// Gets the <see cref="IdentifierNameSyntax"/>s found in the scope.
        /// </summary>
        public IReadOnlyList<IdentifierNameSyntax> IdentifierNames => this.identifierNames;

        /// <summary>
        /// Get a walker that has visited <paramref name="node"/>.
        /// </summary>
        /// <param name="node">The scope.</param>
        /// <returns>A walker that has visited <paramref name="node"/>.</returns>
        public static IdentifierNameWalker Borrow(SyntaxNode node) => BorrowAndVisit(node, () => new IdentifierNameWalker());

        /// <summary>
        /// Get a walker that has all uses of <paramref name="parameter"/>.
        /// </summary>
        /// <param name="parameter">The <see cref="IParameterSymbol"/>.</param>
        /// <param name="semanticModel">The <see cref="SemanticModel"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A walker that has all uses of <paramref name="parameter"/>.</returns>
        public static IdentifierNameWalker For(IParameterSymbol parameter, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (LocalOrParameter.TryCreate(parameter, out var localOrParameter))
            {
                return For(localOrParameter, semanticModel, cancellationToken);
            }

            return Borrow(() => new IdentifierNameWalker());
        }

        /// <summary>
        /// Get a walker that has all uses of <paramref name="local"/>.
        /// </summary>
        /// <param name="local">The <see cref="ILocalSymbol"/>.</param>
        /// <param name="semanticModel">The <see cref="SemanticModel"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A walker that has all uses of <paramref name="local"/>.</returns>
        public static IdentifierNameWalker For(ILocalSymbol local, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (LocalOrParameter.TryCreate(local, out var localOrParameter))
            {
                return For(localOrParameter, semanticModel, cancellationToken);
            }

            return Borrow(() => new IdentifierNameWalker());
        }

        /// <summary>
        /// Get a walker that has all uses of <paramref name="localOrParameter"/>.
        /// </summary>
        /// <param name="localOrParameter">The <see cref="LocalOrParameter"/>.</param>
        /// <param name="semanticModel">The <see cref="SemanticModel"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>A walker that has all uses of <paramref name="localOrParameter"/>.</returns>
        public static IdentifierNameWalker For(LocalOrParameter localOrParameter, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (localOrParameter.TryGetScope(cancellationToken, out var scope))
            {
                var walker = BorrowAndVisit(scope, () => new IdentifierNameWalker());
                walker.RemoveAll(x => !IsMatch(x));
                return walker;
            }

            return Borrow(() => new IdentifierNameWalker());

            bool IsMatch(IdentifierNameSyntax candidate)
            {
                return candidate.Identifier.Text == localOrParameter.Name &&
                       semanticModel.TryGetSymbol(candidate, cancellationToken, out ISymbol symbol) &&
                       symbol.Equals(localOrParameter.Symbol);
            }
        }

        /// <summary>
        /// Try find the first usage of <paramref name="symbol"/>.
        /// </summary>
        /// <param name="node">The scope.</param>
        /// <param name="symbol">The name.</param>
        /// <param name="semanticModel">The <see cref="SemanticModel"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <param name="identifierName">The <see cref="IdentifierNameSyntax"/>.</param>
        /// <returns>True if a match was found.</returns>
        public static bool TryFindFirst(SyntaxNode node, ISymbol symbol, SemanticModel semanticModel, CancellationToken cancellationToken, out IdentifierNameSyntax identifierName)
        {
            if (symbol != null)
            {
                using (var walker = Borrow(node))
                {
                    return walker.TryFindFirst(symbol, semanticModel, cancellationToken, out identifierName);
                }
            }

            identifierName = null;
            return false;
        }

        /// <summary>
        /// Try find the last usage of <paramref name="symbol"/>.
        /// </summary>
        /// <param name="node">The scope.</param>
        /// <param name="symbol">The name.</param>
        /// <param name="semanticModel">The <see cref="SemanticModel"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <param name="identifierName">The <see cref="IdentifierNameSyntax"/>.</param>
        /// <returns>True if a match was found.</returns>
        public static bool TryFindLast(SyntaxNode node, ISymbol symbol, SemanticModel semanticModel, CancellationToken cancellationToken, out IdentifierNameSyntax identifierName)
        {
            if (symbol != null)
            {
                using (var walker = Borrow(node))
                {
                    return walker.TryFindLast(symbol, semanticModel, cancellationToken, out identifierName);
                }
            }

            identifierName = null;
            return false;
        }

        /// <inheritdoc />
        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            this.identifierNames.Add(node);
            base.VisitIdentifierName(node);
        }

        /// <summary>
        /// Try find an <see cref="IdentifierNameSyntax"/> by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="identifierName">The <see cref="IdentifierNameSyntax"/>.</param>
        /// <returns>True if a match was found.</returns>
        public bool TryFind(string name, out IdentifierNameSyntax identifierName)
        {
            foreach (var candidate in this.identifierNames)
            {
                if (candidate.Identifier.ValueText == name)
                {
                    identifierName = candidate;
                    return true;
                }
            }

            identifierName = null;
            return false;
        }

        /// <summary>
        /// Try find the first usage of <paramref name="symbol"/>.
        /// </summary>
        /// <param name="symbol">The name.</param>
        /// <param name="semanticModel">The <see cref="SemanticModel"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <param name="identifierName">The <see cref="IdentifierNameSyntax"/>.</param>
        /// <returns>True if a match was found.</returns>
        public bool TryFindFirst(ISymbol symbol, SemanticModel semanticModel, CancellationToken cancellationToken, out IdentifierNameSyntax identifierName)
        {
            foreach (var candidate in this.identifierNames)
            {
                if (candidate.IsSymbol(symbol, semanticModel, cancellationToken))
                {
                    identifierName = candidate;
                    return true;
                }
            }

            identifierName = null;
            return false;
        }

        /// <summary>
        /// Try find the last usage of <paramref name="symbol"/>.
        /// </summary>
        /// <param name="symbol">The name.</param>
        /// <param name="semanticModel">The <see cref="SemanticModel"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <param name="identifierName">The <see cref="IdentifierNameSyntax"/>.</param>
        /// <returns>True if a match was found.</returns>
        public bool TryFindLast(ISymbol symbol, SemanticModel semanticModel, CancellationToken cancellationToken, out IdentifierNameSyntax identifierName)
        {
            for (var i = this.identifierNames.Count - 1; i >= 0; i--)
            {
                var candidate = this.identifierNames[i];
                if (candidate.IsSymbol(symbol, semanticModel, cancellationToken))
                {
                    identifierName = candidate;
                    return true;
                }
            }

            identifierName = null;
            return false;
        }

        /// <summary>
        /// Filters by <paramref name="match"/>.
        /// </summary>
        /// <param name="match">The predicate for finding items to remove.</param>
        public void RemoveAll(Predicate<IdentifierNameSyntax> match) => this.identifierNames.RemoveAll(match);

        /// <inheritdoc />
        protected override void Clear()
        {
            this.identifierNames.Clear();
        }
    }
}
