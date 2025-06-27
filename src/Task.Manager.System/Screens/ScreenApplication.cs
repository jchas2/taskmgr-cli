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
                        var currentScreen = _screenStack.Peek();
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
                    var ownerScreen = _screenStack.Peek();

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
                        var consoleKeyInfo = Console.ReadKey(intercept: true);
                        consoleKey = consoleKeyInfo.Key;

                        if (consoleKey == ConsoleKey.Escape) {
                            Debug.Assert(_ownerScreen != null);
                            
                            _ownerScreen.Close();
                            _screenStack.Pop();

                            if (_screenStack.Count == 0) {
                                break;
                            }
                            
                            _ownerScreen = _screenStack.Peek();
                            _ownerScreen.Show();
                            
                            continue;
                        }
                        
                        ownerScreen.KeyPressed(consoleKeyInfo);
                    }

                    // TODO: Mouse Events would be awesome.
                }

                // Small delay to prevent busy-wait.
                Thread.Sleep(30);
            }

            while (_screenStack.Count > 0) {
                var screen = _screenStack.Pop();
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
        
        if (false == _registeredScreens.ContainsKey(screen.GetType())) {
            throw new InvalidOperationException($"Screen {screen.GetType()} is not registered.");
        }
        
        _applicationContext.OwnerScreen = screen;
        _applicationContext.RunApplicationLoop();
    }

    public static void ShowScreen<T>() where T : Screen
    {
        if (false == _registeredScreens.ContainsKey(typeof(T))) {
            throw new InvalidOperationException($"Screen {typeof(T)} is not registered.");
        }
        
        var screen = (T)_registeredScreens[typeof(T)];
        
        _applicationContext.OwnerScreen = screen;
    }
}
