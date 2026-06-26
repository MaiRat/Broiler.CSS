using System.Collections.ObjectModel;

namespace Broiler.CSS;

public sealed class CssStyleSheet
{
    private readonly ReadOnlyCollection<CssRule> _rules;
    private readonly ReadOnlyCollection<CssDiagnostic> _diagnostics;

    internal CssStyleSheet(
        IEnumerable<CssRule> rules,
        IEnumerable<CssDiagnostic> diagnostics)
    {
        _rules = rules.ToList().AsReadOnly();
        _diagnostics = diagnostics.ToList().AsReadOnly();
    }

    public IReadOnlyList<CssRule> Rules => _rules;

    public IReadOnlyList<CssDiagnostic> Diagnostics => _diagnostics;
}
