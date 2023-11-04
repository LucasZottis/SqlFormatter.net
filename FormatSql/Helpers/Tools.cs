namespace SqlFormatter.Helpers;

static class Tools
{
    /// <summary>
    /// Indica se o valor pertence ao array
    /// </summary>
    /// <typeparam name="T">Valor e tipo de array</typeparam>
    /// <param name="value">valor a ser testado</param>
    /// <param name="values">matriz de valores</param>
    /// <returns>true se valor for um valor de valores</returns>
    public static bool In<T>( this T value, params T[] values )
    {
        return Array.IndexOf( values, value ) > -1;
    }

    /// <summary>
    /// Indica se o valor pertence ao array
    /// </summary>
    /// <typeparam name="T">Tipo de valor e array</typeparam>
    /// <param name="compare">Sistema de comparação</param>
    /// <param name="value">valor a ser testado</param>
    /// <param name="values">matriz de valores</param>
    /// <returns>true se valor for um valor de valores</returns>
    public static bool In<T>( this T value, IComparer<T> comparer, params T[] values )
    {
        foreach ( T valueToCompare in values )
            if ( comparer.Compare( value, valueToCompare ) == 0 ) 
                return true;

        return false;
    }

    /// <summary>
    /// Retorna a posição de um valor em uma lista de valores
    /// </summary>
    /// <typeparam name="T">Tipo de valor e array</typeparam>
    /// <param name="value">valor a ser testado</param>
    /// <param name="values">matriz de valores</param>
    /// <returns>Posição do valor em valores</returns>
    public static int FindIn<T>( this T value, params T[] values )
    {
        return Array.IndexOf( values, value );
    }

    /// <summary>
    /// Retorna a posição de um valor em uma lista de valores
    /// </summary>
    /// <typeparam name="T">Tipo de valor e array</typeparam>
    /// <param name="value">valor a ser testado</param>
    /// <param name="values">matriz de valores</param>
    /// <returns>Posição do valor em valores</returns>
    public static int FindIn<T>( this T value, IComparer<T> comparer, params T[] values )
    {
        for ( int i = 0; i < values.Length; i++ )
            if ( comparer.Compare( value, values[ i ] ) == 0 )
                return i;
        
        return -1;
    }

}
