using System.Collections.Generic;
using System;

namespace Editor.Utils
{
    public static class RandomExt
    {

        public static T Choose<T>(this Random random, T a, T b) => Calc.GiveMe(random.Next(2), a, b);

        public static T Choose<T>(this Random random, List<T> choices) => choices[random.Next(choices.Count)];
    }
}
