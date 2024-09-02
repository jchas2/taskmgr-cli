namespace Task.Manager.Interop.Win32
{
    public static class FILETIMEExtensions
    {
        public static long ToDateTime(this MinWinBase.FILETIME fileTime)
        {
            ulong highTime = (ulong)fileTime.dwHighDateTime;
            uint lowTime = (uint)fileTime.dwLowDateTime;

            long result = (long)((highTime << 32) + lowTime);

            return result;

            //try {
            //    return DateTime.FromFileTimeUtc(result);
            //}
            //catch {
            //    //TODO: Write to log.
            //    return DateTime.FromFileTime(0xFFFFFFFF);
            //}
        }
    }
}
