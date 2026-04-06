namespace ItreeNet.Data.Extensions
{
    public static class ZeitFormatExtension
    {
        /// <summary>
        /// Formatiert Minuten als "1h 30min", "2h", "45min" etc.
        /// </summary>
        public static string FormatMinuten(this int? minuten)
        {
            if (!minuten.HasValue) return string.Empty;
            return FormatMinuten(minuten.Value);
        }

        /// <summary>
        /// Formatiert Minuten als "1h 30min", "2h", "45min" etc.
        /// </summary>
        public static string FormatMinuten(this int minuten)
        {
            var h = minuten / 60;
            var m = minuten % 60;
            if (h == 0) return $"{m}min";
            if (m == 0) return $"{h}h";
            return $"{h}h {m}min";
        }

        /// <summary>
        /// Formatiert Minuten als "1:30", "0:45", "2:00" etc. (für Reports/Dokumente).
        /// </summary>
        public static string FormatMinutenAlsZeit(this int minuten)
        {
            var h = minuten / 60;
            var m = minuten % 60;
            return $"{h}:{m:D2}";
        }

        /// <summary>
        /// Formatiert Minuten als "1:30", "0:45", "2:00" etc. (für Reports/Dokumente).
        /// </summary>
        public static string FormatMinutenAlsZeit(this int? minuten)
        {
            if (!minuten.HasValue) return string.Empty;
            return FormatMinutenAlsZeit(minuten.Value);
        }

        /// <summary>
        /// Konvertiert Minuten (int) in Dezimalstunden (decimal).
        /// </summary>
        public static decimal MinutenZuStunden(this int minuten)
        {
            return minuten / 60m;
        }

        /// <summary>
        /// Konvertiert Minuten (int?) in Dezimalstunden (decimal?).
        /// </summary>
        public static decimal? MinutenZuStunden(this int? minuten)
        {
            return minuten.HasValue ? minuten.Value / 60m : null;
        }
    }
}
