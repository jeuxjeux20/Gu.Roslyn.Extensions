namespace Gu.Roslyn.CodeFixExtensions
{
    using System;
    using System.Linq;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Helpers for parsing members
    /// </summary>
    public static class Parse
    {
        /// <summary>
        /// Parse a <see cref="FieldDeclarationSyntax"/> from a string.
        /// </summary>
        /// <param name="code">The code text.</param>
        /// <returns>The <see cref="FieldDeclarationSyntax"/></returns>
        public static FieldDeclarationSyntax FieldDeclaration(string code)
        {
            return (FieldDeclarationSyntax)SyntaxFactory.ParseCompilationUnit(code).Members.Single();
        }

        /// <summary>
        /// Parse a <see cref="ConstructorDeclarationSyntax"/> from a string.
        /// </summary>
        /// <param name="code">The code text.</param>
        /// <returns>The <see cref="ConstructorDeclarationSyntax"/></returns>
        public static ConstructorDeclarationSyntax ConstructorDeclaration(string code)
        {
            return (ConstructorDeclarationSyntax)SyntaxFactory.ParseCompilationUnit(code).Members.Single();
        }

        /// <summary>
        /// Parse a <see cref="EventFieldDeclarationSyntax"/> from a string.
        /// </summary>
        /// <param name="code">The code text.</param>
        /// <returns>The <see cref="EventFieldDeclarationSyntax"/></returns>
        public static EventFieldDeclarationSyntax EventFieldDeclaration(string code)
        {
            return (EventFieldDeclarationSyntax)SyntaxFactory.ParseCompilationUnit(code).Members.Single();
        }

        /// <summary>
        /// Parse a <see cref="EventDeclarationSyntax"/> from a string.
        /// </summary>
        /// <param name="code">The code text.</param>
        /// <returns>The <see cref="EventDeclarationSyntax"/></returns>
        public static EventDeclarationSyntax EventDeclaration(string code)
        {
            return (EventDeclarationSyntax)SyntaxFactory.ParseCompilationUnit(code).Members.Single();
        }

        /// <summary>
        /// Parse a <see cref="PropertyDeclarationSyntax"/> from a string.
        /// </summary>
        /// <param name="code">The code text.</param>
        /// <returns>The <see cref="PropertyDeclarationSyntax"/></returns>
        public static PropertyDeclarationSyntax PropertyDeclaration(string code)
        {
            return (PropertyDeclarationSyntax)SyntaxFactory.ParseCompilationUnit(code).Members.Single();
        }

        /// <summary>
        /// Parse a <see cref="MethodDeclarationSyntax"/> from a string.
        /// </summary>
        /// <param name="code">The code text.</param>
        /// <returns>The <see cref="MethodDeclarationSyntax"/></returns>
        public static MethodDeclarationSyntax MethodDeclaration(string code)
        {
            return (MethodDeclarationSyntax)SyntaxFactory.ParseCompilationUnit(code).Members.Single();
        }

        /// <summary>
        /// Parse a <see cref="XmlElementSyntax"/> from a string.
        /// </summary>
        /// <param name="code">The element text including start and end tags.</param>
        /// <returns>The <see cref="XmlElementSyntax"/></returns>
        public static XmlElementSyntax XmlElementSyntax(string code)
        {
            if (SyntaxFactory.ParseLeadingTrivia($"/// {code}").TrySingle(x => x.HasStructure, out var trivia) &&
                trivia.GetStructure() is DocumentationCommentTriviaSyntax triviaSyntax &&
                triviaSyntax.Content.TrySingleOfType(out Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax element))
            {
                return element;
            }

            throw new InvalidOperationException($"Failed parsing {code} into an XmlElementSyntax");
        }

        /// <summary>
        /// Parse a <see cref="DocumentationCommentTriviaSyntax"/> from a string.
        /// </summary>
        /// <param name="code">The element text including start and end tags.</param>
        /// <returns>The <see cref="DocumentationCommentTriviaSyntax"/></returns>
        public static DocumentationCommentTriviaSyntax DocumentationCommentTriviaSyntax(string code)
        {
            if (SyntaxFactory.ParseLeadingTrivia(code).TrySingle(x => x.HasStructure, out var trivia) &&
                trivia.GetStructure() is DocumentationCommentTriviaSyntax triviaSyntax)
            {
                return triviaSyntax;
            }

            throw new InvalidOperationException($"Failed parsing {code} into an XmlElementSyntax");
        }
    }
}
