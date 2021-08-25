using System;

namespace Youtube_Music.Extensions
{
    public static class ArrayExtension
    {
        public static T[] Randomize<T>(this T[] array)
        {
            Random rand = new();

            // For each spot in the array, pick
            // a random item to swap into that spot.
            for (int i = 0; i < array.Length - 1; i++)
            {
                int j = rand.Next(i, array.Length);
                T temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
            return array;
        }
    }
}
