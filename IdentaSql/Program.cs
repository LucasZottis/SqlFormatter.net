using SqlFormatter;
using System.Text.RegularExpressions;

namespace IdentaSql;

internal class Program
{
    static void Main( string[] args )
    {
        string scriptSql = @" SELECT id, nome FROM usuarios WHERE idade > 18;";

        string scriptSqlIdentado = Formatter.Format( scriptSql );
        Console.WriteLine( scriptSqlIdentado );
    }
}