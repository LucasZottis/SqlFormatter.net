using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace SQLFormater
{
    class SQLTree
    {
        Dictionary<string, SqlKeyWord> structureKeyWords;
        bool inBetween = false;

        private static Regex reformatLines = new Regex( "^\\s*", RegexOptions.Multiline );

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

        Queue<ISQLTreeNode> nodes;

        private readonly Formater fieldListFormater;
        private readonly Formater generalFormater;
        private readonly Formater tableListFormater;
        private readonly Formater conditionFormater;

        private SQLTree()
        {
            fieldListFormater = new Formater( FieldListFormater );
            generalFormater = new Formater( GeneralFormater );
            tableListFormater = new Formater( TableListFormater );
            conditionFormater = new Formater( ConditionFormater );


            nodes = new Queue<ISQLTreeNode>();

            structureKeyWords = new Dictionary<string, SqlKeyWord>( StringComparer.InvariantCultureIgnoreCase );
            structureKeyWords.Add( "UNION", new SqlKeyWord( "UNION", null, false ) );
            structureKeyWords.Add( "SELECT", new SqlKeyWord( "SELECT", fieldListFormater, true, "\t" ) );
            structureKeyWords.Add( "INSERT INTO", new SqlKeyWord( "INSERT INTO", tableListFormater, false ) );
            structureKeyWords.Add( "DELETE", new SqlKeyWord( "DELETE", generalFormater, false ) );
            structureKeyWords.Add( "UPDATE", new SqlKeyWord( "UPDATE", generalFormater, false ) );
            structureKeyWords.Add( "SET", new SqlKeyWord( "SET", fieldListFormater, true, "\t" ) );
            structureKeyWords.Add( "INTO", new SqlKeyWord( "INTO", tableListFormater, false ) );
            structureKeyWords.Add( "FROM", new SqlKeyWord( "FROM", tableListFormater, true, "\t" ) );
            structureKeyWords.Add( "WHERE", new SqlKeyWord( "WHERE", conditionFormater, true, "\t" ) );
            structureKeyWords.Add( "HAVING", new SqlKeyWord( "HAVING", conditionFormater, false ) );
            structureKeyWords.Add( "GROUP BY", new SqlKeyWord( "GROUP BY", fieldListFormater, true, "\t" ) );
            structureKeyWords.Add( "ORDER BY", new SqlKeyWord( "ORDER BY", fieldListFormater, true, "\t" ) );
            structureKeyWords.Add( "GO", new SqlKeyWord( "GO", fieldListFormater, true, "\r\n" ) );
        }

        #region préparation du formatage
        public static string FormatSql( string sql )
        {
            SQLTree sqlTree = Parse( sql );

            StringBuilder sqlBuilder = new StringBuilder();
            sqlTree.FormatSql( sqlBuilder, string.Empty );
            sqlBuilder.Replace( "' '", "''" );
            return sqlBuilder.ToString();
        }

        /// <summary>
        /// Préparation du formatage
        /// </summary>
        /// <param name="Query"></param>
        /// <returns></returns>
		private static SQLTree Parse( string Query )
        {
            Stack<SQLTree> trees = new Stack<SQLTree>();

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


            string sql = Regex.Replace( Query, "(" + string.Join( "|", schemes ) + ")", new MatchEvaluator( delegate ( Match match ) {
                if (match.Groups[ "Name" ].Success)
                {
                    return match.Value;
                }
                if (match.Groups[ "nonChar" ].Success)
                {
                    return "\r\n" + match.Value + "\r\n";
                }
                if (match.Groups[ "spaces" ].Success)
                {
                    return "\r\n";
                }
                if (match.Groups[ "insert" ].Success)
                {
                    return "INSERT INTO\r\n";
                }
                if (match.Groups[ "groupBy" ].Success)
                {
                    return "GROUP BY\r\n";
                }
                if (match.Groups[ "orderBy" ].Success)
                {
                    return "ORDER BY\r\n";
                }

                if (match.Groups[ "LeftJoin" ].Success)
                {
                    return "LEFT JOIN\r\n";
                }
                if (match.Groups[ "RightJoin" ].Success)
                {
                    return "RIGHT JOIN\r\n";
                }
                if (match.Groups[ "FullJoin" ].Success)
                {
                    return "FULL JOIN\r\n";
                }
                if (match.Groups[ "CrossJoin" ].Success)
                {
                    return "CROSS JOIN\r\n";
                }
                if (match.Groups[ "FullApply" ].Success)
                {
                    return "FULL APPLY\r\n";
                }
                if (match.Groups[ "OuterApply" ].Success)
                {
                    return "OUTER APPLY\r\n";
                }
                if (match.Groups[ "string" ].Success)
                {
                    return "\r\n" + match.Value;
                }
                return match.Value;
            } ), RegexOptions.ExplicitCapture | RegexOptions.Multiline | RegexOptions.IgnoreCase );

            MatchCollection matches = Regex.Matches( sql, @"(/\*((.|\s)*?)\*/|'((.|\s)*?)'|^(.+?)$)", RegexOptions.Multiline | RegexOptions.ExplicitCapture );

            SQLTree tree = new SQLTree();

            foreach (Match match in matches)
            {
                string line = match.Value.Trim( ' ', '\n', '\r', '\t' );
                if (line == string.Empty)
                {
                }
                else if (line == "(")
                {
                    trees.Push( tree );
                    ParenthesisNode newTree = new ParenthesisNode();
                    newTree.Tree = new SQLTree();
                    tree.nodes.Enqueue( newTree );
                    tree = newTree.Tree;
                }
                else if (line == ")")
                {
                    try
                    {
                        tree = trees.Pop();
                    }
                    catch
                    {
                        throw new FormatException( "Trop de parenthèses fermantes" );
                    }
                }
                else
                {
                    StringNode node = new StringNode();
                    node.Value = line;
                    tree.nodes.Enqueue( node );
                }
            }

            if (trees.Count > 0) throw new FormatException( "Trop de parenthèses ouvrantes" );
            return tree;
        }
        #endregion

        private bool FormatSql( StringBuilder sql, string indentString )
        {
            foreach (ISQLTreeNode node in nodes)
            {
                if (node is ParenthesisNode)
                {
                    SQLTree tree = ((ParenthesisNode) node).Tree;
                    sql.Append( "( " );

                    if (startCondition)
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
                        if (tree.FormatSql( sql, indentString + "\t\t" ))
                        {
                            sql.AppendLine();
                            sql.Append( indentString );

                            if (ContextFormater == conditionFormater) sql.Append( "      " );
                            else if (ContextFormater == fieldListFormater) sql.Append( "\t" );
                            else if (ContextFormater == tableListFormater) sql.Append( "\t" );
                        }
                    }
                    sql.Append( ") " );
                }
                else if (node is StringNode)
                {
                    GeneralFormater( (string) node.Content, sql, indentString );
                }
            }

            return (ContextFormater != null);
        }

        #region système de formatage
        public delegate bool Formater( string line, StringBuilder sql, string indentString );
        Formater ContextFormater = null;

        private bool GeneralFormater( string line, StringBuilder sql, string indentString )
        {
            if (line.StartsWith( "--" ))
            {
                sql.AppendLine( line );
                sql.Append( indentString );
                if (ContextFormater == fieldListFormater || ContextFormater == tableListFormater)
                {
                    sql.Append( "\t" );
                }
                return true;
            }
            if (line.StartsWith( "/*" ))
            {
                sql.AppendLine();
                sql.AppendLine( reformatLines.Replace( line, indentString ) );
                sql.Append( indentString );
                if (ContextFormater == fieldListFormater || ContextFormater == tableListFormater)
                {
                    sql.Append( "\t" );
                }
                return true;
            }

            if (ContextFormater != null && ContextFormater != GeneralFormater)
            {
                if (ContextFormater( line, sql, indentString )) return true;
            }

            if (structureKeyWords.ContainsKey( line ))
            {
                //remet à zéro les indicateurs de position
                inBetween = false;
                inLink = false;
                conditionPrefix = "  ";

                SqlKeyWord keyword = structureKeyWords[ line ];

                if (sql.Length > 0) sql.AppendLine();
                sql.Append( indentString );
                sql.Append( keyword.KeyWord );
                if (keyword.LineFeedAfter)
                {
                    sql.AppendLine();
                    sql.Append( indentString );
                }
                else
                {
                    sql.Append( " " );
                }
                sql.Append( keyword.InsertAfter );
                ContextFormater = keyword.Formater;
                startCondition = ContextFormater == conditionFormater;
                return true;
            }

            if (
                TestLine( line, sql, false, false, string.Empty, string.Empty, keyWords ) ||
                TestLine( line, sql, false, false, string.Empty, string.Empty, nativeFunctions ) ||
                TestLine( line, sql, false, false, string.Empty, string.Empty, 1, "," ) ||
                CaseFormater( line, sql, indentString )
                ) return true;

            sql.Append( line );
            sql.Append( " " );

            return true;
        }

        private bool CaseFormater( string line, StringBuilder sql, string indentString )
        {
            return
                TestLine( line, sql, true, false, indentString + "\t", string.Empty, "CASE", "END" ) ||
                TestLine( line, sql, true, false, indentString + "\t\t", string.Empty, "WHEN", "ELSE" ) ||
                TestLine( line, sql, false, false, string.Empty, string.Empty, "THEN" );
        }

        private bool FieldListFormater( string line, StringBuilder sql, string indentString )
        {
            return
                TestLine( line, sql, false, true, string.Empty, indentString + "\t", 1, "," ) ||
                TestLine( line, sql, false, false, string.Empty, string.Empty, "AS" ) ||
                CaseFormater( line, sql, indentString );
        }

        bool inLink = false;
        private bool TableListFormater( string line, StringBuilder sql, string indentString )
        {
            if (
                TestLine( line, sql, true, false, indentString + "\t", string.Empty, "LEFT JOIN", "RIGHT JOIN", "FULL JOIN", "INNER JOIN", "CROSS JOIN", "CROSS APPLY", "FULL APPLY" ) ||
                TestLine( line, sql, false, true, string.Empty, indentString + "\t", 1, "," )
                )
            {
                inLink = false;
                return true;
            }
            if (
                TestLine( line, sql, false, false, string.Empty, string.Empty, "ON" )
                )
            {
                conditionPrefix = "\t\t  ";
                inLink = true;
                return true;
            }

            if (inLink)
            {
                return ConditionFormater( line, sql, indentString );
            }

            return TestLine( line, sql, false, false, string.Empty, string.Empty, "AS" );
        }

        private bool startCondition = false;
        private string conditionPrefix = "  ";
        private bool ConditionFormater( string line, StringBuilder sql, string indentString )
        {
            startCondition = false;
            if (TestLine( line, sql, false, false, string.Empty, string.Empty, "BETWEEN" ))
            {
                inBetween = true;
                return true;
            }

            if (inBetween && TestLine( line, sql, false, false, string.Empty, string.Empty, "AND" ))
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
            if (pos > -1)
            {
                if (InsertLineBefore) sql.AppendLine();
                sql.Append( InsertBefore );
                sql.Append( tests[ pos ] );
                sql.Append( " " );
                if (InsertLineAfter) sql.AppendLine();
                sql.Append( InsertAfter );
                return true;
            }
            return false;
        }

        private bool TestLine( string line, StringBuilder sql, bool InsertLineBefore, bool InsertLineAfter, string InsertBefore, string InsertAfter, int shift, params string[] tests )
        {
            int pos = line.FindIn( StringComparer.InvariantCultureIgnoreCase, tests );
            if (pos > -1)
            {
                if (InsertLineBefore) sql.AppendLine();
                sql.Append( InsertBefore );
                sql.Insert( sql.Length - shift, tests[ pos ] );
                if (InsertLineAfter) sql.AppendLine();
                sql.Append( InsertAfter );
                return true;
            }
            return false;
        }

    }

    class SqlKeyWord
    {
        public string KeyWord { get; private set; }
        public SQLTree.Formater Formater { get; private set; }
        public bool LineFeedAfter { get; private set; }
        public string InsertAfter { get; private set; }

        internal SqlKeyWord( string KeyWord, SQLTree.Formater Formater, bool LineFeedAfter ) :
            this( KeyWord, Formater, LineFeedAfter, string.Empty )
        {
        }

        internal SqlKeyWord( string KeyWord, SQLTree.Formater Formater, bool LineFeedAfter, string InsertAfter )
        {
            this.KeyWord = KeyWord;
            this.Formater = Formater;
            this.LineFeedAfter = LineFeedAfter;
            this.InsertAfter = InsertAfter;
        }
    }
}

