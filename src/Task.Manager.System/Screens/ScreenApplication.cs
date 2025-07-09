using System.Diagnostics;

namespace Task.Manager.System.Screens;

public sealed class ScreenApplication
{
    public sealed class ScreenApplicationContext
    {
        private Screen? _ownerScreen;
        private Stack<Screen> _screenStack = new();
        private Lock _lock = new();

        internal Screen? OwnerScreen
        {
            get {
                lock (_lock) {
                    return _ownerScreen;
                }
            }
            set {
                ArgumentNullException.ThrowIfNull(value, nameof(OwnerScreen));
                lock (_lock) {
                    if (_screenStack.Count > 0 && _screenStack.Peek() == value) {
                        return;
                    }

                    if (_screenStack.Count > 0) {
                        Screen currentScreen = _screenStack.Peek();
                        currentScreen.Close();
                    }

                    _screenStack.Push(value);
                    _ownerScreen = value;
                    _ownerScreen.Show();
                }
            }
        }

        public void RunApplicationLoop()
        {
            var consoleKey = ConsoleKey.None;
            int screenWidth = Console.WindowWidth;
            int screenHeight = Console.WindowHeight;

            // Main application loop.
            // Blocks the main thread and dispatches events to the loaded screen.
            while (consoleKey != ConsoleKey.F10) {

                lock (_lock) {
                    Screen ownerScreen = _screenStack.Peek();

                    // Resize Events.
                    if (screenWidth != Console.WindowWidth || screenHeight != Console.WindowHeight) {
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
                            Debug.Assert(_ownerScreen != null);
                            
                            _ownerScreen.Close();
                            _ = _screenStack.Pop();

                            if (_screenStack.Count == 0) {
                                break;
                            }
                            
                            _ownerScreen = _screenStack.Peek();
                            _ownerScreen.Show();
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

            while (_screenStack.Count > 0) {
                Screen screen = _screenStack.Pop();
                screen.Close();
            }
        }
    }

    private static readonly Dictionary<Type, Screen> _registeredScreens = new();
    private static readonly ScreenApplicationContext _applicationContext = new();
    private static int _invocationCount = 0;
    
    public static void RegisterScreen(Screen screen)
    {
        if (_registeredScreens.ContainsKey(screen.GetType())) {
            throw new InvalidOperationException($"Screen {screen.GetType()} is already registered.");
        }
        
        _registeredScreens.Add(screen.GetType(), screen);
    }
    
    public static void Run(Screen screen)
    {
        ArgumentNullException.ThrowIfNull(screen, nameof(screen));

        if (++_invocationCount > 1) {
            throw new InvalidOperationException("Screen App is already running.");
        }
        
        if (!_registeredScreens.ContainsKey(screen.GetType())) {
            throw new InvalidOperationException($"Screen {screen.GetType()} is not registered.");
        }
        
        _applicationContext.OwnerScreen = screen;
        _applicationContext.RunApplicationLoop();
    }

    public static void ShowScreen<T>() where T : Screen
    {
        if (!_registeredScreens.ContainsKey(typeof(T))) {
            throw new InvalidOperationException($"Screen {typeof(T)} is not registered.");
        }
        
        Screen screen = (T)_registeredScreens[typeof(T)];
        
        _applicationContext.OwnerScreen = screen;
    }
}
