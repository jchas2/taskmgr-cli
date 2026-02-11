# taskmgr - Cross-Platform Command-Line Task Manager

<p align="center">
  <img src="docs/images/mainscreen.png" alt="taskmgr main screen" width="800"/>
</p>

A powerful, cross-platform command-line task manager with real-time process monitoring, advanced filtering, and rich system information display. Built with .NET 9.0 for Windows and macOS.

## Features

### 🚀 Core Capabilities

- **Real-Time Process Monitoring** - Live updates of CPU, memory, disk I/O, and thread counts
- **Advanced Process Information** - View detailed module lists, thread states, and command-line arguments
- **Multi-Process Selection** - Select and terminate multiple processes simultaneously
- **Smart Filtering** - Filter by process name, username, or PID
- **Flexible Sorting** - Sort by any column (CPU, memory, disk, threads, priority)
- **Rich System Metrics** - Visual meters for CPU cores, memory usage, and system statistics
- **Keyboard-Driven Interface** - Full F1-F10 hotkey support for efficient navigation

### 🎨 Customization

- **5 Built-in Themes** - Colour, Mono, MS-DOS, Tokyo Night, and Matrix
- **Configurable UI** - 26 customizable color keys per theme
- **Adjustable Performance** - Configurable update intervals and process display limits
- **IRIX Mode** - Toggle between per-core and total CPU reporting

### 🔍 What Makes taskmgr Unique

**Deep Process Insights**
- **Thread-Level Information**: View individual thread states, CPU times, and priorities to verify process termination beyond just PID checks
- **Windows Service Details**: Shows actual service names and startup parameters, not just generic `svchost.exe` entries
- **Module Analysis**: Full DLL/library enumeration for each process
- **Command-Line Visibility**: See exact startup parameters for every running process

**Cross-Platform Native Performance**
- Platform-specific optimizations using native APIs (Win32, Mach kernel)
- No generic cross-platform wrappers - direct system calls for maximum performance
- Optimized for both Intel and Apple Silicon architectures

## Screenshots

<p align="center">
  <img src="docs/images/mainscreen.png" alt="Main process list with system statistics" width="800"/>
  <br/>
  <em>Main process list with real-time CPU, memory, and disk I/O metrics</em>
</p>

<p align="center">
  <img src="docs/images/threads.png" alt="Thread view showing detailed thread information" width="800"/>
  <br/>
  <em>Detailed thread information including states, priorities, and CPU times</em>
</p>

## Quick Start

### Installation

#### From Release Binary

Download the latest release for your platform:
- **macOS ARM64**: `taskmgr-osx-arm64`
- **Windows x64**: `taskmgr-win-x64.exe`

```bash
# macOS
chmod +x taskmgr-osx-arm64
./taskmgr-osx-arm64
```

```powershell
# Windows
.\taskmgr-win-x64.exe
```

#### Build from Source

**Prerequisites:**
- .NET 9.0 SDK or later
- macOS 11+ or Windows 10+

**macOS:**
```bash
git clone https://github.com/yourusername/taskmgr-cli.git
cd taskmgr-cli
./build.sh
```

**Windows:**
```powershell
git clone https://github.com/yourusername/taskmgr-cli.git
cd taskmgr-cli
.\build.ps1
```

The compiled binary will be in `src/taskmgr/bin/Debug/net9.0/`.

### Usage

**Basic Usage:**
```bash
taskmgr
```

**Command-Line Options:**
```bash
taskmgr --filter-pid 1234              # Monitor specific process
taskmgr --filter-user username         # Filter by user
taskmgr --filter-process chrome        # Filter by process name
taskmgr --sort-col cpu --sort-desc     # Sort by CPU usage
taskmgr --delay 2000                   # Update every 2 seconds
taskmgr --nprocs 25                    # Show top 25 processes
taskmgr --iterations 10                # Run for 10 iterations then exit
```

**Keyboard Shortcuts:**
- `F1` - Help screen
- `F2` - Setup/configuration
- `F3` - Filter processes
- `F4` - Sort options
- `F5` - Process details (modules, threads, handles)
- `F6` - Multi-select mode
- `F8` - Kill selected process(es)
- `F9` - Process column display
- `F10` - Exit
- `↑/↓` - Navigate process list
- `Space` - Select/deselect process (multi-select mode)

## Configuration

Configuration is stored in `taskmgr.ini` in the application directory.

### Example Configuration

