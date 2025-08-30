using System.Collections.Generic;
using UnityEngine;

namespace SpeciesManagement
{
    /// <summary>
    /// MonsterType CRUD操作の詳細仕様
    /// Web API との連携も考慮した設計
    /// </summary>
    public static class MonsterSpeciesCRUDOperations
    {
        // CREATE - 新規種族作成
        public static class Create
        {
            /// <summary>
            /// 新規種族作成時の必須データ
            /// </summary>
            public class CreateRequest
            {
                public string name;                    // 必須: 種族名
                public string description;             // 任意: 説明
                public BasicStatusData basicStatus;    // 必須: 基本ステータス
                public WeaknessTag weakness;           // 必須: 弱点
                public StrongnessTag strength;         // 必須: 強み
                public List<string> skillIds;          // 任意: スキルID一覧
                public string spriteUrl;              // 任意: 画像URL
                public RarityType rarity;             // 必須: レアリティ
                public CategoryType category;          // 必須: カテゴリ
            }

            /// <summary>
            /// 作成レスポンス
            /// </summary>
            public class CreateResponse
            {
                public bool success;
                public string speciesId;      // 自動生成されるID
                public string message;
                public List<string> errors;   // バリデーションエラー
            }

            /// <summary>
            /// バリデーションルール
            /// </summary>
            public static List<string> ValidateCreateRequest(CreateRequest request)
            {
                var errors = new List<string>();

                // 名前チェック
                if (string.IsNullOrEmpty(request.name))
                    errors.Add("Species name is required");
                else if (request.name.Length > 50)
                    errors.Add("Species name must be 50 characters or less");

                // ステータスチェック
                if (request.basicStatus == null)
                    errors.Add("Basic status is required");
                else
                {
                    if (request.basicStatus.MaxHP <= 0 || request.basicStatus.MaxHP > 999)
                        errors.Add("HP must be between 1 and 999");
                    if (request.basicStatus.ATK < 0 || request.basicStatus.ATK > 999)
                        errors.Add("Attack must be between 0 and 999");
                    if (request.basicStatus.DEF < 0 || request.basicStatus.DEF > 999)
                        errors.Add("Defense must be between 0 and 999");
                    if (request.basicStatus.SPD < 0 || request.basicStatus.SPD > 999)
                        errors.Add("Speed must be between 0 and 999");
                }

                // 重複チェック（MonsterSpeciesManagerで実装）
                // if (MonsterSpeciesManager.Instance.GetSpeciesByName(request.name) != null)
                //     errors.Add("Species with this name already exists");

                return errors;
            }
        }

        // READ - 種族データ取得
        public static class Read
        {
            /// <summary>
            /// 検索条件
            /// </summary>
            public class SearchRequest
            {
                public string nameQuery;              // 名前で部分検索
                public WeaknessTag? weakness;         // 弱点フィルター
                public StrongnessTag? strength;       // 強みフィルター
                public RarityType? rarity;            // レアリティフィルター
                public CategoryType? category;        // カテゴリフィルター
                public int? minHP, maxHP;             // HPの範囲
                public int? minATK, maxATK;           // 攻撃力の範囲
                public string sortBy;                 // ソート基準 (name, hp, attack, etc.)
                public bool ascending;                // 昇順/降順
                public int page;                      // ページ番号 (Web API用)
                public int limit;                     // 1ページあたりの件数
            }

            /// <summary>
            /// 検索レスポンス
            /// </summary>
            public class SearchResponse
            {
                public List<SpeciesData> species; // 種族データ一覧
                public int totalCount;                 // 総件数
                public int currentPage;                // 現在のページ
                public int totalPages;                 // 総ページ数
                public bool hasNext;                   // 次ページの有無
                public bool hasPrevious;               // 前ページの有無
            }

            /// <summary>
            /// 詳細取得レスポンス
            /// </summary>
            public class DetailResponse
            {
                public SpeciesData species;
                public List<RelatedSpeciesData> relatedSpecies; // 類似種族
                public UsageStatistics usage;                   // 使用統計
            }
        }

