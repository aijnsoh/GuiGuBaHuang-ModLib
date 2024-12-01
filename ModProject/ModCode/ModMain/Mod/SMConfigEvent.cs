﻿using Boo.Lang.Compiler.TypeSystem;
using EGameTypeData;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Object;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.SM_CONFIG_EVENT, IsGlobal = true)]
    public class SMConfigEvent : ModEvent
    {
        public const string TITLE = "S&M Configs";

        //Configs
        public float AddAtkRate { get; set; } = 0f;
        public float AddDefRate { get; set; } = 0f;
        public float AddHpRate { get; set; } = 0f;
        public float AddBasisRate { get; set; } = 0f;
        public float AddSpecialMonsterRate { get; set; } = 0f;
        public float AddTaxRate { get; set; } = 0f;
        public float AddInflationRate { get; set; } = 0f;
        public float AddBuildingCostRate { get; set; } = 0f;
        public float AddBankAccountCostRate { get; set; } = 0f;
        public float AddBankFee { get; set; } = 0f;
        public float AddNpcGrowRate { get; set; } = 0f;
        public float AddLevelupExpRate { get; set; } = 0f;
        public bool HideSaveButton { get; set; } = false;
        public bool HideReloadButton { get; set; } = false;
        public bool HideBattleMap { get; set; } = false;
        public bool NoRebirth { get; set; } = false;
        public bool Onelife { get; set; } = false;
        public bool OnlyPortalAtCityAndSect { get; set; } = false;
        public bool NoExpFromBattles { get; set; } = false;

        //UI
        private UIHelper.UICustom1 uiCustom;
        private UIItemBase.UIItemText txtTotalScore;
        private UIItemBase.UIItemComposite slMonstAtk;
        private UIItemBase.UIItemComposite slMonstDef;
        private UIItemBase.UIItemComposite slMonstHp;
        private UIItemBase.UIItemComposite slMonstBasis;
        private UIItemBase.UIItemComposite slMonstSpecialRate;
        private UIItemBase.UIItemComposite slEcoTaxRate;
        private UIItemBase.UIItemComposite slEcoInfRate;
        private UIItemBase.UIItemComposite slEcoBuildingCost;
        private UIItemBase.UIItemComposite slEcoBankAccCost;
        private UIItemBase.UIItemComposite slEcoBankFee;
        private UIItemBase.UIItemComposite slNpcGrowRate;
        private UIItemBase.UIItemComposite slMiscLevelupExp;
        private UIItemBase.UIItemComposite tglSysHideSave;
        private UIItemBase.UIItemComposite tglSysHideReload;
        private UIItemBase.UIItemComposite tglSysHideBattleMap;
        private UIItemBase.UIItemComposite tglSysNoRebirth;
        private UIItemBase.UIItemComposite tglSysOnelife;
        private UIItemBase.UIItemComposite tglSysOnlyPortalAtCityAndSect;
        private UIItemBase.UIItemComposite tglSysNoExpFromBattle;

        //Score
        public static IList<SMItemWork> ScoreCalculator { get; } = new List<SMItemWork>();

        public override void OnLoadGlobal()
        {
            base.OnLoadGlobal();

            ScoreCalculator.Clear();
            Register(() => slMonstAtk, s => (s.Get().Parse<float>() * 100).Parse<int>());
            Register(() => slMonstDef, s => (s.Get().Parse<float>() * 100).Parse<int>());
            Register(() => slMonstHp, s => (s.Get().Parse<float>() * 100).Parse<int>());
            Register(() => slMonstBasis, s => (s.Get().Parse<float>() * 100).Parse<int>());
            Register(() => slMonstSpecialRate, s => (s.Get().Parse<float>() * 3000).Parse<int>());
            Register(() => slEcoTaxRate, s => (s.Get().Parse<float>() * 100).Parse<int>());
            Register(() => slEcoInfRate, s => (s.Get().Parse<float>() * 1000).Parse<int>());
            Register(() => slEcoBuildingCost, s => (s.Get().Parse<float>() * 100).Parse<int>());
            Register(() => slEcoBankAccCost, s => (s.Get().Parse<float>() * 100).Parse<int>());
            Register(() => slEcoBankFee, s => (s.Get().Parse<float>() * 100).Parse<int>());
            Register(() => slNpcGrowRate, s => (s.Get().Parse<float>() * 1000).Parse<int>());
            Register(() => slMiscLevelupExp, s => (s.Get().Parse<float>() * 2000).Parse<int>());
            Register(() => tglSysHideSave, s => 1000, s => s.Get().Parse<bool>(), onChange: (s, v) => tglSysHideReload.Set(false));
            Register(() => tglSysHideReload, s => 5000, s => s.Get().Parse<bool>(), s => tglSysHideSave.Get().Parse<bool>(), onChange: (s, v) => tglSysOnelife.Set(false));
            Register(() => tglSysHideBattleMap, s => 2000, s => s.Get().Parse<bool>());
            Register(() => tglSysNoRebirth, s => 10000, s => s.Get().Parse<bool>());
            Register(() => tglSysOnelife, s => 20000, s => s.Get().Parse<bool>(), s => tglSysHideReload.Get().Parse<bool>());
            Register(() => tglSysOnlyPortalAtCityAndSect, s => 1000, s => s.Get().Parse<bool>());
            Register(() => tglSysNoExpFromBattle, s => 1000, s => s.Get().Parse<bool>());
        }

        private void Register(
            Func<UIItemBase> funcComp, 
            Func<UIItemBase, int> funcCal, 
            Func<UIItemBase, bool> funcCond = null,
            Func<UIItemBase, bool> funcEna = null,
            Func<UIItemBase, object[]> funcFormatter = null,
            Action<UIItemBase, object> onChange = null)
        {
            var formatter = funcFormatter ?? (s =>
            {
                var rs = new object[] { 0, 0 };
                if (s.Parent != null)
                {
                    //point
                    rs[0] = CalCompScore(s.Parent);
                    //%
                    var x = s.Parent as UIItemBase.UIItemComposite;
                    if (x.MainComponent is UIItemBase.UIItemSlider)
                    {
                        rs[1] = (x.Get().Parse<float>() * 100).Parse<int>().ToString("+#;-#;0");
                    }
                }
                return rs;
            });
            ScoreCalculator.Add(new SMItemWork
            {
                Comp = funcComp,
                Cal = funcCal,
                Cond = funcCond ?? (s => true),
                EnaAct = funcEna ?? (s => true),
                Formatter = formatter,
                Change = onChange,
            });
        }

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.Login.uiName)
            {
                var uiLogin = g.ui.GetUI<UILogin>(UIType.Login);
                var modConfigBtn = uiLogin.btnSet.Create().Pos(0f, 3.3f, uiLogin.btnPaperChange.transform.position.z);
                var modConfigText = modConfigBtn.GetComponentInChildren<Text>().Align(TextAnchor.MiddleCenter);
                modConfigText.text = TITLE;
                modConfigBtn.onClick.AddListener((UnityAction)OpenSMConfigs);
            }
        }

        private void OpenSMConfigs()
        {
            uiCustom = UIHelper.UICustom1.Create(TITLE, SetSMConfigs, true);
            int col, row;

            col = 1; row = 0;
            uiCustom.AddText(col, row++, "Monster:").Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleRight);
            slMonstAtk = uiCustom.AddCompositeSlider(col, row++, "ATK", -0.50f, 10.00f, AddAtkRate, "{1}% ({0}P)");
            slMonstDef = uiCustom.AddCompositeSlider(col, row++, "DEF", -0.50f, 10.00f, AddDefRate, "{1}% ({0}P)");
            slMonstHp = uiCustom.AddCompositeSlider(col, row++, "Max HP", -0.50f, 10.00f, AddHpRate, "{1}% ({0}P)");
            slMonstBasis = uiCustom.AddCompositeSlider(col, row++, "Basis", -0.50f, 10.00f, AddBasisRate, "{1}% ({0}P)");
            uiCustom.AddText(col, row++, "(Included Sword, Blade, Spear, Fist, Finger, Palm, Fire, Water, Thunder, Wood, Wind, Earth)").Format(null, 13).Align(TextAnchor.MiddleLeft);
            slMonstSpecialRate = uiCustom.AddCompositeSlider(col, row++, "Special Monster Rate", -0.50f, 1.00f, AddSpecialMonsterRate, "{1}% ({0}P)");

            col = 1; row = 8;
            uiCustom.AddText(col, row++, "Economic:").Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleRight);
            slEcoTaxRate = uiCustom.AddCompositeSlider(col, row++, "Tax Rate", 0.00f, 10.00f, AddTaxRate, "{1}% ({0}P)");
            slEcoInfRate = uiCustom.AddCompositeSlider(col, row++, "Inflation Rate", -0.50f, 3.00f, AddInflationRate, "{1}% ({0}P)");
            slEcoBuildingCost = uiCustom.AddCompositeSlider(col, row++, "Building Cost", 0.00f, 10.00f, AddBuildingCostRate, "{1}% ({0}P)");
            slEcoBankAccCost = uiCustom.AddCompositeSlider(col, row++, "Bank Account Cost", 0.00f, 10.00f, AddBankAccountCostRate, "{1}% ({0}P)");
            slEcoBankFee = uiCustom.AddCompositeSlider(col, row++, "Bank Fee", 0.00f, 100.00f, AddBankFee, "{1}% ({0}P)");

            col = 1; row = 15;
            uiCustom.AddText(col, row++, "NPC:").Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleRight);
            slNpcGrowRate = uiCustom.AddCompositeSlider(col, row++, "Grow Rate", 0.00f, 10.00f, AddNpcGrowRate, "{1}% ({0}P)");

            col = 1; row = 18;
            uiCustom.AddText(col, row++, "Misc:").Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleRight);
            slMiscLevelupExp = uiCustom.AddCompositeSlider(col, row++, "Levelup Exp", 0.00f, 1.00f, AddLevelupExpRate, "{1}% ({0}P)");

            col = 16; row = 0;
            uiCustom.AddText(col, row++, "Systems:").Format(null, 17, FontStyle.Italic).Align(TextAnchor.MiddleRight);
            tglSysHideSave = uiCustom.AddCompositeToggle(col, row++, "Hide Save Button", HideSaveButton, "({0}P)");
            tglSysHideReload = uiCustom.AddCompositeToggle(col, row++, "Hide Reload Button", HideReloadButton, "({0}P)");
            tglSysHideBattleMap = uiCustom.AddCompositeToggle(col, row++, "Hide Battle Map", HideBattleMap, "({0}P)");
            tglSysNoRebirth = uiCustom.AddCompositeToggle(col, row++, "No Rebirth", NoRebirth, "({0}P)");
            tglSysOnelife = uiCustom.AddCompositeToggle(col, row++, "One life", Onelife, "({0}P)");
            tglSysOnlyPortalAtCityAndSect = uiCustom.AddCompositeToggle(col, row++, "Only Portal at City and Sect", OnlyPortalAtCityAndSect, "({0}P)");
            tglSysNoExpFromBattle = uiCustom.AddCompositeToggle(col, row++, "No Exp from Battles", NoExpFromBattles, "({0}P)");

            col = 30; row = 0;
            txtTotalScore = uiCustom.AddText(col, row, "Total score: {0}P").Format(Color.red, 17).Align(TextAnchor.MiddleRight);
            uiCustom.AddButton(col, row += 2, () => SetLevel(0), "Default");
            uiCustom.AddButton(col, row += 2, () => SetLevel(1), "Level 1");
            uiCustom.AddButton(col, row += 2, () => SetLevel(2), "Level 2");
            uiCustom.AddButton(col, row += 2, () => SetLevel(3), "Level 3");
            uiCustom.AddButton(col, row += 2, () => SetLevel(4), "Level 4");
            uiCustom.AddButton(col, row += 2, () => SetLevel(5), "Level 5");
            uiCustom.AddButton(col, row += 2, () => SetLevel(6), "Level 6");
            uiCustom.AddButton(col, row += 2, () => SetLevel(7), "Level 7");
            uiCustom.AddButton(col, row += 2, () => SetLevel(8), "Level 8");
            uiCustom.AddButton(col, row += 2, () => SetLevel(9), "Level 9");
            uiCustom.AddButton(col, row += 2, () => SetLevel(10), "Level 10");
            uiCustom.AddText(14, 26, "You have to start a new game to apply these configs!").Format(Color.red, 17);

            SetWork();
        }

        private void SetWork()
        {
            foreach (var wk in ScoreCalculator)
            {
                var item = wk.Comp.Invoke();
                item.ItemWork = wk;
            }
        }

        private void SetLevel(int level)
        {
            (slMonstAtk.MainComponent as UIItemBase.UIItemSlider).SetPercent(level * 0.10000f);
            (slMonstDef.MainComponent as UIItemBase.UIItemSlider).SetPercent(level * 0.10000f);
            (slMonstHp.MainComponent as UIItemBase.UIItemSlider).SetPercent(level * 0.10000f, 0f);
            (slMonstBasis.MainComponent as UIItemBase.UIItemSlider).SetPercent(level * 0.10000f);
            (slMonstSpecialRate.MainComponent as UIItemBase.UIItemSlider).SetPercent(level * 0.10000f);
            (slEcoTaxRate.MainComponent as UIItemBase.UIItemSlider).SetPercent(level * 0.10000f);
            (slEcoInfRate.MainComponent as UIItemBase.UIItemSlider).SetPercent(level * 0.10000f);
            (slEcoBuildingCost.MainComponent as UIItemBase.UIItemSlider).SetPercent(level * 0.10000f);
            (slEcoBankAccCost.MainComponent as UIItemBase.UIItemSlider).SetPercent(level * 0.10000f);
            (slEcoBankFee.MainComponent as UIItemBase.UIItemSlider).SetPercent(level * 0.10000f);
            (slNpcGrowRate.MainComponent as UIItemBase.UIItemSlider).SetPercent(level * 0.10000f);
            (slMiscLevelupExp.MainComponent as UIItemBase.UIItemSlider).SetPercent(level * 0.10000f);
            tglSysHideBattleMap.Set(level > 1);
            tglSysHideSave.Set(level > 2);
            tglSysNoExpFromBattle.Set(level > 4);
            tglSysOnlyPortalAtCityAndSect.Set(level > 5);
            tglSysHideReload.Set(level > 7);
            tglSysNoRebirth.Set(level > 8);
            tglSysOnelife.Set(level > 9);
        }

        [ErrorIgnore]
        public override void OnTimeUpdate()
        {
            base.OnTimeUpdate();
            uiCustom.UpdateUI();
            txtTotalScore.Set($"Total score: {CalSMTotalScore()}P");
        }

        private void SetSMConfigs()
        {
            AddAtkRate = slMonstAtk.Get().Parse<float>();
            AddDefRate = slMonstDef.Get().Parse<float>();
            AddHpRate = slMonstHp.Get().Parse<float>();
            AddBasisRate = slMonstBasis.Get().Parse<float>();
            AddSpecialMonsterRate = slMonstSpecialRate.Get().Parse<float>();
            AddTaxRate = slEcoTaxRate.Get().Parse<float>();
            AddInflationRate = slEcoInfRate.Get().Parse<float>();
            AddBuildingCostRate = slEcoBuildingCost.Get().Parse<float>();
            AddBankAccountCostRate = slEcoBankAccCost.Get().Parse<float>();
            AddBankFee = slEcoBankFee.Get().Parse<float>();
            AddNpcGrowRate = slNpcGrowRate.Get().Parse<float>();
            AddLevelupExpRate = slMiscLevelupExp.Get().Parse<float>();
            HideSaveButton = tglSysHideSave.Get().Parse<bool>();
            HideReloadButton = tglSysHideReload.Get().Parse<bool>();
            HideBattleMap = tglSysHideBattleMap.Get().Parse<bool>();
            NoRebirth = tglSysNoRebirth.Get().Parse<bool>();
            Onelife = tglSysOnelife.Get().Parse<bool>();
            OnlyPortalAtCityAndSect = tglSysOnlyPortalAtCityAndSect.Get().Parse<bool>();
            NoExpFromBattles = tglSysNoExpFromBattle.Get().Parse<bool>();
            CacheHelper.Save();
        }

        public static int CalCompScore(UIItemBase comp)
        {
            var x = ScoreCalculator.FirstOrDefault(k => k.Comp.Invoke() == comp);
            if (x == null || !x.Cond.Invoke(comp))
                return 0;
            return x.Cal.Invoke(comp);
        }

        public static bool IsEnableComp(UIItemBase comp)
        {
            var x = ScoreCalculator.FirstOrDefault(k => k.Comp.Invoke() == comp);
            if (x == null)
                return false;
            return x != null && x.Cond.Invoke(comp) && comp.IsEnable();
        }

        public static int CalSMTotalScore()
        {
            var rs = 0;
            foreach (var item in ScoreCalculator)
            {
                var comp = item.Comp.Invoke();
                if (comp != null && IsEnableComp(comp))
                {
                    rs += CalCompScore(comp);
                }
            }
            return rs;
        }
    }
}