using SqlFormatter.Interfaces;

namespace SqlFormatter.Models;

internal class StringNode : ISQLTreeNode
{
    private string value;

    internal string Value
    {
        get { return value; }
        set { this.value = value; }
    }

    public object Content => value;

    internal StringNode( string value )
    {
        Value = value;
    }

    internal StringNode() : this( string.Empty ) { }
}