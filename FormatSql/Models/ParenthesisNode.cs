using SqlFormatter.Interfaces;

namespace SqlFormatter.Models;

public class ParenthesisNode : ISQLTreeNode
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
}