using System.Text.RegularExpressions;

namespace SqlFormatter.Helpers;

internal static class QueryHelper
{
    private static string[] _schemes = new string[] {
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

    private static string _pattern = string.Join( "|", _schemes );

    internal static string PrepareQuery( string query )
    {
        var sql = Regex.Replace( query, $"({_pattern})", new MatchEvaluator( delegate ( Match match )
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

        return sql;
    }
}