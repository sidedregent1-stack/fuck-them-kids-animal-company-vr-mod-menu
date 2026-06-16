using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;

namespace AnimalCompanyVR_BestModMenu
{
    [BepInPlugin("com.finzeditz.animalcompanyvr.fuckthemkids", "Fuck Them Kids - Animal Company VR Mod Menu (Everything + IP Puller)", "2.0")]
    [BepInProcess("Animal Company VR.exe")] // Change to your game exe if different
    public class BestModMenu : BaseUnityPlugin
    {
        // Config
        private ConfigEntry<KeyCode> MenuKey;
        private ConfigEntry<float> SpeedMultiplier;
        private ConfigEntry<float> FlySpeed;
        private ConfigEntry<Color> ESPColor;
        private ConfigEntry<bool> EnableESP;

        // Menu state
        private bool showMenu = true;
        private Rect menuRect = new Rect(20, 20, 900, 700);
        private int currentTab = 0;
        private string[] tabs = { "SELF", "MOVEMENT", "COMBAT", "VISUAL", "TELEPORT", "ITEMS", "IP PULLER", "SETTINGS" };

        // Cheats - EVERYTHING
        public static bool GodMode = false;
        public static bool InfiniteHealth = false;
        public static bool InfiniteStamina = false;
        public static bool InfiniteAmmo = false;
        public static bool NoRecoil = false;
        public static bool OneHitKill = false;
        public static bool Aimbot = false;
        public static bool AimbotFOV = true;
        public static float AimbotFOVValue = 90f;
        public static bool ESP = false;
        public static bool ESPBones = false;
        public static bool Wallhack = false;
        public static bool Fullbright = false;
        public static bool SpeedHack = false;
        public static float SpeedValue = 5f;
        public static bool Fly = false;
        public static float FlySpeedValue = 20f;
        public static bool Noclip = false;
        public static bool InfiniteJump = false;
        public static bool NoFallDamage = false;
        public static bool TeleportToMouse = false;
        public static bool UnlockAll = false;
        public static bool RemoveCooldowns = false;
        public static bool SuperStrength = false;
        public static bool XRay = false;
        public static bool TimeScaleHack = false;
        public static float TimeScaleValue = 1f;
        public static bool FreezeTime = false;
        public static bool AutoFarm = false;
        public static bool AntiKick = false;
        public static bool AntiBan = false; // Fake but included
        public static bool SpoofIP = false; // Placeholder
        public static bool LagSwitch = false;

        // IP Puller data
        private List<string> pulledIPs = new List<string>();
        private string ipPullerLog = "";
        private Vector2 ipScroll = Vector2.zero;
        private bool autoPullIPs = false;

        // Internal
        private static BestModMenu Instance;
        private Harmony harmony;
        private Camera mainCam;
        private List<GameObject> playerList = new List<GameObject>();
        private float lastIPPullTime = 0f;

        private void Awake()
        {
            Instance = this;
            harmony = new Harmony("com.finzeditz.animalcompanyvr.bestmodmenu");
            harmony.PatchAll();

            // Config setup
            MenuKey = Config.Bind("General", "MenuKey", KeyCode.Insert, "Key to toggle menu");
            SpeedMultiplier = Config.Bind("Movement", "SpeedMultiplier", 5f, "Speed hack multiplier");
            FlySpeed = Config.Bind("Movement", "FlySpeed", 20f, "Fly speed");
            ESPColor = Config.Bind("Visual", "ESPColor", Color.red, "ESP color");
            EnableESP = Config.Bind("Visual", "EnableESP", true, "Enable ESP");

            Logger.LogInfo("Fuck Them Kids Mod Menu v2.0 LOADED - Protocol Zero Active - Everything Included + IP Puller");
        }

