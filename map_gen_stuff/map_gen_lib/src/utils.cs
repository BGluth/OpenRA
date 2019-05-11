using System;
using System.Collections.Generic;

namespace MapGen
{
    public struct Vector2
    {
        public int x;
        public int y;
    }

    static class Utils
    {
        public static void writeMessage(string message)
        {
            Console.WriteLine(message);
        }

        public static void writeWarning(string message)
        {

        }

        public static void writeError(string message)
        {

        }

        public static T listSwapRemove<T>(IList<T> a, int idx)
        {
            var tmp = a[idx];
            a[idx] = a[a.Count - 1];
            a.RemoveAt(a.Count - 1);
            return tmp;
        }

    }
}
