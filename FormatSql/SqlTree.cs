using SqlFormatter.Helpers;
using SqlFormatter.Interfaces;
using SqlFormatter.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlFormatter;

internal class SqlTree
{
    Dictionary<string, SqlKeyWord> structureKeyWords;
    bool inBetween = false;
    bool inLink = false;
    private bool startCondition = false;
    private string conditionPrefix = "  ";

    private static Regex reformatLines = new Regex( "^\\s*", RegexOptions.Multiline );
    Formater ContextFormater = null;

    private static string[] keyWords =
    {
        "TOP", "DISTINCT",
        "IN", "NOT", "LIKE",
        "AS", "ON"
    };

    private static string[] nativeFunctions =
    {
        "SUM", "AVG", "MAX", "MIN", "COUNT",
        "CONVERT", "ROUND", "EXISTS", "ISNULL",
        "GETDATE", "DATEPART", "DATEADD", "DATEDIFF"
    };

    internal Queue<ISQLTreeNode> nodes;

    private readonly Formater fieldListFormater;
    private readonly Formater generalFormater;
    private readonly Formater tableListFormater;
    private readonly Formater conditionFormater;

    internal SqlTree()
    {
        fieldListFormater = new Formater( FieldListFormater );
        generalFormater = new Formater( GeneralFormater );
        tableListFormater = new Formater( TableListFormater );
        conditionFormater = new Formater( ConditionFormater );


        nodes = new Queue<ISQLTreeNode>();

        structureKeyWords = new Dictionary<string, SqlKeyWord>( StringComparer.InvariantCultureIgnoreCase )
        {
            { "UNION", new SqlKeyWord( "UNION", null, false ) },
            { "SELECT", new SqlKeyWord( "SELECT", fieldListFormater, true, "\t" ) },
            { "INSERT INTO", new SqlKeyWord( "INSERT INTO", tableListFormater, false ) },
            { "DELETE", new SqlKeyWord( "DELETE", generalFormater, false ) },
            { "UPDATE", new SqlKeyWord( "UPDATE", generalFormater, false ) },
            { "SET", new SqlKeyWord( "SET", fieldListFormater, true, "\t" ) },
            { "INTO", new SqlKeyWord( "INTO", tableListFormater, false ) },
            { "FROM", new SqlKeyWord( "FROM", tableListFormater, true, "\t" ) },
            { "WHERE", new SqlKeyWord( "WHERE", conditionFormater, true, "\t" ) },
            { "HAVING", new SqlKeyWord( "HAVING", conditionFormater, false ) },
            { "GROUP BY", new SqlKeyWord( "GROUP BY", fieldListFormater, true, "\t" ) },
            { "ORDER BY", new SqlKeyWord( "ORDER BY", fieldListFormater, true, "\t" ) },
            { "GO", new SqlKeyWord( "GO", fieldListFormater, true, "\r\n" ) }
        };
    }

    internal bool FormatSql( StringBuilder sql, string indentString )
    {
        foreach ( ISQLTreeNode node in nodes )
        {
            if ( node is ParenthesisNode )
            {
                SqlTree tree = ( ( ParenthesisNode ) node ).Tree;
                sql.Append( "( " );

                if ( startCondition )
                {
                    tree.ContextFormater = tree.conditionFormater;
                    tree.startCondition = true;
                    tree.startCondition = true;
                    tree.conditionPrefix = string.Empty;
                    sql.AppendLine();
                    sql.Append( indentString );
                    sql.Append( "\t    " );
                    tree.FormatSql( sql, indentString + "\t" );
                    sql.AppendLine();
                    sql.Append( indentString );
                    sql.Append( "    " );
                }
                else
                {
                    if ( tree.FormatSql( sql, indentString + "\t\t" ) )
                    {
                        sql.AppendLine();
                        sql.Append( indentString );

                        if ( ContextFormater == conditionFormater ) sql.Append( "      " );
                        else if ( ContextFormater == fieldListFormater ) sql.Append( "\t" );
                        else if ( ContextFormater == tableListFormater ) sql.Append( "\t" );
                    }
                }

                sql.Append( ") " );
            }
            else if ( node is StringNode )
                GeneralFormater( ( string ) node.Content, sql, indentString );
        }

        return ( ContextFormater != null );
    }

    #region sistema de formatação

