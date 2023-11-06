using SqlFormatter;
using System.Text.RegularExpressions;

namespace IdentaSql;

internal class Program
{
    static void Main( string[] args )
    {
        string sql1 = @" SELECT id, nome FROM usuarios WHERE idade > 18";
        string sql2 = @" SELECT id, nome FROM usuarios WHERE idade in ( 18, 6, 9 )";

        Console.WriteLine( Formatter.Format( sql1 ) );
        Console.WriteLine();
        Console.WriteLine( Formatter.Format( sql2 ) );
    }
}