using System;
using System.Collections;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using HMUI;
using BeatSaberMarkupLanguage;
using UnityEngine.UI;

namespace Saberfetch
{
    public class SaberfetchController : MonoBehaviour
    {
        public static SaberfetchController Instance { get; private set; }

        private Process process;

        //for canvas and text
        private GameObject canvasGO;

        private GameObject tmpTextGO;

        private TextMeshProUGUI tmpText;

        // update frequency
        private float updateInterval = 0.5f;

        private Vector3 usageMB;

        private GameObject[] objCount;

        // system info
        private string cpuType;

        private string gpuType;

        private string totalRAM;

        private string operatingSys;


        // on awake
        void Start()
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);

            // to get memory usage
            process = Process.GetCurrentProcess();

            /*
            //imgui is ass
            canvasGO = new GameObject("SFCanvas");
            canvasGO.AddComponent<Canvas>();
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.transform.SetParent(this.transform);
            Canvas UICanvas = canvasGO.GetComponent<Canvas>();
            UICanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);

            tmpTextGO = new GameObject("TMPInstance");
            tmpTextGO.transform.SetParent(canvasGO.transform);
            tmpTextGO.AddComponent<RectTransform>();
            tmpText = BeatSaberUI.CreateText(tmpTextGO.GetComponent<RectTransform>(), "test", new Vector2(0,0));
            tmpText.fontSize = 32;*/

            // set system info
            cpuType = $"{SystemInfo.processorType} [{SystemInfo.processorCount} Cores]";
            gpuType = $"{SystemInfo.graphicsDeviceName} [{SystemInfo.graphicsMemorySize}]";
            totalRAM = SystemInfo.systemMemorySize.ToString();
            operatingSys = SystemInfo.operatingSystem.ToString();
        }

        // called on update
        void Update()
        {
            
            //region get ram usage
            long engineUsageBytes = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong();
            long monoUsageBytes = UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong();
            long totalRAMUsage = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();

            //oh god what the fuck
            usageMB = new Vector3(totalRAMUsage, monoUsageBytes, engineUsageBytes);
            usageMB /= (1024f * 1024f);

            // try to get prefab (and note count)

        }

        void OnGUI()
        {
            // please fix this
            var statsRect = new Rect(10, 10 , 350, 280);

            // make labels compact and easy to read
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.padding = new RectOffset(0, 0, 0, 0);
            labelStyle.margin = new RectOffset(0,0,0,0);

            //titles
            GUIStyle titleLabel = new GUIStyle(GUI.skin.label);
            titleLabel.padding = new RectOffset(0, 0, 12, 0);
            titleLabel.margin = new RectOffset(0, 0, 0, 3);

            // make window
            GUI.color = new Color(0, 0, 0, 0.6f);
            GUILayout.BeginArea(statsRect, GUI.skin.box);

            GUI.color = Color.white; // reset for text

            // window title
            GUILayout.Label("==Saberfetch Stats==");

            //RAM
            GUILayout.Label("Application RAM", titleLabel);
            GUILayout.Label($"- UnityEngine: {usageMB.z:F1} MB", labelStyle);
            GUILayout.Label($"- Mono: {usageMB.y:F1} MB", labelStyle);
            GUILayout.Label($"- Allocated: {usageMB.x:F1} MB", labelStyle);

            // FPS and milliseconds
            GUILayout.Label("Rendering", titleLabel);
            GUILayout.Label($"- FPS: {(1 / Time.deltaTime):F1}", labelStyle);
            GUILayout.Label($"- Render Time: {(Time.deltaTime * 1000):F1} ms", labelStyle);

            // object counts
            GUILayout.Label("System Info", titleLabel);
            GUILayout.Label($"- CPU: {cpuType}", labelStyle);
            GUILayout.Label($"- GPU: {gpuType}", labelStyle);
            GUILayout.Label($"- RAM: {totalRAM} MB", labelStyle);
            GUILayout.Label($"- OS: {operatingSys}", labelStyle);



            GUILayout.EndArea();

        }
    }
}
