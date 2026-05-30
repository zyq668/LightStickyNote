# LightStickyNote

一个面向 Windows 桌面的轻量便签 MVP。它默认以右侧常驻便签窗口启动，支持任务快速新增、编辑、完成、取消完成、删除、自动保存、托盘驻留，以及现在可直接双击启动和设置开机自启动。

## 技术栈

- C# / .NET 8
- WPF
- SQLite (`Microsoft.Data.Sqlite`)
- JSON 配置
- 接近 MVVM 的分层结构

## 为什么选 WPF + SQLite

WPF 是 Windows 原生桌面方案，内存占用和启动成本明显低于 Electron，适合做长期常驻的轻量工具。SQLite 是本地单文件数据库，适合离线自动保存，也方便后续扩展多便签、历史和标签，而不需要引入服务端数据库。

## 项目结构

```text
D:\CodexProjects\LightStickyNote
├─ src
├─ tests
├─ docs
├─ tools
├─ data-sample
├─ Launch-LightStickyNote.cmd
├─ 启动便签.cmd
└─ README.md
```

## 构建前准备

本项目把 .NET SDK 和 NuGet 缓存放在 D 盘：

- `D:\DevTools\dotnet`
- `D:\DevTools\nuget-packages`
- `D:\DevTools\dotnet-home`

如果直接用脚本，环境变量会自动设置好。

## 如何构建

```powershell
cd D:\CodexProjects\LightStickyNote
powershell -ExecutionPolicy Bypass -File .\tools\Build.ps1
```

## 如何运行

推荐的两种方式：

### 方式 1：直接双击

双击项目根目录下的：

`D:\CodexProjects\LightStickyNote\启动便签.cmd`

中文脚本会调用纯 ASCII 文件名的 `Launch-LightStickyNote.cmd`，这样开机自启动不会受 Windows 命令行代码页影响。

### 方式 2：命令行启动

```powershell
cd D:\CodexProjects\LightStickyNote
powershell -ExecutionPolicy Bypass -File .\tools\Run.ps1
```

## 开机自启动

应用底部提供了 `开机自启动` 复选框。

- 勾选后：程序会在当前用户的 Windows Startup 目录下写入一个很小的启动脚本
- 取消勾选后：对应启动脚本会被移除

Startup 目录中的脚本会间接调用项目根目录的 `Launch-LightStickyNote.cmd`，因此只要项目仍在 `D:\CodexProjects\LightStickyNote`，开机自启动就能正常工作。

## 数据库文件在哪里

运行后会生成在：

`D:\CodexProjects\LightStickyNote\src\LightStickyNote.App\bin\Debug\net8.0-windows\user-data\lightstickynote.db`

## 配置文件在哪里

运行后会生成在：

`D:\CodexProjects\LightStickyNote\src\LightStickyNote.App\bin\Debug\net8.0-windows\user-data\appsettings.json`

## 如何清空测试数据

```powershell
cd D:\CodexProjects\LightStickyNote
powershell -ExecutionPolicy Bypass -File .\tools\Clear-Data.ps1
```

## 当前已完成功能

- 单主便签窗口启动
- 默认靠右显示
- 始终置顶开关
- 开机自启动开关
- 双击 `启动便签.cmd` 启动应用
- 快速新增任务
- 任务文本内联编辑
- 完成/取消完成
- 完成任务删除线显示
- 删除任务
- 自动保存到 SQLite
- 重启后恢复任务内容
- 重启后恢复窗口位置和大小
- 托盘显示、隐藏、退出
- 数据库自动初始化和建表

## 后续可扩展方向

- 多便签 UI
- 历史归档和操作审计
- 标签和筛选
- 快捷键新增任务
- 透明度调节 UI
- 更正式的安装包或发布版目录
- 通过 `SummaryService` 接入 AI 总结能力
