using SqlFormatter.Builders;
using SqlFormatter.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlFormatter;

public static class Formatter
{
    public static string Format( string sql )
    {
        var sqlTree = SqlTreeBuilder.Build( sql );
        var sqlBuilder = new StringBuilder();

        sqlTree.FormatSql( sqlBuilder, string.Empty );
        sqlBuilder.Replace( "' '", "''" );

        return sqlBuilder.ToString();
    }
}