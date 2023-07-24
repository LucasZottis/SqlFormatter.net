using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLFormater
{
    static class Tools
    {
        /// <summary>
        /// Indique si la valeur appartient aux tableau
        /// </summary>
        /// <typeparam name="T">Type de la valeur et du tableau</typeparam>
        /// <param name="value">valeur à tester</param>
        /// <param name="values">tableau de valeurs</param>
        /// <returns>vrai si value est une valeur de values</returns>
        public static bool In<T>( this T value, params T[] values )
        {
            return Array.IndexOf<T>( values, value ) > -1;
        }

        /// <summary>
        /// Indique si la valeur appartient aux tableau
        /// </summary>
        /// <typeparam name="T">Type de la valeur et du tableau</typeparam>
        /// <param name="comparer">Systeme de comparaison</param>
        /// <param name="value">valeur à tester</param>
        /// <param name="values">tableau de valeurs</param>
        /// <returns>vrai si value est une valeur de values</returns>
        public static bool In<T>( this T value, IComparer<T> comparer, params T[] values )
        {
            foreach (T valueToCompare in values)
            {
                if (comparer.Compare( value, valueToCompare ) == 0) return true;
            }
            return false;
        }

        /// <summary>
        /// Retourne la position d'une valeur dans une liste de valeurs
        /// </summary>
        /// <typeparam name="T">Type de la valeur et du tableau</typeparam>
        /// <param name="value">valeur à tester</param>
        /// <param name="values">tableau de valeurs</param>
        /// <returns>Position de value dans values</returns>
        public static int FindIn<T>( this T value, params T[] values )
        {
            return Array.IndexOf<T>( values, value );
        }

        /// <summary>
        /// Retourne la position d'une valeur dans une liste de valeurs
        /// </summary>
        /// <typeparam name="T">Type de la valeur et du tableau</typeparam>
        /// <param name="value">valeur à tester</param>
        /// <param name="values">tableau de valeurs</param>
        /// <returns>Position de value dans values</returns>
        public static int FindIn<T>( this T value, IComparer<T> comparer, params T[] values )
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (comparer.Compare( value, values[ i ] ) == 0) return i;
            }
            return -1;
        }

    }
}
