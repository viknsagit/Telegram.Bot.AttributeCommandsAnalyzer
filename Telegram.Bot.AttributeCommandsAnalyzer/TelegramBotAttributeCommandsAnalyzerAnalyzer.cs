using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Telegram.Bot.AttributeCommandsAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TelegramBotAttributeCommandsAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TelegramBotAttributeCommandsAnalyzer";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));

        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            if (!methodDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                if (methodDeclaration.AttributeLists.SelectMany(attrList => attrList.Attributes).Any(attr => attr.Name.ToString() == "TextCommand"))
                {
                    var diagnostic = Diagnostic.Create(Rule, methodDeclaration.GetLocation(), methodDeclaration.Identifier.Text, "TextCommand");
                    context.ReportDiagnostic(diagnostic);
                }
                else if (methodDeclaration.AttributeLists.SelectMany(attrList => attrList.Attributes).Any(attr => attr.Name.ToString() == "CallbackCommand"))
                {
                    var diagnostic = Diagnostic.Create(Rule, methodDeclaration.GetLocation(), methodDeclaration.Identifier.Text, "CallbackCommand");
                    context.ReportDiagnostic(diagnostic);
                }
                else if (methodDeclaration.AttributeLists.SelectMany(attrList => attrList.Attributes).Any(attr => attr.Name.ToString() == "ReplyCommand"))
                {
                    var diagnostic = Diagnostic.Create(Rule, methodDeclaration.GetLocation(), methodDeclaration.Identifier.Text, "ReplyCommand");
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}