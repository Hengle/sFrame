﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class sLogOut : MonoBehaviour {

    struct Log
    {
        public string message;
        public string stackTrace;
        public LogType type;
    }

    public KeyCode toggleKey = KeyCode.BackQuote;
    //给予移动平台使用
    public bool shakeToOpen = true;

    public float shakeAcceleration = 3f;

    public bool restrictLogCount = false;

    public int maxLogs = 1000;

    readonly List<Log> logs = new List<Log>();
    Vector2 scrollPosition;
    bool visible;
    bool collapse;

    static readonly Dictionary<LogType, Color> logTypeColors = new Dictionary<LogType, Color>
        {
            { LogType.Assert, Color.white },
            { LogType.Error, Color.red },
            { LogType.Exception, Color.red },
            { LogType.Log, Color.white },
            { LogType.Warning, Color.yellow },
        };

    const string windowTitle = "Console";
    const int margin = 20;
    static readonly GUIContent clearLabel = new GUIContent("Clear", "Clear the contents of the console.");
    static readonly GUIContent collapseLabel = new GUIContent("Collapse", "Hide repeated messages.");

    readonly Rect titleBarRect = new Rect(0, 0, 10000, 20);
    Rect windowRect = new Rect(margin, margin, Screen.width - (margin * 2), Screen.height - (margin * 2));

    void OnEnable()
    {
#if UNITY_5
        Application.logMessageReceived += HandleLog;
#else
            Application.RegisterLogCallback(HandleLog);  
#endif
    }

    void OnDisable()
    {
#if UNITY_5
        Application.logMessageReceived -= HandleLog;
#else
            Application.RegisterLogCallback(null);  
#endif
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            visible = !visible;
        }

        if (shakeToOpen && Input.acceleration.sqrMagnitude > shakeAcceleration)
        {
            visible = true;
        }
    }

    void OnGUI()
    {
        if (!visible)
        {
            return;
        }

        windowRect = GUILayout.Window(123456, windowRect, DrawConsoleWindow, windowTitle);
    }

    /// <summary>  
    /// Displays a window that lists the recorded logs.  
    /// </summary>  
    /// <param name="windowID">Window ID.</param>  
    void DrawConsoleWindow(int windowID)
    {
        DrawLogsList();
        DrawToolbar();

        // Allow the window to be dragged by its title bar.  
        GUI.DragWindow(titleBarRect);
    }

    /// <summary>  
    /// Displays a scrollable list of logs.  
    /// </summary>  
    void DrawLogsList()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        // Iterate through the recorded logs.  
        for (var i = 0; i < logs.Count; i++)
        {
            var log = logs[i];

            // Combine identical messages if collapse option is chosen.  
            if (collapse && i > 0)
            {
                var previousMessage = logs[i - 1].message;

                if (log.message == previousMessage)
                {
                    continue;
                }
            }

            GUI.contentColor = logTypeColors[log.type];
            GUILayout.Label(log.message);
        }

        GUILayout.EndScrollView();

        // Ensure GUI colour is reset before drawing other components.  
        GUI.contentColor = Color.white;
    }

    /// <summary>  
    /// Displays options for filtering and changing the logs list.  
    /// </summary>  
    void DrawToolbar()
    {
        GUILayout.BeginHorizontal();

        if (GUILayout.Button(clearLabel))
        {
            logs.Clear();
        }

        collapse = GUILayout.Toggle(collapse, collapseLabel, GUILayout.ExpandWidth(false));

        GUILayout.EndHorizontal();
    }

    /// <summary>  
    /// Records a log from the log callback.  
    /// </summary>  
    /// <param name="message">Message.</param>  
    /// <param name="stackTrace">Trace of where the message came from.</param>  
    /// <param name="type">Type of message (error, exception, warning, assert).</param>  
    void HandleLog(string message, string stackTrace, LogType type)
    {
        logs.Add(new Log
        {
            message = message,
            stackTrace = stackTrace,
            type = type,
        });

        TrimExcessLogs();
    }

    /// <summary>  
    /// Removes old logs that exceed the maximum number allowed.  
    /// </summary>  
    void TrimExcessLogs()
    {
        if (!restrictLogCount)
        {
            return;
        }

        var amountToRemove = Mathf.Max(logs.Count - maxLogs, 0);

        if (amountToRemove == 0)
        {
            return;
        }

        logs.RemoveRange(0, amountToRemove);
    }
}