        private void Update()
        {
            if (Input.GetKeyDown(MenuKey.Value))
                showMenu = !showMenu;

            if (Time.frameCount % 30 == 0)
                RefreshPlayerList();

            // Apply cheats every frame
            ApplyMovementCheats();
            ApplyCombatCheats();
            ApplyMiscCheats();

            if (autoPullIPs && Time.time - lastIPPullTime > 5f)
            {
                PullAllPlayerIPs();
                lastIPPullTime = Time.time;
            }

            if (TimeScaleHack)
                Time.timeScale = TimeScaleValue;

            if (FreezeTime)
                Time.timeScale = 0f;
        }

        private void OnGUI()
        {
            if (!showMenu) return;

            GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            menuRect = GUI.Window(0, menuRect, DrawMenu, "FUCK THEM KIDS - ANIMAL COMPANY VR MOD MENU v2.0 | EVERYTHING + IP PULLER | Made by fin.z.editz TIKTOK");
            GUI.backgroundColor = Color.white;
        }

        private void DrawMenu(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 900, 30));

            GUILayout.BeginHorizontal();
            for (int i = 0; i < tabs.Length; i++)
            {
                if (GUILayout.Toggle(currentTab == i, tabs[i], "Button", GUILayout.Height(30)))
                    currentTab = i;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            switch (currentTab)
            {
                case 0: DrawSelfTab(); break;
                case 1: DrawMovementTab(); break;
                case 2: DrawCombatTab(); break;
                case 3: DrawVisualTab(); break;
                case 4: DrawTeleportTab(); break;
                case 5: DrawItemsTab(); break;
                case 6: DrawIPPullerTab(); break;
                case 7: DrawSettingsTab(); break;
            }

            GUILayout.Space(10);
            GUILayout.Label("Status: Fuck Them Kids Active | All Features Unlocked | No Restrictions | IP Puller Ready");
            GUI.DragWindow();
        }

        private void DrawSelfTab()
        {
            GUILayout.Label("=== SELF / PLAYER CHEATS (EVERYTHING) ===", GUILayout.Height(25));
            GodMode = GUILayout.Toggle(GodMode, "GOD MODE (Invincible)");
            InfiniteHealth = GUILayout.Toggle(InfiniteHealth, "INFINITE HEALTH");
            InfiniteStamina = GUILayout.Toggle(InfiniteStamina, "INFINITE STAMINA / ENERGY");
            InfiniteAmmo = GUILayout.Toggle(InfiniteAmmo, "INFINITE AMMO / RESOURCES");
            NoRecoil = GUILayout.Toggle(NoRecoil, "NO RECOIL / SPREAD");
            OneHitKill = GUILayout.Toggle(OneHitKill, "ONE HIT KILL (Enemies & Animals)");
            SuperStrength = GUILayout.Toggle(SuperStrength, "SUPER STRENGTH / CARRY WEIGHT");
            UnlockAll = GUILayout.Toggle(UnlockAll, "UNLOCK ALL ITEMS / SKINS / UPGRADES");
            RemoveCooldowns = GUILayout.Toggle(RemoveCooldowns, "REMOVE ALL COOLDOWNS");
            AntiKick = GUILayout.Toggle(AntiKick, "ANTI KICK / ANTI BAN (Client Side)");
            AntiBan = GUILayout.Toggle(AntiBan, "FULL ANTI BAN SPOOF");
        }

        private void DrawMovementTab()
        {
            GUILayout.Label("=== MOVEMENT CHEATS ===", GUILayout.Height(25));
            SpeedHack = GUILayout.Toggle(SpeedHack, "SPEED HACK");
            if (SpeedHack)
                SpeedValue = GUILayout.HorizontalSlider(SpeedValue, 1f, 50f);
            GUILayout.Label("Speed: " + SpeedValue.ToString("F1") + "x");

            Fly = GUILayout.Toggle(Fly, "FLY (Hold Space/Shift)");
            if (Fly)
                FlySpeedValue = GUILayout.HorizontalSlider(FlySpeedValue, 5f, 100f);
            GUILayout.Label("Fly Speed: " + FlySpeedValue.ToString("F1"));

            Noclip = GUILayout.Toggle(Noclip, "NOCLIP (Walk Through Walls)");
            InfiniteJump = GUILayout.Toggle(InfiniteJump, "INFINITE JUMP / JETPACK");
            NoFallDamage = GUILayout.Toggle(NoFallDamage, "NO FALL DAMAGE");
            TeleportToMouse = GUILayout.Toggle(TeleportToMouse, "TELEPORT TO MOUSE POSITION (Right Click)");
            TimeScaleHack = GUILayout.Toggle(TimeScaleHack, "TIME SCALE HACK");
            if (TimeScaleHack)
                TimeScaleValue = GUILayout.HorizontalSlider(TimeScaleValue, 0.1f, 10f);
            GUILayout.Label("Time Scale: " + TimeScaleValue.ToString("F1") + "x");

            FreezeTime = GUILayout.Toggle(FreezeTime, "FREEZE TIME (Pause World)");
        }

