﻿using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine.Events;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using MOD_nE7UL2.Enum;
using static DataBuildTown;
using System;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Sell item
    /// </summary>
    [Cache(ModConst.REAL_MARKET_EVENT)]
    public class RealMarketEvent : ModEvent
    {
        public static float MIN_RATE
        {
            get
            {
                return ModMain.ModObj.InGameCustomSettings.RealMarketConfigs.MinSellRate;
            }
        }
        public static float MAX_RATE
        {
            get
            {
                return ModMain.ModObj.InGameCustomSettings.RealMarketConfigs.MaxSellRate;
            }
        }

        private UIPropSell uiPropSell;
        private MapBuildBase curMainTown;
        private Text txtMarketST;
        private Text txtPrice2;
        private Text txtWarningMsg;
        private Text txtInfo;

        public IDictionary<string, float> MarketPriceRate { get; set; } = new Dictionary<string, float>();

        public override void OnLoadGame()
        {
            base.OnLoadGame();
            foreach (var town in g.world.build.GetBuilds().ToArray().Where(x => x.allBuildSub.ContainsKey(MapBuildSubType.TownMarketPill)))
            {
                if (!MarketPriceRate.ContainsKey(town.buildData.id))
                {
                    MarketPriceRate.Add(town.buildData.id, 100.00f);
                }
            }
        }

        public override void OnMonthly()
        {
            base.OnMonthly();
            var eventSellRate = ModMain.ModObj.InGameCustomSettings.RealMarketConfigs.GetAddSellRate();
            foreach (var town in g.world.build.GetBuilds().ToArray().Where(x => x.allBuildSub.ContainsKey(MapBuildSubType.TownMarketPill)))
            {
                MarketPriceRate[town.buildData.id] = CommonTool.Random(MIN_RATE + eventSellRate, MAX_RATE + eventSellRate) + (GetMerchantIncRate() * 100.0f);
            }
        }

        [EventCondition]
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            uiPropSell = MonoBehaviour.FindObjectOfType<UIPropSell>();
            curMainTown = g.world.build.GetBuild(g.world.playerUnit.data.unitData.GetPoint());
            if (uiPropSell != null && curMainTown != null)
            {
                if (txtMarketST == null)
                {
                    //add component
                    txtMarketST = MonoBehaviour.Instantiate(uiPropSell.textMoney, uiPropSell.transform, false);
                    txtMarketST.transform.position = new Vector3(uiPropSell.textMoney.transform.position.x, uiPropSell.textMoney.transform.position.y - 0.2f);
                    txtMarketST.verticalOverflow = VerticalWrapMode.Overflow;
                    txtMarketST.horizontalOverflow = HorizontalWrapMode.Overflow;

                    var merchantIncRate = GetMerchantIncRate();
                    txtInfo = MonoBehaviour.Instantiate(uiPropSell.textMoney, uiPropSell.transform, false);
                    txtInfo.text = $"Price rate: {MarketPriceRate[curMainTown.buildData.id]:0.00}%";
                    if (merchantIncRate > 0.00f)
                        txtInfo.text += $" (Merchant +{merchantIncRate * 100.0f:0.00}%)";
                    txtInfo.transform.position = new Vector3(uiPropSell.textMoney.transform.position.x, uiPropSell.textMoney.transform.position.y - 0.4f);
                    txtInfo.verticalOverflow = VerticalWrapMode.Overflow;
                    txtInfo.horizontalOverflow = HorizontalWrapMode.Overflow;
                    txtInfo.color = Color.red;

                    txtPrice2 = MonoBehaviour.Instantiate(uiPropSell.textPrice, uiPropSell.transform, false);
                    txtPrice2.transform.position = new Vector3(uiPropSell.textPrice.transform.position.x, uiPropSell.textPrice.transform.position.y - 0.2f);
                    txtPrice2.verticalOverflow = VerticalWrapMode.Overflow;
                    txtPrice2.horizontalOverflow = HorizontalWrapMode.Overflow;

                    txtWarningMsg = MonoBehaviour.Instantiate(uiPropSell.textPrice, uiPropSell.transform, false);
                    txtWarningMsg.text = $"Over price";
                    txtWarningMsg.transform.position = new Vector3(uiPropSell.btnOK.transform.position.x, uiPropSell.btnOK.transform.position.y);
                    txtWarningMsg.verticalOverflow = VerticalWrapMode.Overflow;
                    txtWarningMsg.horizontalOverflow = HorizontalWrapMode.Overflow;
                    txtWarningMsg.color = Color.red;
                    txtWarningMsg.gameObject.SetActive(false);

                    uiPropSell.btnOK.onClick.m_Calls.Clear();
                    uiPropSell.btnOK.onClick.m_Calls.m_RuntimeCalls.Insert(0, new InvokableCall((UnityAction)SellEvent));
                }
            }
        }

        public override void OnCloseUIEnd(CloseUIEnd e)
        {
            base.OnCloseUIEnd(e);
            uiPropSell = MonoBehaviour.FindObjectOfType<UIPropSell>();
            curMainTown = g.world.build.GetBuild(g.world.playerUnit.data.unitData.GetPoint());
            if (uiPropSell == null || curMainTown == null)
            {
                txtMarketST = null;
                txtPrice2 = null;
                txtWarningMsg = null;
                txtInfo = null;
            }
        }

        [ErrorIgnore]
        [EventCondition]
        public override void OnTimeUpdate()
        {
            base.OnTimeUpdate();
            if (uiPropSell != null && curMainTown != null && txtMarketST != null)
            {
                var budget = MapBuildPropertyEvent.GetBuildProperty(curMainTown);
                var totalPrice = GetTotalPrice();
                var cashback = (totalPrice * ((MarketPriceRate[curMainTown.buildData.id] - 100.00f) / 100.00f)).Parse<int>();
                txtMarketST.text = $"Market: {budget} Spirit Stones";
                txtPrice2.text = $"/{budget} Spirit Stones";
                uiPropSell.textMoney.text = $"Owned: {g.world.playerUnit.GetUnitMoney()} Spirit Stones";
                uiPropSell.textPrice.text = $"Total: {totalPrice + cashback} ({cashback})";
                uiPropSell.btnOK.gameObject.SetActive(totalPrice <= budget);
                txtWarningMsg.gameObject.SetActive(totalPrice > budget);
            }
        }

        private void SellEvent()
        {
            var totalPrice = GetTotalPrice();
            var cashback = (totalPrice * ((MarketPriceRate[curMainTown.buildData.id] - 100.00f) / 100.00f)).Parse<int>();
            g.world.playerUnit.AddUnitMoney((totalPrice + cashback).Parse<int>());
            MapBuildPropertyEvent.AddBuildProperty(curMainTown, -totalPrice);

            foreach (var item in uiPropSell.selectProps.allProps.ToArray())
            {
                g.world.playerUnit.data.unitData.propData.DelProps(item.soleID, item.propsCount);
                uiPropSell.selectProps.allProps.Remove(item);
            }

            uiPropSell.UpdateHasList();
            uiPropSell.UpdateSellList();
            uiPropSell.UpdateTitle();
        }

        private long GetTotalPrice()
        {
            return uiPropSell.selectProps.allProps.ToArray().Sum(x => x.propsInfoBase.sale.Parse<long>() * x.propsCount);
        }

        private float GetMerchantIncRate()
        {
            var merchantLvl = MerchantLuckEnum.Merchant.GetCurLevel(g.world.playerUnit);
            var merchantIncRate = 0.00f;
            if (merchantLvl > 0)
                merchantIncRate += merchantLvl * MerchantLuckEnum.Merchant.IncSellValueEachLvl;
            var uType = UnitTypeEvent.GetUnitTypeEnum(g.world.playerUnit);
            if (uType == UnitTypeEnum.Merchant)
                merchantIncRate += uType.CustomLuck.CustomEffects[ModConst.UTYPE_LUCK_EFX_SELL_VALUE].Value0.Parse<float>();
            return merchantIncRate;
        }
    }
}
