namespace Broiler.CSS.Dom;

/// <summary>
/// Opt-in diagnostics for the style engine.
///
/// <para>
/// When <see cref="DeclarationRejected"/> is set, the engine reports every CSS
/// declaration it drops because the value failed validation
/// (<c>IsAcceptableDeclarationValue</c> in <see cref="CssStyleEngine"/>). It is
/// <b>off by default</b>: each drop site is a single null-check, so there is no
/// cost during normal rendering.
/// </para>
///
/// <para>
/// The WPT runner wires this up to surface CSS that is <i>silently</i> discarded
/// — e.g. an unsupported <c>text-align:-webkit-right</c> — which frequently gates
/// many tests at once with no other signal. A single high-count entry in the
/// "dropped declarations" report usually points straight at a missing feature
/// (WPT issue #1100, where this exact case masked a whole css-align cluster).
/// </para>
/// </summary>
public static class CssEngineDiagnostics
{
    /// <summary>
    /// Invoked with <c>(property, value)</c> whenever a declaration is dropped
    /// because its value failed validation. May be invoked from multiple threads,
    /// so the handler must be thread-safe. Values are reported verbatim (the
    /// consumer is responsible for any truncation/aggregation).
    /// </summary>
    public static Action<string, string>? DeclarationRejected { get; set; }

    /// <summary>
    /// Reports a rejected declaration to <see cref="DeclarationRejected"/> if set.
    /// </summary>
    internal static void ReportRejected(string property, string value)
        => DeclarationRejected?.Invoke(property, value);
}
