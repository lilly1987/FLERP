using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FLERP
{
    [BepInPlugin("Game.Lilly.Plugin", "Lilly", "1.0")]
    public class Lilly : BaseUnityPlugin
    {
        public static ManualLogSource logger;

        static Harmony harmony;

        public ConfigEntry<BepInEx.Configuration.KeyboardShortcut> ShowCounter;

        private ConfigEntry<bool> isGUIOn;
        private ConfigEntry<bool> isOpen;
        private ConfigEntry<float> uiW;
        private ConfigEntry<float> uiH;

        public int windowId = 542;
        public Rect windowRect;

        public string windowName = "";
        public string FullName = "Plugin";
        public string ShortName = "P";

        GUILayoutOption h;
        GUILayoutOption w;
        public Vector2 scrollPosition;


        static Type buildGadgetMenu;
        public static ConfigEntry<int> rerollCost;
        //public static int rerollCost = 1;
        public static ConfigEntry<int> baseGadgetCount;
        public static ConfigEntry<int> rerollCostItem;
        public static ConfigEntry<int> addGainXP;
        public static ConfigEntry<float> pickupRadius;
        public static ConfigEntry<bool> removeFromShop;
        public static ConfigEntry<bool> customShop;

        public static ConfigEntry<bool> eMultOn;

        public static ConfigEntry<float> eHealthMult;
        public static ConfigEntry<float> eArmorMult;
        public static ConfigEntry<float> eDamageMult;
        public static ConfigEntry<float> eSpeedMult;
        
        public static ConfigEntry<float> eHealthAdd;
        public static ConfigEntry<float> eArmorAdd;
        public static ConfigEntry<float> eDamageAdd;
        public static ConfigEntry<float> eSpeedAdd;

        public void Awake()
        {
            logger = Logger;
            Logger.LogMessage("Awake");

            ShowCounter = Config.Bind("GUI", "isGUIOnKey", new KeyboardShortcut(KeyCode.Keypad0));// 이건 단축키

            isGUIOn = Config.Bind("GUI", "isGUIOn", true);
            isOpen = Config.Bind("GUI", "isOpen", true);
            isOpen.SettingChanged += IsOpen_SettingChanged;
            uiW = Config.Bind("GUI", "uiW", 300f);
            uiH = Config.Bind("GUI", "uiH", 600f);

            if (isOpen.Value)
                windowRect = new Rect(Screen.width - 65, 0, uiW.Value, 800);
            else
                windowRect = new Rect(Screen.width - uiW.Value, 0, uiW.Value, 800);

            IsOpen_SettingChanged(null, null);

            // BuildGadgetMenu.rerollCost : int @0400067A
            //buildGadgetMenu = AccessTools.TypeByName("BuildGadgetMenu");
            buildGadgetMenu = typeof(BuildGadgetMenu);
            
            rerollCost = Config.Bind("Game", "rerollCost", 1);
            rerollCostItem = Config.Bind("Game", "rerollCostItem", 4);
            baseGadgetCount = Config.Bind("Game", "baseGadgetCount", 8);
            removeFromShop = Config.Bind("Game", "removeFromShop", false);
            customShop = Config.Bind("Game", "customShop", true);
            
            
            pickupRadius = Config.Bind("Game", "pickupRadius", 50f);
            addGainXP = Config.Bind("Game", "addGainXP", 9);

            eMultOn = Config.Bind("Game", "eMultOn", false);
            //eHealthMult = Config.Bind("Game", "eMult", 2f);

            eHealthMult = Config.Bind("Game", "eHealthMult", 2f);
            eArmorMult = Config.Bind("Game", "eArmorMult", 2f);
            eDamageMult = Config.Bind("Game", "eDamageMult", 2f);
            eSpeedMult = Config.Bind("Game", "eSpeedMult", 2f);
            
            eHealthAdd = Config.Bind("Game", "eHealthAdd", 1f);
            eArmorAdd = Config.Bind("Game", "eArmorAdd", 1f);
            eDamageAdd = Config.Bind("Game", "eDamageAdd", 1f);
            eSpeedAdd = Config.Bind("Game", "eSpeedAdd", 1f);
        }

        public void IsOpen_SettingChanged(object sender, EventArgs e)
        {
            logger.LogInfo($"IsOpen_SettingChanged {isOpen.Value} , {isGUIOn.Value},{windowRect.x} ");
            if (isOpen.Value)
            {
                h = GUILayout.Height(uiH.Value);
                w = GUILayout.Width(uiW.Value);
                windowName = FullName;
                windowRect.x -= (uiW.Value - 64);
            }
            else
            {
                h = GUILayout.Height(40);
                w = GUILayout.Width(60);
                windowName = ShortName;
                windowRect.x += (uiW.Value - 64);
            }
        }

        public void OnEnable()
        {
            Logger.LogWarning("OnEnable");
            // 하모니 패치
            harmony = Harmony.CreateAndPatchAll(typeof(Lilly));
        }

        public void Update()
        {
            if (ShowCounter.Value.IsUp())// 단축키가 일치할때
            {
                isGUIOn.Value = !isGUIOn.Value;// 보이거나 안보이게. 이런 배열이였네 지웠음
                                               //MyLog.LogMessage("IsUp", ShowCounter.Value.MainKey);
            }
        }


        public void OnGUI()
        {
            if (!isGUIOn.Value)
                return;

            windowRect.x = Mathf.Clamp(windowRect.x, -windowRect.width + 4, Screen.width - 4);
            windowRect.y = Mathf.Clamp(windowRect.y, -windowRect.height + 4, Screen.height - 4);
            windowRect = GUILayout.Window(windowId, windowRect, WindowFunction, windowName, w, h);
        }

        public virtual void WindowFunction(int id)
        {
            GUI.enabled = true; // 기능 클릭 가능

            GUILayout.BeginHorizontal();// 가로 정렬
                                        // 라벨 추가
                                        //GUILayout.Label(windowName, GUILayout.Height(20));
                                        // 안쓰는 공간이 생기더라도 다른 기능으로 꽉 채우지 않고 빈공간 만들기
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { isOpen.Value = !isOpen.Value; }
            if (GUILayout.Button("x", GUILayout.Width(20), GUILayout.Height(20))) { isGUIOn.Value = false; }
            GUI.changed = false;

            GUILayout.EndHorizontal();// 가로 정렬 끝

            if (!isOpen.Value) // 닫혔을때
            {
            }
            else // 열렸을때
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);

                //

                GUILayout.Label("=== Shop ===");

                if (GUILayout.Button($"custom Shop : {customShop.Value}")) { customShop.Value = !customShop.Value; }
                if (GUILayout.Button($"removeFromShop : {removeFromShop.Value}")) { removeFromShop.Value = !removeFromShop.Value; }

                //
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Gadget rerollCost : {rerollCost.Value}");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { AccessTools.Field(buildGadgetMenu, "rerollCost").SetValue(BuildGadgetMenu.instance, --rerollCost.Value); }
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { AccessTools.Field(buildGadgetMenu, "rerollCost").SetValue(BuildGadgetMenu.instance, ++rerollCost.Value); }
                GUILayout.EndHorizontal();
                //             
                if (GadgetManager.instance != null)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"MaxGadgetCount : {GadgetManager.instance.MaxGadgetCount}");
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { GadgetManager.instance.MaxGadgetCount--; }
                    if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { GadgetManager.instance.MaxGadgetCount++; }
                    GUILayout.EndHorizontal();
                }
                //
                GUILayout.BeginHorizontal();
                GUILayout.Label($"baseGadgetCount : {baseGadgetCount.Value}");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { baseGadgetCount.Value--; }
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { baseGadgetCount.Value++; }
                GUILayout.EndHorizontal();
                //

                GUILayout.Label("=== Shop ===");

                GUILayout.BeginHorizontal();
                GUILayout.Label($"Item rerollCost -= : {rerollCostItem.Value}");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { rerollCostItem.Value--; }
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { rerollCostItem.Value++; }
                GUILayout.EndHorizontal();
                //

                GUILayout.Label("=== XP ===");

                GUILayout.BeginHorizontal();
                GUILayout.Label($"pickupRadius : {pickupRadius.Value}");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { pickupRadius.Value -= 5f; }
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { pickupRadius.Value += 5f; }
                GUILayout.EndHorizontal();
                //

                GUILayout.BeginHorizontal();
                GUILayout.Label($"addGainXP : {addGainXP.Value}");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { addGainXP.Value--; }
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { addGainXP.Value++; }
                GUILayout.EndHorizontal();
                //

                GUILayout.Label("=== Enemy ===");
                               
                                
                GUILayout.Label("--- when Spawn ---");
                GUILayout.Label("HP=HealthMult * (1f + 0.8f * currNgBuffRatio)*eHealthMult");
                if (GUILayout.Button($"Mult apply: {eMultOn.Value}")) { eMultOn.Value = !eMultOn.Value; }
                GUILayout.BeginHorizontal();
                GUILayout.Label($"eHealthMult : {eHealthMult.Value}");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("1", GUILayout.Width(20), GUILayout.Height(20))) { eHealthMult.Value = 1f; }
                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { eHealthMult.Value -= 0.1f; }
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { eHealthMult.Value += 0.1f; }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label($"eArmorMult : {eArmorMult.Value}");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("1", GUILayout.Width(20), GUILayout.Height(20))) { eArmorMult.Value = 1f; }
                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { eArmorMult.Value -= 0.1f; }
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { eArmorMult.Value += 0.1f; }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label($"eDamageMult : {eDamageMult.Value}");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("1", GUILayout.Width(20), GUILayout.Height(20))) { eDamageMult.Value = 1f; }
                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { eDamageMult.Value -= 0.1f; }
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { eDamageMult.Value += 0.1f; }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label($"eSpeedMult : {eSpeedMult.Value}");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("1", GUILayout.Width(20), GUILayout.Height(20))) { eSpeedMult.Value = 1f; }
                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { eSpeedMult.Value -= 0.1f; }
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { eSpeedMult.Value += 0.1f; }
                GUILayout.EndHorizontal();

                GUILayout.Label("--- edit property ---");
                GUILayout.Label("HealthMult reset when game start");
                GUILayout.Label("HealthMult-=eHealthAdd");
                //GUILayout.Label("HealthMult+=eHealthAdd");
                //GUILayout.BeginHorizontal();
                //GUILayout.Label($"eHealthAdd : {eHealthAdd.Value}");
                //GUILayout.FlexibleSpace();
                //if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { eHealthAdd.Value -= 0.1f; }
                //if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { eHealthAdd.Value += 0.1f; }
                //GUILayout.EndHorizontal();
                //GUILayout.BeginHorizontal();
                //GUILayout.Label($"eArmorAdd : {eArmorAdd.Value}");
                //GUILayout.FlexibleSpace();
                //if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { eArmorAdd.Value -= 0.1f; }
                //if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { eArmorAdd.Value += 0.1f; }
                //GUILayout.EndHorizontal();
                //GUILayout.BeginHorizontal();
                //GUILayout.Label($"eDamageAdd : {eDamageAdd.Value}");
                //GUILayout.FlexibleSpace();
                //if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { eDamageAdd.Value -= 0.1f; }
                //if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { eDamageAdd.Value += 0.1f; }
                //GUILayout.EndHorizontal();
                //GUILayout.BeginHorizontal();
                //GUILayout.Label($"eSpeedAdd : {eSpeedAdd.Value}");
                //GUILayout.FlexibleSpace();
                //if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { eSpeedAdd.Value -= 0.1f; }
                //if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { eSpeedAdd.Value += 0.1f; }
                //GUILayout.EndHorizontal();

                if (EnemySpawner.instance != null)
                {

                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"HealthMult : {EnemySpawner.instance.HealthMult}");
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { EnemySpawner.instance.HealthMult -= eHealthAdd.Value; }
                    if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { EnemySpawner.instance.HealthMult += eHealthAdd.Value; }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"ArmorMult : {EnemySpawner.instance.ArmorMult}");
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { EnemySpawner.instance.HealthMult -= eArmorAdd.Value ; }
                    if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { EnemySpawner.instance.HealthMult += eArmorAdd.Value ; }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"DamageMult : {EnemySpawner.instance.DamageMult}");
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { EnemySpawner.instance.HealthMult -= eDamageAdd.Value ; }
                    if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { EnemySpawner.instance.HealthMult += eDamageAdd.Value ; }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"SpeedMult : {EnemySpawner.instance.SpeedMult}");
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { EnemySpawner.instance.HealthMult -= eSpeedAdd.Value ; }
                    if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { EnemySpawner.instance.HealthMult += eSpeedAdd.Value ; }
                    GUILayout.EndHorizontal();

                }
                else
                {
                    GUILayout.Label("EnemySpawner is bull");
                }

                GUILayout.EndScrollView();
            }
            GUI.enabled = true;
            GUI.DragWindow(); // 창 드레그 가능하게 해줌. 마지막에만 넣어야함
        }



        public void OnDisable()
        {
            Logger.LogWarning("OnDisable");
            harmony?.UnpatchSelf();

        }

        [HarmonyPatch(typeof(BuildGadgetMenu), MethodType.Constructor)]
        [HarmonyPostfix]
        public static void BuildGadgetMenuCtor(ref int ___rerollCost)
        {
            logger.LogWarning($"BuildGadgetMenu.ctor {___rerollCost}");
            //rerollCost = ___rerollCost;
            ___rerollCost = rerollCost.Value;
            //rerollCost=(int) AccessTools.Field(buildGadgetMenu, "rerollCost").GetValue(BuildGadgetMenu.instance);
        }

        [HarmonyPatch(typeof(GadgetManager), "SetMaxGadgetCount")]
        [HarmonyPrefix]
        //public void SetMaxGadgetCount(int newGameLevel)
        public static bool SetMaxGadgetCount(int newGameLevel)
        {
            logger.LogWarning($"SetMaxGadgetCount {newGameLevel}");
            GadgetManager.instance.MaxGadgetCount = baseGadgetCount.Value + newGameLevel;
            return false;
        }

        [HarmonyPatch(typeof(XPPicker), MethodType.Constructor)]
        [HarmonyPostfix]
        public static void XPPickerCtor(ref float ___pickupRadius)
        {
            //logger.LogWarning($"XPPicker.ctor {___pickupRadius}");
            ___pickupRadius = pickupRadius.Value;
        }

        static Dictionary<GadgetSO, AGadget> upgradableGadgets;

        [HarmonyPatch(typeof(GadgetManager), MethodType.Constructor)]
        [HarmonyPostfix]
        public static void GadgetManagerCtor(ref Dictionary<GadgetSO, AGadget> ___upgradableGadgets)
        {
            logger.LogWarning($"GadgetManager.ctor");
            upgradableGadgets = ___upgradableGadgets;
        }

        [HarmonyPatch(typeof(ItemWindow), "OnReroll")]
        [HarmonyPostfix]
        public static void OnReroll(ref int ___rerollCost)
        {
            logger.LogWarning($"OnReroll {___rerollCost}");
            ___rerollCost -= rerollCostItem.Value;
        }

        [HarmonyPatch(typeof(BuildGadgetMenu), "RemoveFromShop")]
        [HarmonyPrefix]
        public static bool RemoveFromShop()
        {
            logger.LogWarning($"RemoveFromShop");
            if (!removeFromShop.Value)
            {
                BuildGadgetMenu.instance.UpdateShopUI();
            }
            return removeFromShop.Value;
        }

        [HarmonyPatch(typeof(BuildGadgetMenu), "RefreshShop")]
        [HarmonyPrefix]
        public static bool RefreshShop(ShopTierSO ___shopTierSO, int ___shopTier, GadgetSO[] ___currShopGadgets, List<BuildGadgetItem> ___shopItems)
        {
            logger.LogWarning($"RefreshShop");
            if (customShop.Value)
            {
                ShopTier shopTier = ___shopTierSO.shopTiers[___shopTier];
                //for (int i = 0; i < 5; i++)
                //{
                //    int tier = shopTier.RollProbability();
                //    ___currShopGadgets[i] = GadgetData.instance.GetRandomGadget(tier);
                //}
                ___currShopGadgets[0] = GadgetData.instance.GetRandomGadget(0);
                ___currShopGadgets[1] = GadgetData.instance.GetRandomGadget(1);
                ___currShopGadgets[2] = GadgetData.instance.GetRandomGadget(2);
                ___currShopGadgets[3] = GadgetData.instance.GetRandomGadget(3);
                if (upgradableGadgets.Count > 0)
                {
                    ___currShopGadgets[4] = upgradableGadgets.ElementAt(UnityEngine.Random.Range(0, upgradableGadgets.Count)).Key;
                }
                else
                {
                    int tier = shopTier.RollProbability();
                    ___currShopGadgets[4] = GadgetData.instance.GetRandomGadget(tier);
                }
                // GadgetManager.instance.GetGadget(this.currHover.CurrGadget);
                foreach (BuildGadgetItem buildGadgetItem in ___shopItems)
                {
                    buildGadgetItem.ApplySpring();
                }
                BuildGadgetMenu.instance.UpdateShopUI();
            }
            return !customShop.Value;
        }

        [HarmonyPatch(typeof(XPManager), "GainXP")]
        [HarmonyPrefix]
        public static void GainXP(ref int ___currXP, int xpId)
        {
            //logger.LogWarning($"RemoveFromShop");
            if (GameEnder.GameOver)
            {
                return;
            }
            if (xpId == 0)
            {
                ___currXP += addGainXP.Value;
            }
        }
        /*
        [HarmonyPatch(typeof(EnemySpawner), "SpawnEnemy")]
        [HarmonyPostfix]
        public static void HealthMult(ref List<GameObject> __result, float ___currNgBuffRatio)
        {
            if (!eMultOn.Value)
            {
                return ;
            }
            foreach (GameObject gameObject in __result)
            {
                gameObject.GetComponent<HealthManager>().SetMaxHealthModifier(EnemySpawner.instance.HealthMult * (1f + 0.8f * ___currNgBuffRatio)* eHealthMult.Value);
                gameObject.GetComponent<HealthUnit>().ArmorMult*=eArmorMult.Value;
                gameObject.GetComponent<Steerer>().MoveSpeedMult*=eSpeedMult.Value;
                gameObject.GetComponent<AEnemy>().DamageMult*=eDamageMult.Value;
            }
        }
        */

        [HarmonyPatch(typeof(HealthManager), "SetMaxHealthModifier")]
        [HarmonyPrefix]
        public static void SetMaxHealthModifier(ref float modifier)
        {
            if (!eMultOn.Value)
            {
                return;
            }
            modifier *= eHealthMult.Value;
        }
        

        [HarmonyPatch(typeof(HealthUnit), "ArmorMult",MethodType.Setter)]
        [HarmonyPrefix]
        public static void SetArmorMult(ref float __0)
        {
            if (!eMultOn.Value)
            {
                return;
            }
            __0 *= eArmorMult.Value;
        }
        

        [HarmonyPatch(typeof(Steerer), "MoveSpeedMult", MethodType.Setter)]
        [HarmonyPrefix]
        public static void SetMoveSpeedMult(ref float __0)
        {
            if (!eMultOn.Value)
            {
                return;
            }
            __0 *= eSpeedMult.Value;
        }
        
        [HarmonyPatch(typeof(AEnemy), "DamageMult", MethodType.Setter)]
        [HarmonyPrefix]
        public static void SetDamageMult(ref float __0)
        {
            if (!eMultOn.Value)
            {
                return;
            }
            __0 *= eDamageMult.Value;
        }

    }
}
