# Speed Cube Timer - TDD Approach

A Windows desktop speedcubing timer application built with **Test-Driven Development** principles.

## Project Structure

```
SpeedCubeTimer/
├── SpeedCubeTimer.csproj                    # Main application project
├── Services/
│   ├── ITimerService.cs                     # Timer interface
│   ├── TimerService.cs                      # Timer implementation
│   ├── IStatisticsService.cs                # Statistics interface
│   └── StatisticsService.cs                 # Statistics implementation
├── App.xaml                                 # Application resources
├── App.xaml.cs                              # Application code-behind
├── MainWindow.xaml                          # UI layout
├── MainWindow.xaml.cs                       # UI logic
├── SpeedCubeTimer.Tests/
│   ├── SpeedCubeTimer.Tests.csproj          # Test project
│   ├── TimerServiceTests.cs                 # Timer unit tests
│   └── StatisticsServiceTests.cs            # Statistics unit tests
└── README.md                                # This file
```

## TDD Architecture

### Core Services (Testable)

**ITimerService & TimerService**
- Manages timer state (running, stopped)
- Records solve times
- Provides elapsed time calculations
- No UI dependencies - fully testable

**IStatisticsService & StatisticsService**
- Calculates best, worst, average times
- Formats times for display
- Pure functions - easily unit tested

### Unit Tests

Using **xUnit** testing framework with 15+ test cases covering:
- Timer initialization and state management
- Start/stop/reset operations
- Time recording and history
- Statistics calculations (min, max, average)
- Time formatting
- Edge cases (empty lists, null values)

## Testing

### Run All Tests
```bash
dotnet test
```

### Run Tests with Verbose Output
```bash
dotnet test --verbosity detailed
```

### Run Specific Test Class
```bash
dotnet test --filter "ClassName=SpeedCubeTimer.Tests.TimerServiceTests"
```

## Building

```bash
dotnet build
```

## Running

```bash
dotnet run --project SpeedCubeTimer/SpeedCubeTimer.csproj
```

## Features

- **Accurate Timer**: High-precision millisecond timing
- **Keyboard Controls**:
  - **SPACE**: Start/Stop timer
  - **R**: Reset timer
- **Time Recording**: Automatic solve time capture
- **Statistics**: Best, average, worst time tracking
- **Session Management**: Clear history option
- **Clean UI**: Modern, intuitive interface

## Test Coverage

### TimerServiceTests (9 tests)
- Initial state verification
- Start/stop operations
- Time recording
- Reset functionality
- Multiple solve tracking
- History clearing

### StatisticsServiceTests (8 tests)
- Best time calculation
- Worst time calculation
- Average time calculation
- Time formatting
- Edge case handling
- Complex dataset validation

## Requirements

- Windows 10 or later
- .NET 8.0 SDK
- .NET 8.0 Runtime

## Development Workflow (TDD)

1. **Red Phase**: Write failing tests
2. **Green Phase**: Implement code to pass tests
3. **Refactor Phase**: Improve code quality
4. **Repeat**: Add new features with tests first

## Future Enhancements

- Session persistence (save/load)
- Official competition format (inspection time)
- Scramble display integration
- Multiple solving methods (2x2, 3x3, etc.)
- CSV export functionality
- Training mode with targets
- Statistical analysis (Ao5, Ao12)
