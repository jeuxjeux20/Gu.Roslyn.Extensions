namespace Gu.Roslyn.AnalyzerExtensions
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// A wrapper for a local or a parameter.
    /// </summary>
    [DebuggerDisplay("{this.Symbol}")]
    public struct LocalOrParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocalOrParameter"/> struct.
        /// </summary>
        /// <param name="local">The <see cref="ILocalSymbol"/>.</param>
        public LocalOrParameter(ILocalSymbol local)
            : this((ISymbol)local)
        {
            if (local == null)
            {
                throw new ArgumentNullException(nameof(local));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalOrParameter"/> struct.
        /// </summary>
        /// <param name="parameter">The <see cref="IParameterSymbol"/>.</param>
        public LocalOrParameter(IParameterSymbol parameter)
            : this((ISymbol)parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }
        }

        private LocalOrParameter(ISymbol symbol)
        {
            this.Symbol = symbol;
        }

        /// <summary>
        /// Gets the symbol.
        /// </summary>
        public ISymbol Symbol { get; }

        /// <summary>
        /// Gets the type.
        /// </summary>
        public ITypeSymbol Type => (this.Symbol as ILocalSymbol)?.Type ??
                                   ((IParameterSymbol)this.Symbol).Type;

        /// <summary>
        /// Gets the containing symbol.
        /// </summary>
        public ISymbol ContainingSymbol => this.Symbol.ContainingSymbol;

        /// <summary> Gets the symbol name. Returns the empty string if unnamed. </summary>
        public string Name => (this.Symbol as ILocalSymbol)?.Name ?? ((IParameterSymbol)this.Symbol).Name;

        /// <summary>
        /// Try create a <see cref="LocalOrParameter"/> from <paramref name="symbol"/>.
        /// </summary>
        /// <param name="symbol">The <see cref="ISymbol"/>.</param>
        /// <param name="result">The <see cref="LocalOrParameter"/> if symbol was a local or a parameter.</param>
        /// <returns>True if created a <see cref="LocalOrParameter"/> from <paramref name="symbol"/>.</returns>
        public static bool TryCreate(ISymbol symbol, out LocalOrParameter result)
        {
            switch (symbol)
            {
                case ILocalSymbol local:
                    result = new LocalOrParameter(local);
                    return true;
                case IParameterSymbol parameter:
                    result = new LocalOrParameter(parameter);
                    return true;
                default:
                    result = default(LocalOrParameter);
                    return false;
            }
        }

        /// <summary>
        /// Try to get the scope where <see cref="Symbol"/> is visible.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <param name="scope">The scope.</param>
        /// <returns>True if a scope could be determined.</returns>
        public bool TryGetScope(CancellationToken cancellationToken, out SyntaxNode scope)
        {
            switch (this.Symbol)
            {
                case ILocalSymbol local:
                    return local.TryGetScope(cancellationToken, out scope);
                case IParameterSymbol parameter:
                    return parameter.ContainingSymbol.TrySingleDeclaration(cancellationToken, out scope);
                default:
                    throw new InvalidOperationException("Should never get here.");
            }
        }
    }
}
