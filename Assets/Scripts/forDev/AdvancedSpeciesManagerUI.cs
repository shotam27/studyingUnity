using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI;
using SpeciesManagement;

namespace ForDev
{
    /// <summary>
    /// 高度な種族管理UI - 追加・編集・削除機能付き
    /// </summary>
    public class AdvancedSpeciesManagerUI : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject listPanel;
        [SerializeField] private GameObject editPanel;
        [SerializeField] private GameObject confirmPanel;
        
        [Header("List View")]
        [SerializeField] private Transform speciesListParent;
        [SerializeField] private GameObject speciesListItemPrefab;
        [SerializeField] private TMP_InputField searchInput;
        [SerializeField] private TMP_Dropdown filterDropdown;
        [SerializeField] private TextMeshProUGUI countText;
        
        [Header("Edit View")]
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private TMP_InputField hpInput;
        [SerializeField] private TMP_InputField atkInput;
        [SerializeField] private TMP_InputField defInput;
        [SerializeField] private TMP_InputField spdInput;
        [SerializeField] private TMP_Dropdown weaknessDropdown;
        [SerializeField] private TMP_Dropdown strengthDropdown;
        
        [Header("Action Buttons")]
        [SerializeField] private Button addButton;
        [SerializeField] private Button editButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button refreshButton;
        
        [Header("Confirm Dialog")]
        [SerializeField] private TextMeshProUGUI confirmText;
        [SerializeField] private Button confirmYesButton;
        [SerializeField] private Button confirmNoButton;
        
    private List<GameObject> speciesListItems = new List<GameObject>();
    private Species selectedSpecies;
    private Species editingSpecies;
        private System.Action pendingAction;
        private string currentSearchQuery = "";
        private FilterType currentFilter = FilterType.All;
        
        private enum FilterType
        {
            All,
            ByWeakness,
            ByStrength,
            ByHPRange
        }
        
        private void Start()
        {
            SetupUI();
            SetupEventListeners();
            RefreshSpeciesList();
        }
        
        #region UI Setup
        
        private void SetupUI()
        {
            ShowListPanel();
            
            // フィルターオプション設定
            if (filterDropdown != null)
            {
                filterDropdown.options.Clear();
                filterDropdown.options.Add(new TMP_Dropdown.OptionData("All Species"));
                filterDropdown.options.Add(new TMP_Dropdown.OptionData("By Weakness"));
                filterDropdown.options.Add(new TMP_Dropdown.OptionData("By Strength"));
                filterDropdown.options.Add(new TMP_Dropdown.OptionData("By HP Range"));
                filterDropdown.value = 0;
            }
            
            // 弱点・強さドロップダウン設定
            SetupElementDropdowns();
        }
        
        private void SetupElementDropdowns()
        {
            // 弱点ドロップダウン
            if (weaknessDropdown != null)
            {
                weaknessDropdown.options.Clear();
                foreach (WeaknessTag tag in System.Enum.GetValues(typeof(WeaknessTag)))
                {
                    weaknessDropdown.options.Add(new TMP_Dropdown.OptionData(tag.ToString()));
                }
            }
            
            // 強さドロップダウン
            if (strengthDropdown != null)
            {
                strengthDropdown.options.Clear();
                foreach (StrongnessTag tag in System.Enum.GetValues(typeof(StrongnessTag)))
                {
                    strengthDropdown.options.Add(new TMP_Dropdown.OptionData(tag.ToString()));
                }
            }
        }
        
        private void SetupEventListeners()
        {
            // ボタンイベント
            addButton?.onClick.AddListener(StartAddNewSpecies);
            editButton?.onClick.AddListener(StartEditSelectedSpecies);
            deleteButton?.onClick.AddListener(ConfirmDeleteSelectedSpecies);
            saveButton?.onClick.AddListener(SaveCurrentEdit);
            cancelButton?.onClick.AddListener(CancelCurrentEdit);
            refreshButton?.onClick.AddListener(RefreshSpeciesList);
            
            // 確認ダイアログ
            confirmYesButton?.onClick.AddListener(ExecutePendingAction);
            confirmNoButton?.onClick.AddListener(CancelPendingAction);
            
            // 検索・フィルター
            searchInput?.onValueChanged.AddListener(OnSearchQueryChanged);
            filterDropdown?.onValueChanged.AddListener(OnFilterChanged);
            
            // MonsterSpeciesManager イベント
            if (MonsterSpeciesManager.Instance != null)
            {
                MonsterSpeciesManager.Instance.OnSpeciesListChanged += RefreshSpeciesList;
            }
        }
        