    private bool GeneralFormater( string line, StringBuilder sql, string indentString )
    {
        if ( line.StartsWith( "--" ) )
        {
            sql.AppendLine( line );
            sql.Append( indentString );

            if ( ContextFormater == fieldListFormater || ContextFormater == tableListFormater )
                sql.Append( "\t" );

            return true;
        }
        if ( line.StartsWith( "/*" ) )
        {
            sql.AppendLine();
            sql.AppendLine( reformatLines.Replace( line, indentString ) );
            sql.Append( indentString );

            if ( ContextFormater == fieldListFormater || ContextFormater == tableListFormater )
                sql.Append( "\t" );

            return true;
        }

        if ( ContextFormater != null && ContextFormater != GeneralFormater )
            if ( ContextFormater( line, sql, indentString ) )
                return true;

        if ( structureKeyWords.ContainsKey( line ) )
        {
            //remet à zéro les indicateurs de position
            inBetween = false;
            inLink = false;
            conditionPrefix = "  ";

            SqlKeyWord keyword = structureKeyWords[ line ];

            if ( sql.Length > 0 )
                sql.AppendLine();

            sql.Append( indentString );
            sql.Append( keyword.KeyWord );

            if ( keyword.LineFeedAfter )
            {
                sql.AppendLine();
                sql.Append( indentString );
            }
            else
                sql.Append( " " );

            sql.Append( keyword.InsertAfter );
            ContextFormater = keyword.Formater;
            startCondition = ContextFormater == conditionFormater;

            return true;
        }

        var isTrue = TestLine( line, sql, false, false, string.Empty, string.Empty, keyWords )
            || TestLine( line, sql, false, false, string.Empty, string.Empty, nativeFunctions )
            || TestLine( line, sql, false, false, string.Empty, string.Empty, 1, "," )
            || CaseFormater( line, sql, indentString );

        if ( isTrue )
            return true;

        sql.Append( line );
        sql.Append( " " );

        return true;
    }

    private bool CaseFormater( string line, StringBuilder sql, string indentString )
    {
        return TestLine( line, sql, true, false, indentString + "\t", string.Empty, "CASE", "END" ) 
            || TestLine( line, sql, true, false, indentString + "\t\t", string.Empty, "WHEN", "ELSE" ) 
            || TestLine( line, sql, false, false, string.Empty, string.Empty, "THEN" );
    }

    private bool FieldListFormater( string line, StringBuilder sql, string indentString )
    {
        return TestLine( line, sql, false, true, string.Empty, indentString + "\t", 1, "," ) 
            || TestLine( line, sql, false, false, string.Empty, string.Empty, "AS" ) 
            || CaseFormater( line, sql, indentString );
    }

    private bool TableListFormater( string line, StringBuilder sql, string indentString )
    {
        var isTrue = TestLine( line, sql, true, false, indentString + "\t", string.Empty, "LEFT JOIN", "RIGHT JOIN", "FULL JOIN", "INNER JOIN", "CROSS JOIN", "CROSS APPLY", "FULL APPLY" )
            || TestLine( line, sql, false, true, string.Empty, indentString + "\t", 1, "," );

        if ( isTrue )
        {
            inLink = false;
            return true;
        }
        if ( TestLine( line, sql, false, false, string.Empty, string.Empty, "ON" ) )
        {
            conditionPrefix = "\t\t  ";
            inLink = true;
            return true;
        }

        if ( inLink )
            return ConditionFormater( line, sql, indentString );

        return TestLine( line, sql, false, false, string.Empty, string.Empty, "AS" );
    }

    private bool ConditionFormater( string line, StringBuilder sql, string indentString )
    {
        startCondition = false;

        if ( TestLine( line, sql, false, false, string.Empty, string.Empty, "BETWEEN" ) )
        {
            inBetween = true;
            return true;
        }

        if ( inBetween && TestLine( line, sql, false, false, string.Empty, string.Empty, "AND" ) )
        {
            inBetween = false;
            return true;
        }

        if (
            TestLine( line, sql, true, false, indentString + conditionPrefix + "\t", string.Empty, "AND" ) ||
            TestLine( line, sql, true, false, indentString + conditionPrefix + " \t", string.Empty, "OR" )
            )
        {
            startCondition = true;
            return true;
        }
        return false;
    }

    #endregion

    private bool TestLine( string line, StringBuilder sql, bool InsertLineBefore, bool InsertLineAfter, string InsertBefore, string InsertAfter, params string[] tests )
    {
        int pos = line.FindIn( StringComparer.InvariantCultureIgnoreCase, tests );
        if ( pos > -1 )
        {
            if ( InsertLineBefore ) sql.AppendLine();
            sql.Append( InsertBefore );
            sql.Append( tests[ pos ] );
            sql.Append( " " );
            if ( InsertLineAfter ) sql.AppendLine();
            sql.Append( InsertAfter );
            return true;
        }
        return false;
    }

    private bool TestLine( string line, StringBuilder sql, bool InsertLineBefore, bool InsertLineAfter, string InsertBefore, string InsertAfter, int shift, params string[] tests )
    {
        int pos = line.FindIn( StringComparer.InvariantCultureIgnoreCase, tests );
        if ( pos > -1 )
        {
            if ( InsertLineBefore ) sql.AppendLine();
            sql.Append( InsertBefore );
            sql.Insert( sql.Length - shift, tests[ pos ] );
            if ( InsertLineAfter ) sql.AppendLine();
            sql.Append( InsertAfter );
            return true;
        }
        return false;
    }
}