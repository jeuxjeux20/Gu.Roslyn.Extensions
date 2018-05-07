namespace Gu.Roslyn.AnalyzerExtensions
{
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public sealed class AssignmentExecutionWalker : ExecutionWalker<AssignmentExecutionWalker>
    {
        private readonly List<AssignmentExpressionSyntax> assignments = new List<AssignmentExpressionSyntax>();
        private readonly List<ArgumentSyntax> arguments = new List<ArgumentSyntax>();
        private readonly List<LocalDeclarationStatementSyntax> localDeclarations = new List<LocalDeclarationStatementSyntax>();

        private AssignmentExecutionWalker()
        {
        }

        /// <summary>
        /// Gets a list with all <see cref="AssignmentExpressionSyntax"/> in the scope.
        /// </summary>
        public IReadOnlyList<AssignmentExpressionSyntax> Assignments => this.assignments;

        /// <inheritdoc />
        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            this.assignments.Add(node);
            base.VisitAssignmentExpression(node);
        }

        /// <inheritdoc />
        public override void VisitArgument(ArgumentSyntax node)
        {
            this.arguments.Add(node);
            base.VisitArgument(node);
        }

        /// <inheritdoc />
        public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            this.localDeclarations.Add(node);
            base.VisitLocalDeclarationStatement(node);
        }

        /// <summary>
        /// Get a walker that has visited <paramref name="node"/>
        /// </summary>
        /// <param name="node">The node</param>
        /// <param name="scope">The scope</param>
        /// <param name="semanticModel">The <see cref="SemanticModel"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns>A walker that has visited <paramref name="node"/></returns>
        public static AssignmentExecutionWalker Borrow(SyntaxNode node, Scope scope, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            return BorrowAndVisit(node, scope, semanticModel, cancellationToken, () => new AssignmentExecutionWalker());
        }

        /// <summary>
        /// Get the first <see cref="AssignmentExpressionSyntax"/> for <paramref name="symbol"/>
        /// </summary>
        /// <param name="symbol">The symbol to find assignments for.</param>
        /// <param name="node">The node to walk.</param>
        /// <param name="scope">The scope</param>
        /// <param name="semanticModel">The <see cref="SemanticModel"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <param name="assignment">The first assignment if any.</param>
        /// <returns>True if an assignment was found for the symbol.</returns>
        public static bool FirstFor(ISymbol symbol, SyntaxNode node, Scope scope, SemanticModel semanticModel, CancellationToken cancellationToken, out AssignmentExpressionSyntax assignment)
        {
            assignment = null;
            if (symbol == null ||
                node == null)
            {
                return false;
            }

            using (var walker = Borrow(node, scope, semanticModel, cancellationToken))
            {
                foreach (var candidate in walker.Assignments)
                {
                    if (semanticModel.TryGetSymbol(candidate.Left, cancellationToken, out ISymbol assignedSymbol) &&
                        SymbolComparer.Equals(symbol, assignedSymbol))
                    {
                        assignment = candidate;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Get the only <see cref="AssignmentExpressionSyntax"/> for <paramref name="symbol"/>
        /// </summary>
        /// <param name="symbol">The symbol to find assignments for.</param>
        /// <param name="node">The node to walk.</param>
        /// <param name="scope">The scope</param>
        /// <param name="semanticModel">The <see cref="SemanticModel"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <param name="assignment">The single assignment.</param>
        /// <returns>True if a single assignment was found for the symbol.</returns>
        public static bool SingleFor(ISymbol symbol, SyntaxNode node, Scope scope, SemanticModel semanticModel, CancellationToken cancellationToken, out AssignmentExpressionSyntax assignment)
        {
            assignment = null;
            if (symbol == null ||
                node == null)
            {
                return false;
            }

            using (var walker = Borrow(node, scope, semanticModel, cancellationToken))
            {
                foreach (var candidate in walker.Assignments)
                {
                    if (semanticModel.TryGetSymbol(candidate.Left, cancellationToken, out ISymbol assignedSymbol) &&
                        SymbolComparer.Equals(symbol, assignedSymbol))
                    {
                        if (assignment != null)
                        {
                            assignment = null;
                            return false;
                        }

                        assignment = candidate;
                    }
                }
            }

            return assignment != null;
        }

        /// <summary>
        /// Get the only <see cref="AssignmentExpressionSyntax"/> for <paramref name="symbol"/>
        /// </summary>
        /// <param name="symbol">The symbol to find assignments for.</param>
        /// <param name="node">The node to walk.</param>
        /// <param name="scope">The scope</param>
        /// <param name="semanticModel">The <see cref="SemanticModel"/></param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <param name="assignment">The single assignment.</param>
        /// <returns>True if a single assignment was found for the symbol.</returns>
        public static bool FirstWith(ISymbol symbol, SyntaxNode node, Scope scope, SemanticModel semanticModel, CancellationToken cancellationToken, out AssignmentExpressionSyntax assignment)
        {
            return FirstWith(symbol, node, scope, semanticModel, cancellationToken, out assignment, null);
        }

        /// <inheritdoc />
        protected override void Clear()
        {
            this.assignments.Clear();
            this.arguments.Clear();
            this.SemanticModel = null;
            this.CancellationToken = CancellationToken.None;
            base.Clear();
        }

        private static bool FirstWith(ISymbol symbol, SyntaxNode node, Scope scope, SemanticModel semanticModel, CancellationToken cancellationToken, out AssignmentExpressionSyntax assignment, PooledSet<ISymbol> visited = null)
        {
            assignment = null;
            if (symbol == null ||
                node == null)
            {
                return false;
            }

            using (var walker = Borrow(node, scope, semanticModel, cancellationToken))
            {
                foreach (var candidate in walker.assignments)
                {
                    if (IsMatch(symbol, candidate.Right, semanticModel, cancellationToken))
                    {
                        assignment = candidate;
                        return true;
                    }
                }

                foreach (var declaration in walker.localDeclarations)
                {
                    if (declaration.Declaration is VariableDeclarationSyntax variableDeclaration &&
                        variableDeclaration.Variables.TryFirst(x => x.Initializer != null, out var variable) &&
                        IsMatch(symbol, variable.Initializer.Value, semanticModel, cancellationToken) &&
                        semanticModel.GetDeclaredSymbolSafe(variable, cancellationToken) is ILocalSymbol local)
                    {
                        using (var current = visited.IncrementUsage())
                        {
                            if (current.Add(local) &&
                                FirstWith(local, node, scope, semanticModel, cancellationToken, out assignment, current))
                            {
                                return true;
                            }
                        }
                    }
                }

                if (scope != Scope.Member)
                {
                    foreach (var argument in walker.arguments)
                    {
                        if (argument.Expression is IdentifierNameSyntax identifierName &&
                            identifierName.Identifier.ValueText == symbol.Name &&
                            semanticModel.GetSymbolSafe(argument.Parent?.Parent, cancellationToken) is IMethodSymbol method &&
                            method.TrySingleDeclaration(cancellationToken, out BaseMethodDeclarationSyntax methodDeclaration) &&
                            method.TryFindParameter(argument, out var parameter))
                        {
                            using (var current = visited.IncrementUsage())
                            {
                                if (current.Add(parameter) &&
                                    FirstWith(parameter, methodDeclaration, scope, semanticModel, cancellationToken, out assignment, current))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        private static bool IsMatch(ISymbol symbol, ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            switch (expression)
            {
                case ConditionalExpressionSyntax conditional:
                    return IsMatch(symbol, conditional.WhenTrue, semanticModel, cancellationToken) ||
                           IsMatch(symbol, conditional.WhenFalse, semanticModel, cancellationToken);
                case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.CoalesceExpression):
                    return IsMatch(symbol, binary.Left, semanticModel, cancellationToken) ||
                           IsMatch(symbol, binary.Right, semanticModel, cancellationToken);
                case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.AsExpression):
                    return IsMatch(symbol, binary.Left, semanticModel, cancellationToken);
                case CastExpressionSyntax cast:
                    return IsMatch(symbol, cast.Expression, semanticModel, cancellationToken);
                case ObjectCreationExpressionSyntax objectCreation when objectCreation.ArgumentList != null && objectCreation.ArgumentList.Arguments.TryFirst(x => SymbolComparer.Equals(symbol, semanticModel.GetSymbolSafe(x.Expression, cancellationToken)), out ArgumentSyntax _):
                    return true;
                default:
                    if (symbol.IsEither<ILocalSymbol, IParameterSymbol>())
                    {
                        return expression is IdentifierNameSyntax identifierName &&
                               identifierName.Identifier.ValueText == symbol.Name &&
                               SymbolComparer.Equals(symbol, semanticModel.GetSymbolSafe(expression, cancellationToken));
                    }

                    return SymbolComparer.Equals(symbol, semanticModel.GetSymbolSafe(expression, cancellationToken));
            }
        }
    }
}
