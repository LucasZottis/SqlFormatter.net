namespace SqlFormatter.Models;

internal class SqlKeyWord
{
    internal string KeyWord { get; private set; }
    internal Formater Formater { get; private set; }
    internal bool LineFeedAfter { get; private set; }
    internal string InsertAfter { get; private set; }

    internal SqlKeyWord( string KeyWord, Formater Formater, bool LineFeedAfter ) :
        this( KeyWord, Formater, LineFeedAfter, string.Empty ) { }

    internal SqlKeyWord( string KeyWord, Formater Formater, bool LineFeedAfter, string InsertAfter )
    {
        this.KeyWord = KeyWord;
        this.Formater = Formater;
        this.LineFeedAfter = LineFeedAfter;
        this.InsertAfter = InsertAfter;
    }
}