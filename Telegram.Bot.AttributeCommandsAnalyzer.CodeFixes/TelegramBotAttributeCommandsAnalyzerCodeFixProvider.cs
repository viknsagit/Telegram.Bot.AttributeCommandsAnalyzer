using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Telegram.Bot.AttributeCommandsAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TelegramBotAttributeCommandsAnalyzerCodeFixProvider)), Shared]
    public class TelegramBotAttributeCommandsAnalyzerCodeFixProvider : CodeFixProvider
    {
        public override sealed ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(TelegramBotAttributeCommandsAnalyzerAnalyzer.DiagnosticId); }
        }

        public override sealed FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override sealed async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CodeFixResources.CodeFixTitle,
                    createChangedSolution: c => MakeMethodStaticAsync(context.Document, declaration, c),
                    equivalenceKey: nameof(CodeFixResources.CodeFixTitle)),
                diagnostic);
        }

        private async Task<Solution> MakeMethodStaticAsync(Document document, MethodDeclarationSyntax methodDeclaration, CancellationToken cts)
        {
            var root = await document.GetSyntaxRootAsync(cts).ConfigureAwait(false);

            var newModifiers = methodDeclaration.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.StaticKeyword));
            var newMethodDeclaration = methodDeclaration.WithModifiers(newModifiers);

            var newRoot = root.ReplaceNode(methodDeclaration, newMethodDeclaration);

            return document.Project.Solution.WithDocumentSyntaxRoot(document.Id, newRoot);
        }
    }
}