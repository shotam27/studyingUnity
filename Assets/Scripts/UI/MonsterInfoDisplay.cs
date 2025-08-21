using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    /// <summary>
    /// 再利用可能なモンスター情報表示コンポーネント
    /// MonsterUIから抽出した共通部分
    /// </summary>
    public class MonsterInfoDisplay : MonoBehaviour
    {
        [Header("Basic Info")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Image monsterImage;

        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private TextMeshProUGUI atkText;
        [SerializeField] private TextMeshProUGUI defText;
        [SerializeField] private TextMeshProUGUI spdText;

        [Header("Status")]
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("Skills")]
        [SerializeField] private Transform skillsParent;
        [SerializeField] private GameObject skillItemPrefab;

        private List<GameObject> skillItems = new List<GameObject>();

        /// <summary>
        /// Monsterの情報を表示
        /// </summary>
        public void DisplayMonster(Monster monster)
        {
            if (monster == null)
            {
                ClearDisplay();
                return;
            }

            // 基本情報
            SetText(nameText, monster.NickName);
            SetText(levelText, $"Lv.{monster.Level}");

            // ステータス
            SetText(hpText, $"HP: {monster.CurrentHP}/{monster.MaxHP}");
            SetText(atkText, $"ATK: {monster.ATK}");
            SetText(defText, $"DEF: {monster.DEF}");
            SetText(spdText, $"SPD: {monster.SPD}");

            // 状態
            string status = monster.IsDead ? "Dead" : "Alive";
            SetText(statusText, $"Status: {status}");

            // モンスタータイプの画像
            if (monsterImage != null && monster.MonsterType?.Sprite != null)
            {
                monsterImage.sprite = monster.MonsterType.Sprite;
                monsterImage.gameObject.SetActive(true);
            }
            else if (monsterImage != null)
            {
                monsterImage.gameObject.SetActive(false);
            }

            // スキル表示
            DisplaySkills(monster.LearnedSkills);
        }

        /// <summary>
        /// MonsterTypeの情報を表示（レベル情報なし）
        /// </summary>
        public void DisplayMonsterType(MonsterType monsterType, int displayLevel = 1)
        {
            if (monsterType == null)
            {
                ClearDisplay();
                return;
            }

            // 基本情報
            SetText(nameText, monsterType.MonsterTypeName);
            SetText(levelText, $"Lv.{displayLevel} (Base)");

            // ベースステータス
            var stats = monsterType.BasicStatus;
            SetText(hpText, $"HP: {stats.MaxHP}");
            SetText(atkText, $"ATK: {stats.ATK}");
            SetText(defText, $"DEF: {stats.DEF}");
            SetText(spdText, $"SPD: {stats.SPD}");

            // 属性情報
            string statusInfo = $"Weak: {monsterType.WeaknessTag}, Strong: {monsterType.StrongnessTag}";
            SetText(statusText, statusInfo);

            // 画像
            if (monsterImage != null && monsterType.Sprite != null)
            {
                monsterImage.sprite = monsterType.Sprite;
                monsterImage.gameObject.SetActive(true);
            }
            else if (monsterImage != null)
            {
                monsterImage.gameObject.SetActive(false);
            }

            // 基本スキル表示
            DisplaySkills(monsterType.BasicSkills);
        }

        /// <summary>
        /// スキル一覧を表示
        /// </summary>
        public void DisplaySkills(List<Skill> skills)
        {
            // 既存のスキルアイテムを削除
            foreach (var item in skillItems)
            {
                if (item != null)
                    Destroy(item);
            }
            skillItems.Clear();

            if (skillsParent == null || skillItemPrefab == null || skills == null)
                return;

            // スキルアイテムを作成
            foreach (var skill in skills)
            {
                if (skill == null) continue;

                var skillItem = Instantiate(skillItemPrefab, skillsParent);
                skillItems.Add(skillItem);

                // スキル情報を設定
                var skillText = skillItem.GetComponentInChildren<TextMeshProUGUI>();
                if (skillText != null)
                {
                    skillText.text = $"{skill.SkillName}\nDmg: {skill.Damage}, Range: {skill.Range}";
                }
            }
        }

        /// <summary>
        /// 表示をクリア
        /// </summary>
        public void ClearDisplay()
        {
            SetText(nameText, "---");
            SetText(levelText, "---");
            SetText(hpText, "---");
            SetText(atkText, "---");
            SetText(defText, "---");
            SetText(spdText, "---");
            SetText(statusText, "---");

            if (monsterImage != null)
                monsterImage.gameObject.SetActive(false);

            // スキルアイテムをクリア
            foreach (var item in skillItems)
            {
                if (item != null)
                    Destroy(item);
            }
            skillItems.Clear();
        }

        private void SetText(TextMeshProUGUI text, string value)
        {
            if (text != null)
                text.text = value;
        }
    }
}
