# Launcher And Autostart Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a double-click launcher and an in-app Windows startup toggle for LightStickyNote.

**Architecture:** Keep the current WPF app intact, but add a lightweight launcher path at the project root plus a startup registration service that writes a small startup script into the current user's Startup folder. Expose the startup flag through `AppSettings` and `MainViewModel`, and surface it in the main window without introducing a separate settings screen.

**Tech Stack:** C# / .NET 8, WPF, xUnit, PowerShell / CMD launch scripts

---

### Task 1: Lock Startup Registration Behavior With Tests

**Files:**
- Create: `D:\CodexProjects\LightStickyNote\tests\LightStickyNote.App.Tests\StartupRegistrationServiceTests.cs`
- Modify: `D:\CodexProjects\LightStickyNote\tests\LightStickyNote.App.Tests\LightStickyNote.App.Tests.csproj`

- [ ] **Step 1: Write the failing test**
- [ ] **Step 2: Run test to verify it fails**
- [ ] **Step 3: Write minimal implementation**
- [ ] **Step 4: Run test to verify it passes**

### Task 2: Add Launcher Path Discovery And Script Generation

**Files:**
- Modify: `D:\CodexProjects\LightStickyNote\src\LightStickyNote.App\Infrastructure\AppPaths.cs`
- Create: `D:\CodexProjects\LightStickyNote\src\LightStickyNote.App\Services\LauncherScriptService.cs`
- Modify: `D:\CodexProjects\LightStickyNote\src\LightStickyNote.App\App.xaml.cs`

- [ ] **Step 1: Write the failing test**
- [ ] **Step 2: Run test to verify it fails**
- [ ] **Step 3: Write minimal implementation**
- [ ] **Step 4: Run test to verify it passes**

### Task 3: Wire Autostart Toggle Into Settings And UI

**Files:**
- Modify: `D:\CodexProjects\LightStickyNote\src\LightStickyNote.App\Models\AppSettings.cs`
- Modify: `D:\CodexProjects\LightStickyNote\src\LightStickyNote.App\ViewModels\MainViewModel.cs`
- Modify: `D:\CodexProjects\LightStickyNote\src\LightStickyNote.App\MainWindow.xaml`

- [ ] **Step 1: Write the failing test**
- [ ] **Step 2: Run test to verify it fails**
- [ ] **Step 3: Write minimal implementation**
- [ ] **Step 4: Run test to verify it passes**

### Task 4: Improve Manual Launch Path And Docs

**Files:**
- Create: `D:\CodexProjects\LightStickyNote\启动便签.cmd`
- Modify: `D:\CodexProjects\LightStickyNote\tools\Run.ps1`
- Modify: `D:\CodexProjects\LightStickyNote\README.md`
- Modify: `D:\CodexProjects\LightStickyNote\docs\architecture.md`

- [ ] **Step 1: Add launcher script**
- [ ] **Step 2: Update run script**
- [ ] **Step 3: Update docs**
- [ ] **Step 4: Verify manual double-click path**
