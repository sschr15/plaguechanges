using static System.Math;

namespace PlagueChanges
{
    public static class Util
    {
        public static int AwayFromZero(double d)
        {
            return d > 0 ? (int) Ceiling(d) : (int) Floor(d);
        }
    }
}