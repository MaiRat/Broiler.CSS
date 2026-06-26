using Broiler.Dom;

namespace Broiler.CSS.Dom.Tests;

public sealed class CssStyleEngineTests
{
    private static CssStyleEngine EngineWith(string css, ICssSelectorStateProvider? state = null)
    {
        var engine = new CssStyleEngine(state);
        engine.AddStyleSheet(new CssParser().ParseStyleSheet(css));
        return engine;
    }

    private static (DomDocument Document, DomElement Html, DomElement Body) NewDocument()
    {
        var document = new DomDocument();
        var html = document.CreateElement("html");
        var body = document.CreateElement("body");
        document.AppendChild(html);
        html.AppendChild(body);
        return (document, html, body);
    }

    [Fact]
    public void Cascade_Resolves_By_Specificity_Then_Source_Order()
    {
        var (_, _, body) = NewDocument();
        var div = body.OwnerDocument.CreateElement("div");
        div.Id = "x";
        div.ClassName = "c";
        body.AppendChild(div);

        var engine = EngineWith("div { color: red; } .c { color: green; } #x { color: blue; }");

        Assert.Equal("blue", engine.GetComputedStyle(div).GetPropertyValue("color"));
    }

    [Fact]
    public void Cascade_Later_Declaration_Wins_On_Specificity_Tie()
    {
        var (_, _, body) = NewDocument();
        var div = body.OwnerDocument.CreateElement("div");
        div.ClassName = "c";
        body.AppendChild(div);

        var engine = EngineWith(".c { color: red; } .c { color: green; }");

        Assert.Equal("green", engine.GetComputedStyle(div).GetPropertyValue("color"));
    }

    [Fact]
    public void Important_Declaration_Beats_Higher_Specificity_Normal()
    {
        var (_, _, body) = NewDocument();
        var div = body.OwnerDocument.CreateElement("div");
        div.Id = "x";
        div.ClassName = "c";
        body.AppendChild(div);

        var engine = EngineWith("#x { color: blue; } .c { color: green !important; }");

        Assert.Equal("green", engine.GetComputedStyle(div).GetPropertyValue("color"));
    }

    [Fact]
    public void Inline_Style_Beats_Selector_Declarations()
    {
        var (_, _, body) = NewDocument();
        var div = body.OwnerDocument.CreateElement("div");
        div.SetAttribute("style", "color: orange;");
        body.AppendChild(div);

        var engine = EngineWith("div { color: red; }");

        Assert.Equal("orange", engine.GetComputedStyle(div).GetPropertyValue("color"));
    }

    [Fact]
    public void Inherited_Property_Flows_To_Child_Without_Own_Declaration()
    {
        var (_, _, body) = NewDocument();
        var parent = body.OwnerDocument.CreateElement("div");
        parent.ClassName = "p";
        var child = body.OwnerDocument.CreateElement("span");
        body.AppendChild(parent);
        parent.AppendChild(child);

        var engine = EngineWith(".p { color: purple; }");

        Assert.Equal("purple", engine.GetComputedStyle(child).GetPropertyValue("color"));
    }

    [Fact]
    public void Non_Inherited_Property_Falls_Back_To_Initial_Value()
    {
        var (_, _, body) = NewDocument();
        var div = body.OwnerDocument.CreateElement("div");
        body.AppendChild(div);

        var engine = EngineWith("body { display: flex; }");
        var style = engine.GetComputedStyle(div);

        // display is not inherited, so the child keeps its initial value.
        Assert.Equal("inline", style.GetPropertyValue("display"));
        Assert.Equal("rgb(0, 0, 0)", style.GetPropertyValue("color"));
    }

    [Fact]
    public void Shorthand_Margin_Expands_To_Longhands()
    {
        var (_, _, body) = NewDocument();
        var div = body.OwnerDocument.CreateElement("div");
        body.AppendChild(div);

        var engine = EngineWith("div { margin: 10px 20px; }");
        var style = engine.GetComputedStyle(div);

        Assert.Equal("10px", style.GetPropertyValue("margin-top"));
        Assert.Equal("20px", style.GetPropertyValue("margin-right"));
        Assert.Equal("10px", style.GetPropertyValue("margin-bottom"));
        Assert.Equal("20px", style.GetPropertyValue("margin-left"));
    }

    [Fact]
    public void Custom_Property_Is_Inherited_And_Var_Is_Resolved()
    {
        var (_, html, body) = NewDocument();
        html.SetAttribute("style", "--accent: teal;");
        var div = body.OwnerDocument.CreateElement("div");
        body.AppendChild(div);

        var engine = EngineWith("div { color: var(--accent); }");

        Assert.Equal("teal", engine.GetComputedStyle(div).GetPropertyValue("color"));
    }

