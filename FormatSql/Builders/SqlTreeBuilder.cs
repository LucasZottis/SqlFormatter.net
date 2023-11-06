using SqlFormatter.Helpers;
using SqlFormatter.Models;
using System.Text.RegularExpressions;

namespace SqlFormatter.Builders;

public static class SqlTreeBuilder
{
    /// <summary>
    /// Preparação da formatação
    /// </summary>
    /// <param name="Query"></param>
    /// <return></return>
    private static SqlTree Parse( string query )
    {
        var sql = QueryHelper.PrepareQuery( query );
        var matches = Regex.Matches( sql, @"(/\*((.|\s)*?)\*/|'((.|\s)*?)'|^(.+?)$)", RegexOptions.Multiline | RegexOptions.ExplicitCapture );
        var trees = new Stack<SqlTree>();
        var tree = new SqlTree();

        foreach ( Match match in matches )
        {
            var line = match.Value.Trim( ' ', '\n', '\r', '\t' );

            if ( line == string.Empty ) 
                continue;
            else if ( line == "(" )
            {
                trees.Push( tree );
                var newTree = new ParenthesisNode();
                newTree.Tree = new SqlTree();
                tree.nodes.Enqueue( newTree );
                tree = newTree.Tree;
            }
            else if ( line == ")" )
            {
                try
                {
                    tree = trees.Pop();
                }
                catch
                {
                    throw new FormatException( "Muitos parênteses de fechamento" );
                }
            }
            //else
                //tree.nodes.Enqueue( new StringNode( line ) );
        }

        if ( trees.Count > 0 )
            throw new FormatException( "Muitos parênteses de abertura" );

        return tree;
    }

    public static SqlTree Build( string sql )
    {
        return Parse( sql );
    }
}