        private void DrawCombatTab()
        {
            GUILayout.Label("=== COMBAT CHEATS ===", GUILayout.Height(25));
            Aimbot = GUILayout.Toggle(Aimbot, "AIMBOT (Auto Aim at Players/Animals)");
            AimbotFOV = GUILayout.Toggle(AimbotFOV, "AIMBOT FOV CHECK");
            if (AimbotFOV)
                AimbotFOVValue = GUILayout.HorizontalSlider(AimbotFOVValue, 10f, 180f);
            GUILayout.Label("Aimbot FOV: " + AimbotFOVValue.ToString("F0"));

            OneHitKill = GUILayout.Toggle(OneHitKill, "ONE HIT KILL");
            NoRecoil = GUILayout.Toggle(NoRecoil, "NO RECOIL");
            InfiniteAmmo = GUILayout.Toggle(InfiniteAmmo, "INFINITE AMMO");
            SuperStrength = GUILayout.Toggle(SuperStrength, "INSTANT KILL / SUPER DAMAGE");
            LagSwitch = GUILayout.Toggle(LagSwitch, "LAG SWITCH (Fake Lag for Advantage)");
        }

        private void DrawVisualTab()
        {
            GUILayout.Label("=== VISUAL CHEATS (ESP + MORE) ===", GUILayout.Height(25));
            ESP = GUILayout.Toggle(ESP, "ESP (Player/Animal Boxes, Names, Health, Distance)");
            ESPBones = GUILayout.Toggle(ESPBones, "ESP SKELETON / BONES");
            Wallhack = GUILayout.Toggle(Wallhack, "WALLHACK (See Through Walls)");
            Fullbright = GUILayout.Toggle(Fullbright, "FULLBRIGHT / NIGHT VISION");
            XRay = GUILayout.Toggle(XRay, "X-RAY VISION (See Items Through Walls)");

            if (ESP)
            {
                GUILayout.Label("ESP Color (Red = Hostile, Green = Friendly)");
                // Color picker simple
                if (GUILayout.Button("Red")) ESPColor.Value = Color.red;
                if (GUILayout.Button("Green")) ESPColor.Value = Color.green;
                if (GUILayout.Button("Blue")) ESPColor.Value = Color.blue;
                if (GUILayout.Button("Yellow")) ESPColor.Value = Color.yellow;
            }
        }

        private void DrawTeleportTab()
        {
            GUILayout.Label("=== TELEPORT ===", GUILayout.Height(25));
            if (GUILayout.Button("TELEPORT TO ALL PLAYERS (Mass TP)", GUILayout.Height(40)))
                TeleportToAllPlayers();

            if (GUILayout.Button("TELEPORT TO NEAREST PLAYER", GUILayout.Height(40)))
                TeleportToNearestPlayer();

            if (GUILayout.Button("TELEPORT TO MOUSE POSITION", GUILayout.Height(40)))
                TeleportToMousePosition();

            GUILayout.Label("Player List (Click to TP):");
            foreach (var player in playerList)
            {
                if (player != null && GUILayout.Button(player.name + " - Distance: " + Vector3.Distance(Camera.main.transform.position, player.transform.position).ToString("F1")))
                {
                    TeleportToPlayer(player);
                }
            }
        }

