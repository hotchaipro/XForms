using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using XForma.Analyzers;
using Xunit;

namespace XForma.Tests
{
    // XF0003: each kind of look fails in the XForma assembly, what's behavior
    // says so and stays, and elsewhere (a theme, an app) it's all fine
    public class LooklessControlsAnalyzerTests
    {
        // What the XForma assembly itself has, over the real framework
        private const string Prelude = @"
using System;
using XForma;
using XForma.Primitives;

namespace XForma.Controls
{
    [AttributeUsage(AttributeTargets.Field)]
    internal sealed class BehaviorConstantAttribute : Attribute
    {
        public BehaviorConstantAttribute(string reason) { }
    }

    public abstract class TemplatedControl : View
    {
        protected override void Render(DrawingContext drawingContext) { }
    }
}
";

        private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(
            string source,
            string assemblyName = "XForma")
        {
            var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator)
                .Where(p => !Path.GetFileName(p).StartsWith("XForma", StringComparison.Ordinal))
                .Select(p => MetadataReference.CreateFromFile(p))
                .Append(MetadataReference.CreateFromFile(typeof(View).Assembly.Location));

            var compilation = CSharpCompilation.Create(
                assemblyName,
                new[] { CSharpSyntaxTree.ParseText(Prelude + source) },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            Assert.Empty(compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));

            return await compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new LooklessControlsAnalyzer())).GetAnalyzerDiagnosticsAsync();
        }

        private static string InControl(
            string members)
        {
            return @"
namespace XForma.Controls
{
    public class MyControl : TemplatedControl
    {
        " + members + @"
    }
}";
        }

        [Theory]
        [InlineData("public Color C => Color.FromRgb(1, 2, 3);", "Color.FromRgb")]
        [InlineData("public Color C => Colors.White;", "Colors.White")]
        [InlineData("public Color C => new Color();", "literal color")]
        [InlineData("protected override void Render(DrawingContext d) { }", "Drawing in MyControl")]
        [InlineData("public const float Size2 = 20.0f;", "constant Size2")]
        [InlineData("private const float Private = 20.0f;", "constant Private")]
        [InlineData("private static readonly float Dodge = 20.0f;", "constant Dodge")]
        [InlineData("private static readonly Thickness Dodge = default;", "constant Dodge")]
        [InlineData("public void F() { const float Local = 20.0f; }", "local constant Local")]
        [InlineData("public MyControl() { this.Margin = new Thickness(4); }", "literal Thickness")]
        [InlineData("public Size S => new Size(30, 30);", "literal Size")]
        [InlineData("public MyControl() { this.Height = 60; }", "literal Height")]
        [InlineData("public TextBlock B => new TextBlock() { FontSize = 12 };", "literal FontSize")]
        [InlineData("public TextBlock B => new TextBlock() { FontWeight = FontWeight.SemiBold };", "font weight SemiBold")]
        [InlineData("public EasingFunction E => new CubicEase(EasingMode.EaseOut);", "easing CubicEase")]
        [InlineData("public Angle A => Angle.FromDegrees(90);", "literal rotation")]
        public async Task EachLookFails(
            string members,
            string message)
        {
            var diagnostics = await AnalyzeAsync(InControl(members));

            var diagnostic = Assert.Single(diagnostics);
            Assert.Equal("XF0003", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
            Assert.Contains(message, diagnostic.GetMessage());
        }

        [Fact]
        public async Task AConstantThatSaysItsBehaviorStays()
        {
            var diagnostics = await AnalyzeAsync(InControl(@"
                [BehaviorConstant(""Where the layout changes, not how it looks"")]
                public const float CompactThreshold = 640.0f;"));

            Assert.Empty(diagnostics);
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        public async Task AnExemptionNeedsAReason(
            string reason)
        {
            var diagnostics = await AnalyzeAsync(InControl($@"
                [BehaviorConstant(""{reason}"")]
                public const float CompactThreshold = 640.0f;"));

            Assert.Contains("gives no reason", Assert.Single(diagnostics).GetMessage());
        }

        [Theory]
        [InlineData("public Color C => Colors.Transparent;")]
        [InlineData("public MyControl() { this.Margin = new Thickness(0); }")]
        [InlineData("public void F(float end) { this.Margin = new Thickness(0, 0, end, 0); }")]
        [InlineData("public MyControl() { this.Height = 0; }")]
        [InlineData("public Color C => this.ActualTheme.Colors.Accent;")]
        [InlineData("public const int Count = 8;")]
        public async Task WhatIsntALookPasses(
            string members)
        {
            Assert.Empty(await AnalyzeAsync(InControl(members)));
        }

        [Fact]
        public async Task AConstantOffAViewPasses()
        {
            var diagnostics = await AnalyzeAsync(@"
namespace XForma.Controls
{
    internal sealed class VelocityTracker
    {
        private const float MaxSpeed = 8000;
    }
}");

            Assert.Empty(diagnostics);
        }

        // The rule's only for XForma: a theme package and an app are where looks go
        [Theory]
        [InlineData("XForma.Themes.Basic")]
        [InlineData("SoonerOrLater.XForms")]
        public async Task OtherAssembliesMayHaveLooks(
            string assemblyName)
        {
            var diagnostics = await AnalyzeAsync(
                InControl("public const float Size2 = 20.0f; public Color C => Colors.White; protected override void Render(DrawingContext d) { }"),
                assemblyName);

            Assert.Empty(diagnostics);
        }
    }
}
