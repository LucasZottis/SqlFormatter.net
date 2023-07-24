using SQLFormater;
using System.Text.RegularExpressions;

namespace IdentaSql
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string scriptSql = @" SELECT id, nome FROM usuarios WHERE idade > 18;";

            //string scriptSqlIdentado = IdentarSql(scriptSql, 5);
            string scriptSqlIdentado = SQLTree.FormatSql(scriptSql);
            Console.WriteLine(scriptSqlIdentado);
        }

        public static string IdentarSql(string scriptSql, int nivelIdentacao = 0)
        {
            // Remover espaços em branco desnecessários
            scriptSql = Regex.Replace(scriptSql, @"\s+", " ");

            // Adicionar uma nova linha antes de cada palavra-chave
            scriptSql = Regex.Replace(scriptSql, @"\b(INSERT|SELECT|UPDATE|DELETE|FROM|JOIN|WHERE|ORDER BY|GROUP BY)\b", Environment.NewLine + "$1");

            // Adicionar indentação
            scriptSql = Regex.Replace(scriptSql, @"[\r\n]+", m =>
            {
                string indentacao = new string('\t', nivelIdentacao);
                nivelIdentacao += Regex.Matches(m.Value, @"\b((?:SELECT|FROM|JOIN|WHERE|ORDER BY|GROUP BY)\b|(\),))").Count;
                return Environment.NewLine + indentacao + m.Value.TrimStart();
            });

            return scriptSql;
        }
    }
}