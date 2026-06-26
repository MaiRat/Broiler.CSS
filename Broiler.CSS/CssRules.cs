using System.Collections.ObjectModel;

namespace Broiler.CSS;

public enum CssRuleKind
{
    Style,
    AtRule,
}

public abstract class CssRule
{
    protected CssRule(CssRuleKind kind, CssSourceRange range)
    {
        Kind = kind;
        Range = range;
    }

    public CssRuleKind Kind { get; }

    public CssSourceRange Range { get; }
}

public sealed class CssStyleRule : CssRule
{
    internal CssStyleRule(
        CssSelectorList selectors,
        CssDeclarationBlock declarations,
        CssSourceRange range)
        : base(CssRuleKind.Style, range)
    {
        Selectors = selectors;
        Declarations = declarations;
    }

    public CssSelectorList Selectors { get; }

    public CssDeclarationBlock Declarations { get; }
}

public sealed class CssAtRule : CssRule
{
    private readonly ReadOnlyCollection<CssRule> _rules;

    internal CssAtRule(
        string name,
        string prelude,
        string? blockText,
        CssDeclarationBlock? declarations,
        IEnumerable<CssRule>? rules,
        CssSourceRange range)
        : base(CssRuleKind.AtRule, range)
    {
        Name = name;
        Prelude = prelude;
        BlockText = blockText;
        Declarations = declarations;
        _rules = (rules ?? []).ToList().AsReadOnly();
    }

    public string Name { get; }

    public string Prelude { get; }

    public string? BlockText { get; }

    public CssDeclarationBlock? Declarations { get; }

    public IReadOnlyList<CssRule> Rules => _rules;

    public bool HasBlock => BlockText is not null;
}
