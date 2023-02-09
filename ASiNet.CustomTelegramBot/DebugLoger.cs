namespace ASiNet.CustomTelegramBot
{
#if DEBUG
    internal static class DebugLoger
    {
        private static readonly object _locker = new();
        public static void InfoLog(params string[] textParameters)
        {
            lock (_locker)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"[{DateTime.Now.ToString("T")}] [INFO]: [{string.Join(", ", textParameters)}]");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public static void WarningLog(params string[] textParameters)
        {
            lock (_locker)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[{DateTime.Now.ToString("T")}] [WARNING]: [{string.Join(", ", textParameters)}]");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public static void ErrorLog(Exception ex)
        {
            lock (_locker)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now.ToString("T")}] [ERROR]: [Source: {ex.Source ?? "<empty>"}, Msg: {ex.Message}]");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
#endif
}
