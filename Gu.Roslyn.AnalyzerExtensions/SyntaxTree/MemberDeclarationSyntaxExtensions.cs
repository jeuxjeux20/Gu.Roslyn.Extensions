namespace Gu.Roslyn.AnalyzerExtensions
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Helper methods for <see cref="MemberDeclarationSyntax"/>.
    /// </summary>
    public static class MemberDeclarationSyntaxExtensions
    {
        /// <summary>
        /// Get the <see cref="DocumentationCommentTriviaSyntax"/> for the accessor if it exists.
        /// </summary>
        /// <param name="member">The <see cref="MemberDeclarationSyntax"/>.</param>
        /// <param name="comment">The returned <see cref="DocumentationCommentTriviaSyntax"/>.</param>
        /// <returns>True if a single <see cref="DocumentationCommentTriviaSyntax"/> was found.</returns>
        public static bool TryGetDocumentationComment(this MemberDeclarationSyntax member, out DocumentationCommentTriviaSyntax comment)
        {
            if (member.HasLeadingTrivia &&
                member.GetLeadingTrivia() is SyntaxTriviaList triviaList &&
                triviaList.TrySingle(x => x.HasStructure && x.GetStructure() is DocumentationCommentTriviaSyntax, out var commentTrivia) &&
                commentTrivia.GetStructure() is DocumentationCommentTriviaSyntax triviaSyntax)
            {
                comment = triviaSyntax;
                return true;
            }

            comment = null;
            return false;
        }

        /// <summary>
        /// Get the leading whitespace for the accessor.
        /// </summary>
        /// <param name="member">The <see cref="MemberDeclarationSyntax"/>.</param>
        /// <returns>The string with the leading whitespace.</returns>
        public static string LeadingWhitespace(this MemberDeclarationSyntax member)
        {
            if (member.HasLeadingTrivia &&
                member.GetLeadingTrivia() is var triviaList &&
                triviaList.TryFirst(x => x.IsKind(SyntaxKind.WhitespaceTrivia), out var trivia))
            {
                return trivia.ToString();
            }

            return null;
        }

        /// <summary>
        /// Get the leading whitespace for the accessor.
        /// </summary>
        /// <param name="accessor">The <see cref="MemberDeclarationSyntax"/>.</param>
        /// <returns>The string with the leading whitespace.</returns>
        public static string LeadingWhitespace(this AccessorDeclarationSyntax accessor)
        {
            if (accessor.HasLeadingTrivia &&
                accessor.GetLeadingTrivia() is var triviaList &&
                triviaList.TryFirst(x => x.IsKind(SyntaxKind.WhitespaceTrivia), out var trivia))
            {
                return trivia.ToString();
            }

            return null;
        }
    }
}
