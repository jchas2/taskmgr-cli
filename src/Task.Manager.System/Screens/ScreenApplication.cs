namespace Task.Manager.System.Screens;

public sealed class ScreenApplication
{
    public sealed class ScreenApplicationContext
    {
        private Screen? _mainScreen;

        public ScreenApplicationContext() { }

        public Screen? MainScreen { get => _mainScreen; }

        public void RunApplicationLoop(Screen? mainScreen)
        {
            ArgumentNullException.ThrowIfNull(mainScreen, nameof(mainScreen));

            _mainScreen = mainScreen;
            _mainScreen.Show();

            var consoleKey = ConsoleKey.None;
            int screenWidth = Console.WindowWidth;
            int screenHeight = Console.WindowHeight;
            
            // Main application loop. Blocks the main thread and dispatches events to the main screen.
            while (consoleKey != ConsoleKey.Escape) {
            
                // Resize Events.
                if (screenWidth != Console.WindowWidth || screenHeight != Console.WindowHeight) {
                    _mainScreen.Resize();
                    screenWidth = Console.WindowWidth;
                    screenHeight = Console.WindowHeight;
                }
                
                // Key Events.
                if (Console.KeyAvailable) {
                    var consoleKeyInfo = Console.ReadKey(intercept: true);
                    consoleKey = consoleKeyInfo.Key;
                    _mainScreen.KeyPressed(consoleKeyInfo);
                }
            
                // TODO: Mouse Events would be awesome.
            
                // TODO: Attach context to any Screen that calls .Show()? This way the message loop will swtich
                // to dispatching to the screen. Better than having this loop in each screen.
                
                // Small delay to prevent busy-wait.
                Thread.Sleep(30);
            }
            
            _mainScreen.Close();
        }
    }

    private static ScreenApplicationContext _applicationContext = new();
    
    public static void Run(Screen screen)
    {
        ArgumentNullException.ThrowIfNull(screen, nameof(screen));
        
        _applicationContext.RunApplicationLoop(screen);
    }
}