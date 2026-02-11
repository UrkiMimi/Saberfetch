using System;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Saberfetch.Configuration;
using BeatSaberMarkupLanguage;
using UnityEngine.UI;
using System.Collections;

namespace Saberfetch
{
    public class SaberfetchController : MonoBehaviour
    {
        public static SaberfetchController Instance { get; private set; }

        private Process process;

        //for canvas and text
        private Canvas canvas;

        private TextMeshProUGUI tmpText;

        // update frequency
        private float updateInterval = 0.5f;

        private readonly int fontSize = 12;

        private float time;

        private Vector3 usageMB;

        private int noteCount;

        private int obstacleCount;

        // system info
        private string cpuType;

        private string gpuType;

        private string totalRAM;

        private string operatingSys;

        private string sceneName;

        private string bsVersion;

        private string unityVersion;

        // config
        internal PluginConfig conf;


        // on awake
        void Start()
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);

            if (!conf.enabled)
            {
                Destroy(this.gameObject);
            }

            // to get memory usage
            process = Process.GetCurrentProcess();

            // check for fpfc
            var commandArgs = Environment.GetCommandLineArgs();
            bool fpfcEnabled = false;
            foreach (var arg in commandArgs) {
                if (arg == "fpfc")
                {
                    fpfcEnabled = true;
                }
            }

            // set system info
            cpuType = $"{SystemInfo.processorType} [{SystemInfo.processorCount} Cores]";
            gpuType = $"{SystemInfo.graphicsDeviceName} [{SystemInfo.graphicsMemorySize} MB]";
            totalRAM = SystemInfo.systemMemorySize.ToString();
            operatingSys = SystemInfo.operatingSystem.ToString();

            // get version info
            bsVersion = Application.version.ToString();
            unityVersion = Application.unityVersion.ToString();
            UpdateCounters(); // call update on first frame
            CreateUI();
        }

        // called on update
        void Update()
        {
            time += Time.unscaledDeltaTime;
            if (time > updateInterval)
            {
                time = 0.0f;
                UpdateCounters();
            }
        }

        void OnGUI()
        {
            // please fix this
            var statsRect = new Rect(10, 10 , 350, 380);

            // make labels compact and easy to read
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.padding = new RectOffset(0, 0, 0, 0);
            labelStyle.margin = new RectOffset(0,0,0,0);

            //titles
            GUIStyle titleLabel = new GUIStyle(GUI.skin.label);
            titleLabel.padding = new RectOffset(0, 0, 12, 0);
            titleLabel.margin = new RectOffset(0, 0, 0, 3);

            // make window
            GUI.color = new Color(0, 0, 0, 0.9f);
            GUILayout.BeginArea(statsRect, GUI.skin.box);

            GUI.color = Color.white; // reset for text

            // window title
            GUILayout.Label("==Saberfetch Stats==");

            //RAM
            if (conf.showMemory)
            {
                GUILayout.Label("Application Memory", titleLabel);
                GUILayout.Label($"- UnityEngine: {usageMB.z:F1} MB", labelStyle);
                // change text to red if mono is over allocated
                if (usageMB.y >= usageMB.x)
                {
                    GUI.color = Color.red;
                }
                GUILayout.Label($"- Mono: {usageMB.y:F1} MB", labelStyle);
                GUI.color = Color.white;
                GUILayout.Label($"- Allocated: {usageMB.x:F1} MB", labelStyle);
            }

            // FPS and milliseconds
            if (conf.showRendering)
            {
                GUILayout.Label("Rendering", titleLabel);
                GUILayout.Label($"- FPS: {(1 / Time.unscaledDeltaTime):F1}", labelStyle);
                GUILayout.Label($"- Render Time: {(Time.unscaledDeltaTime * 1000):F1} ms", labelStyle);
            }


            // system info
            if (conf.showSystemInfo)
            {
                GUILayout.Label("System Info", titleLabel);
                GUILayout.Label($"- CPU: {cpuType}", labelStyle);
                GUILayout.Label($"- GPU: {gpuType}", labelStyle);
                GUILayout.Label($"- RAM: {totalRAM} MB", labelStyle);
                GUILayout.Label($"- OS: {operatingSys}", labelStyle);
            }


            //misc
            if (conf.showOtherCounters)
            {
                GUILayout.Label("Miscellaneous", titleLabel);
                GUILayout.Label($"- Scene: {sceneName}", labelStyle);
                GUILayout.Label($"- BS Version: {bsVersion}", labelStyle);
                GUILayout.Label($"- Unity Version: {unityVersion}", labelStyle);
                if (conf.countBeatmapObjects)
                {
                    GUILayout.Label($"- Notes: {noteCount}", labelStyle);
                    GUILayout.Label($"- Obstacles: {obstacleCount}", labelStyle);
                }
            }

            GUILayout.EndArea();

        }

        private void UpdateCounters()
        {
            //region get ram usage
            if (conf.showMemory)
            {
                long engineUsageBytes = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong();
                long monoUsageBytes = UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong();
                long totalRAMUsage = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();

                //oh god what the fuck
                usageMB = new Vector3(totalRAMUsage, monoUsageBytes, engineUsageBytes);
                usageMB /= (1024f * 1024f);
            }


            // note count
            if (conf.showOtherCounters)
            {
                if (conf.countBeatmapObjects)
                {
                    noteCount = FindObjectsOfType<GameNoteController>().Length;
                    noteCount += FindObjectsOfType<BombNoteController>().Length;
                    noteCount += FindObjectsOfType<BurstSliderGameNoteController>().Length;

                    obstacleCount = FindObjectsOfType<ObstacleController>().Length;
                }

                //scene name
                sceneName = SceneManager.GetActiveScene().name;
            }
        }

        private void CreateUI()
        {
            // canvas setup
            canvas = this.gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = this.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(Screen.width, Screen.height);
            this.gameObject.AddComponent<RectTransform>();

            tmpText = BeatSaberUI.CreateText(this.transform as RectTransform, "test", new Vector2(0.0f,0.0f));
            tmpText.rectTransform.offsetMin = new Vector2(10f, 10f);
            StartCoroutine(MenuStartDelay(tmpText));
            tmpText.fontSize = fontSize;

        }

        // wait for font
        IEnumerator MenuStartDelay(TextMeshProUGUI tmp)
        {
            yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "MainMenu");
            yield return new WaitUntil(() => BeatSaberUI.MainTextFont != null);
            tmp.font = BeatSaberUI.MainTextFont;
        }
    }
}