        // UPDATE - 種族データ更新
        public static class Update
        {
            /// <summary>
            /// 更新リクエスト
            /// </summary>
            public class UpdateRequest
            {
                public string speciesId;              // 更新対象のID
                public string name;                   // 更新後の名前
                public string description;            // 更新後の説明
                public BasicStatusData basicStatus;   // 更新後のステータス
                public WeaknessTag weakness;          // 更新後の弱点
                public StrongnessTag strength;        // 更新後の強み
                public List<string> skillIds;         // 更新後のスキル一覧
                public string spriteUrl;             // 更新後の画像URL
                public RarityType rarity;            // 更新後のレアリティ
                public CategoryType category;         // 更新後のカテゴリ
                public string updateReason;          // 更新理由（ログ用）
            }

            /// <summary>
            /// 更新レスポンス
            /// </summary>
            public class UpdateResponse
            {
                public bool success;
                public SpeciesData updatedSpecies; // 更新後のデータ
                public string message;
                public List<string> errors;
                public ChangeLog changeLog;            // 変更履歴
            }

            /// <summary>
            /// 変更履歴
            /// </summary>
            public class ChangeLog
            {
                public string fieldName;     // 変更されたフィールド
                public string oldValue;      // 変更前の値
                public string newValue;      // 変更後の値
                public string timestamp;     // 変更日時
                public string userId;        // 変更者（Web版用）
            }
        }

        // DELETE - 種族データ削除
        public static class Delete
        {
            /// <summary>
            /// 削除リクエスト
            /// </summary>
            public class DeleteRequest
            {
                public string speciesId;         // 削除対象のID
                public string deleteReason;      // 削除理由
                public bool forceDelete;         // 強制削除フラグ
            }

            /// <summary>
            /// 削除レスポンス
            /// </summary>
            public class DeleteResponse
            {
                public bool success;
                public string message;
                public List<string> warnings;    // 削除に関する警告
                public DependencyCheck dependencies; // 依存関係チェック結果
            }

            /// <summary>
            /// 依存関係チェック
            /// </summary>
            public class DependencyCheck
            {
                public int existingMonsterCount;     // この種族のモンスター個体数
                public List<string> relatedData;     // 関連するデータ
                public bool canSafelyDelete;         // 安全に削除可能か
            }
        }

        // 共通データ構造
        [System.Serializable]
        public class SpeciesData
        {
            public string id;
            public string name;
            public string description;
            public BasicStatusData basicStatus;
            public WeaknessTag weakness;
            public StrongnessTag strength;
            public List<SkillData> skills;
            public SpriteData sprite;
            public RarityType rarity;
            public CategoryType category;
            public string createdAt;
            public string updatedAt;
            public int version;                       // 楽観的排他制御用
        }

        [System.Serializable]
        public class BasicStatusData
        {
            public int MaxHP;
            public int ATK;
            public int DEF;
            public int SPD;
        }

        [System.Serializable]
        public class SkillData
        {
            public string id;
            public string name;
            public int damage;
            public string element;
            public string description;
        }

        [System.Serializable]
        public class SpriteData
        {
            public string path;        // ローカルパス
            public string url;         // Web URL
            public string thumbnailUrl; // サムネイルURL
        }

        public enum RarityType
        {
            Common,
            Uncommon,
            Rare,
            Epic,
            Legendary
        }

        public enum CategoryType
        {
            Beast,
            Dragon,
            Elemental,
            Humanoid,
            Undead,
            Plant,
            Machine,
            Spirit
        }

        [System.Serializable]
        public class RelatedSpeciesData
        {
            public string id;
            public string name;
            public float similarity; // 類似度 (0.0 - 1.0)
        }

        [System.Serializable]
        public class UsageStatistics
        {
            public int totalCreatedMonsters;    // この種族から作成されたモンスター数
            public int activeInParties;         // パーティで使用中の数
            public float popularityRank;       // 人気ランキング
            public string lastUsed;            // 最後に使用された日時
        }
    }
}
