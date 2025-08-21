using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI;

namespace ForDev
{
    /// <summary>
    /// 開発用：登録されているMonsterTypeの一覧表示・確認UI
    /// </summary>
    public class MonsterSpeciesUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Transform speciesListParent;
        [SerializeField] private GameObject speciesListItemPrefab;
        [SerializeField] private UI.MonsterInfoDisplay detailDisplay;
        [SerializeField] private Button refreshButton;
        [SerializeField] private TextMeshProUGUI infoText;

        [Header("Debug Options")]
        [SerializeField] private bool showDebugBackground = true;
        [SerializeField] private Color debugBackgroundColor = new Color(0.2f, 0.2f, 0.8f, 0.15f);

        private List<GameObject> speciesListItems = new List<GameObject>();
        private MonsterType selectedSpecies;

        private void Start()
        {
            // ボタンイベント設定
            if (refreshButton != null)
                refreshButton.onClick.AddListener(RefreshSpeciesList);

            // MonsterManagerの初期化を待ってから表示
            StartCoroutine(InitializeAfterManagerReady());
        }

        /// <summary>
        /// MonsterManager準備後に初期化
        /// </summary>
        private System.Collections.IEnumerator InitializeAfterManagerReady()
        {
            // MonsterManagerが初期化されるまで待機
            while (MonsterManager.Instance == null)
            {
                yield return null;
            }

            // 追加で1フレーム待機
            yield return null;

            // 初期表示
            RefreshSpeciesList();
        }

        /// <summary>
        /// MonsterType一覧を更新
        /// </summary>
        public void RefreshSpeciesList()
        {
            Debug.Log("=== RefreshSpeciesList Debug ===");

            // 既存のリストアイテムを削除
            ClearSpeciesList();

            // リスト親にレイアウトを設定（重なり防止）
            SetupListLayout();

            // MonsterManagerから登録済みのMonsterTypeを取得
            if (MonsterManager.Instance == null)
            {
                Debug.LogError("MonsterManager.Instance is NULL!");
                SetInfoText("MonsterManager not found!");
                return;
            }

            var allSpecies = MonsterManager.Instance.AllMonsterTypes;
            Debug.Log($"Found {allSpecies.Count} MonsterType(s)");

            // 情報テキスト更新
            SetInfoText($"Registered Species: {allSpecies.Count}");

            // リストアイテム作成
            for (int i = 0; i < allSpecies.Count; i++)
            {
                var species = allSpecies[i];
                if (species == null)
                {
                    Debug.LogWarning($"MonsterType at index {i} is null");
                    continue;
                }

                CreateSpeciesListItem(species, i);
            }

            Debug.Log($"Created {speciesListItems.Count} species list items");
            Debug.Log("=== End RefreshSpeciesList Debug ===");
        }

        /// <summary>
        /// リスト親のレイアウトを設定（重なり防止）
        /// </summary>
        private void SetupListLayout()
        {
            if (speciesListParent == null) return;

            // VerticalLayoutGroupを追加/取得
            var vlg = speciesListParent.GetComponent<VerticalLayoutGroup>();
            if (vlg == null)
            {
                vlg = speciesListParent.gameObject.AddComponent<VerticalLayoutGroup>();
            }

            // レイアウト設定
            vlg.spacing = 5f;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // ContentSizeFitterも追加
            var csf = speciesListParent.GetComponent<ContentSizeFitter>();
            if (csf == null)
            {
                csf = speciesListParent.gameObject.AddComponent<ContentSizeFitter>();
            }
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            Debug.Log("Setup list layout for species list parent");
        }

        /// <summary>
        /// MonsterType用のリストアイテムを作成
        /// </summary>
        private void CreateSpeciesListItem(MonsterType species, int index)
        {
            if (speciesListItemPrefab == null || speciesListParent == null)
            {
                Debug.LogError("speciesListItemPrefab or speciesListParent is null");
                return;
            }

            // 表示テキスト作成
            string displayText = $"[{index}] {species.MonsterTypeName ?? "Unnamed"}";
            if (species.BasicStatus != null)
            {
                displayText += $" (HP:{species.BasicStatus.MaxHP} ATK:{species.BasicStatus.ATK})";
            }

            // リストアイテム作成
            var listItem = UI.ListItemHelper.CreateListItem(
                speciesListItemPrefab, 
                speciesListParent, 
                displayText, 
                () => SelectSpecies(species)
            );

            if (listItem != null)
            {
                speciesListItems.Add(listItem);

                // デバッグ背景追加
                if (showDebugBackground)
                {
                    UI.ListItemHelper.AddDebugBackground(listItem, debugBackgroundColor);
                }

                Debug.Log($"Created species list item: {displayText}");
            }
        }

