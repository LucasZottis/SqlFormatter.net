using SqlFormatter;

namespace IdentaSql;

internal class Program
{
    static void Main( string[] args )
    {
        Console.WriteLine( Formatter.Format( "SELECT id, nome FROM usuarios WHERE idade > 18" ) );
        Console.WriteLine();
        Console.WriteLine( Formatter.Format( "SELECT id, nome FROM usuarios WHERE idade in ( 18, 6, 9 )" ) );
    }
}