```ini
[filter]
pid=-1
username=
process=

[stats]
cols=511           # Bitmask for visible columns
delay=1500         # Update delay in milliseconds
nprocs=-1          # Number of processes to display (-1 = all)

[sort]
col=Cpu           # Sort by CPU usage
asc=false         # Descending order

[ux]
confirm-task-delete=true
default-theme=theme-colour
highlight-daemons=true
use-irix-cpu-reporting=true   # macOS default: true, Windows: false
```

### Available Themes

- `theme-colour` - Colorful default theme
- `theme-mono` - Monochrome theme
- `theme-msdos` - Retro MS-DOS style
- `theme-tokyo-night` - Tokyo Night color palette
- `theme-matrix` - Matrix green theme

Each theme supports 26 customizable color keys for comprehensive UI styling.

## Architecture

### Project Structure

```
taskmgr-cli/
├── src/
│   ├── taskmgr/                          # Main application
│   │   ├── Process/                      # Process monitoring engine
│   │   │   ├── Processor.cs              # Core processing loop
│   │   │   └── ProcessorInfo.cs          # Process metadata
│   │   ├── Gui/                          # Terminal UI components
│   │   │   ├── Controls/                 # UI controls (list, meters, etc.)
│   │   │   └── MainScreen.cs             # Main application screen
│   │   ├── Commands/                     # User command implementations
│   │   └── Configuration/                # Config management
│   ├── Task.Manager.System/              # Cross-platform abstraction layer
│   │   ├── Process/                      # Process, module, thread services
│   │   ├── SystemInfo.cs                 # System metrics (CPU, memory)
│   │   ├── SystemInfo.Windows.cs         # Windows-specific implementation
│   │   └── SystemInfo.MacOS.cs           # macOS-specific implementation
│   ├── Task.Manager.Interop.Win32/       # Windows P/Invoke bindings
│   ├── Task.Manager.Interop.Mach/        # macOS Mach kernel bindings
│   └── Task.Manager.Cli.Utils/           # Console utilities
└── tests/                                # Comprehensive test suite
```

### Technical Design

#### Multi-Layered Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     taskmgr (Main App)                      │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │   Processor  │  │  GUI Controls│  │   Commands   │     │
│  └──────────────┘  └──────────────┘  └──────────────┘     │
└────────────────────────────┬────────────────────────────────┘
                             │
┌────────────────────────────┴────────────────────────────────┐
│              Task.Manager.System (Abstraction)              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │ ProcessService│ │  SystemInfo  │  │ ModuleService│     │
│  └──────────────┘  └──────────────┘  └──────────────┘     │
└────────────┬───────────────────────────────┬────────────────┘
             │                               │
    ┌────────┴─────────┐         ┌──────────┴───────────┐
    │ Interop.Win32    │         │   Interop.Mach       │
    │ (Windows APIs)   │         │   (macOS Kernel)     │
    └──────────────────┘         └──────────────────────┘
```

#### Key Components

**Processor Loop** (`Process/Processor.cs`)
- Dual-threaded design: data collection + UI monitoring
- Configurable iteration limits and delays
- Platform-specific CPU calculation (IRIX vs non-IRIX mode)
- Thread-safe data sharing via locks
- **Important**: Uses `[MethodImpl(MethodImplOptions.NoOptimization)]` on macOS ARM64 Release builds to prevent JIT optimization bugs

**System Abstraction** (`Task.Manager.System`)
- Platform detection via MSBuild constants (`__WIN32__`, `__APPLE__`)
- Unified interface for process enumeration, system metrics, and module inspection
- Native implementations for maximum performance:
  - **Windows**: Win32 API via P/Invoke (PSAPI, PDH, WMI)
  - **macOS**: Mach kernel APIs, libproc, sysctl

**GUI Components** (`Gui/Controls`)
- Terminal-based UI using ANSI escape sequences
- Custom controls: ListView, Metre (progress bar), HeaderControl
- Event-driven command pattern for user interactions
- Responsive design adapting to terminal dimensions

**Configuration** (`Configuration/AppConfig.cs`)
- INI-based configuration with auto-save
- Theme system with 26 color keys per theme
- Runtime configuration changes via Setup screen (F2)

### CPU Reporting Modes

**IRIX Mode** (macOS default)
- 100% = full utilization of ONE CPU core
- Consistent with macOS Activity Monitor
- Can exceed 100% on multi-core systems (e.g., 400% on 4-core system)

**Non-IRIX Mode** (Windows default)
- 100% = full utilization of ALL CPU cores
- Consistent with Windows Task Manager
- Always ranges from 0-100%

Toggle via `use-irix-cpu-reporting` configuration key.

## Building from Source

### Prerequisites

- **.NET 9.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Platform Requirements**:
  - macOS 11+ (Big Sur or later)
  - Windows 10+ (x64)

### Build Commands

**Quick Build (Debug):**
```bash
# macOS/Linux
./build.sh