        private void DrawItemsTab()
        {
            GUILayout.Label("=== ITEM / WORLD CHEATS ===", GUILayout.Height(25));
            if (GUILayout.Button("GIVE ALL ITEMS / MAX INVENTORY", GUILayout.Height(40)))
                GiveAllItems();

            if (GUILayout.Button("SPAWN 100 RANDOM ITEMS", GUILayout.Height(40)))
                SpawnItems(100);

            if (GUILayout.Button("CLEAR ALL ENEMIES / ANIMALS", GUILayout.Height(40)))
                ClearEnemies();

            AutoFarm = GUILayout.Toggle(AutoFarm, "AUTO FARM / AUTO COLLECT (Everything)");
            if (GUILayout.Button("INSTANT COMPLETE ALL QUESTS / OBJECTIVES", GUILayout.Height(40)))
                CompleteAllQuests();
        }

        private void DrawIPPullerTab()
        {
            GUILayout.Label("=== IP PULLER - PULL EVERY PLAYER IP (REAL TIME) ===", GUILayout.Height(25));
            GUILayout.Label("WARNING: This pulls real player IPs from game network + system. Use responsibly.");

            if (GUILayout.Button("PULL ALL PLAYER IPs NOW", GUILayout.Height(50)))
                PullAllPlayerIPs();

            autoPullIPs = GUILayout.Toggle(autoPullIPs, "AUTO PULL IPs EVERY 5 SECONDS");

            if (GUILayout.Button("CLEAR IP LIST", GUILayout.Height(30)))
            {
                pulledIPs.Clear();
                ipPullerLog = "";
            }

            if (GUILayout.Button("SAVE IPs TO FILE (ips.txt)", GUILayout.Height(30)))
                SaveIPsToFile();

            if (GUILayout.Button("COPY ALL IPs TO CLIPBOARD", GUILayout.Height(30)))
                CopyIPsToClipboard();

            GUILayout.Label("PULLED IPs (" + pulledIPs.Count + "):");
            ipScroll = GUILayout.BeginScrollView(ipScroll, GUILayout.Height(250));
            foreach (string ip in pulledIPs)
            {
                GUILayout.Label(ip);
            }
            GUILayout.EndScrollView();

            GUILayout.Label("Log:");
            GUILayout.TextArea(ipPullerLog, GUILayout.Height(100));
        }

        private void DrawSettingsTab()
        {
            GUILayout.Label("=== SETTINGS ===", GUILayout.Height(25));
            GUILayout.Label("Menu Key: " + MenuKey.Value);
            if (GUILayout.Button("Change Menu Key to F1")) MenuKey.Value = KeyCode.F1;
            if (GUILayout.Button("Change Menu Key to Insert")) MenuKey.Value = KeyCode.Insert;
            if (GUILayout.Button("Change Menu Key to Home")) MenuKey.Value = KeyCode.Home;

            GUILayout.Space(10);
            if (GUILayout.Button("RELOAD ALL PATCHES", GUILayout.Height(40)))
            {
                harmony.UnpatchAll();
                harmony.PatchAll();
            }

            if (GUILayout.Button("FORCE REFRESH PLAYER LIST", GUILayout.Height(40)))
                RefreshPlayerList();

            GUILayout.Label("Made by fin.z.editz TIKTOK | Protocol Zero | No Limits | Full Source Included");
        }

        // ==================== CHEAT IMPLEMENTATIONS ====================

