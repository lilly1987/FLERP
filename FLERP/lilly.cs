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

        public int windowId = 542;
        public Rect WindowRect;

        public string windowName = "";
        public string FullName = "Plugin";
        public string ShortName = "P";

        GUILayoutOption h;
        GUILayoutOption w;
        public Vector2 scrollPosition;


        static Type buildGadgetMenu;
        public static int rerollCost = 1;

        public void Awake()
        {
            logger = Logger;
            Logger.LogMessage("Awake");

            ShowCounter = Config.Bind("GUI", "isGUIOnKey", new KeyboardShortcut(KeyCode.Keypad0));// 이건 단축키

            isGUIOn = Config.Bind("GUI", "isGUIOn", true);
            isOpen = Config.Bind("GUI", "isOpen", true);
            isOpen.SettingChanged += IsOpen_SettingChanged;

            if (isOpen.Value)
                WindowRect = new Rect(Screen.width - 65, 0, 200, 800);
            else
                WindowRect = new Rect(Screen.width - 200, 0, 200, 800);

            IsOpen_SettingChanged(null, null);

            // BuildGadgetMenu.rerollCost : int @0400067A
            //buildGadgetMenu = AccessTools.TypeByName("BuildGadgetMenu");
            buildGadgetMenu = typeof(BuildGadgetMenu);            
        }

        public void IsOpen_SettingChanged(object sender, EventArgs e)
        {
            logger.LogInfo($"IsOpen_SettingChanged {isOpen.Value} , {isGUIOn.Value},{WindowRect.x} ");
            if (isOpen.Value)
            {
                h = GUILayout.Height(800);
                w = GUILayout.Width(300);
                windowName = FullName;
                WindowRect.x -= 135;
            }
            else
            {
                h = GUILayout.Height(40);
                w = GUILayout.Width(60);
                windowName = ShortName;
                WindowRect.x += 135;
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

            WindowRect = GUILayout.Window(windowId, WindowRect, WindowFunction, windowName, w, h);
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

                GUILayout.BeginHorizontal();
                GUILayout.Label($"Gadget rerollCost : {rerollCost}");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("<", GUILayout.Width(20), GUILayout.Height(20))) { AccessTools.Field(buildGadgetMenu, "rerollCost").SetValue(BuildGadgetMenu.instance, --rerollCost); }
                if (GUILayout.Button(">", GUILayout.Width(20), GUILayout.Height(20))) { AccessTools.Field(buildGadgetMenu, "rerollCost").SetValue(BuildGadgetMenu.instance, ++rerollCost); }
                GUILayout.EndHorizontal();


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
            ___rerollCost = rerollCost;
            //rerollCost=(int) AccessTools.Field(buildGadgetMenu, "rerollCost").GetValue(BuildGadgetMenu.instance);
        }
    }
}
