using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
 
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ERS
{
#if WINDOWS || XBOX
    static class MainProgram
    {
        public static BangGame game = null;
        public static SpriteBatch spriteBatch;
        public static Random random;
        

        /// <summary>
        /// Starts Bang.
        /// </summary>
        static void Main(string[] args)
        {
            random = new Random();
            game = new BangGame();
            game.Run();
        }


        //Simple utility methods

        //Takes a full, sorted array and adds a space to it, then places the element in the correct place.
        public static void Insert(ref IComparable[] array, IComparable x)
        {
            if (array == null || array.Length == 0) return;
            bool added = false;
            IComparable[] copy = new IComparable[array.Length + 1];

            for (int i = array.Length; i >= 0; i--)
            {
                if (!added)
                {
                    if (x.CompareTo(array[i - 1]) < 0)
                        copy[i] = array[i - 1];
                    else
                    { copy[i] = x; added = true; }
                }
                else
                    copy[i] = array[i];
            }

            array = copy;
        }

        //Inserts the object into a given index in the array.
        public static void Insert(ref object[] array, object x, int index)
        {
            if (array == null || array.Length == 0) return;
            bool added = false;
            object[] copy = new object[array.Length + 1];

            for (int i = 0; i < array.Length - 1; i++)
            {
                if (!added)
                {
                    if (i < index)
                        copy[i] = array[i];
                    else
                    { copy[i] = x; added = true; }
                }
                else
                    copy[i] = array[i - 1];
            }

            array = copy;
        }

        //Moves all the null objects to the back of the array.
        public static void MoveNullsToBack(object[] array, int count)
        {
            if (array == null || count > array.Length || array.Length == 0 || count < 0)
                return;

            int highest = count;
            for (int k = 0; k < highest; k++)
                if (k < array.Length && array[k] == null)
                    highest++;
            highest--;

            int c = 0;
            int afterHigh = highest;
            for (int i = 0; i < afterHigh; i++)
                if (array[i] == null)
                {
                    while (array[highest - c] == null && highest - c > i)
                    { c++; afterHigh--; }
                    array[i] = array[highest - c];
                    array[highest - c] = null;
                }
        }

        //Flips the two index values given.
        public static void Swap(object[] array, int a, int b)
        {
            if (array == null || array.Length == 0 || a < 0 || b < 0 || a >= array.Length || b >= array.Length)
                return;
            object placeholder = array[a];
            array[a] = array[b];
            array[b] = placeholder;
        }

        /// <summary>
        /// Returns true or false based off of the random chance given.
        /// </summary>
        /// <param name="percent">.9 means 90% true, 10% false.</param>
        /// <returns></returns>
        public static bool Chance(double percent)
        {
            return random.NextDouble() >= percent ? false : true;
        }
    }

#endif
}

