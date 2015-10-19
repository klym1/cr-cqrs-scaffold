using System;

namespace TimeKeeping
{
    public static class Clock
    {
        private static Func<DateTime> _nowValueProvider;

        static Clock()
        {
            _nowValueProvider = () => DateTime.Now;
        }

        public static void Initialize(Func<DateTime> nowValueProvider)
        {
            _nowValueProvider = nowValueProvider;
        }

        public static DateTime Now => _nowValueProvider();
        public static DateTime Today => Now.Date;
    }
}