# Windows
.\build.ps1
```

**Custom Build:**
```bash
./eng/build.sh --clean --restore --build --config Release
```

**Run Tests:**
```bash
./test.sh           # macOS/Linux
.\test.ps1          # Windows
```

**Publish Release:**
```bash
./publish.sh        # macOS/Linux
.\publish.ps1       # Windows
```

### Build Options

The build script (`eng/build.sh` / `eng\build.ps1`) supports:

| Option | Description |
|--------|-------------|
| `--clean` | Clean the solution |
| `--restore` | Restore NuGet packages |
| `--build` | Build the solution |
| `--test` | Run unit tests |
| `--publish` | Publish self-contained binary |
| `--deploy` | Publish Release with AOT compilation |
| `-c, --config <config>` | Build configuration (Debug/Release) |

### Publishing Platform-Specific Binaries

**macOS ARM64:**
```bash
dotnet publish src/taskmgr/taskmgr.csproj \
  -c Release \
  -r osx-arm64 \
  --self-contained \
  -p:PublishAot=true
```

**Windows x64:**
```powershell
dotnet publish src\taskmgr\taskmgr.csproj `
  -c Release `
  -r win-x64 `
  --self-contained `
  -p:PublishAot=true
```

Published binaries are located in `src/taskmgr/bin/Release/net9.0/{RID}/publish/`.

### Code Coverage

Generate test coverage reports:

```bash
# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

# Generate HTML report
reportgenerator \
  -reports:"./TestResults/**/coverage.cobertura.xml" \
  -targetdir:"coverage-report" \
  -reporttypes:Html

# View report
open coverage-report/index.html       # macOS
start coverage-report/index.html      # Windows
```

## Platform-Specific Notes

### macOS

- **Apple Silicon Optimization**: Release builds require `[MethodImpl(MethodImplOptions.NoOptimization)]` on hot-path methods to prevent JIT optimizer from eliminating cancellation checks
- **IRIX Mode Default**: CPU percentages can exceed 100% (per-core reporting)
- **Permissions**: May require elevated privileges for full process access

### Windows

- **Service Details**: Shows full service name and startup parameters for `svchost.exe` processes
- **Non-IRIX Mode Default**: CPU percentages range from 0-100% (total system)
- **Administrator Mode**: Run as Administrator for access to all processes

## Contributing

Contributions are welcome! Please follow these guidelines:

1. **Fork the repository** and create a feature branch
2. **Write tests** for new functionality
3. **Follow C# coding conventions** - see existing codebase for style
4. **Update documentation** for user-facing changes
5. **Submit a pull request** with a clear description

### Development Setup

```bash
git clone https://github.com/yourusername/taskmgr-cli.git
cd taskmgr-cli
./build.sh --clean --restore --build --test
```

### Running Tests

```bash
dotnet test                                    # Run all tests
dotnet test --filter "ProcessorTests"          # Run specific test class
dotnet test --logger "console;verbosity=detailed"  # Verbose output
```

## Troubleshooting

### High CPU Usage in Release Build (macOS)

**Symptom**: Release build immediately pegs CPU at 600%+
**Cause**: Aggressive JIT optimization eliminates cancellation token checks
**Fix**: Already implemented via `[MethodImpl(MethodImplOptions.NoOptimization)]` on `RunInternal` and `RunMonitorInternal` methods

### Permission Denied Errors

**macOS/Linux**: Run with `sudo` for full system access:
```bash
sudo ./taskmgr
```

**Windows**: Run as Administrator (right-click → "Run as administrator")

### Terminal Display Issues

Ensure your terminal supports:
- ANSI escape sequences
- Minimum dimensions: 80×24 characters
- UTF-8 encoding

Recommended terminals:
- **macOS**: Terminal.app, iTerm2
- **Windows**: Windows Terminal, ConEmu

## License

[Specify your license here - e.g., MIT, Apache 2.0]

## Acknowledgments

Built with:
- [.NET 9.0](https://dotnet.microsoft.com/) - Cross-platform runtime
- [System.CommandLine](https://github.com/dotnet/command-line-api) - Command-line parsing
- Native platform APIs (Win32, Mach kernel)

## Support

- **Issues**: [GitHub Issues](https://github.com/yourusername/taskmgr-cli/issues)
- **Discussions**: [GitHub Discussions](https://github.com/yourusername/taskmgr-cli/discussions)

---

<p align="center">
  Made with ❤️ by the taskmgr team
</p>
