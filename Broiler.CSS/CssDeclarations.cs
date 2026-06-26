using System.Collections.ObjectModel;

namespace Broiler.CSS;

public sealed class CssDeclaration
{
    internal CssDeclaration(
        string name,
        CssValue value,
        bool important,
        CssSourceRange range)
    {
        Name = name;
        Value = value;
        Important = important;
        Range = range;
    }

    public string Name { get; }

    public CssValue Value { get; }

    public bool Important { get; }

    public CssSourceRange Range { get; }
}

public sealed class CssDeclarationBlock
{
    private readonly ReadOnlyCollection<CssDeclaration> _declarations;

    internal CssDeclarationBlock(IEnumerable<CssDeclaration> declarations)
    {
        _declarations = declarations.ToList().AsReadOnly();
    }

    public IReadOnlyList<CssDeclaration> Declarations => _declarations;

    public CssDeclaration? GetLastDeclaration(string propertyName) =>
        _declarations.LastOrDefault(declaration =>
            string.Equals(declaration.Name, propertyName, StringComparison.OrdinalIgnoreCase));

    public string? GetPropertyValue(string propertyName) =>
        GetLastDeclaration(propertyName)?.Value.Text;
}
