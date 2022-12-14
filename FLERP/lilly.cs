using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FLERP
{
    [BepInPlugin("Game.Lilly.Plugin", "Lilly", "1.1.2.3")]
    public class Lilly : BaseUnityPlugin
    {
        //=======================================================

        public static ManualLogSource logger;

        public static Harmony harmony;

        public ConfigEntry<BepInEx.Configuration.KeyboardShortcut> ShowCounter;
        public ConfigEntry<BepInEx.Configuration.KeyboardShortcut> ShowCounter2;

        public ConfigEntry<bool> isGUIOn;
        public ConfigEntry<bool> isOpen;
        public ConfigEntry<float> uiW;
        public ConfigEntry<float> uiH;

        public int windowId = 542;
        public Rect windowRect;

        public string title = "";
        public string windowName = "";
        public string FullName = "Plugin";
        public string ShortName = "P";

        public GUILayoutOption h;
        public GUILayoutOption w;
        public Vector2 scrollPosition;

        //=======================================================

        public static Type buildGadgetMenu;
        public static ConfigEntry<int> rerollCost;
        //public static int rerollCost = 1;
        public static ConfigEntry<int> baseGadgetCount;
        public static ConfigEntry<int> rerollCostItem;
        public static ConfigEntry<int> addGainXP;
        public static ConfigEntry<int> vItemMax;

        public static ConfigEntry<bool> addGoldNG;

        public static ConfigEntry<bool> sortChg;

        public static ConfigEntry<bool> survivedOn;
        public static ConfigEntry<int> survived;

        public static ConfigEntry<float> pickupRadius;
        public static ConfigEntry<bool> removeFromShop;
        public static ConfigEntry<bool> customShop;

        public static ConfigEntry<bool> rndPos;
        public static ConfigEntry<bool> customRandomSpawnPosition;
        public static ConfigEntry<float> crspMin;
        public static ConfigEntry<float> crspMax;

        public static ConfigEntry<bool> eMultOn;
        public static ConfigEntry<bool> eMultRndOn;

        public static ConfigEntry<float> eMultRnd;

        public static ConfigEntry<float> eHealthMult;
        public static ConfigEntry<float> eArmorMult;
        public static ConfigEntry<float> eDamageMult;
        public static ConfigEntry<float> eSpeedMult;

        //public static ConfigEntry<float> eHealthAdd;
        //public static ConfigEntry<float> eArmorAdd;
        //public static ConfigEntry<float> eDamageAdd;
        //public static ConfigEntry<float> eSpeedAdd;

        public static ConfigEntry<float> eSizeMult;
        public static ConfigEntry<int> eQuantityMult;

        public static ConfigEntry<float> mHealthMult;

        public static ConfigEntry<float> interval1;
        public static ConfigEntry<float> interval2;
        //public const float speed = 1 / 8f;

        public static CodeMatch v1200 = new CodeMatch(OpCodes.Ldc_I4, 1200);
        public static CodeMatch v10 = new CodeMatch(OpCodes.Ldc_I4_S, (SByte)10);
        public static CodeInstruction vSurvived;
        public static CodeInstruction vItem;

        static XPPicker xPPicker;
        static System.Reflection.FieldInfo pickupRadius_;

        static float eMultRndMin;
        static float eMultRndMax;

        public void Awake()
        {
            logger = Logger;
            Logger.LogMessage("Awake");

            ShowCounter = Config.Bind("GUI", "isGUIOnKey", new KeyboardShortcut(KeyCode.Keypad0));// 이건 단축키
            ShowCounter2 = Config.Bind("GUI", "isOpenKey", new KeyboardShortcut(KeyCode.KeypadPeriod));// 이건 단축키

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

            //=======================================================
            try
            {

                // BuildGadgetMenu.rerollCost : int @0400067A
                //buildGadgetMenu = AccessTools.TypeByName("BuildGadgetMenu");
                buildGadgetMenu = typeof(BuildGadgetMenu);

                Logger.LogMessage("play");

                sortChg = Config.Bind("Game", "sortChg", true);

                addGoldNG = Config.Bind("Game", "addGoldNG", true);

                survivedOn = Config.Bind("Game", "survivedOn", true);
                survived = Config.Bind("Game", "survived", 0);

                vItemMax = Config.Bind("Game", "vItemMax", 20);

                Logger.LogMessage("shop");

                rerollCost = Config.Bind("Game", "rerollCost", 1);
                rerollCostItem = Config.Bind("Game", "rerollCostItem", 4);
                baseGadgetCount = Config.Bind("Game", "baseGadgetCount", 8);
                removeFromShop = Config.Bind("Game", "removeFromShop", true);

                customShop = Config.Bind("Game", "customShop", true);

                Logger.LogMessage("xp");

                pickupRadius = Config.Bind("Game", "pickupRadius", 50f);
                addGainXP = Config.Bind("Game", "addGainXP", 1);

                Logger.LogMessage("r");

                customRandomSpawnPosition = Config.Bind("Game", "customRandomSpawnPosition", true);
                rndPos = Config.Bind("Game", "rndPos", true);

                crspMin = Config.Bind("Game", "crspMin", 35f);
                crspMax = Config.Bind("Game", "crspMax", 45f);

                eMultOn = Config.Bind("Game", "eMultOn", true);
                eMultRndOn = Config.Bind("Game", "eMultRndOn", true);
                //eMult = Config.Bind("Game", "eMult", 2f);

                eMultRnd = Config.Bind("Game", "eMultRnd", 1.25f);

                Logger.LogMessage("e");

                eHealthMult = Config.Bind("Game", "eHealthMult", 1f);
                eArmorMult = Config.Bind("Game", "eArmorMult", 1f);
                eDamageMult = Config.Bind("Game", "eDamageMult", 1f);
                eSpeedMult = Config.Bind("Game", "eSpeedMult", 1f);

                eSizeMult = Config.Bind("Game", "eSizeMult", 1f);
                eQuantityMult = Config.Bind("Game", "eQuantityMult", 1);

                interval1 = Config.Bind("Game", "interval", 1f / 8f);
                interval2 = Config.Bind("Game", "interval", 1f);

                Logger.LogMessage("m");

                //eHealthAdd = Config.Bind("Game", "eHealthAdd", 1f);
                //eArmorAdd = Config.Bind("Game", "eArmorAdd", 1f);
                //eDamageAdd = Config.Bind("Game", "eDamageAdd", 1f);
                //eSpeedAdd = Config.Bind("Game", "eSpeedAdd", 1f);


                mHealthMult = Config.Bind("Game", "mHealthMult", 1f);

                Logger.LogMessage("Awake6");

                SetRndValue();

                vSurvived = new CodeInstruction(
                                                OpCodes.Ldc_I4,
                                                survived.Value
                                                );
                vItem = new CodeInstruction(
                                                OpCodes.Ldc_I4,
                                                vItemMax.Value
                                                );
                pickupRadius_ = AccessTools.Field(typeof(XPPicker), "pickupRadius");
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
            //=======================================================
        }

        public void IsOpen_SettingChanged(object sender, EventArgs e)
        {
            //logger.LogInfo($"IsOpen_SettingChanged {isOpen.Value} , {isGUIOn.Value},{windowRect.x} ");
            if (isOpen.Value)
            {
                title = ShowCounter.Value.ToString() + "," + ShowCounter2.Value.ToString();
                h = GUILayout.Height(uiH.Value);
                w = GUILayout.Width(uiW.Value);
                windowName = FullName;
                windowRect.x -= (uiW.Value - 64);
            }
            else
            {
                title = "";
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
            try
            {
                harmony = Harmony.CreateAndPatchAll(typeof(Lilly));
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }

        }

        public void Update()
        {
            if (ShowCounter.Value.IsUp())// 단축키가 일치할때
            {
                isGUIOn.Value = !isGUIOn.Value;
            }
            if (ShowCounter2.Value.IsUp())// 단축키가 일치할때
            {
                isOpen.Value = !isOpen.Value;
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
            if (isOpen.Value) GUILayout.Label(title);
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
                GUILayout.Label("=== Play ===");

                if (GUILayout.Button($"sort change : {sortChg.Value}")) { sortChg.Value = !sortChg.Value; }
                if (GUILayout.Button($"gold add NG : {addGoldNG.Value}")) { addGoldNG.Value = !addGoldNG.Value; }
                if (GUILayout.Button("gold add 100")) { MoneyManager.instance.AddMoney(100); }

                GUILayout.Label("=== Time ===");
                GUILayout.Label("need game restart");

                if (GUILayout.Button($"survived time : {survivedOn.Value}")) { survivedOn.Value = !survivedOn.Value; }

                GUILayout.BeginHorizontal();
                GUILayout.Label($"survived time : {survived.Value}");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { survived.Value -= 60; }
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { survived.Value += 60; }
                GUILayout.EndHorizontal();

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

                GUILayout.Label("=== Item ===");

                GUILayout.BeginHorizontal();
                GUILayout.Label($"Item rerollCost -= : {rerollCostItem.Value}");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { rerollCostItem.Value--; }
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { rerollCostItem.Value++; }
                GUILayout.EndHorizontal();

                GUILayout.Label("--- need restart ---");
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Item max : {vItemMax.Value}");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { vItemMax.Value--; }
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { vItemMax.Value++; }
                GUILayout.EndHorizontal();
                

                GUILayout.Label("=== XP ===");

                GUILayout.BeginHorizontal();
                GUILayout.Label($"pickupRadius : {pickupRadius.Value}");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { pickupRadius_.SetValue(xPPicker, pickupRadius.Value -= 5); }
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { pickupRadius_.SetValue(xPPicker, pickupRadius.Value += 5); }
                GUILayout.EndHorizontal();
                //

                GUILayout.BeginHorizontal();
                GUILayout.Label($"addGainXP : {addGainXP.Value}");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { addGainXP.Value--; }
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { addGainXP.Value++; }
                GUILayout.EndHorizontal();
                //

                GUILayout.Label("=== Gadget ===");

                GUILayout.Label("--- when Spawn ---");

                GUILayout.BeginHorizontal();
                GUILayout.Label($"mHealthMult : {mHealthMult.Value}");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("1", GUILayout.Width(20), GUILayout.Height(20))) { mHealthMult.Value = 1f; }
                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { mHealthMult.Value -= interval1.Value; }
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { mHealthMult.Value += interval1.Value; }
                GUILayout.EndHorizontal();

                GUILayout.Label("=== Enemy ===");

                GUILayout.Label("--- custom Position ---");

                if (GUILayout.Button($"random positon : {rndPos.Value}")) { rndPos.Value = !rndPos.Value; }
                if (GUILayout.Button($"random pool Position : {customRandomSpawnPosition.Value}")) { customRandomSpawnPosition.Value = !customRandomSpawnPosition.Value; }

                GUILayout.BeginHorizontal();
                GUILayout.Label($"min : {crspMin.Value}");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("R", GUILayout.Width(20), GUILayout.Height(20))) { crspMin.Value = (float)crspMin.DefaultValue; }
                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { crspMin.Value -= 5f; }
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { crspMin.Value += 5f; }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label($"max : {crspMax.Value}");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("R", GUILayout.Width(20), GUILayout.Height(20))) { crspMax.Value = (float)crspMax.DefaultValue; }
                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { crspMax.Value -= 5f; }
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { crspMax.Value += 5f; }
                GUILayout.EndHorizontal();


                GUILayout.Label("--- when Spawn ---");
                GUILayout.Label("ex) HP=HealthMult * (1f + 0.8f * currNgBuffRatio)*eHealthMult*Rnd(1,eMultRnd)");
                if (GUILayout.Button($"Mult apply : {eMultOn.Value}")) { eMultOn.Value = !eMultOn.Value; }

                if (GUILayout.Button($"Mult Rnd : {eMultRndOn.Value}")) { eMultRndOn.Value = !eMultRndOn.Value; }

                GUILayout.BeginHorizontal();
                GUILayout.Label($"eMultRnd : {eMultRnd.Value}");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("1", GUILayout.Width(20), GUILayout.Height(20))) { eMultRnd.Value = 1f; }
                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { eMultRnd.Value -= interval1.Value; SetRndValue(); }
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { eMultRnd.Value += interval1.Value; SetRndValue(); }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label($"eHealthMult : {eHealthMult.Value}");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("1", GUILayout.Width(20), GUILayout.Height(20))) { eHealthMult.Value = 1f; }
                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { eHealthMult.Value -= interval1.Value; }
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { eHealthMult.Value += interval1.Value; }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label($"eArmorMult : {eArmorMult.Value}");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("1", GUILayout.Width(20), GUILayout.Height(20))) { eArmorMult.Value = 1f; }
                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { eArmorMult.Value -= interval1.Value; }
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { eArmorMult.Value += interval1.Value; }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label($"eDamageMult : {eDamageMult.Value}");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("1", GUILayout.Width(20), GUILayout.Height(20))) { eDamageMult.Value = 1f; }
                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { eDamageMult.Value -= interval1.Value; }
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { eDamageMult.Value += interval1.Value; }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label($"eSpeedMult : {eSpeedMult.Value}");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("1", GUILayout.Width(20), GUILayout.Height(20))) { eSpeedMult.Value = 1f; }
                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { eSpeedMult.Value -= interval1.Value; }
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { eSpeedMult.Value += interval1.Value; }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label($"eSizeMult : {eSizeMult.Value}");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("1", GUILayout.Width(20), GUILayout.Height(20))) { eSizeMult.Value = 1f; }
                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { eSizeMult.Value -= interval1.Value; }
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { eSizeMult.Value += interval1.Value; }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label($"eQuantityMult : {eQuantityMult.Value}");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("1", GUILayout.Width(20), GUILayout.Height(20))) { eQuantityMult.Value = 1; }
                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { eQuantityMult.Value--; }
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { eQuantityMult.Value++; }
                GUILayout.EndHorizontal();

                GUILayout.Label("--- edit property ---");
                GUILayout.Label("reset when game start");
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
                    if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { EnemySpawner.instance.HealthMult -= interval2.Value; }
                    if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { EnemySpawner.instance.HealthMult += interval2.Value; }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"ArmorMult : {EnemySpawner.instance.ArmorMult}");
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { EnemySpawner.instance.HealthMult -= interval2.Value; }
                    if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { EnemySpawner.instance.HealthMult += interval2.Value; }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"DamageMult : {EnemySpawner.instance.DamageMult}");
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { EnemySpawner.instance.HealthMult -= interval2.Value; }
                    if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { EnemySpawner.instance.HealthMult += interval2.Value; }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"SpeedMult : {EnemySpawner.instance.SpeedMult}");
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20))) { EnemySpawner.instance.HealthMult -= interval2.Value; }
                    if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20))) { EnemySpawner.instance.HealthMult += interval2.Value; }
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

        private static void SetRndValue()
        {
            if (eMultRnd.Value < 1f)
            {
                if (eMultRnd.Value < interval1.Value)
                {
                    eMultRnd.Value = interval1.Value;
                }
                eMultRndMin = eMultRnd.Value;
                eMultRndMax = 1f;
            }
            else
            {
                eMultRndMin = 1f;
                eMultRndMax = eMultRnd.Value;
            }
        }

        public void OnDisable()
        {
            logger.LogWarning("OnDisable");
            harmony?.UnpatchSelf();

        }

        #region 변수 및 선언

        public static Dictionary<GadgetSO, AGadget> upgradableGadgets;

        [HarmonyPatch(typeof(GadgetManager), MethodType.Constructor)]
        [HarmonyPostfix]
        public static void GadgetManagerCtor(ref Dictionary<GadgetSO, AGadget> ___upgradableGadgets)
        {
            //logger.LogWarning($"GadgetManager.ctor");
            upgradableGadgets = ___upgradableGadgets;
        }

        private static IEnumerable<CodeInstruction> GetCodeMatcher(IEnumerable<CodeInstruction> instructions, CodeMatch codeMatches, CodeInstruction codeInstruction)
        {
            try
            {
                //logger.LogWarning($"{codeMatches?.opcodes?[0]},{codeMatches?.operands?[0]} , {codeMatches?.operands?[0].GetType().Name}");
                logger.LogWarning($"opcodes {codeMatches.opcodes.Count}");
                for (int i = 0; i < codeMatches.opcodes.Count; i++)
                {
                    logger.LogWarning($"{codeMatches.opcodes[i]}");
                }                
                logger.LogWarning($"operands {codeMatches.operands.Count}");
                for (int i = 0; i < codeMatches.operands.Count; i++)
                {
                    logger.LogWarning($"{codeMatches?.operands[i]} , {codeMatches.operands[i].GetType().Name}");
                }
                logger.LogWarning($"codeInstruction {codeInstruction?.opcode},{codeInstruction?.operand} , {codeInstruction?.operand.GetType().Name}");
            }
            catch (Exception e)
            {
                logger.LogError($"CodeMatcher1 {e}");
                return instructions;
            }
            try
            {
                var c = new CodeMatcher(instructions);
                for (int i = 0; ; i++)
                {
                    c = c.MatchForward(false, codeMatches);
                    logger.LogMessage($"CodeMatcher , {i} , {c.Pos} , {c.Length}");
                    if (c.Pos < c.Length)
                    {
                        c = c.SetInstruction(codeInstruction);
                    }
                    else
                    {
                        if (i == 0)
                        {
                            logger.LogError($"CodeMatcher not match");
                            foreach (var item in instructions)
                            {
                                logger.LogWarning($"{item.opcode},{item.operand} ");//, { codeMatches?.opcodes[0] == item.opcode} , {codeMatches?.operands[0] == item.operand} , {item.operand?.GetType().Name}");
                            }
                        }
                        return c.InstructionEnumeration();
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError($"CodeMatcher2 {e}");
                return instructions;
            }
        }

        #endregion

        #region Play

        [HarmonyPatch(typeof(GameEnder), "InitiateLoseGame")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> InitiateLoseGame(IEnumerable<CodeInstruction> instructions)
        {
            logger.LogMessage($"GameEnder.InitiateLoseGame");
            return GetCodeMatcher(instructions, v1200, vSurvived);
        }


        [HarmonyPatch(typeof(GameEnder), "LoseGameCR", MethodType.Enumerator)]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> LoseGameCR(IEnumerable<CodeInstruction> instructions)
        {
            logger.LogMessage($"GameEnder.LoseGameCR");
            return GetCodeMatcher(instructions, v1200, vSurvived);
        }
        /*
        public static bool add = false;

        [HarmonyPatch(typeof(MySortedList<GadgetSO, float>), "Add")]
        [HarmonyPrefix]
        private static void Add()
        {
            add = true;
        }
        */
        [HarmonyPatch(typeof(MySortedList<GadgetSO, float>), "SortValueAt")]
        [HarmonyPrefix]
        private static bool SortValueAt(ref int __result, ref int index, List<ValueTuple<GadgetSO, float>> ___sortedList, Dictionary<GadgetSO, int> ___indexDict)
        {

            if (sortChg.Value)
            {
                //while (index != 0 && this.sortedList[index].Item2.CompareTo(this.sortedList[index - 1].Item2) > 0)
                //{
                //    ValueTuple<T, K> value = this.sortedList[index - 1];
                //    this.sortedList[index - 1] = this.sortedList[index];
                //    this.sortedList[index] = value;
                //    this.indexDict[this.sortedList[index].Item1] = index;
                //    index--;
                //}
                //int max = index;   
                //if (add)
                //{
                //    index = 0;
                //    add=false;
                //}
                //logger.LogMessage($"SortValueAt {index} , {___sortedList[index].Item1.name} , {___sortedList[index].Item2} ");
                while (
                    index != ___sortedList.Count-1 
                    && ___sortedList[index].Item2.CompareTo(___sortedList[index + 1].Item2) > 0 
                    && ___sortedList[index + 1].Item2 != 0)
                {
                    ValueTuple<GadgetSO, float> value = ___sortedList[index + 1];
                    ___sortedList[index + 1] = ___sortedList[index];
                    ___sortedList[index] = value;
                    ___indexDict[___sortedList[index].Item1] = index;
                    //logger.LogWarning($"SortValueAt {index} , {___sortedList[index].Item1.name} , {___sortedList[index + 1].Item1.name} , {___sortedList[index].Item2} , {___sortedList[index + 1].Item2}");
                    index++;
                }

                while (index != 0 
                    && ___sortedList[index].Item2>0
                    &&(
                        ___sortedList[index].Item2.CompareTo(___sortedList[index - 1].Item2) < 0 
                        || ___sortedList[index - 1].Item2 == 0
                    )
                )
                {
                    ValueTuple<GadgetSO, float> value = ___sortedList[index - 1];
                    ___sortedList[index - 1] = ___sortedList[index];
                    ___sortedList[index] = value;
                    ___indexDict[___sortedList[index].Item1] = index;
                    //logger.LogWarning($"SortValueAt {index} , {___sortedList[index].Item1.name} , {___sortedList[index - 1].Item1.name} , {___sortedList[index].Item2} , {___sortedList[index - 1].Item2}");
                    index--;
                }
            }

            __result = index;
            //logger.LogInfo($"SortValueAt {__result} , {index} , {___sortedList.Count} , {___indexDict.Count}");
            return !sortChg.Value;
        }

        [HarmonyPatch(typeof(MoneyManager), "Awake")]
        [HarmonyPostfix]
        public static void Awake2()
        {
            if (addGoldNG.Value)
            {
                //logger.LogWarning($"MoneyManager Awake2 {MoneyManager.instance.MoneyAmount} , {OptionsManager.CurrNgLevel}");
                MoneyManager.instance.AddMoney(OptionsManager.CurrNgLevel);
            }
        }


        [HarmonyPatch(typeof(HealthManager), "SetMaxHealth")]
        [HarmonyPrefix]
        public static void SetMaxHealth(ref float maxHealth, bool ___isFriendly)
        {
            if (___isFriendly)
            {
                //logger.LogWarning($"SetMaxHealth {maxHealth}");
                maxHealth *= mHealthMult.Value;
            }
        }

        [HarmonyPatch(typeof(GadgetManager), "SetMaxGadgetCount")]
        [HarmonyPrefix]
        //public void SetMaxGadgetCount(int newGameLevel)
        public static bool SetMaxGadgetCount(int newGameLevel)
        {
            //logger.LogWarning($"SetMaxGadgetCount {newGameLevel}");
            GadgetManager.instance.MaxGadgetCount = baseGadgetCount.Value + newGameLevel;
            return false;
        }

        #endregion

        #region XP

        [HarmonyPatch(typeof(XPPicker), MethodType.Constructor)]
        [HarmonyPostfix]
        public static void XPPickerCtor(XPPicker __instance, ref float ___pickupRadius)
        {
            //logger.LogWarning($"XPPicker.ctor {___pickupRadius}");
            xPPicker = __instance;
            ___pickupRadius = pickupRadius.Value;

        }

        [HarmonyPatch(typeof(XPManager), "GainXP")]
        [HarmonyPrefix]
        public static void GainXP(ref int ___currXP, int xpId)
        {
            //logger.LogWarning($"XPManager.GainXP : {___currXP}");
            if (GameEnder.GameOver)
            {
                return;
            }
            if (xpId == 0)
            {
                ___currXP += addGainXP.Value;
            }
        }

        #endregion

        #region 아이템

        [HarmonyPatch(typeof(ItemWindow), "SetupItemScreen")]
        [HarmonyPrefix]
        public static void SetupItemScreen()
        {
            logger.LogWarning($"ItemManager.SetupItemScreen");
        }

        [HarmonyPatch(typeof(ItemManager), MethodType.Constructor)]
        [HarmonyPostfix]
        public static void ItemManagerCtor(ref PauseItem[] ___pauseItemList)
        {
            logger.LogWarning($"ItemManager.ctor");
            ___pauseItemList = new PauseItem[vItemMax.Value];
        }

        /// <summary>
        /// 실제론 작동 안함
        /// </summary>
        /// <param name="__result"></param>
        /// <param name="___numItemsAcquired"></param>
        /// <param name="___pauseItemList"></param>
        /// <returns></returns>
        [HarmonyPatch(typeof(ItemManager), "CanGetItem", MethodType.Getter)]
        //[HarmonyPatch(typeof(ItemManager), "get_CanGetItem")]
        [HarmonyPrefix]
        public static bool CanGetItem(ref bool __result,ref int ___numItemsAcquired, ref PauseItem[] ___pauseItemList)
        {
            __result = ___numItemsAcquired < vItemMax.Value; //vItemMax.Value;
            logger.LogWarning($"CanGetItem {__result} , {___numItemsAcquired} {___pauseItemList.Length}");
            return false;
        }
        /*

        [HarmonyPatch(typeof(ItemManager), "Awake")]
        [HarmonyPrefix]
        public static void ItemManagerAwake(ref int ___numItemsAcquired)
        {
            logger.LogWarning($"ItemManager.Awake {___numItemsAcquired}");            
        }

        [HarmonyPatch(typeof(ItemManager), "AcquireItem")]
        [HarmonyPrefix]
        public static void ItemManagerAcquireItem(ref int ___numItemsAcquired)
        {
            logger.LogWarning($"ItemManager.AcquireItem {___numItemsAcquired}");            
        }
        [HarmonyPatch(typeof(ItemManager), "CanGetItem", MethodType.Getter)]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> CanGetItem(IEnumerable<CodeInstruction> instructions)
        {
            logger.LogWarning($"CanGetItem");
            return GetCodeMatcher(instructions, v10, vItem);
        }
        */
        [HarmonyPatch(typeof(ItemManager), "Awake")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> ItemManagerAwake(IEnumerable<CodeInstruction> instructions)
        {
            logger.LogWarning($"ItemManagerAwake");
            return GetCodeMatcher(instructions, v10, vItem);
        }

        [HarmonyPatch(typeof(ItemManager), "AcquireItem")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AcquireItem(IEnumerable<CodeInstruction> instructions)
        {
            logger.LogWarning($"AcquireItem");
            return GetCodeMatcher(instructions, v10, vItem);
        }
        /*

        public static bool CanGetItem2()
        {
            var f=(int)AccessTools.Field(typeof(ItemManager), "numItemsAcquired").GetValue(ItemManager.instance);
            logger.LogWarning($"CanGetItem2 {f} , {vItemMax.Value}");
            return f < vItemMax.Value;
        }


       [HarmonyPatch(typeof(XPManager), "GainXP")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> GainXP(IEnumerable<CodeInstruction> instructions)
        {
            logger.LogWarning($"GainXP");
            //
            return GetCodeMatcher(instructions
                , new CodeMatch(
                    OpCodes.Callvirt,
                    //typeof(ItemManager).GetMethod("CanGetItem", System.Reflection.BindingFlags.GetProperty)
                    AccessTools.Method(typeof(ItemManager), "get_CanGetItem")
                    )
                , new CodeInstruction(
                    OpCodes.Callvirt,
                    //typeof(Lilly).GetMethod("CanGetItem2",System.Reflection.BindingFlags.all)
                    AccessTools.Method(typeof(Lilly), "CanGetItem2")
                    )
                );
        }
        */
        // =============================================

        [HarmonyPatch(typeof(ItemWindow), "OnReroll")]
        [HarmonyPostfix]
        public static void OnReroll(ref int ___rerollCost)
        {
            //logger.LogWarning($"OnReroll {___rerollCost}");
            ___rerollCost -= rerollCostItem.Value;
            //ItemWindow.instance.UpdateRerollText();
        }

        #endregion

        #region 상점

        [HarmonyPatch(typeof(BuildGadgetMenu), MethodType.Constructor)]
        [HarmonyPostfix]
        public static void BuildGadgetMenuCtor(ref int ___rerollCost)
        {
            //logger.LogWarning($"BuildGadgetMenu.ctor {___rerollCost}");
            //rerollCost = ___rerollCost;
            ___rerollCost = rerollCost.Value;
            //rerollCost=(int) AccessTools.Field(buildGadgetMenu, "rerollCost").GetValue(BuildGadgetMenu.instance);
        }




        [HarmonyPatch(typeof(BuildGadgetMenu), "RemoveFromShop")]
        [HarmonyPrefix]
        public static bool RemoveFromShop()
        {
            //   logger.LogWarning($"RemoveFromShop");
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
            //   logger.LogWarning($"RefreshShop");
            if (customShop.Value)
            {
                ShopTier shopTier = ___shopTierSO.shopTiers[___shopTier];
                //for (int i = 0; i < 5; i++)
                //{
                //    int tier = shopTier.RollProbability();
                //    ___currShopGadgets[i] = GadgetData.instance.GetRandomGadget(tier);
                //}
                if (GadgetManager.instance.CurrGadgetCount < GadgetManager.instance.MaxGadgetCount)
                {
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
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                        ___currShopGadgets[i] = upgradableGadgets.ElementAt(UnityEngine.Random.Range(0, upgradableGadgets.Count)).Key;
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

        #endregion

        #region 적

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

        public static float SetMult(float modifier, float m)
        {
            if (!eMultOn.Value)
            {
                return modifier;
            }
            if (eMultRndOn.Value)
            {
                modifier *= m * UnityEngine.Random.Range(eMultRndMin, eMultRndMax);
            }
            else
            {
                modifier *= m;
            }
            return modifier;
        }

        /// <summary>
        /// 적 전용
        /// </summary>
        /// <param name="modifier"></param>
        [HarmonyPatch(typeof(HealthManager), "SetMaxHealthModifier")]
        [HarmonyPrefix]
        public static void SetMaxHealthModifier(ref float modifier, HealthManager __instance)
        {
            modifier = SetMult(modifier, eHealthMult.Value);
            //logger.LogWarning($"SetMaxHealthModifier {modifier} , {__instance.currHealth} , {__instance.MaxHealth}");
        }

        [HarmonyPatch(typeof(HealthUnit), "ArmorMult", MethodType.Setter)]
        [HarmonyPrefix]
        public static void SetArmorMult(ref float __0)
        {
            __0 = SetMult(__0, eArmorMult.Value);
        }

        [HarmonyPatch(typeof(Steerer), "MoveSpeedMult", MethodType.Setter)]
        [HarmonyPrefix]
        public static void SetMoveSpeedMult(ref float __0)
        {
            __0 = SetMult(__0, eSpeedMult.Value);
        }

        [HarmonyPatch(typeof(AEnemy), "DamageMult", MethodType.Setter)]
        [HarmonyPrefix]
        public static void SetDamageMult(ref float __0)
        {
            __0 = SetMult(__0, eDamageMult.Value);
        }

        [HarmonyPatch(typeof(EnemySpawner), "GetRandomSpawnPosition")]
        [HarmonyPrefix]
        public static bool GetRandomSpawnPosition(ref Vector2 __result)
        {
            if (!customRandomSpawnPosition.Value)
            {
                return true;
            }
            var v = UnityEngine.Random.insideUnitCircle;
            __result = v * (crspMax.Value - crspMin.Value) + v.normalized * crspMin.Value;
            return false;
        }

        // static int cnt;
        /// <summary>
        /// 소환 크기
        /// </summary>
        /// <param name="__result"></param>
        /// <param name="pair"></param>
        [HarmonyPatch(typeof(EnemySpawner), "SpawnEnemy")]
        [HarmonyPostfix]
        public static void SpawnEnemy(List<GameObject> __result, ref ValueTuple<GameObject, int> pair)
        {
            if (rndPos.Value)
            {
                Vector2 v;
                foreach (var item in __result)
                {
                    v = item.transform.position;
                    GetRandomSpawnPosition(ref v);
                    item.transform.position = v;
                }
            }

            if (!eMultOn.Value)
            {
                return;
            }
            //logger.LogWarning($"SpawnEnemy {__result.Count}");
            Vector3 vector3 = pair.Item1.transform.localScale;
            //AEnemy a;
            if (eMultRndOn.Value)
            {
                foreach (var item in __result)
                {
                    //a = item.GetComponent<AEnemy>();
                    //vector3 = a.PoolKey.transform.localScale;
                    //logger.LogWarning($"SpawnEnemy , {vector3.x} , {vector3.y} , {a.PoolKey.name} , {++cnt} ");
                    //vector3 = item.transform.localScale = vector3 * UnityEngine.Random.Range(1 / eMultRnd.Value, eMultRnd.Value);
                    //logger.LogWarning($"SpawnEnemy , {vector3.x} , {vector3.y} , {item.name}");
                    item.transform.localScale = vector3 * eSizeMult.Value * UnityEngine.Random.Range(1 / eSizeMult.Value, eSizeMult.Value);
                }
            }
            else
            {
                foreach (var item in __result)
                {
                    item.transform.localScale = vector3 * eSizeMult.Value;
                }
            }

        }

        /// <summary>
        /// 소환 갯수 수정
        /// </summary>
        /// <param name="pair"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        [HarmonyPatch(typeof(EnemyPooler), "Create")]
        [HarmonyPrefix]
        public static void Create(ref ValueTuple<GameObject, int> pair, Vector2 position, Quaternion rotation)
        {
            if (!eMultOn.Value)
            {
                return;
            }
            //logger.LogWarning($"Create {pair.Item1.name} , {pair.Item2} , {position} , {rotation}");
            pair.Item2 *= eQuantityMult.Value;
        }



        /*

        /// <summary>
        /// ___steerer != null 참일 경우 적
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="damage"></param>
        /// <param name="armorPen"></param>
        /// <param name="___steerer"></param>
        [HarmonyPatch(typeof(HealthUnit), "TakeDamage")]
        [HarmonyPrefix]
        public static void TakeDamage(HealthUnit __instance, float damage, float armorPen, Steerer ___steerer)
        {
            logger.LogWarning($"TakeDamage {___steerer != null} , {damage} , {armorPen} , {__instance.TotalArmor}");
        }

        /// <summary>
        /// ___isFriendly==false일 경우 적
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="amount"></param>
        /// <param name="___isFriendly"></param>
        [HarmonyPatch(typeof(HealthManager), "LoseHealth")]
        [HarmonyPrefix]
        public static void LoseHealth(HealthManager __instance, float amount, bool ___isFriendly)
        {
            logger.LogWarning($"LoseHealth {___isFriendly} , {amount} , {__instance.currHealth} , {__instance.MaxHealth}");
        }

        [HarmonyPatch(typeof(HealthManager), "LoseHealth")]
        [HarmonyPostfix]
        public static void LoseHealth2(HealthManager __instance, float __result, bool ___isFriendly)
        {
            logger.LogWarning($"LoseHealth {___isFriendly} ,  {__result} , {__instance.currHealth} , {__instance.MaxHealth}");
        }

        */


        #endregion
    }
}
