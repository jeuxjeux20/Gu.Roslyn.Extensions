// ReSharper disable UnusedMember.Global
namespace Gu.Roslyn.AnalyzerExtensions
{
    using System;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// The safe versions handle situations like partial classes when the node is not in the same syntax tree.
    /// </summary>
    public static partial class SemanticModelExt
    {
        /// <summary>
        /// Try getting the GetConstantValue for the node.
        /// Gets the semantic model for the tree if the node is not in the tree corresponding to <paramref name="semanticModel"/>.
        /// </summary>
        /// <typeparam name="T">The symbol.</typeparam>
        /// <param name="semanticModel">The <see cref="SemanticModel"/>.</param>
        /// <param name="node">The <see cref="SyntaxNode"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <param name="value">The symbol if found.</param>
        /// <returns>True if a symbol was found.</returns>
        public static bool TryGetConstantValue<T>(this SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken, out T value)
        {
            if (semanticModel.GetConstantValueSafe(node, cancellationToken) is Optional<object> optional &&
                optional.HasValue)
            {
                if (optional.Value is T temp)
                {
                    value = temp;
                    return true;
                }

                if (optional.Value == null)
                {
                    value = default(T);
                    return default(T) == null;
                }

                // We can't use GetTypeInfo() here as it brings in System.Reflection.Extensions that does not work in VS.
                if (default(T) is Enum &&
                    Enum.GetUnderlyingType(typeof(T)) == optional.Value.GetType())
                {
                    // ReSharper disable once PossibleInvalidCastException
                    value = (T)optional.Value;
                    return true;
                }
            }

            value = default(T);
            return false;
        }

        /// <summary>
        /// Try getting the GetTypeInfo for the node.
        /// Gets the semantic model for the tree if the node is not in the tree corresponding to <paramref name="semanticModel"/>.
        /// </summary>
        /// <param name="semanticModel">The <see cref="SemanticModel"/>.</param>
        /// <param name="node">The <see cref="SyntaxNode"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <param name="type">The symbol if found.</param>
        /// <returns>True if a symbol was found.</returns>
        public static bool TryGetType(this SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken, out ITypeSymbol type)
        {
            if (semanticModel.GetTypeInfoSafe(node, cancellationToken).Type is ITypeSymbol temp)
            {
                type = temp;
                return true;
            }

            type = null;
            return false;
        }

        /// <summary>
        /// Check if (destination)(object)expression will work.
        /// </summary>
        /// <param name="semanticModel">The <see cref="SemanticModel"/>.</param>
        /// <param name="expression">The expression containing the value.</param>
        /// <param name="destination">The type to cast to.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>True if a boxed instance can be cast.</returns>
        public static bool IsRepresentationPreservingConversion(this SemanticModel semanticModel, ExpressionSyntax expression, ITypeSymbol destination, CancellationToken cancellationToken)
        {
            if (expression == null || destination == null)
            {
                return false;
            }

            var conversion = semanticModel.SemanticModelFor(expression)
                                          .ClassifyConversion(expression, destination, isExplicitInSource: true);
            if (conversion.IsNumeric)
            {
                return conversion.IsIdentity;
            }

            if (conversion.IsNullable)
            {
                return conversion.IsIdentity ||
                       (destination is INamedTypeSymbol namedType &&
                        namedType.TypeArguments.TrySingle(out var typeArg) &&
                        IsRepresentationPreservingConversion(semanticModel, expression, typeArg, cancellationToken));
            }

            if (conversion.IsUnboxing)
            {
                switch (expression)
                {
                    case CastExpressionSyntax cast:
                        return IsRepresentationPreservingConversion(semanticModel, cast.Expression, destination, cancellationToken);
                    case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.AsExpression):
                        return IsRepresentationPreservingConversion(semanticModel, binary.Left, destination, cancellationToken);
                }

                return false;
            }

            return conversion.IsImplicit;
        }

        /// <summary>
        /// Same as SemanticModel.GetConstantValue().Symbol but works when <paramref name="node"/> is not in the syntax tree.
        /// Gets the semantic model for the tree if the node is not in the tree corresponding to <paramref name="semanticModel"/>.
        /// </summary>
        /// <param name="semanticModel">The <see cref="SemanticModel"/>.</param>
        /// <param name="node">The <see cref="SyntaxNode"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>An <see cref="ISymbol"/> or null.</returns>
        public static Optional<object> GetConstantValueSafe(this SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken)
        {
            return semanticModel.SemanticModelFor(node)
                                ?.GetConstantValue(node, cancellationToken) ?? default(Optional<object>);
        }

        /// <summary>
        /// Same as SemanticModel.GetTypeInfo().Symbol but works when <paramref name="node"/> is not in the syntax tree.
        /// Gets the semantic model for the tree if the node is not in the tree corresponding to <paramref name="semanticModel"/>.
        /// </summary>
        /// <param name="semanticModel">The <see cref="SemanticModel"/>.</param>
        /// <param name="node">The <see cref="SyntaxNode"/>.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>An <see cref="Microsoft.CodeAnalysis.TypeInfo"/> or default(TypeInfo).</returns>
        public static TypeInfo GetTypeInfoSafe(this SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken)
        {
            return semanticModel.SemanticModelFor(node)
                                ?.GetTypeInfo(node, cancellationToken) ?? default(TypeInfo);
        }

        /// <summary>
        /// Gets the semantic model for <paramref name="expression"/>
        /// This can be needed for partial classes.
        /// </summary>
        /// <param name="semanticModel">The semantic model.</param>
        /// <param name="expression">The expression.</param>
        /// <returns>The semantic model that corresponds to <paramref name="expression"/>.</returns>
        public static SemanticModel SemanticModelFor(this SemanticModel semanticModel, SyntaxNode expression)
        {
            if (semanticModel == null ||
                expression == null ||
                expression.IsMissing)
            {
                return null;
            }

            if (ReferenceEquals(semanticModel.SyntaxTree, expression.SyntaxTree))
            {
                return semanticModel;
            }

            return Cache.GetOrAdd(expression.SyntaxTree, GetSemanticModel);

            SemanticModel GetSemanticModel(SyntaxTree syntaxTree)
            {
                if (semanticModel.Compilation.ContainsSyntaxTree(expression.SyntaxTree))
                {
                    return semanticModel.Compilation.GetSemanticModel(expression.SyntaxTree);
                }

                foreach (var metadataReference in semanticModel.Compilation.References)
                {
                    if (metadataReference is CompilationReference compilationReference &&
                        compilationReference.Compilation.ContainsSyntaxTree(expression.SyntaxTree))
                    {
                        return compilationReference.Compilation.GetSemanticModel(expression.SyntaxTree);
                    }
                }

                return null;
            }
        }
    }
}
