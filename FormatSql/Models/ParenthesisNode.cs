using SqlFormatter.Interfaces;

namespace SqlFormatter.Models;

internal class ParenthesisNode : ISQLTreeNode
{
    private SqlTree tree;

    internal SqlTree Tree
    {
        get { return tree; }
        set { tree = value; }
    }

    public object Content => tree;

    internal ParenthesisNode( SqlTree sqlTree )
    {
        Tree = sqlTree;
    }

    internal ParenthesisNode() : this( new SqlTree() ) { }
}