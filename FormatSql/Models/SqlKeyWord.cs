namespace SqlFormatter.Models;

public class SqlKeyWord
{
    public string KeyWord { get; private set; }
    public Formater Formater { get; private set; }
    public bool LineFeedAfter { get; private set; }
    public string InsertAfter { get; private set; }

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