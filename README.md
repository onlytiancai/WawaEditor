# WawaEditor: High-Performance Text Editor for Large Files

WawaEditor is a Windows Forms-based text editor designed to efficiently handle both small and large text files. It provides optimized performance through virtual text rendering and intelligent memory management, making it particularly suitable for working with files exceeding 5MB in size.

The editor features a modern tabbed interface, advanced text manipulation capabilities, and smart memory management. It automatically switches between standard and large file handling modes based on file size, ensuring optimal performance regardless of the content being edited. Key features include configurable word wrapping, font customization, find/replace functionality, and a robust undo/redo system.

## Repository Structure
```
├── AppConfig.cs               # Configuration management and persistence
├── docs/                      # Documentation directory
│   └── high-perf-editor.md   # Performance optimization documentation
├── FindReplaceDialog.cs      # Search and replace functionality implementation
├── LargeFileHandler.cs       # Optimized large file processing system
├── LargeTextEditorTabPage.cs # Specialized tab page for large files
├── Logger.cs                 # Application logging functionality
├── MainForm.cs               # Main application window and core logic
├── MainForm.Designer.cs      # UI layout and component definitions
├── Program.cs                # Application entry point and initialization
├── TextEditorTabPage.cs      # Standard text editor tab implementation
└── VirtualTextBox.cs         # Virtual scrolling text display component
```

## Usage Instructions
### Prerequisites
- Windows operating system
- .NET 6.0 Runtime or later
- Minimum 4GB RAM recommended for large file handling
- Visual Studio 2019 or later (for development)

### Installation
1. **For Users:**
```bash
# Download the latest release
# Extract the zip file to your preferred location
# Run WawaEditor.exe
```

2. **For Developers:**
```bash
# Clone the repository
git clone https://github.com/yourusername/WawaEditor.git

# Open in Visual Studio
cd WawaEditor
start WawaEditor.sln

# Build the solution
dotnet build
```

### Quick Start
1. Launch WawaEditor
2. Open a file:
   ```
   File -> Open or Ctrl+O
   ```
3. Basic operations:
   - Create new file: Ctrl+N
   - Save: Ctrl+S
   - Find: Ctrl+F
   - Replace: Ctrl+H

### More Detailed Examples
1. **Working with Large Files:**
```csharp
// Files > 5MB automatically use virtual mode
File -> Open -> Select large file
// Editor automatically switches to optimized mode
```

2. **Customizing the Editor:**
```
Format -> Font          // Change font family and size
Format -> Word Wrap     // Toggle word wrapping
```

### Troubleshooting
1. **Performance Issues with Large Files**
   - Problem: Slow loading of large files
   - Solution: 
     ```
     # Check if virtual mode is active
     - Look for "Large File Mode" in status bar
     - Ensure sufficient free memory
     ```

2. **Configuration Issues**
   - Problem: Settings not persisting
   - Solution:
     ```
     # Verify config file
     Check wawaeditor_config.json in application directory
     # Reset configuration
     Delete wawaeditor_config.json and restart application
     ```

## Data Flow
WawaEditor processes text files through a layered architecture that optimizes memory usage and performance. The system determines the appropriate editor mode based on file size and manages content through virtual or direct loading.

```ascii
[File System] -> [File Handler] -> [Editor Component] -> [UI Layer]
     |              |                    |                  |
     |         Size Check          Memory Management    User Input
     |              |                    |                  |
     └──────────────┴────────────────────┴──────────────────┘
```

Key component interactions:
1. File Handler determines appropriate loading strategy based on file size
2. Large files (>5MB) use virtual loading with line indexing
3. Standard files use direct loading with full text buffering
4. UI components adapt to the selected loading strategy
5. Changes are buffered and persisted efficiently
6. Configuration settings are maintained across sessions
7. Status updates are propagated through event system