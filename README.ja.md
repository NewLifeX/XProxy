# XProxy - プロキシサーバー

![GitHub License](https://img.shields.io/github/license/NewLifeX/XProxy?logo=github)
![.NET](https://img.shields.io/badge/.NET-net10.0-512BD4?logo=dotnet)
![NativeAOT](https://img.shields.io/badge/NativeAOT-ready-brightgreen?logo=dotnet)
![platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-blue)
[![contributors](https://img.shields.io/github/contributors/NewLifeX/XProxy?logo=github)](https://github.com/NewLifeX/XProxy/graphs/contributors)

<!-- lang:start -->
🌐 [English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [Español](README.es.md) | [Deutsch](README.de.md) | [Français](README.fr.md) | [Português](README.pt.md) | [Русский](README.ru.md) | [한국어](README.ko.md) | [العربية](README.ar.md)
<!-- lang:end -->

**XProxy** は NewLife エコシステムのプロキシサーバーホストです。1 つのプロセスで複数の `ProxyBase` サービス（NAT リバースプロキシ、HTTP フォワードプロキシ、HTTP リバースプロキシ）をホストします。高性能な NewLife.Net ネットワークスタック上で動作し、NewLife.Agent により Windows サービスまたは Linux デーモンとしてインストールでき、NativeAOT で公開でき、NewLife.Core による UI メッセージのローカライズに対応します。

> ℹ️ この翻訳は遅れている場合があります。最新の内容は [English README](README.md) を参照してください。

![XProxy console](Doc/Demo.png)

**目次**：[クイックスタート](#クイックスタート) · [機能](#機能) · [設定](#設定) · [NativeAOT](#nativeaot) · [ローカライズ](#ローカライズ) · [エコシステムマップ](#エコシステムマップ) · [ビルドとテスト](#ビルドとテスト) · [新生命開発チーム](#新生命開発チーム) · [ライセンス](#ライセンス)

## クイックスタート

.NET 10 SDK が必要です。

```bash
git clone https://github.com/NewLifeX/XProxy.git
cd XProxy
dotnet run --project XProxy
```

初回実行時に `Config/XProxy.config` が作成され、次のデモプロキシが起動します。

| デモ | プロバイダー | リッスン | 転送先 |
| --- | --- | --- | --- |
| Demo1 | NAT | `tcp://0.0.0.0:3388` | `tcp://localhost:3389` |
| Demo2 | Http | `tcp://0.0.0.0:1080` | HTTP フォワードプロキシ（Basic 認証 `user=` / `pass=newlife`） |
| Demo3 | HttpIp | `tcp://0.0.0.0:8080` | `tcp://www.newlifex.com:80` |
| Demo4 | HttpReverse | `tcp://0.0.0.0:8081` | `tcp://www.newlifex.com:80` |

対話型コンソールから Windows サービスとしてインストール（`-i`）したり、サービス実行をシミュレート（`-run`）できます。インストールには管理者権限が必要です。

## 機能

- **NAT リバースプロキシ**：任意の TCP/UDP ポートを転送先サーバーへ転送します。連続ポートとプレフィックス検証に対応します。
- **HTTP フォワードプロキシ**：absolute-form リクエスト（`GET http://...` と `GET https://...`）を受け付け、Basic 認証、プロキシ専用ヘッダーの削除、HTTP/HTTPS へのスキームに応じた転送を行います。
- **HTTP リバースプロキシ**：リバース公開サイト向けに `Host` と `Referer` を書き換えます。
- **複数 IP 送信元**：`HttpIp` は `any`、`round`、`random`、`origin` の各モードでローカル送信元アドレスを選択します。
- **サービスホスティング**：NewLife.Agent により Windows サービス、systemd ユニット、コンソールアプリとして動作します。
- **分散トレーシング**：Stardust サーバーが設定されている場合にスパンを送信します（JIT ビルド。NativeAOT では無効）。
- **設定のホットリロード**：`Config/XProxy.config` の変更を 10 秒以内に反映します。
- **NativeAOT とローカライズ**：自己完結型のネイティブ実行ファイルとして公開でき、実行時メッセージは選択した言語に従います。

## 設定

初回実行時に `Config/XProxy.config` が生成されます。

```xml
<Setting>
  <Debug>true</Debug>
  <Items>
    <ProxyItem Name="Web" Provider="HttpReverse" Enable="true"
               Local="tcp://0.0.0.0:8081" Remote="tcp://www.newlifex.com:80" Config="" />
  </Items>
  <Language>ja</Language>
</Setting>
```

`Provider` には `NAT`、`Http`、`HttpIp`、`HttpReverse` を指定できます。`Config` はプロバイダーごとに異なります。

| プロバイダー | 設定キー |
| --- | --- |
| `NAT` | `Ports`（連続ポート数）、`Prefix`（16 進プレフィックス検証） |
| `Http` | `user`、`pass`、`realm`（Basic プロキシ認証用） |
| `HttpIp` | `IpMode` = `Any` / `Round` / `Random` / `Origin` |

## NativeAOT

```bash
# コマンドライン
dotnet publish XProxy/XProxy.csproj -c Release -r win-x64 -p:PublishAot=true

# 同梱の公開プロファイル
dotnet publish XProxy/XProxy.csproj -c Release -p:PublishProfile=win-x64-aot
```

公開された実行ファイルには .NET ランタイムが含まれ、別途インストールする必要はありません。プロキシ転送、設定、サービスホスティングは NativeAOT でも動作します。Stardust トレーシングは動的アセンブリリフレクションに依存するため無効になり、起動時にメッセージが表示されます。

## ローカライズ

- 英語が原文およびフォールバック言語です。簡体字中国語と繁体字中国語のカタログは `XProxy/Locales` に含まれ、その他の言語は英語にフォールバックします。NewLife.Core 自体は 31 言語を内蔵しています。
- `Config/XProxy.config` の `Language`（例：`en`、`zh-CN`、`zh-TW`）を設定するか、`NewLife.Setting.Current.Language` を呼び出します。空の場合は OS の UI 言語に従います。
- `XProxy/Locales/<culture>.yml` を追加するとカタログを拡張できます。キーは英語の原文と完全に一致させる必要があります。

<!-- ecosystem:start -->
## エコシステムマップ

NewLife は、独立したバージョン管理を行うオープンソースプロジェクトの階層型エコシステムです。以下のマップはすべての NewLife リポジトリで逐語的に同一の内容を共有しており、各プロジェクトはコードリポジトリと主要な NuGet パッケージにリンクしています。

| プロジェクト | 概要 | ダウンロード数 |
| --- | --- | --- |
| **基盤** | | |
| [NewLife.Core](https://github.com/NewLifeX/X) | ログ、構成、キャッシュ、ネットワーク、シリアライズ、スケジューリング、APM トレーシング。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Core?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Core) |
| [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | データミドルウェア：単一テーブルで数百億件級、自動シャーディング、読み書き分離、10 種類以上のデータベースに対応。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCode?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCode) |
| [NewLife.Net](https://github.com/NewLifeX/NewLife.Net) | 高性能ネットワークライブラリ：単一マシンで 177 万 pps、14.5 Gbps（TCP 1KB、2026-10 実測）、400 万 Tcp 長接続。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Net?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Net) |
| [NewLife.Remoting](https://github.com/NewLifeX/NewLife.Remoting) | CS アプリケーションと IoT デバイス向けのプロトコル通信と RPC フレームワーク。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Remoting?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Remoting) |
| [NewLife.Agent](https://github.com/NewLifeX/NewLife.Agent) | .NET アプリケーションを OS デーモンとしてインストール：Windows サービスと Linux systemd。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Agent?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Agent) |
| [NewLife.Templates](https://github.com/NewLifeX/NewLife.Zero) | コンソール、Web、API、サービス、データ、IoT 向けの `dotnet new` プロジェクトテンプレート。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Templates?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Templates) |
| **データとメッセージング** | | |
| [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | マイクロ秒級の遅延、数百万 ops/s のスループット、メッセージキュー内蔵の Redis クライアント。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Redis?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Redis) |
| [NewLife.RocketMQ](https://github.com/NewLifeX/NewLife.RocketMQ) | Apache RocketMQ と Alibaba Cloud Message Queue に対応した純マネージド RocketMQ クライアント。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.RocketMQ?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.RocketMQ) |
| [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | MQTT クライアントとサーバー。Alibaba Cloud IoT への接続を含みます。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MQTT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MQTT) |
| [NewLife.NovaDb](https://github.com/NewLifeX/NewLife.NovaDb) | C# 製ハイブリッドデータベース：組み込み/ネットワークの両モード、リレーショナル、時系列、メッセージキュー、KV を 1 つに。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.NovaDb?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.NovaDb) |
| [NewLife.MySql](https://github.com/NewLifeX/NewLife.MySql) | NewLife データスタック上に構築された MySql ドライバーとヘルパー層。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MySql?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MySql) |
| **Web、AI、分散** | | |
| [NewLife.Cube](https://github.com/NewLifeX/NewLife.Cube) | 高速管理プラットフォームフレームワーク：権限、SSO、OAuth サーバー、CRUD スキャフォールディング。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Cube?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Cube) |
| [Stardust](https://github.com/NewLifeX/Stardust) | 分散サービスプラットフォーム：ノード管理、APM、構成センター、登録センター、リリースセンター。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Stardust?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Stardust) |
| [AntJob](https://github.com/NewLifeX/AntJob) | シャーディング、リトライ、バックフィルに対応した分散タスクとリアルタイムデータのスケジューリングプラットフォーム。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AntJob?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AntJob) |
| [NewLife.AI](https://github.com/NewLifeX/NewLife.AI) | 46 社の LLM プロバイダーに統一接続するクライアントと、すぐに使える Web チャットアプリ ChatAI。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AI?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AI) |
| **産業と IoT** | | |
| [NewLife.IoT](https://github.com/NewLifeX/NewLife.IoT) | プロトコル契約とデバイス抽象を定義する IoT 標準ライブラリ。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IoT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IoT) |
| [NewLife.Modbus](https://github.com/NewLifeX/NewLife.Modbus) | NewLife.IoT 上に実装された Modbus TCP/RTU/ASCII。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Modbus?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Modbus) |
| [NewLife.Siemens](https://github.com/NewLifeX/NewLife.Siemens) | NewLife.IoT 上に実装されたシーメンス PLC プロトコル。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Siemens?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Siemens) |
| [NewLife.Omron](https://github.com/NewLifeX/NewLife.Omron) | NewLife.IoT 上に実装されたオムロン PLC プロトコル。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Omron?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Omron) |
| [NewLife.Melsec](https://github.com/NewLifeX/NewLife.Melsec) | NewLife.IoT 上に実装された三菱 Melsec PLC プロトコル。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Melsec?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Melsec) |
| [NewLife.OPC](https://github.com/NewLifeX/NewLife.OPC) | 産業データアクセス向けの OPC クライアント。 | — |
| [NewLife.BACnet](https://github.com/NewLifeX/NewLife.BACnet) | BACnet ビル管理プロトコルスタック。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.BACnet?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.BACnet) |
| [ZeroIoT](https://github.com/NewLifeX/ZeroIoT) | ゲートウェイとデバイス管理を備えた軽量なオープンソース IoT プラットフォーム。 | — |
| **車載テレマティクス** | | |
| [NewLife.JT808](https://github.com/NewLifeX/NewLife.JT808) | JT/T 808 車両位置情報プロトコル：70 以上のメッセージボディ、2011/2013/2019 に対応。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT808?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT808) |
| [NewLife.JT809](https://github.com/NewLifeX/NewLife.JT809) | 運輸管理部門向けの JT/T 809 プラットフォーム間プロトコル。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT809?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT809) |
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | JT/T 1078 車載ビデオ：RTP 解析、FLV/fMP4 リマックス、ライブ配信。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT1078?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT1078) |
| **ドメインコンポーネント** | | |
| [NewLife.Office](https://github.com/NewLifeX/NewLife.Office) | Excel、Word、PPT、PDF など 15 以上のドキュメント形式を 1 パッケージで。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Office?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Office) |
| [NewLife.Audio](https://github.com/NewLifeX/NewLife.Audio) | クロスプラットフォーム音声：コーデック、デバイス IO、DSP、コンテナ、ストリーミング。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Audio?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Audio) |
| [NewLife.Video](https://github.com/NewLifeX/NewLife.Video) | ビデオコーデック、コンテナ形式、フレーム処理、ストリーミング。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Video?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Video) |
| [NewLife.Map](https://github.com/NewLifeX/NewLife.Map) | 百度地図、高徳地図、テンセント地図、天地図を統一インターフェイス化。ジオコーディングと座標変換に対応。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Map?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Map) |
| [NewLife.IP](https://github.com/NewLifeX/NewLife.IP) | IPv4 位置情報：オフラインのメモリマップデータベースとオンライン API のフォールバック。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IP?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IP) |
| **ツールと製品** | | |
| [CrazyCoder](https://github.com/NewLifeX/XCoder) | 開発者向けツールボックス：ネットワーク、シリアルポート、暗号化、正規表現、Modbus、MQTT。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCoder?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCoder) |
| [GitCandy](https://github.com/NewLifeX/GitCandy) | セルフホスト型 Git ソースコード管理システム。 | — |
| [LuckyClover](https://github.com/NewLifeX/LuckyClover) | 古いホストから起動した際に対応する .NET ランタイムを自動インストールします。 | — |

> バッジは各プロジェクトの主要パッケージを示します。テーマ、拡張、サテライトパッケージは各 NuGet ページを参照してください。ここに記載していない 30 以上のパッケージ（FTP、LoRa、Office 拡張、各種プロトコルドライバーなど）は [NewLifeX 組織](https://github.com/NewLifeX) と [newlifex.com](https://newlifex.com) で公開しています。
<!-- ecosystem:end -->

## ビルドとテスト

```bash
dotnet build XProxy.slnx -c Release
```

`Test` は実行中の HTTP プロキシ経由でリクエストを送る手動統合コンソールです。このリポジトリには自動テストプロジェクトはありません。HTTP ヘッダー構築は NewLife.Core の単体テストでカバーされています。

バージョン形式は `3.2.yyyyMMdd`、ビルド出力は `Bin/Server` です。

<!-- team:start -->
## 新生命開発チーム

<img src="https://newlifex.com/logo.png" width="48" align="right" alt="NewLife" />

新生命チームは 2002 年に設立され、20 年以上にわたり .NET と IoT の技術エコシステムに注力し、100 近いオープンソースプロジェクトを主導し（うち 34 が主要プロジェクト）、電力、物流、医療、産業制御、インターネットなどの業界にソフトウェアとハードウェアを一体化したソリューションを提供しています。「学問に先後なし、達する者が師である」を信条としています。

オープンソースコンポーネントは NuGet で累計 <!--nuget-total:start-->6.48M<!--nuget-total:end--> 回ダウンロードされています（エコシステム全体、2026-10 時点）。

ウェブサイト：https://newlifex.com · オープンソース組織：https://github.com/NewLifeX

<details>
<summary>WeChat 公式アカウント：智能大石头（スキャンしてフォロー）</summary>

![智能大石头](https://newlifex.com/stone.jpg)

</details>
<!-- team:end -->

## ライセンス

本プロジェクトは [MIT](LICENSE) ライセンスで提供されます。出典表示なしで変更・再配布できます。

> 本プロジェクトが役に立ったら、Star、チームへの共有、Issue/PR での改善提案を歓迎します。
