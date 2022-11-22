using System;
using advent_of_qode_server.Controllers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace advent_of_qode_server.Logic
{
    public class ScoreCalulator
    {
        const double _k = 10.93389831;
        const double _m = 42;

        public static int Calculate(DateTime started, DateTime now)
        {
            TimeSpan span = now - started;

            var timeScore = span.TotalMilliseconds;
            timeScore = timeScore * _k / 1000;
            var scoreToReduce = (int)Math.Floor(timeScore);

            int maxScore = 207;
            int returnScore = maxScore - scoreToReduce;
            if (returnScore < _m)
                return (int)_m;

            return returnScore;
        }
    }
}
