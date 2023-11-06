using SqlFormatter.Builders;
using SqlFormatter.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlFormatter;

public static class Formatter
{
    /// <resumo>
    /// Preparação da formatação
    /// </sumário>
    /// <param name="Query"></param>
    /// <retorna></retorna>
    private static SqlTree Parse( string Query )
    {
        Stack<SqlTree> trees = new Stack<SqlTree>();

        string[] schemes = new string[] {
            "(?<Comment>\\-\\-.*$)",
            "(?<Comment>\\/\\*(.|\\s)*?\\*\\/)",
            "(?<string>'(.|\\s)*?')",
            "(?<operator>\\<\\>|\\<=|\\>=)",
            "(?<insert>INSERT\\s+INTO)",
            "(?<groupBy>GROUP\\s+BY)",
            "(?<orderBy>ORDER\\s+BY)",
            "(?<LeftJoin>LEFT\\s+(OUTER\\s+)?JOIN)",
            "(?<RightJoin>RIGHT\\s+(OUTER\\s+)?JOIN)",
            "(?<FullJoin>FULL\\s+(OUTER\\s+)?JOIN)",
            "(?<CrossJoin>CROSS\\s+JOIN)",
            "(?<FullApply>CROSS\\s+APPLY)",
            "(?<OuterApply>OUTER\\s+APPLY)",
            "(?<InnerJoin>(INNER\\s+)?JOIN)",
            "(?<Variable>@\\w+)",
            "(?<Name>\\[.*?\\])",
            "(?<dot>\\.)",
            "(?<spaces>\\s+)",
            "(?<nonChar>\\W)"
        };


        string sql = Regex.Replace( Query, "(" + string.Join( "|", schemes ) + ")", new MatchEvaluator( delegate ( Match match )
        {
            if ( match.Groups[ "Name" ].Success )
                return match.Value;

            if ( match.Groups[ "nonChar" ].Success )
                return "\r\n" + match.Value + "\r\n";

            if ( match.Groups[ "spaces" ].Success )
                return "\r\n";

            if ( match.Groups[ "insert" ].Success )
                return "INSERT INTO\r\n";

            if ( match.Groups[ "groupBy" ].Success )
                return "GROUP BY\r\n";

            if ( match.Groups[ "orderBy" ].Success )
                return "ORDER BY\r\n";

            if ( match.Groups[ "LeftJoin" ].Success )
                return "LEFT JOIN\r\n";

            if ( match.Groups[ "RightJoin" ].Success )
                return "RIGHT JOIN\r\n";

            if ( match.Groups[ "FullJoin" ].Success )
                return "FULL JOIN\r\n";

            if ( match.Groups[ "CrossJoin" ].Success )
                return "CROSS JOIN\r\n";

            if ( match.Groups[ "FullApply" ].Success )
                return "FULL APPLY\r\n";

            if ( match.Groups[ "OuterApply" ].Success )
                return "OUTER APPLY\r\n";

            if ( match.Groups[ "string" ].Success )
                return "\r\n" + match.Value;

            return match.Value;
        } ), RegexOptions.ExplicitCapture | RegexOptions.Multiline | RegexOptions.IgnoreCase );

        MatchCollection matches = Regex.Matches( sql, @"(/\*((.|\s)*?)\*/|'((.|\s)*?)'|^(.+?)$)", RegexOptions.Multiline | RegexOptions.ExplicitCapture );

        SqlTree tree = new SqlTree();

        foreach ( Match match in matches )
        {
            string line = match.Value.Trim( ' ', '\n', '\r', '\t' );
            if ( line == string.Empty ) { }
            else if ( line == "(" )
            {
                trees.Push( tree );
                ParenthesisNode newTree = new ParenthesisNode();
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
            else
            {
                StringNode node = new StringNode();
                node.Value = line;
                tree.nodes.Enqueue( node );
            }
        }

        if ( trees.Count > 0 )
            throw new FormatException( "Muitos parênteses de abertura" );

        return tree;
    }

    public static string Format( string sql )
    {
        var sqlTree = SqlTreeBuilder.Build( sql );
        var sqlBuilder = new StringBuilder();

        sqlTree.FormatSql( sqlBuilder, string.Empty );
        sqlBuilder.Replace( "' '", "''" );

        return sqlBuilder.ToString();
    }
}