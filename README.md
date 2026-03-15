# taskmgr-cli
A powerful, cross-platform command-line task manager with real-time process monitoring, advanced filtering, and rich system information display. Built with .NET Native for Windows and macOS.

![TaskManager](./docs/images/taskmgr.gif)

## Features

### Core Capabilities

- **Real-Time Process Monitoring** - Live updates of CPU & GPU, memory, disk I/O, Network I/O and thread counts
- **Advanced Process Information** - View detailed module lists, thread states, and command-line arguments
- **Multi-Process Selection** - Select and terminate multiple processes simultaneously
- **Smart Filtering** - Filter by process name, username, or PID
- **Flexible Sorting** - Sort by any column (Process, PID, User, Priority, CPU%, Threads, GPU%, Memory, Disk, Path)
- **Rich System Metrics** - Visual meters for CPU, GPU, memory usage, Virtual/Swap, Disk and Network
- **Keyboard-Driven Interface** - Full F1-F10 hotkey support for navigation

### Customization

- **5 Built-in Themes** - Colour, Mono, MS-DOS, Tokyo Night, and Matrix
- **Configurable UI** - 26 customizable color keys per theme
- **Adjustable Performance** - Configurable update intervals and process display limits
- **IRIX Mode** - Toggle between per-core and total CPU reporting

### What Makes taskmgr Unique

**Deep Process Insights**
- **Thread-Level Information**: View individual thread states, CPU times, and priorities to verify process state
- **Windows Service Details**: Shows actual service names and startup parameters, not just generic `svchost.exe` entries
- **Module Analysis**: Full DLL/library enumeration for each process (not on all platforms yet)

**Cross-Platform Native Performance**
- Platform-specific optimizations using native APIs (Win32, Mach kernel)
- No generic cross-platform wrappers - direct system calls for maximum performance

## Installation

### macOS (Homebrew)

The easiest way to install on macOS:

```bash
brew tap jchas2/taskmgr
brew install taskmgr
```

To update to the latest version:

```bash
brew update
brew upgrade taskmgr
```

### Direct Download

Download the latest release for your platform:

**macOS:**
- [macOS ARM64 (Apple Silicon)](https://github.com/jchas2/taskmgr-cli/releases/latest) - for M1/M2/M3 Macs
- [macOS x64 (Intel)](https://github.com/jchas2/taskmgr-cli/releases/latest) - for Intel Macs

**Windows:**
- [Windows x64](https://github.com/jchas2/taskmgr-cli/releases/latest)

After downloading:
1. Extract the archive
2. Run with elevated privileges:
   - **macOS**: `sudo ./taskmgr`
   - **Windows**: Run PowerShell or Command Prompt as Administrator, then run `taskmgr.exe`

### Build from Source

**Prerequisites:**
- .NET 10.0 SDK or later
- macOS 11+ or Windows 10+

**Clone and Build:**

macOS:
```bash
git clone https://github.com/jchas2/taskmgr-cli.git
cd taskmgr-cli
./eng/build.sh --restore --build --config Release
```

Windows:
```powershell
git clone https://github.com/jchas2/taskmgr-cli.git
cd taskmgr-cli
.\eng\build.ps1 -restore -build -config Release
```

**Run Tests:**
```bash
./eng/build.sh --test           # macOS
.\eng\build.ps1 -test           # Windows
```

**Publish Native Executable:**

macOS (ARM64):
```bash
./eng/build.sh --publish --config Release --runtime osx-arm64
```

macOS (Intel):
```bash
./eng/build.sh --publish --config Release --runtime osx-x64
```

Windows:
```powershell
.\eng\build.ps1 -publish -config Release -runtime win-x64
```

Published binaries are located in `src/taskmgr/bin/Release/net10.0/{runtime}/publish/`.

## Configuration

Configuration is stored in `taskmgr.ini` in the application directory.
