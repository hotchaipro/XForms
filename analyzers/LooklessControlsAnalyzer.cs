using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace XForma.Analyzers;

/// <summary>
/// Keeps looks out of the XForma assembly (XF0003): a control says what it does and what state
/// it's in, and its theme says how it looks. In XForma itself this flags a literal color, drawing
/// outside the template machinery, a numeric constant on a view, and a literal size, spacing,
/// font, easing or decorative transform. A theme package, or an app, is where these belong, and
/// the rule doesn't apply there.
/// </summary>
/// <remarks>
/// A constant that's behavior rather than look (how far a swipe has to go) says why with
/// [BehaviorConstant("...")], which only excuses a numeric constant. The check is on the syntax a
/// control would be written in, not everything a value could flow through, so a value computed
/// from nothing can get past it; it's there so moving a look back in takes a decision.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LooklessControlsAnalyzer : DiagnosticAnalyzer
{
    private const string XFormaAssemblyName = "XForma";
    private const string ExemptionAttributeName = "XForma.Controls.BehaviorConstantAttribute";

    // The types that draw: the template machinery, and Border, which has no template
    private static readonly ImmutableHashSet<string> DrawingTypes = ImmutableHashSet.Create(
        "TemplatedControl",
        "TemplatedContentControl",
        "ListViewItem",
        "Border",
        "ControlTemplate",
        "TemplateHost");

    // Properties whose literal value is a look
    private static readonly ImmutableHashSet<string> LookProperties = ImmutableHashSet.Create(
        "Width",
        "Height",
        "MinWidth",
        "MinHeight",
        "MaxWidth",
        "MaxHeight",
        "Spacing",
        "RowSpacing",
        "ColumnSpacing",
        "CornerRadius",
        "FontSize",
        "Scale");

    private static readonly DiagnosticDescriptor LookInControls = new DiagnosticDescriptor(
        id: "XF0003",
        title: "A look in the XForma controls",
        messageFormat: "{0}: this is a look, which belongs in the theme (a style in xforma/Theming/ControlStyles.cs, with its value in the theme package)",
        category: "XForma.Look",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "XForma's controls are lookless: their colors, drawing, sizes, fonts and decorative motion come from their theme. A constant that's behavior says why with [BehaviorConstant(\"...\")].");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(LookInControls);

    public override void Initialize(
        AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    private static void OnCompilationStart(
        CompilationStartAnalysisContext context)
    {
        var compilation = context.Compilation;
        if (compilation.AssemblyName != XFormaAssemblyName)
        {
            return;
        }

        var types = new FrameworkTypes(compilation);
        context.RegisterSyntaxNodeAction(c => AnalyzeName(c, types), SyntaxKind.IdentifierName);
        context.RegisterSyntaxNodeAction(c => AnalyzeCreation(c, types), SyntaxKind.ObjectCreationExpression, SyntaxKind.ImplicitObjectCreationExpression);
        context.RegisterSyntaxNodeAction(c => AnalyzeAssignment(c, types), SyntaxKind.SimpleAssignmentExpression);
        context.RegisterSyntaxNodeAction(c => AnalyzeInvocation(c, types), SyntaxKind.InvocationExpression);
        context.RegisterSyntaxNodeAction(c => AnalyzeField(c, types), SyntaxKind.FieldDeclaration);
        context.RegisterSyntaxNodeAction(c => AnalyzeLocal(c, types), SyntaxKind.LocalDeclarationStatement);
    }

    private static void Report(
        SyntaxNodeAnalysisContext context,
        Location location,
        string what)
    {
        context.ReportDiagnostic(Diagnostic.Create(LookInControls, location, what));
    }

    // A literal color (Color.FromRgb, Colors.Red), or drawing where it doesn't belong
    private static void AnalyzeName(
        SyntaxNodeAnalysisContext context,
        FrameworkTypes types)
    {
        var name = (IdentifierNameSyntax)context.Node;

        // A documentation comment's reference mentions a member; it doesn't use it
        if (name.Ancestors().OfType<CrefSyntax>().Any())
        {
            return;
        }

        var symbol = context.SemanticModel.GetSymbolInfo(name, context.CancellationToken).Symbol;
        switch (symbol)
        {
            case IMethodSymbol method when SymbolEqualityComparer.Default.Equals(method.ContainingType, types.Color) && method.Name.StartsWith("From", StringComparison.Ordinal):
                Report(context, name.GetLocation(), $"The literal color Color.{method.Name}");
                break;

            case IFieldSymbol or IPropertySymbol when SymbolEqualityComparer.Default.Equals(symbol.ContainingType, types.Colors) && (symbol.Name != "Transparent"):
                Report(context, name.GetLocation(), $"The literal color Colors.{symbol.Name}");
                break;

            case INamedTypeSymbol type when SymbolEqualityComparer.Default.Equals(type, types.DrawingContext):
                var enclosing = EnclosingType(context, name);
                if ((enclosing is not null) && !IsDrawingType(enclosing))
                {
                    Report(context, name.GetLocation(), $"Drawing in {enclosing.Name}, which isn't a template");
                }

                break;
        }
    }

    // new Color(...), a literal Thickness or Size, and an easing of the control's own
    private static void AnalyzeCreation(
        SyntaxNodeAnalysisContext context,
        FrameworkTypes types)
    {
        var creation = (BaseObjectCreationExpressionSyntax)context.Node;
        if (context.SemanticModel.GetTypeInfo(creation, context.CancellationToken).Type is not INamedTypeSymbol type)
        {
            return;
        }

        if (SymbolEqualityComparer.Default.Equals(type, types.Color))
        {
            Report(context, creation.GetLocation(), "A literal color");
        }
        else if (DerivesFrom(type, types.EasingFunction))
        {
            Report(context, creation.GetLocation(), $"The easing {type.Name} (use the theme's Motion)");
        }
        else if ((SymbolEqualityComparer.Default.Equals(type, types.Thickness) || SymbolEqualityComparer.Default.Equals(type, types.Size))
            && (creation.ArgumentList is not null)
            && creation.ArgumentList.Arguments.Any(a => IsLiteralOtherThan(a.Expression)))
        {
            Report(context, creation.GetLocation(), $"A literal {type.Name}");
        }
    }

    // this.Height = 60, Spacing = 2, FontWeight = FontWeight.SemiBold
    private static void AnalyzeAssignment(
        SyntaxNodeAnalysisContext context,
        FrameworkTypes types)
    {
        var assignment = (AssignmentExpressionSyntax)context.Node;
        if (context.SemanticModel.GetSymbolInfo(assignment.Left, context.CancellationToken).Symbol is not IPropertySymbol property)
        {
            return;
        }

        if (SymbolEqualityComparer.Default.Equals(property.Type, types.FontWeight)
            && (context.SemanticModel.GetSymbolInfo(assignment.Right, context.CancellationToken).Symbol is IFieldSymbol { IsConst: true } weight)
            && SymbolEqualityComparer.Default.Equals(weight.ContainingType, types.FontWeight))
        {
            Report(context, assignment.GetLocation(), $"The font weight {weight.Name}");
            return;
        }

        if (LookProperties.Contains(property.Name)
            && ((property.Name == "Scale") ? IsLiteralOtherThan(assignment.Right, 0, 1) : IsLiteralOtherThan(assignment.Right)))
        {
            Report(context, assignment.GetLocation(), $"The literal {property.Name}");
        }
    }

    // Angle.FromDegrees(90), animator.ScaleTo(0.75f, ...)
    private static void AnalyzeInvocation(
        SyntaxNodeAnalysisContext context,
        FrameworkTypes types)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var arguments = invocation.ArgumentList.Arguments;
        if ((arguments.Count == 0) || (context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol method))
        {
            return;
        }

        if (SymbolEqualityComparer.Default.Equals(method.ContainingType, types.Angle) && method.Name.StartsWith("From", StringComparison.Ordinal))
        {
            if (IsLiteralOtherThan(arguments[0].Expression))
            {
                Report(context, invocation.GetLocation(), "A literal rotation");
            }
        }
        else if ((method.Name == "ScaleTo") && IsLiteralOtherThan(arguments[0].Expression, 0, 1))
        {
            Report(context, invocation.GetLocation(), "A literal scale");
        }
    }

    // A numeric constant, or its static readonly equivalent, on a view
    private static void AnalyzeField(
        SyntaxNodeAnalysisContext context,
        FrameworkTypes types)
    {
        var field = (FieldDeclarationSyntax)context.Node;
        bool isConstant = field.Modifiers.Any(SyntaxKind.ConstKeyword)
            || (field.Modifiers.Any(SyntaxKind.StaticKeyword) && field.Modifiers.Any(SyntaxKind.ReadOnlyKeyword));
        var enclosing = EnclosingType(context, field);
        if (!isConstant || (enclosing is null) || !DerivesFrom(enclosing, types.View))
        {
            return;
        }

        foreach (var variable in field.Declaration.Variables)
        {
            if ((context.SemanticModel.GetDeclaredSymbol(variable, context.CancellationToken) is not IFieldSymbol symbol) || !IsLookType(symbol.Type, types))
            {
                continue;
            }

            var exemption = symbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == ExemptionAttributeName);
            if (exemption is null)
            {
                Report(context, variable.GetLocation(), $"The constant {symbol.Name} on {enclosing.Name} (if it's behavior, say why with [BehaviorConstant])");
            }
            else if (string.IsNullOrWhiteSpace(exemption.ConstructorArguments.FirstOrDefault().Value as string))
            {
                Report(context, variable.GetLocation(), $"The constant {symbol.Name}'s [BehaviorConstant] gives no reason");
            }
        }
    }

    // A local constant can't say why it's behavior, so it's a field if it is
    private static void AnalyzeLocal(
        SyntaxNodeAnalysisContext context,
        FrameworkTypes types)
    {
        var local = (LocalDeclarationStatementSyntax)context.Node;
        var enclosing = EnclosingType(context, local);
        if (!local.IsConst || (enclosing is null) || !DerivesFrom(enclosing, types.View))
        {
            return;
        }

        foreach (var variable in local.Declaration.Variables)
        {
            if ((context.SemanticModel.GetDeclaredSymbol(variable, context.CancellationToken) is ILocalSymbol symbol) && IsLookType(symbol.Type, types))
            {
                Report(context, variable.GetLocation(), $"The local constant {symbol.Name} on {enclosing.Name} (if it's behavior, make it a field that says why with [BehaviorConstant])");
            }
        }
    }

    private static bool IsLookType(
        ITypeSymbol type,
        FrameworkTypes types)
    {
        return (type.SpecialType == SpecialType.System_Single)
            || (type.SpecialType == SpecialType.System_Double)
            || SymbolEqualityComparer.Default.Equals(type, types.Thickness)
            || SymbolEqualityComparer.Default.Equals(type, types.Size);
    }

    private static INamedTypeSymbol? EnclosingType(
        SyntaxNodeAnalysisContext context,
        SyntaxNode node)
    {
        var declaration = node.Ancestors().OfType<BaseTypeDeclarationSyntax>().FirstOrDefault();
        return (declaration is null) ? null : context.SemanticModel.GetDeclaredSymbol(declaration, context.CancellationToken);
    }

    private static bool IsDrawingType(
        INamedTypeSymbol type)
    {
        for (var current = type; current is not null; current = current.ContainingType)
        {
            if (DrawingTypes.Contains(current.Name))
            {
                return true;
            }
        }

        return false;
    }

    private static bool DerivesFrom(
        INamedTypeSymbol type,
        INamedTypeSymbol? baseType)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, baseType))
            {
                return true;
            }
        }

        return false;
    }

    // Whether the expression is a number (perhaps negated) other than these (0 by default)
    private static bool IsLiteralOtherThan(
        ExpressionSyntax expression,
        params double[] neutral)
    {
        if (expression is PrefixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.UnaryMinusExpression } negative)
        {
            expression = negative.Operand;
        }

        if ((expression is not LiteralExpressionSyntax { RawKind: (int)SyntaxKind.NumericLiteralExpression } literal))
        {
            return false;
        }

        double value = Convert.ToDouble(literal.Token.Value, System.Globalization.CultureInfo.InvariantCulture);
        return (neutral.Length == 0) ? (value != 0) : !neutral.Contains(value);
    }

    private sealed class FrameworkTypes
    {
        public FrameworkTypes(
            Compilation compilation)
        {
            this.Angle = compilation.GetTypeByMetadataName("XForma.Angle");
            this.Color = compilation.GetTypeByMetadataName("XForma.Color");
            this.Colors = compilation.GetTypeByMetadataName("XForma.Colors");
            this.DrawingContext = compilation.GetTypeByMetadataName("XForma.DrawingContext");
            this.EasingFunction = compilation.GetTypeByMetadataName("XForma.EasingFunction");
            this.FontWeight = compilation.GetTypeByMetadataName("XForma.FontWeight");
            this.Size = compilation.GetTypeByMetadataName("XForma.Size");
            this.Thickness = compilation.GetTypeByMetadataName("XForma.Thickness");
            this.View = compilation.GetTypeByMetadataName("XForma.View");
        }

        public INamedTypeSymbol? Angle { get; }

        public INamedTypeSymbol? Color { get; }

        public INamedTypeSymbol? Colors { get; }

        public INamedTypeSymbol? DrawingContext { get; }

        public INamedTypeSymbol? EasingFunction { get; }

        public INamedTypeSymbol? FontWeight { get; }

        public INamedTypeSymbol? Size { get; }

        public INamedTypeSymbol? Thickness { get; }

        public INamedTypeSymbol? View { get; }
    }
}
