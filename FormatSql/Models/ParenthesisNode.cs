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

    public object Content
    {
        get
        {
            return tree;
        }
    }

    public ParenthesisNode( SqlTree sqlTree )
    {
        Tree = sqlTree;
    }

    public ParenthesisNode() : this( new SqlTree() ) { }
}