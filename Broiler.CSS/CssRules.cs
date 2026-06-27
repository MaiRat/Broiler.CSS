using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Broiler.CSS;

public enum CssRuleKind
{
    Style,
    AtRule,
}

public abstract class CssRule(CssRuleKind kind, CssSourceRange range)
{
    public CssRuleKind Kind { get; } = kind;

    public CssSourceRange Range { get; } = range;
}

public sealed class CssStyleRule(CssSelectorList selectors, CssDeclarationBlock declarations, CssSourceRange range) :
    CssRule(CssRuleKind.Style, range)
{
    public CssSelectorList Selectors { get; } = selectors;

    public CssDeclarationBlock Declarations { get; } = declarations;
}

public sealed class CssAtRule(string name, string prelude, string? blockText, CssDeclarationBlock? declarations,
    IEnumerable<CssRule>? rules, CssSourceRange range) : CssRule(CssRuleKind.AtRule, range)
{
    private readonly ReadOnlyCollection<CssRule> _rules = (rules ?? []).ToList().AsReadOnly();

    public string Name { get; } = name;

    public string Prelude { get; } = prelude;

    public string? BlockText { get; } = blockText;

    public CssDeclarationBlock? Declarations { get; } = declarations;

    public IReadOnlyList<CssRule> Rules => _rules;

    public bool HasBlock => BlockText is not null;
}
