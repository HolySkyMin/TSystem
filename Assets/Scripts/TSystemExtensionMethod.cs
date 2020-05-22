using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSystem
{
    public static class TSystemExtensionMethod
    {
        public static bool IsEither<T>(this T type, params T[] elements) where T : IComparable
        {
            for (int i = 0; i < elements.Length; i++)
                if (EqualityComparer<T>.Default.Equals(type, elements[i]))
                    return true;
            return false;
        }

        public static bool IsBetween<T>(this T value, T left, T right, bool allowLeftEqual = false, bool allowRightEqual = false) where T : IComparable<T>
        {
            var leftComp = Comparer<T>.Default.Compare(value, left);
            var rightComp = Comparer<T>.Default.Compare(right, value);
            if ((allowLeftEqual ? leftComp >= 0 : leftComp > 0) && (allowRightEqual ? rightComp >= 0 : rightComp > 0))
                return true;
            else
                return false;
        }
    }
}