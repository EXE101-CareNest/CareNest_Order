namespace Shared.Helper
{
    public class TimeHelper
    {
        /// <summary>
        /// Trả về thời gian hiện tại ở định dạng UTC (DateTimeOffset)
        /// </summary>
        public static DateTimeOffset GetUtcNow()
        {
            return DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Trả về thời gian local của hệ thống (nếu cần dùng)
        /// </summary>
        public static DateTimeOffset GetLocalNow()
        {
            return DateTimeOffset.Now;
        }

        /// <summary>
        /// Lấy thời gian theo múi giờ cụ thể (ví dụ: "Asia/Ho_Chi_Minh")
        /// </summary>
        public static DateTimeOffset GetTimeByTimeZone(string timeZoneId)
        {
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone);
            return localTime;
        }
    }
}
