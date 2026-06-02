# LightStickyNote

<img src="assets/branding/app-icon-transparent.png" alt="LightStickyNote 透明图标" width="120" />

LightStickyNote 是一个面向 Windows 桌面的轻量便签工具。

它不追求复杂的项目管理，而是专注于最常用的桌面待办动作：快速记录、随手编辑、勾选完成、自动保存、贴边隐藏和托盘常驻。当前版本已经补上了任务提醒能力，适合用来放置今日任务、临时提醒和简单待办事项。

## 当前功能特点

- **非常轻量**：使用 C#、WPF 和 SQLite 构建，不依赖 Electron，也不需要额外数据库服务。
- **快速记录**：底部输入框输入任务后按 `Enter` 即可新增待办。
- **便捷编辑**：任务文本可以直接在列表中修改，不需要进入单独编辑页。
- **完成反馈**：勾选任务后会显示完成状态和删除线，也可以再次取消完成。
- **安全删除**：删除任务前会在便签内部弹出确认卡片，减少误触。
- **提醒功能**：每条任务都可以通过右侧三点菜单设置提醒。
- **提醒编辑面板**：支持 `指定时间` 和 `倒计时` 两种方式，任务可直接手输日期、小时、分钟，并支持 1 分钟粒度。
- **专注快捷入口**：在倒计时提醒里内置 `番茄专注` 和 `深度专注` 按钮，可一键切换为 25 分钟或 50 分钟倒计时。
- **提醒状态展示**：设置提醒后，任务右侧会显示克制的时间标记；逾期任务会切换为单独的提醒色。
- **到点通知**：到达提醒时间后，会通过托盘通知提醒当前任务。
- **自动取消提醒**：任务完成后会自动清除提醒，避免完成项继续触发通知。
- **进度感知**：顶部展示剩余任务数量和完成进度条。
- **边缘吸附隐藏**：窗口贴到屏幕右侧后可以自动收起，鼠标靠近时再展开。
- **托盘常驻**：支持隐藏到系统托盘，并从托盘恢复或退出应用。
- **自动保存**：任务内容、完成状态、提醒信息、窗口位置和窗口大小会自动保存。
- **本地存储**：数据保存在本机 SQLite 文件中，不依赖云服务。

## 适合的使用场景

- 放在桌面右侧记录今天要完成的事项
- 记录容易忘记的小任务，并在关键时间点提醒自己
- 工作过程中边做边勾选，保持轻量节奏
- 想用简单便签，不想装完整任务管理软件

## 界面操作

1. 在窗口底部输入任务，按 `Enter` 新增。
2. 直接点击任务文字进行编辑。
3. 勾选任务左侧复选框，将任务标记为已完成。
4. 点击任务右侧三点菜单，可选择 `设置提醒 / 编辑提醒` 或 `删除任务`。
5. 在提醒面板中选择 `指定时间` 或 `倒计时`；如果想快速进入专注节奏，也可以直接点击 `番茄专注`（25 分钟）或 `深度专注`（50 分钟），保存后任务右侧会显示提醒时间。
6. 点击右上角齿轮，打开悬浮设置卡片。
7. 在设置卡片中控制始终置顶、开机自启动、边缘吸附隐藏和透明度。

## 技术栈

- C# / .NET 8
- WPF
- SQLite (`Microsoft.Data.Sqlite`)
- JSON 配置
- 接近 MVVM 的分层结构

## 项目结构

```text
LightStickyNote/
├─ assets/
│  └─ branding/
├─ docs/
├─ src/
│  └─ LightStickyNote.App/
├─ tests/
├─ tools/
├─ Launch-LightStickyNote.cmd
└─ README.md
```

## 构建环境

需要安装 [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)。

如果本机系统 `dotnet` 没有 SDK，也可以使用仓库当前采用的开发环境路径：

- `D:\DevTools\dotnet\dotnet.exe`

## 如何构建

```powershell
dotnet restore .\LightStickyNote.sln
dotnet build .\LightStickyNote.sln -c Debug
```

如果要显式使用本机开发环境中的 SDK：

```powershell
& 'D:\DevTools\dotnet\dotnet.exe' build .\LightStickyNote.sln -c Debug
```

## 如何运行

开发环境中可以直接运行：

```powershell
dotnet run --project .\src\LightStickyNote.App\LightStickyNote.App.csproj -c Debug
```

也可以直接双击：

- `Launch-LightStickyNote.cmd`

或者执行：

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Run.ps1
```

## 生成便携分发包

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Publish-Portable.ps1
```

默认输出：

- `artifacts\LightStickyNote-win-x64-portable\LightStickyNote.App.exe`
- `artifacts\LightStickyNote-win-x64-portable.zip`

## 数据文件

开发环境默认数据目录：

- `src\LightStickyNote.App\bin\Debug\net8.0-windows\user-data\lightstickynote.db`
- `src\LightStickyNote.App\bin\Debug\net8.0-windows\user-data\appsettings.json`

## 测试

```powershell
dotnet test .\tests\LightStickyNote.App.Tests\LightStickyNote.App.Tests.csproj -c Debug
```

## 后续扩展方向

- 多便签 / 多列表
- 标签和筛选
- 历史归档与恢复
- 更完整的提醒策略，例如提前提醒和重复提醒

## 应用图片示例

![界面示例 1](https://raw.gitcode.com/user-images/assets/9992847/43c9e04d-c347-409a-827f-97388c0f97ba/3c6d6a8713c42110540a7cbeb1906c60.png)
![界面示例 2](https://raw.gitcode.com/user-images/assets/9992847/cde3c95d-5a1f-47cd-b274-f0525fe3c418/4dbdbf3a1dc6c85d0f17058ba2311837.png)
![image.png](https://raw.gitcode.com/user-images/assets/9992847/aa1f5fca-14d0-4314-8332-0c7b4536aa4e/image.png 'image.png')
![界面示例 3](https://raw.gitcode.com/user-images/assets/9992847/904b745b-9127-440f-8594-4d2526721e11/image.png)
![界面示例 4](https://raw.gitcode.com/user-images/assets/9992847/757b5cc1-d9a7-470e-b8ef-4c37973e8028/d234166391193ddfd73f634468b8909d.png)
