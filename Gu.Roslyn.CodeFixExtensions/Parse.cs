namespace Gu.Roslyn.CodeFixExtensions
{
    using System;
    using System.Linq;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Helpers for parsing members.
    /// </summary>
    public static class Parse
    {
        /// <summary>
        /// Parse a <see cref="FieldDeclarationSyntax"/> from a string.
        /// </summary>
        /// <param name="code">The code text.</param>
        /// <param name="leadingWhitespace">The whitespace to adjust each row to have.</param>
        /// <returns>The <see cref="FieldDeclarationSyntax"/>.</returns>
        public static FieldDeclarationSyntax FieldDeclaration(string code, string leadingWhitespace = null)
        {
            return (FieldDeclarationSyntax)SyntaxFactory.ParseCompilationUnit(code.WithLeadingWhiteSpace(leadingWhitespace)).Members.Single();
        }

        /// <summary>
        /// Parse a <see cref="ConstructorDeclarationSyntax"/> from a string.
        /// </summary>
        /// <param name="code">The code text.</param>
        /// <param name="leadingWhitespace">The whitespace to adjust each row to have.</param>
        /// <returns>The <see cref="ConstructorDeclarationSyntax"/>.</returns>
        public static ConstructorDeclarationSyntax ConstructorDeclaration(string code, string leadingWhitespace = null)
        {
            return (ConstructorDeclarationSyntax)SyntaxFactory.ParseCompilationUnit(code.WithLeadingWhiteSpace(leadingWhitespace)).Members.Single();
        }

        /// <summary>
        /// Parse a <see cref="EventFieldDeclarationSyntax"/> from a string.
        /// </summary>
        /// <param name="code">The code text.</param>
        /// <param name="leadingWhitespace">The whitespace to adjust each row to have.</param>
        /// <returns>The <see cref="EventFieldDeclarationSyntax"/>.</returns>
        public static EventFieldDeclarationSyntax EventFieldDeclaration(string code, string leadingWhitespace = null)
        {
            return (EventFieldDeclarationSyntax)SyntaxFactory.ParseCompilationUnit(code.WithLeadingWhiteSpace(leadingWhitespace)).Members.Single();
        }

        /// <summary>
        /// Parse a <see cref="EventDeclarationSyntax"/> from a string.
        /// </summary>
        /// <param name="code">The code text.</param>
        /// <param name="leadingWhitespace">The whitespace to adjust each row to have.</param>
        /// <returns>The <see cref="EventDeclarationSyntax"/>.</returns>
        public static EventDeclarationSyntax EventDeclaration(string code, string leadingWhitespace = null)
        {
            return (EventDeclarationSyntax)SyntaxFactory.ParseCompilationUnit(code.WithLeadingWhiteSpace(leadingWhitespace)).Members.Single();
        }

        /// <summary>
        /// Parse a <see cref="PropertyDeclarationSyntax"/> from a string.
        /// </summary>
        /// <param name="code">The code text.</param>
        /// <param name="leadingWhitespace">The whitespace to adjust each row to have.</param>
        /// <returns>The <see cref="PropertyDeclarationSyntax"/>.</returns>
        public static PropertyDeclarationSyntax PropertyDeclaration(string code, string leadingWhitespace = null)
        {
            return (PropertyDeclarationSyntax)SyntaxFactory.ParseCompilationUnit(code.WithLeadingWhiteSpace(leadingWhitespace)).Members.Single();
        }

        /// <summary>
        /// Parse a <see cref="MethodDeclarationSyntax"/> from a string.
        /// </summary>
        /// <param name="code">The code text.</param>
        /// <param name="leadingWhitespace">The whitespace to adjust each row to have.</param>
        /// <returns>The <see cref="MethodDeclarationSyntax"/>.</returns>
        public static MethodDeclarationSyntax MethodDeclaration(string code, string leadingWhitespace = null)
        {
            return (MethodDeclarationSyntax)SyntaxFactory.ParseCompilationUnit(code.WithLeadingWhiteSpace(leadingWhitespace)).Members.Single();
        }

        /// <summary>
        /// Parse a <see cref="XmlElementSyntax"/> from a string.
        /// </summary>
        /// <param name="text">The element text including start and end tags.</param>
        /// <param name="leadingWhitespace">The whitespace to adjust each row to have.</param>
        /// <returns>The <see cref="XmlElementSyntax"/>.</returns>
        public static XmlElementSyntax XmlElementSyntax(string text, string leadingWhitespace = null)
        {
            if (DocumentationCommentTriviaSyntax(text.WithLeadingWhiteSpace("/// "), leadingWhitespace ?? string.Empty) is var comment &&
                comment.Content.TrySingleOfType(out XmlElementSyntax element))
            {
                return element;
            }

            throw new InvalidOperationException($"Failed parsing {text} into an XmlElementSyntax");
        }

        /// <summary>
        /// Parse a <see cref="DocumentationCommentTriviaSyntax"/> from a string.
        /// Lines are expected to start with ///.
        /// </summary>
        /// <param name="text">
        /// The element text including start and end tags.
        /// Lines are expected to start with ///.
        /// </param>
        /// <param name="leadingWhitespace">The whitespace to adjust each row to have.</param>
        /// <returns>The <see cref="DocumentationCommentTriviaSyntax"/>.</returns>
        public static DocumentationCommentTriviaSyntax DocumentationCommentTriviaSyntax(string text, string leadingWhitespace = null)
        {
            if (LeadingTrivia(text, leadingWhitespace) is var triviaList &&
                triviaList.TrySingle(x => x.HasStructure, out var withStructure) &&
                withStructure.GetStructure() is DocumentationCommentTriviaSyntax comment)
            {
                return comment;
            }

            throw new InvalidOperationException($"Failed parsing {text} into a DocumentationCommentTriviaSyntax");
        }

        /// <summary>
        /// Parse a <see cref="SyntaxTriviaList"/> from a string.
        /// Lines are expected to start with ///.
        /// </summary>
        /// <param name="text">
        /// The element text including start and end tags.
        /// Lines are expected to start with ///.
        /// </param>
        /// <param name="leadingWhitespace">The whitespace to adjust each row to have.</param>
        /// <returns>The <see cref="SyntaxTriviaList"/>.</returns>
        public static SyntaxTriviaList LeadingTrivia(string text, string leadingWhitespace = null)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (!text.EndsWith("\n"))
            {
                text += Environment.NewLine;
            }

            var code = text.WithLeadingWhiteSpace(leadingWhitespace);
            return SyntaxFactory.ParseLeadingTrivia(code);
        }

        /// <summary>
        /// Parse a <see cref="AttributeListSyntax"/> from a string.
        /// </summary>
        /// <param name="text">
        /// The attribute text including start and end [].
        /// </param>
        /// <param name="leadingWhitespace">The whitespace to adjust each row to have.</param>
        /// <returns>The <see cref="SyntaxTriviaList"/>.</returns>
        public static AttributeListSyntax AttributeList(string text, string leadingWhitespace = null)
        {
            var code = $"{text.WithLeadingWhiteSpace(leadingWhitespace)}\r\n{leadingWhitespace}public class Foo {{}}";
            if (SyntaxFactory.ParseCompilationUnit(code) is CompilationUnitSyntax compilationUnit &&
                compilationUnit.Members.TrySingleOfType(out ClassDeclarationSyntax member) &&
                member.AttributeLists.TrySingle(out var attributeList))
            {
                return attributeList;
            }

            throw new InvalidOperationException($"Failed parsing {text} into a DocumentationCommentTriviaSyntax");
        }
    }
}