    [Fact]
    public void Var_Falls_Back_When_Custom_Property_Is_Missing()
    {
        var (_, _, body) = NewDocument();
        var div = body.OwnerDocument.CreateElement("div");
        body.AppendChild(div);

        var engine = EngineWith("div { color: var(--missing, crimson); }");

        Assert.Equal("crimson", engine.GetComputedStyle(div).GetPropertyValue("color"));
    }

    [Fact]
    public void Media_Query_Applies_Only_When_Environment_Matches()
    {
        var (_, _, body) = NewDocument();
        var div = body.OwnerDocument.CreateElement("div");
        body.AppendChild(div);

        var engine = EngineWith("div { color: red; } @media (min-width: 500px) { div { color: green; } }");

        Assert.Equal("red", engine.GetComputedStyle(div, new CssEnvironment(300, 600)).GetPropertyValue("color"));
        Assert.Equal("green", engine.GetComputedStyle(div, new CssEnvironment(800, 600)).GetPropertyValue("color"));
    }

    [Fact]
    public void Pseudo_Element_Rules_Match_Only_For_That_Pseudo()
    {
        var (_, _, body) = NewDocument();
        var div = body.OwnerDocument.CreateElement("div");
        body.AppendChild(div);

        var engine = EngineWith("div::before { content: \"hi\"; }");

        Assert.Equal("\"hi\"", engine.GetComputedStyle(div, pseudoElement: "::before").GetPropertyValue("content"));
        Assert.NotEqual("\"hi\"", engine.GetComputedStyle(div).GetPropertyValue("content"));
    }

    [Fact]
    public void Relative_Font_Weight_Resolves_Against_Inherited_Weight()
    {
        var (_, _, body) = NewDocument();
        var parent = body.OwnerDocument.CreateElement("div");
        parent.ClassName = "p";
        var child = body.OwnerDocument.CreateElement("span");
        child.ClassName = "c";
        body.AppendChild(parent);
        parent.AppendChild(child);

        var engine = EngineWith(".p { font-weight: bold; } .c { font-weight: bolder; }");

        Assert.Equal("700", engine.GetComputedStyle(parent).GetPropertyValue("font-weight"));
        Assert.Equal("900", engine.GetComputedStyle(child).GetPropertyValue("font-weight"));
    }

    [Fact]
    public void Computed_Style_Is_Recomputed_After_Attribute_Mutation()
    {
        var (_, _, body) = NewDocument();
        var div = body.OwnerDocument.CreateElement("div");
        body.AppendChild(div);

        var engine = EngineWith(".active { color: green; } div { color: red; }");

        Assert.Equal("red", engine.GetComputedStyle(div).GetPropertyValue("color"));

        div.ClassName = "active";

        Assert.Equal("green", engine.GetComputedStyle(div).GetPropertyValue("color"));
    }

    [Fact]
    public void Adding_A_Stylesheet_Invalidates_Cached_Results()
    {
        var (_, _, body) = NewDocument();
        var div = body.OwnerDocument.CreateElement("div");
        body.AppendChild(div);

        var engine = new CssStyleEngine();
        engine.AddStyleSheet(new CssParser().ParseStyleSheet("div { color: red; }"));
        Assert.Equal("red", engine.GetComputedStyle(div).GetPropertyValue("color"));

        engine.AddStyleSheet(new CssParser().ParseStyleSheet("div { color: blue; }"));
        Assert.Equal("blue", engine.GetComputedStyle(div).GetPropertyValue("color"));
    }

    [Fact]
    public void Registered_Custom_Property_Honours_Inherits_False()
    {
        var (_, html, body) = NewDocument();
        html.SetAttribute("style", "--g: 10px;");
        var div = body.OwnerDocument.CreateElement("div");
        body.AppendChild(div);

        var engine = EngineWith("@property --g { syntax: \"<length>\"; inherits: false; initial-value: 2px; }");
        var style = engine.GetComputedStyle(div);

        // Non-inheriting registered property resets to its registered initial value.
        Assert.Equal("2px", style.GetPropertyValue("--g"));
    }

    [Fact]
    public void Missing_Element_Returns_Empty_Style()
    {
        var engine = EngineWith("div { color: red; }");
        Assert.Same(CssComputedStyle.Empty, engine.GetComputedStyle(null!));
    }
}