        #endregion
        
        #region Panel Management
        
        private void ShowListPanel()
        {
            listPanel?.SetActive(true);
            editPanel?.SetActive(false);
            confirmPanel?.SetActive(false);
        }
        
        private void ShowEditPanel()
        {
            listPanel?.SetActive(false);
            editPanel?.SetActive(true);
            confirmPanel?.SetActive(false);
        }
        
        private void ShowConfirmPanel(string message, System.Action action)
        {
            if (confirmText != null) confirmText.text = message;
            pendingAction = action;
            
            listPanel?.SetActive(false);
            editPanel?.SetActive(false);
            confirmPanel?.SetActive(true);
        }
        
        #endregion
        
        #region Species List Management
        
        public void RefreshSpeciesList()
        {
            ClearSpeciesList();
            
            if (MonsterSpeciesManager.Instance == null)
            {
                Debug.LogError("MonsterSpeciesManager not found!");
                return;
            }
            
            var allSpecies = GetFilteredSpecies();
            UpdateCountText(allSpecies.Count);
            
            for (int i = 0; i < allSpecies.Count; i++)
            {
                CreateSpeciesListItem(allSpecies[i], i);
            }
            
            // ボタン状態更新
            UpdateActionButtons();
        }
        
        private List<Species> GetFilteredSpecies()
        {
            if (MonsterSpeciesManager.Instance == null) return new List<Species>();
            
            var allSpecies = MonsterSpeciesManager.Instance.AllSpecies;
            
            // 検索フィルター適用
            if (!string.IsNullOrEmpty(currentSearchQuery))
            {
                allSpecies = allSpecies.Where(s => 
                    s.SpeciesName.ToLower().Contains(currentSearchQuery.ToLower())
                ).ToList();
            }
            
            // カテゴリフィルター適用
            switch (currentFilter)
            {
                case FilterType.ByWeakness:
                    // 実装時に特定の弱点でフィルタリング
                    break;
                case FilterType.ByStrength:
                    // 実装時に特定の強さでフィルタリング
                    break;
                case FilterType.ByHPRange:
                    // 実装時にHP範囲でフィルタリング
                    break;
            }
            
            return allSpecies;
        }
        
        private void CreateSpeciesListItem(Species species, int index)
        {
            if (speciesListItemPrefab == null || speciesListParent == null) return;
            
            string displayText = $"[{index}] {species.SpeciesName}";
            if (species.BasicStatus != null)
            {
                displayText += $" (HP:{species.BasicStatus.MaxHP})";
            }
            
            var listItem = ListItemHelper.CreateListItem(
                speciesListItemPrefab,
                speciesListParent,
                displayText,
                () => SelectSpecies(species)
            );
            
            if (listItem != null)
            {
                speciesListItems.Add(listItem);
                
                // 選択状態の表示
                if (selectedSpecies == species)
                {
                    var image = listItem.GetComponent<Image>();
                    if (image != null) image.color = Color.yellow;
                }
            }
        }
        
        private void ClearSpeciesList()
        {
            foreach (var item in speciesListItems)
            {
                if (item != null) DestroyImmediate(item);
            }
            speciesListItems.Clear();
        }
        
        private void SelectSpecies(Species species)
        {
            selectedSpecies = species;
            UpdateActionButtons();
            RefreshSpeciesList(); // 選択状態の表示更新
            
            Debug.Log($"Selected species: {species?.SpeciesName}");
        }
        
        #endregion
        
        #region Edit Operations
        
        private void StartAddNewSpecies()
        {
            editingSpecies = null;
            ClearEditFields();
            ShowEditPanel();
        }
        
        private void StartEditSelectedSpecies()
        {
            if (selectedSpecies == null) return;
            
            editingSpecies = selectedSpecies;
            PopulateEditFields(selectedSpecies);
            ShowEditPanel();
        }
        
