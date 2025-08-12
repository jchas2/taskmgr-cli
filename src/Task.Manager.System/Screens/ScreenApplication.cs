using System.Diagnostics;
using Task.Manager.System.Controls.MessageBox;

namespace Task.Manager.System.Screens;

public sealed class ScreenApplication
{
    public sealed class ScreenApplicationContext
    {
        private Screen? ownerScreen;
        private Stack<Screen> screenStack = new();
        private Lock @lock = new();

        internal Screen? OwnerScreen
        {
            get {
                lock (@lock) {
                    return ownerScreen;
                }
            }
            set {
                ArgumentNullException.ThrowIfNull(value, nameof(OwnerScreen));
                lock (@lock) {
                    if (screenStack.Count > 0 && screenStack.Peek() == value) {
                        return;
                    }

                    if (screenStack.Count > 0) {
                        Screen currentScreen = screenStack.Peek();
                        currentScreen.Close();
                    }

                    screenStack.Push(value);
                    ownerScreen = value;
                    
                    FitScreenToConsole(ownerScreen);
                    
                    ownerScreen.Show();
                }
            }
        }

        private void FitScreenToConsole(Screen screen)
        {
            screen.X = 0;
            screen.Y = 0;
            screen.Width = Console.WindowWidth;
            screen.Height = Console.WindowHeight;
        }
        
        public void RunApplicationLoop()
        {
            var consoleKey = ConsoleKey.None;
            int screenWidth = Console.WindowWidth;
            int screenHeight = Console.WindowHeight;

            // Main application loop.
            // Blocks the main thread and dispatches events to the loaded screen.
            while (consoleKey != ConsoleKey.F10) {

                lock (@lock) {
                    Screen ownerScreen = screenStack.Peek();

                    // Resize Events.
                    if (screenWidth != Console.WindowWidth || screenHeight != Console.WindowHeight) {
                        FitScreenToConsole(ownerScreen);
                        ownerScreen.Resize();
                        ownerScreen.Draw();
                        screenWidth = Console.WindowWidth;
                        screenHeight = Console.WindowHeight;
                        continue;
                    }

                    // Key Events.
                    if (Console.KeyAvailable) {
                        ConsoleKeyInfo consoleKeyInfo = Console.ReadKey(intercept: true);
                        consoleKey = consoleKeyInfo.Key;
                        var handled = false;
                        ownerScreen.KeyPressed(consoleKeyInfo, ref handled);

                        if (!handled && consoleKey == ConsoleKey.Escape) {
                            Debug.Assert(this.ownerScreen != null);
                            
                            this.ownerScreen.Close();
                            _ = screenStack.Pop();

                            if (screenStack.Count == 0) {
                                break;
                            }
                            
                            this.ownerScreen = screenStack.Peek();
                            this.ownerScreen.Show();
                            
                            continue;
                        }
                    }
                }

                if (Console.KeyAvailable) {
                    continue;
                }

                // Small delay to prevent busy-wait.
                Thread.Sleep(30);
            }

            while (screenStack.Count > 0) {
                Screen screen = screenStack.Pop();
                screen.Close();
            }
        }
    }

    private static readonly Dictionary<Type, Screen> registeredScreens = new();
    private static readonly ScreenApplicationContext applicationContext = new();
    private static int invocationCount = 0;

    public static void RegisterScreen(Screen screen)
    {
        if (registeredScreens.ContainsKey(screen.GetType())) {
            throw new InvalidOperationException($"Screen {screen.GetType()} is already registered.");
        }
        
        registeredScreens.Add(screen.GetType(), screen);
    }
    
    public static void Run(Screen screen)
    {
        ArgumentNullException.ThrowIfNull(screen, nameof(screen));

        if (++invocationCount > 1) {
            throw new InvalidOperationException("Screen App is already running.");
        }
        
        if (!registeredScreens.ContainsKey(screen.GetType())) {
            throw new InvalidOperationException($"Screen {screen.GetType()} is not registered.");
        }
        
        applicationContext.OwnerScreen = screen;
        applicationContext.RunApplicationLoop();
    }

    public static void ShowScreen<T>() where T : Screen
    {
        if (!registeredScreens.ContainsKey(typeof(T))) {
            throw new InvalidOperationException($"Screen {typeof(T)} is not registered.");
        }
        
        Screen screen = (T)registeredScreens[typeof(T)];
        
        applicationContext.OwnerScreen = screen;
    }
}
