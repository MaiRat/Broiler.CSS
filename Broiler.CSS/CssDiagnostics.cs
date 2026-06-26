namespace Broiler.CSS;

public enum CssDiagnosticSeverity
{
    Info,
    Warning,
    Error,
}

public readonly record struct CssSourceRange(int Start, int Length)
{
    public int End => Start + Length;
}

public sealed record CssDiagnostic(
    string Code,
    string Message,
    CssDiagnosticSeverity Severity,
    CssSourceRange Range);
