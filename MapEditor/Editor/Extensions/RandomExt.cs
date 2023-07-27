using System.Collections.Generic;
using System;

namespace Editor.Extensions
{
    public static class RandomExt
    {

        public static T Choose<T>(this Random random, T a, T b) => Calc.GiveMe(random.Next(2), a, b);

        public static T Choose<T>(this Random random, T a, T b, T c) => Calc.GiveMe(random.Next(3), a, b, c);

        public static T Choose<T>(this Random random, T a, T b, T c, T d) => Calc.GiveMe(random.Next(4), a, b, c, d);

        public static T Choose<T>(this Random random, T a, T b, T c, T d, T e) => Calc.GiveMe(random.Next(5), a, b, c, d, e);

        public static T Choose<T>(this Random random, T a, T b, T c, T d, T e, T f) => Calc.GiveMe(random.Next(6), a, b, c, d, e, f);

        public static T Choose<T>(this Random random, List<T> choices) => choices[random.Next(choices.Count)];
    }
}
