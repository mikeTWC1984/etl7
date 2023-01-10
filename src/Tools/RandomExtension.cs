using System;

namespace ETL.Tools
{
    public static class RandomExtension
    {
        public static long NextLong(this Random random, long maxValue)
        {
            if (maxValue < 1)
                throw new ArgumentOutOfRangeException("maxValue", "'maxValue'must be greater then 0.");
            
            byte[] buffer = new byte[8];
            random.NextBytes(buffer);

            return  Math.Abs(BitConverter.ToInt64(buffer, 0) % maxValue);
        }
    }
}