        /// <summary>
        /// MonsterTypeを選択
        /// </summary>
        private void SelectSpecies(MonsterType species)
        {
            selectedSpecies = species;
            Debug.Log($"Selected species: {species?.MonsterTypeName ?? "null"}");

            // 詳細表示更新
            if (detailDisplay != null)
            {
                detailDisplay.gameObject.SetActive(true);
                detailDisplay.DisplayMonsterType(species);
            }

            // 追加情報をログ出力
            LogSpeciesDetails(species);
        }

        /// <summary>
        /// MonsterTypeの詳細をログ出力（デバッグ用）
        /// </summary>
        private void LogSpeciesDetails(MonsterType species)
        {
            if (species == null) return;

            Debug.Log($"=== Species Details: {species.MonsterTypeName} ===");
            Debug.Log($"Basic Status: HP:{species.BasicStatus?.MaxHP} ATK:{species.BasicStatus?.ATK} DEF:{species.BasicStatus?.DEF} SPD:{species.BasicStatus?.SPD}");
            Debug.Log($"Weakness: {species.WeaknessTag}, Strongness: {species.StrongnessTag}");
            Debug.Log($"Basic Skills Count: {species.BasicSkills?.Count ?? 0}");
            if (species.BasicSkills != null)
            {
                for (int i = 0; i < species.BasicSkills.Count; i++)
                {
                    var skill = species.BasicSkills[i];
                    Debug.Log($"  Skill[{i}]: {skill?.SkillName ?? "null"} (Dmg:{skill?.Damage ?? 0})");
                }
            }
            Debug.Log($"Sprite: {(species.Sprite != null ? "Available" : "None")}");
            Debug.Log("=== End Species Details ===");
        }

        /// <summary>
        /// リストをクリア
        /// </summary>
        private void ClearSpeciesList()
        {
            foreach (var item in speciesListItems)
            {
                if (item != null)
                    Destroy(item);
            }
            speciesListItems.Clear();
        }

        /// <summary>
        /// 情報テキストを設定
        /// </summary>
        private void SetInfoText(string text)
        {
            if (infoText != null)
                infoText.text = text;
        }

        /// <summary>
        /// サンプルMonsterTypeを作成（開発用）
        /// </summary>
        [ContextMenu("Create Sample Species")]
        public void CreateSampleSpecies()
        {
            Debug.Log("Creating sample species for development...");

            // MonsterManagerが存在しない場合は作成を試みる
            if (MonsterManager.Instance == null)
            {
                Debug.LogWarning("MonsterManager not found, cannot create sample species");
                return;
            }

            // サンプル作成者を探す
            var sampleCreator = FindObjectOfType<SampleDataCreator>();
            if (sampleCreator != null)
            {
                // リフレクションで一時的なMonsterTypeを作成
                var method = typeof(SampleDataCreator).GetMethod("CreateTemporaryMonsterTypes", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method?.Invoke(sampleCreator, null);

                Debug.Log("Sample species creation attempted");
                RefreshSpeciesList();
            }
            else
            {
                Debug.LogWarning("SampleDataCreator not found");
            }
        }

        /// <summary>
        /// 選択中のMonsterTypeからサンプルモンスターを作成
        /// </summary>
        [ContextMenu("Create Monster from Selected Species")]
        public void CreateMonsterFromSelectedSpecies()
        {
            if (selectedSpecies == null)
            {
                Debug.LogWarning("No species selected");
                return;
            }

            if (MonsterManager.Instance == null)
            {
                Debug.LogError("MonsterManager not found");
                return;
            }

            // ランダムな名前とレベルでモンスターを作成
            string[] sampleNames = { "Test Alpha", "Sample Beta", "Dev Gamma", "Debug Delta", "Verify Epsilon" };
            string randomName = sampleNames[Random.Range(0, sampleNames.Length)];
            int randomLevel = Random.Range(1, 11);

            var monster = MonsterManager.Instance.CreateAndAddMonster(selectedSpecies, randomName, randomLevel);
            if (monster != null)
            {
                Debug.Log($"Created monster: {monster.NickName} (Lv.{monster.Level}) from species {selectedSpecies.MonsterTypeName}");
            }
            else
            {
                Debug.LogError("Failed to create monster from selected species");
            }
        }
    }
}
