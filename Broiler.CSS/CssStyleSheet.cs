using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Broiler.CSS;

public sealed class CssStyleSheet(IEnumerable<CssRule> rules, IEnumerable<CssDiagnostic> diagnostics)
{
    private readonly ReadOnlyCollection<CssRule> _rules = rules.ToList().AsReadOnly();
    private readonly ReadOnlyCollection<CssDiagnostic> _diagnostics = diagnostics.ToList().AsReadOnly();

    public IReadOnlyList<CssRule> Rules => _rules;

    public IReadOnlyList<CssDiagnostic> Diagnostics => _diagnostics;
}
