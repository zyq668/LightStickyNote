# LightStickyNote

LightStickyNote 是一个面向 Windows 桌面的轻量便签工具。

它不追求复杂的笔记管理，而是专注于日常最常用的动作：快速记录、随手编辑、勾选完成、自动保存。窗口可以常驻桌面右侧并保持置顶，适合用来放置今日任务、临时提醒和简单待办事项。

## 软件特点

- **非常轻量**：使用 C#、WPF 和 SQLite 构建，不依赖 Electron，也不需要启动额外的数据库服务。
- **快速记录**：在底部输入任务后按 `Enter` 即可新增待办。
- **便捷编辑**：任务文本可以直接在便签中修改，不需要打开额外弹窗。
- **完成删除线**：勾选任务后会自动显示删除线，也可以随时取消完成状态。
- **一键删除**：不再需要的任务可以直接移除。
- **自动保存**：任务内容、完成状态、窗口位置和窗口大小会自动保存。
- **重启恢复**：关闭并重新启动后，之前记录的内容仍然存在。
- **桌面置顶**：便签默认显示在屏幕右侧，并支持始终置顶。
- **托盘驻留**：支持从系统托盘显示、隐藏和退出应用。
- **极简界面**：无边框石墨玻璃风格，主界面只保留任务和快速输入。
- **集中设置**：点击右上角齿轮后，可以开启或关闭置顶、开机自启动，并调节透明度。
- **玻璃透明度**：可以在 `45%` 到 `100%` 之间调节石墨面板透明度，桌面内容会真实透出，文字和按钮保持清晰。
- **本地存储**：数据保存在本机 SQLite 文件中，不依赖云服务。

## 适合什么场景

- 放在桌面右侧记录今天要完成的事项
- 临时保存容易忘记的小任务
- 在工作过程中快速勾选已经完成的内容
- 希望使用简单便签，不想安装复杂任务管理软件

## 界面操作

1. 在窗口底部输入任务，按 `Enter` 新增。
2. 直接点击任务文字进行编辑。
3. 勾选任务左侧复选框，将任务标记为已完成。
4. 已完成任务会显示删除线，再次取消勾选即可恢复。
5. 点击任务右侧的 `×` 删除任务。
6. 点击右上角齿轮，打开悬浮设置卡片。
7. 在设置卡片中控制始终置顶、开机自启动和窗口透明度。

## 技术栈

- C# / .NET 8
- WPF
- SQLite (`Microsoft.Data.Sqlite`)
- JSON 配置
- 接近 MVVM 的分层结构

## 为什么选择 WPF + SQLite

WPF 是 Windows 原生桌面方案，启动成本和资源占用明显低于 Electron，适合做长期常驻的轻量工具。

SQLite 是本地单文件数据库，适合离线自动保存，也方便后续扩展多便签、历史记录和标签功能，不需要引入服务端数据库。

## 项目结构

```text
LightStickyNote/
├─ src/
├─ tests/
├─ docs/
├─ tools/
├─ Launch-LightStickyNote.cmd
├─ 启动便签.cmd
└─ README.md
```

## 构建环境

构建项目需要安装 [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)。

## 如何构建

```powershell
dotnet restore .\LightStickyNote.sln
dotnet build .\LightStickyNote.sln -c Debug
```

## 如何运行

开发环境中可以直接运行：

```powershell
dotnet run --project .\src\LightStickyNote.App\LightStickyNote.App.csproj -c Debug
```

## 生成便携分发包

运行发布脚本：

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Publish-Portable.ps1
```

脚本会在 `artifacts` 目录生成 Windows x64 自包含单文件版本和对应 ZIP。接收方无需安装 .NET，解压后直接双击 `LightStickyNote.App.exe` 即可运行。

便携版本会在 EXE 同级目录创建 `user-data` 文件夹保存 SQLite 数据库和 JSON 配置。移动 EXE 时，请一起保留该文件夹。

## 开机自启动

点击应用右上角齿轮后，悬浮设置卡片中提供了 `开机自启动` 开关。

- 勾选后，程序会在当前用户的 Windows Startup 目录中写入一个轻量启动脚本。
- 取消勾选后，对应启动脚本会自动移除。

## 数据文件

数据库文件：

`src\LightStickyNote.App\bin\Debug\net8.0-windows\user-data\lightstickynote.db`

配置文件：

`src\LightStickyNote.App\bin\Debug\net8.0-windows\user-data\appsettings.json`

## 清空测试数据

```powershell
Remove-Item -Recurse -Force .\src\LightStickyNote.App\bin\Debug\net8.0-windows\user-data
```

## 后续扩展方向

- 多便签
- 历史归档和操作记录
- 标签和筛选
- 快捷键新增任务
- 带卸载入口的正式安装包
- 通过 `SummaryService` 接入 AI 总结能力