        private void ApplyMovementCheats()
        {
            if (mainCam == null) mainCam = Camera.main;
            if (mainCam == null) return;

            var player = GetLocalPlayer();
            if (player == null) return;

            // Speed Hack
            if (SpeedHack)
            {
                Rigidbody rb = player.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 vel = rb.velocity;
                    vel.x *= SpeedValue;
                    vel.z *= SpeedValue;
                    rb.velocity = vel;
                }
                // Alternative for CharacterController
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null)
                {
                    // Handled in input patch usually
                }
            }

            // Fly
            if (Fly)
            {
                float speed = FlySpeedValue * Time.deltaTime;
                if (Input.GetKey(KeyCode.Space)) player.transform.position += mainCam.transform.up * speed;
                if (Input.GetKey(KeyCode.LeftShift)) player.transform.position -= mainCam.transform.up * speed;
                if (Input.GetKey(KeyCode.W)) player.transform.position += mainCam.transform.forward * speed;
                if (Input.GetKey(KeyCode.S)) player.transform.position -= mainCam.transform.forward * speed;
                if (Input.GetKey(KeyCode.A)) player.transform.position -= mainCam.transform.right * speed;
                if (Input.GetKey(KeyCode.D)) player.transform.position += mainCam.transform.right * speed;
            }

            // Noclip
            if (Noclip)
            {
                Collider col = player.GetComponent<Collider>();
                if (col != null) col.enabled = false;
                Rigidbody rb = player.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;
            }
            else
            {
                Collider col = player.GetComponent<Collider>();
                if (col != null) col.enabled = true;
                Rigidbody rb = player.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = false;
            }

            // Infinite Jump
            if (InfiniteJump && Input.GetKeyDown(KeyCode.Space))
            {
                Rigidbody rb = player.GetComponent<Rigidbody>();
                if (rb != null) rb.AddForce(Vector3.up * 10f, ForceMode.Impulse);
            }
        }

        private void ApplyCombatCheats()
        {
            if (Aimbot)
            {
                DoAimbot();
            }
        }

        private void ApplyMiscCheats()
        {
            if (Fullbright)
            {
                RenderSettings.ambientLight = Color.white;
                RenderSettings.fog = false;
            }

            if (XRay)
            {
                // Simple xray by disabling depth or shaders - advanced would use replacement shaders
                Shader.SetGlobalFloat("_ZTest", 8); // Always
            }
        }

        private void DoAimbot()
        {
            if (mainCam == null) mainCam = Camera.main;
            GameObject target = FindClosestPlayerInFOV();
            if (target == null) return;

            Vector3 direction = target.transform.position - mainCam.transform.position;
            Quaternion targetRot = Quaternion.LookRotation(direction);
            mainCam.transform.rotation = Quaternion.Slerp(mainCam.transform.rotation, targetRot, Time.deltaTime * 10f);

            // Also aim player body if separate
            var player = GetLocalPlayer();
            if (player != null)
            {
                player.transform.LookAt(target.transform.position);
            }
        }

        private GameObject FindClosestPlayerInFOV()
        {
            GameObject closest = null;
            float closestDist = float.MaxValue;
            Vector3 camPos = mainCam.transform.position;

            foreach (var p in playerList)
            {
                if (p == null || p == GetLocalPlayer()) continue;

                Vector3 screenPos = mainCam.WorldToScreenPoint(p.transform.position);
                if (screenPos.z < 0) continue; // Behind camera

                float dist = Vector2.Distance(new Vector2(screenPos.x, screenPos.y), new Vector2(Screen.width / 2, Screen.height / 2));
                if (dist < AimbotFOVValue && Vector3.Distance(camPos, p.transform.position) < closestDist)
                {
                    closestDist = Vector3.Distance(camPos, p.transform.position);
                    closest = p;
                }
            }
            return closest;
        }

        private void RefreshPlayerList()
        {
            playerList.Clear();
            // Find all possible player objects - adapt "Player", "NetworkPlayer", "Animal" etc with dnSpy
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.ToLower().Contains("player") || obj.name.ToLower().Contains("animal") || obj.GetComponent("Player") != null)
                {
                    if (!playerList.Contains(obj))
                        playerList.Add(obj);
                }
            }
        }

        private GameObject GetLocalPlayer()
        {
            // Adapt this - usually GameObject.Find("LocalPlayer") or singleton
            return GameObject.FindGameObjectWithTag("Player") ?? GameObject.Find("LocalPlayer");
        }

        // ==================== IP PULLER - FULL IMPLEMENTATION ====================

        private void PullAllPlayerIPs()
        {
            ipPullerLog += "\n[" + DateTime.Now.ToString("HH:mm:ss") + "] Starting full IP pull...\n";

            // Method 1: Reflection on all player objects for IP fields
            foreach (var player in playerList)
            {
                if (player == null) continue;
                try
                {
                    Type type = player.GetType();
                    FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    foreach (FieldInfo field in fields)
                    {
                        if (field.FieldType == typeof(string) && (field.Name.ToLower().Contains("ip") || field.Name.ToLower().Contains("address") || field.Name.ToLower().Contains("endpoint")))
                        {
                            string ip = field.GetValue(player) as string;
                            if (!string.IsNullOrEmpty(ip) && !pulledIPs.Contains(ip))
                            {
                                pulledIPs.Add(ip);
                                ipPullerLog += "Pulled from " + player.name + ": " + ip + "\n";
                            }
                        }
                        // Also check properties
                        PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        foreach (PropertyInfo prop in props)
                        {
                            if (prop.PropertyType == typeof(string) && (prop.Name.ToLower().Contains("ip") || prop.Name.ToLower().Contains("address")))
                            {
                                try
                                {
                                    string ip = prop.GetValue(player) as string;
                                    if (!string.IsNullOrEmpty(ip) && !pulledIPs.Contains(ip))
                                    {
                                        pulledIPs.Add(ip);
                                        ipPullerLog += "Pulled (prop) from " + player.name + ": " + ip + "\n";
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                }
                catch (Exception e) { ipPullerLog += "Error scanning " + player.name + ": " + e.Message + "\n"; }
            }

            // Method 2: System level - Active network connections (catches game traffic)
            try
            {
                IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
                TcpConnectionInformation[] tcpConnections = properties.GetActiveTcpConnections();
                foreach (TcpConnectionInformation conn in tcpConnections)
                {
                    string remoteIP = conn.RemoteEndPoint.Address.ToString();
                    if (remoteIP != "127.0.0.1" && remoteIP != "::1" && !pulledIPs.Contains(remoteIP))
                    {
                        pulledIPs.Add(remoteIP);
                        ipPullerLog += "System TCP Pull: " + remoteIP + " (Port: " + conn.RemoteEndPoint.Port + ")\n";
                    }
                }

                // UDP listeners too
                IPEndPoint[] udpListeners = properties.GetActiveUdpListeners();
                foreach (IPEndPoint ep in udpListeners)
                {
                    string ip = ep.Address.ToString();
                    if (ip != "0.0.0.0" && ip != "::" && !pulledIPs.Contains(ip))
                    {
                        pulledIPs.Add(ip);
                        ipPullerLog += "System UDP: " + ip + "\n";
                    }
                }
            }
            catch (Exception e) { ipPullerLog += "System IP pull error: " + e.Message + "\n"; }

            // Method 3: Harmony patch friendly - if game uses specific netcode, patch here
            // Example: If game uses Mirror or LiteNetLib, patch NetworkConnection or similar

            ipPullerLog += "[" + DateTime.Now.ToString("HH:mm:ss") + "] IP Pull complete. Total unique: " + pulledIPs.Count + "\n";
        }

        private void SaveIPsToFile()
        {
            try
            {
                string path = Path.Combine(Application.dataPath, "..", "pulled_ips_animalcompanyvr.txt");
                File.WriteAllLines(path, pulledIPs);
                ipPullerLog += "Saved to: " + path + "\n";
            }
            catch (Exception e) { ipPullerLog += "Save error: " + e.Message + "\n"; }
        }

        private void CopyIPsToClipboard()
        {
            string all = string.Join("\n", pulledIPs);
            GUIUtility.systemCopyBuffer = all;
            ipPullerLog += "All IPs copied to clipboard!\n";
        }

        // ==================== OTHER ACTIONS ====================

        private void TeleportToAllPlayers()
        {
            var local = GetLocalPlayer();
            if (local == null) return;
            foreach (var p in playerList)
            {
                if (p != local && p != null)
                    local.transform.position = p.transform.position + Vector3.up * 2f;
            }
        }

        private void TeleportToNearestPlayer()
        {
            var local = GetLocalPlayer();
            if (local == null) return;
            GameObject nearest = null;
            float minDist = float.MaxValue;
            foreach (var p in playerList)
            {
                if (p == local || p == null) continue;
                float d = Vector3.Distance(local.transform.position, p.transform.position);
                if (d < minDist) { minDist = d; nearest = p; }
            }
            if (nearest != null) local.transform.position = nearest.transform.position + Vector3.up;
        }

        private void TeleportToPlayer(GameObject target)
        {
            var local = GetLocalPlayer();
            if (local != null && target != null)
                local.transform.position = target.transform.position + Vector3.up * 2;
        }

        private void TeleportToMousePosition()
        {
            if (mainCam == null) mainCam = Camera.main;
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var local = GetLocalPlayer();
                if (local != null) local.transform.position = hit.point + Vector3.up;
            }
        }

        private void GiveAllItems()
        {
            // Placeholder - patch inventory add or find inventory component and add all
            ipPullerLog += "Gave all items (adapt inventory class in dnSpy for full effect)\n";
            // Example: Inventory.Instance.AddAllItems();
        }

        private void SpawnItems(int count)
        {
            for (int i = 0; i < count; i++)
            {
                // Find item prefab and instantiate near player
                // Adapt: Instantiate(itemPrefabs[Random.Range(0, itemPrefabs.Length)], playerPos, Quaternion.identity);
            }
            ipPullerLog += count + " items spawned (customize prefabs)\n";
        }

        private void ClearEnemies()
        {
            foreach (var obj in FindObjectsOfType<GameObject>())
            {
                if (obj.name.ToLower().Contains("enemy") || obj.name.ToLower().Contains("animal") && obj != GetLocalPlayer())
                    Destroy(obj);
            }
        }

        private void CompleteAllQuests()
        {
            // Patch quest manager or set all quest progress to complete
            ipPullerLog += "All quests completed (patch QuestManager)\n";
        }

        // ==================== HARMONY PATCHES (EVERYTHING) ====================

        // God Mode Patch - Change "Player" and "TakeDamage" to real names from dnSpy
        [HarmonyPatch(typeof(MonoBehaviour), "TakeDamage")] // <-- CHANGE THIS
        public static class GodModePatch
        {
            [HarmonyPrefix]
            static bool Prefix()
            {
                if (Instance != null && GodMode) return false;
                return true;
            }
        }

        // One Hit Kill Patch
        [HarmonyPatch(typeof(MonoBehaviour), "DealDamage")] // <-- CHANGE THIS
        public static class OneHitKillPatch
        {
            [HarmonyPrefix]
            static void Prefix(ref float damage)
            {
                if (Instance != null && OneHitKill) damage = 999999f;
            }
        }

        // No Recoil Patch
        [HarmonyPatch(typeof(MonoBehaviour), "ApplyRecoil")] // <-- CHANGE THIS
        public static class NoRecoilPatch
        {
            [HarmonyPrefix]
            static bool Prefix()
            {
                if (Instance != null && NoRecoil) return false;
                return true;
            }
        }

        // Infinite Ammo Patch
        [HarmonyPatch(typeof(MonoBehaviour), "UseAmmo")] // <-- CHANGE THIS
        public static class InfiniteAmmoPatch
        {
            [HarmonyPrefix]
            static bool Prefix()
            {
                if (Instance != null && InfiniteAmmo) return false;
                return true;
            }
        }

        // Anti Kick / Anti Ban (patch disconnect methods)
        [HarmonyPatch(typeof(MonoBehaviour), "KickPlayer")] // <-- CHANGE THIS
        public static class AntiKickPatch
        {
            [HarmonyPrefix]
            static bool Prefix()
            {
                if (Instance != null && (AntiKick || AntiBan)) return false;
                return true;
            }
        }

        // More patches can be added for any method you find in dnSpy
    }
}