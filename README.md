# Monster Management System

Unity製のモンスター管理システムプロジェクトです。

## 機能

### 開発用ツール (ForDev)
- **MonsterSpeciesUI**: 登録されているMonsterTypeの一覧表示・確認UI
- **DevSampleDataCreator**: 開発用のサンプルデータ作成ツール

### UI コンポーネント
- **MonsterInfoDisplay**: 再利用可能なモンスター情報表示コンポーネント
- **ListItemHelper**: 統一されたリストアイテム作成ユーティリティ

### コアシステム
- **MonsterManager**: モンスターデータの中央管理システム
- **MonsterType**: モンスター種族データのScriptableObject
- **Monster**: 個体モンスターのクラス

## 開発環境
- Unity 2022.3 LTS以上
- TextMeshPro

## 使用方法

### サンプルデータの作成
1. DevSampleDataCreatorコンポーネントをシーンに配置
2. Context Menuから"Create Dev Sample Data"を実行

### 種族確認UI
1. MonsterSpeciesUIコンポーネントをCanvasに配置
2. 必要なUI要素を設定
3. 種族一覧から選択して詳細を確認

## プロジェクト構成

```
Assets/
├── Scripts/
│   ├── UI/
│   │   ├── MonsterInfoDisplay.cs
│   │   └── ListItemHelper.cs
│   └── ForDev/
│       ├── MonsterSpeciesUI.cs
│       └── DevSampleDataCreator.cs
├── Scenes/
│   └── SampleScene.unity
└── ...
```

## 開発履歴
- 2025/08: 初期開発版リリース
  - 基本的なモンスター管理システム
  - 開発用UI作成
  - 再利用可能なUIコンポーネント分離
