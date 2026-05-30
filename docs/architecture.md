# LightStickyNote Architecture

## 第一阶段 MVP 范围

第一阶段交付一个可用的 Windows 轻量便签主窗口：

- 单便签窗口
- 任务增删改查
- 完成状态切换
- 自动保存
- 置顶和右侧停靠
- 托盘最小能力
- SQLite 本地持久化
- JSON 配置
- 双击启动脚本
- 开机自启动开关

明确不在本阶段投入复杂实现的能力：

- AI 总结
- 云同步
- 多端同步
- 富文本
- 复杂标签系统
- 完整历史管理后台

## 当前模块划分

```text
LightStickyNote.App
├─ Data
│  └─ DatabaseInitializer
├─ Infrastructure
│  ├─ AppPaths
│  ├─ ObservableObject
│  └─ RelayCommand
├─ Models
│  ├─ AppSettings
│  ├─ Note
│  └─ NoteItem
├─ Repositories
│  └─ NoteRepository
├─ Services
│  ├─ LauncherScriptService
│  ├─ SettingsService
│  ├─ StartupRegistrationService
│  ├─ TrayIconService
│  └─ WindowPlacementService
├─ ViewModels
│  ├─ MainViewModel
│  └─ TaskItemViewModel
└─ MainWindow / App
```

职责说明：

- `App` 负责启动、依赖拼装、脚本准备、异常提示和托盘生命周期。
- `MainWindow` 只保留窗口事件和输入桥接。
- `MainViewModel` 负责主便签状态、自动保存节流、任务操作和自启动开关联动。
- `NoteRepository` 负责 SQLite 读写。
- `SettingsService` 负责 JSON 配置。
- `LauncherScriptService` 负责生成项目根目录的双击启动脚本。
- `StartupRegistrationService` 负责在当前用户 Startup 目录中创建或移除自启动脚本。
- `WindowPlacementService` 负责窗口位置和尺寸恢复。
- `TrayIconService` 负责托盘显示、隐藏与退出。

## SQLite 数据表设计

当前数据库初始化时会创建以下表：

### notes

- `id`
- `title`
- `created_at`
- `updated_at`
- `archived_at`
- `position_x`
- `position_y`
- `width`
- `height`
- `is_pinned`

### note_items

- `id`
- `note_id`
- `text`
- `is_done`
- `sort_order`
- `created_at`
- `updated_at`
- `completed_at`

### note_history

- `id`
- `note_id`
- `item_id`
- `action`
- `old_value`
- `new_value`
- `created_at`

### tags

- `id`
- `name`
- `color`

### note_tags

- `note_id`
- `tag_id`

## 为什么 JSON 只用于配置，SQLite 用于业务数据

JSON 更适合存放应用级配置，比如：

- 是否置顶
- 是否开机自启动
- 默认透明度
- 默认窗口尺寸
- 关闭时是否隐藏到托盘

SQLite 更适合存放业务数据，因为未来数据会越来越结构化：

- 一个便签下面会有多条任务
- 多便签会引入 note 和 note_items 的一对多关系
- 历史、标签、汇总都需要结构化查询和扩展

## 启动与自启动设计

### 双击启动

项目根目录提供：

`启动便签.cmd`

它只负责调用纯 ASCII 文件名的入口：

`Launch-LightStickyNote.cmd`

ASCII 入口再调用：

`tools\Run.ps1`

`Run.ps1` 会：

1. 设置当前开发环境所需的 .NET 环境变量
2. 在需要时构建解决方案
3. 直接启动构建产物 `LightStickyNote.App.exe`

### 开机自启动

应用内勾选 `开机自启动` 后，`StartupRegistrationService` 会在当前用户 Startup 目录生成一个很小的脚本：

```text
LightStickyNote Startup.cmd
```

这个脚本再去调用项目根目录的 `Launch-LightStickyNote.cmd`。ASCII 文件名可以规避 Windows 命令行代码页差异，启动逻辑也只有一份。

## 后续扩展方向

### 多便签

现有 `notes` 表已经支持多条 note 记录。后续只需要：

- 在 UI 层增加便签列表/切换器
- 给 `NoteRepository` 增加按 note 查询与创建能力
- 给窗口管理层增加多窗口或分页承载策略

### 历史管理

`note_history` 已预留。后续可以在任务新增、编辑、完成、删除时记录变更，用于：

- 查看操作历史
- 恢复误删内容
- 做阶段总结

### 范围总结

如果未来做“本周完成了什么”这类总结，可以直接从：

- `note_items.completed_at`
- `note_history`

中按时间范围聚合，不需要改动核心便签编辑逻辑。

### AI 总结

后续如果要接 LLM，总结能力应通过独立的 `SummaryService` 接口进入：

```text
MainViewModel -> SummaryService -> LLM Provider
```

原则：

- 不要把 LLM 调用塞进 `NoteRepository`
- 不要让核心增删改查依赖网络
- 不要让便签基础使用流程被 AI 能力污染