        private void ClearEditFields()
        {
            if (nameInput != null) nameInput.text = "";
            if (hpInput != null) hpInput.text = "100";
            if (atkInput != null) atkInput.text = "50";
            if (defInput != null) defInput.text = "50";
            if (spdInput != null) spdInput.text = "50";
            if (weaknessDropdown != null) weaknessDropdown.value = 0;
            if (strengthDropdown != null) strengthDropdown.value = 0;
        }
        
        private void PopulateEditFields(Species species)
        {
            if (nameInput != null) nameInput.text = species.SpeciesName ?? "";
            
            if (species.BasicStatus != null)
            {
                if (hpInput != null) hpInput.text = species.BasicStatus.MaxHP.ToString();
                if (atkInput != null) atkInput.text = species.BasicStatus.ATK.ToString();
                if (defInput != null) defInput.text = species.BasicStatus.DEF.ToString();
                if (spdInput != null) spdInput.text = species.BasicStatus.SPD.ToString();
            }
            
            if (weaknessDropdown != null) weaknessDropdown.value = (int)species.WeaknessTag;
            if (strengthDropdown != null) strengthDropdown.value = (int)species.StrongnessTag;
        }
        
        private void SaveCurrentEdit()
        {
            // 入力検証
            if (string.IsNullOrEmpty(nameInput?.text))
            {
                Debug.LogWarning("Species name is required");
                return;
            }
            
            // 新規作成の場合
            if (editingSpecies == null)
            {
                CreateNewSpecies();
            }
            else
            {
                UpdateExistingSpecies();
            }
            
            ShowListPanel();
            RefreshSpeciesList();
        }
        
        private void CreateNewSpecies()
        {
                // 実際の実装では ScriptableObject.CreateInstance<Species>() を使う
            Debug.Log($"Would create new species: {nameInput.text}");
                // TODO: 実際のSpecies作成ロジック
        }
        
        private void UpdateExistingSpecies()
        {
            // 既存の種族データ更新
            Debug.Log($"Would update species: {editingSpecies.SpeciesName}");
            // TODO: 実際のデータ更新ロジック
        }
        
        private void CancelCurrentEdit()
        {
            ShowListPanel();
        }
        
        #endregion
        
        #region Delete Operations
        
        private void ConfirmDeleteSelectedSpecies()
        {
            if (selectedSpecies == null) return;
            
            string message = $"Delete species '{selectedSpecies.SpeciesName}'?\\nThis action cannot be undone.";
            ShowConfirmPanel(message, () => DeleteSelectedSpecies());
        }
        
        private void DeleteSelectedSpecies()
        {
            if (selectedSpecies == null || MonsterSpeciesManager.Instance == null) return;
            
            bool success = MonsterSpeciesManager.Instance.RemoveSpecies(selectedSpecies);
            if (success)
            {
                selectedSpecies = null;
                ShowListPanel();
                RefreshSpeciesList();
                Debug.Log("Species deleted successfully");
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnSearchQueryChanged(string query)
        {
            currentSearchQuery = query;
            RefreshSpeciesList();
        }
        
        private void OnFilterChanged(int filterIndex)
        {
            currentFilter = (FilterType)filterIndex;
            RefreshSpeciesList();
        }
        
        private void ExecutePendingAction()
        {
            pendingAction?.Invoke();
            pendingAction = null;
            ShowListPanel();
        }
        
        private void CancelPendingAction()
        {
            pendingAction = null;
            ShowListPanel();
        }
        
        #endregion
        
        #region UI Updates
        
        private void UpdateActionButtons()
        {
            bool hasSelection = selectedSpecies != null;
            
            if (editButton != null) editButton.interactable = hasSelection;
            if (deleteButton != null) deleteButton.interactable = hasSelection;
        }
        
        private void UpdateCountText(int count)
        {
            if (countText != null)
            {
                countText.text = $"Species Count: {count}";
            }
        }
        
        #endregion
        
        private void OnDestroy()
        {
            // イベント解除
            if (MonsterSpeciesManager.Instance != null)
            {
                MonsterSpeciesManager.Instance.OnSpeciesListChanged -= RefreshSpeciesList;
            }
        }
    }
}
