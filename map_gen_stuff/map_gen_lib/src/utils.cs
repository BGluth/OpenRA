using System;
using System.Collections.Generic;

namespace MapGen
{
    public struct Vector2
    {
        public int x;
        public int y;

        public Vector2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    static class Utils
    {
        public static void writeMessage(string message)
        {
            Console.WriteLine(message);
        }

        public static void writeWarning(string message)
        {
            Console.WriteLine("WARNING: {}", message);
        }

        public static void writeError(string message)
        {
            Console.WriteLine("ERROR: {}", message);
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
