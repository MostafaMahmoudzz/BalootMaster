using System.Collections;
using System.Collections.Generic;

public static class ListExtentions
{
    private static System.Random random = new System.Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static T Last<T>(this IList<T> list)
    {
        return list[list.Count - 1];
    }

    public static T Front<T>(this IList<T> list)
    {
        return list[0];
    }

    public static T PopLast<T>(this IList<T> list)
    {
        T obj = list.Last();
        list.RemoveAt(list.Count - 1);
        return obj;
    }

    public static T PopFront<T>(this IList<T> list)
    {
        T obj = list.Front();
        list.RemoveAt(0);
        return obj;
    }
}
