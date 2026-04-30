# iV2EX — Agent Guide

## Project
V2EX community client for Windows. WinUI 3 desktop app, single `.sln` project, target `net10.0-windows10.0.22621.0`, AOT-compiled.

## Build & Run
```bash
dotnet build iV2EX/iV2EX.csproj
dotnet run --project iV2EX/iV2EX.csproj
```
No test suite, no CI.

## Architecture
- **Entry**: `App.xaml.cs` — single-instance enforcement, creates `MainWindow`
- **Shell**: `MainWindow.xaml.cs` — title bar customization, cookie restore, initial navigation
- **Navigation**: Custom `PageStack` (`Util/PageStack.cs`). Use `PageStack.Next("Left","Right",typeof(X),param)` — never call `Frame.Navigate()` directly
- **Layout**: Two-panel responsive — `MainPage.LeftPivot` (tabs: 首页/节点/我的) + `MainPage.RightFrame`. Switches between master-detail and single-page at 600px width
- **Auth**: Cookie-based. Persisted via `ApplicationData.Current.LocalSettings["Cookies"]`, restored into `HttpClientHandler.CookieContainer` on startup

## Key directories
| Dir | Purpose |
|-----|---------|
| `Views/` | XAML pages loaded into RightFrame |
| `Fragments/` | Sub-views embedded in MainPage Pivot tabs |
| `Controls/` | Reusable custom controls (HtmlTextBlock, ToastTips, RefreshButton) |
| `Model/` | Plain data objects (TopicModel, ReplyModel, MemberModel, NodeModel, etc.) |
| `GetData/` | `ApiClient` (HTTP to v2ex.com) + `DomParse` (AngleSharp HTML scraping) |
| `TupleModel/` | Generic `PagesBaseModel<T>` for paginated results |
| `Util/` | PageStack, IncrementalLoadingCollection (ISupportIncrementalLoading), Converter, helpers |
| `Generated Files/CsWinRT/` | Auto-generated WinRT projections — do not edit |

## Key gotchas
- **PublishAot=true** — reflection/`dynamic` in new code will fail at runtime unless annotated with `DynamicallyAccessedMembers`
- **Rx.NET** is the standard event pattern: `Observable.FromEventPattern(...).ObserveOn(DispatcherQueueScheduler.Current).Subscribe(...)` — follow existing examples
- **PageStack.Back()** uses static `MainPage.RightPart`/`LeftPart` — those fields are set in `MainPage` constructor, be aware when changing navigation
- **Cookies** are stored as a semicolon-separated string in LocalSettings; V2EX login flow requires scraping `once` tokens from signin page HTML
- The project was fully migrated to WinUI 3 — no UWP dependencies remain

## Dependencies
- **AngleSharp** — HTML parsing (V2EX has no REST API for most content, uses HTML scraping)
- **Newtonsoft.Json** — JSON for `/api/nodes/all.json` endpoint
- **System.Reactive** — reactive event handling
- **Microsoft.WindowsAppSDK** — WinUI 3 runtime
- **Microsoft.Windows.CsWinRT** — WinRT interop codegen
- **Microsoft.ApplicationInsights** — telemetry
