namespace BinLiteServer
{
    public static class Snowflake
    {
        private static ulong lastTime;
        private static ulong increment;

        public static readonly ulong Epoch = 1635314400000;

        public static ulong Generate(ulong instance = 0)
        {
            var timeComponent = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var sequence = lastTime == timeComponent ? ++increment : (increment = 0);
            lastTime = timeComponent;

            return ((((timeComponent - Epoch) << 10) | instance) << 12) | sequence;
        }

        public static DateTimeOffset GetTime(ulong snowflake)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds((long)(snowflake / 4194304 + Epoch));
        }
    }
}
