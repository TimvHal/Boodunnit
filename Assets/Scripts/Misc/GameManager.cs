using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager
{
    /// <summary>
    /// If the game paused?
    /// </summary>
    public static bool IsPaused { get; set; }
    public static bool IsCutscenePlaying { get; set; }

    public static bool PlayerHasAllClues = false;
    public static bool PlayerIsInEndState = false;
    public static Collider CurrentHighlightedCollider { get; set; }

    private static bool _cursorIsLocked = true;

    /// <summary>
    /// Is the Cursor locked or not
    /// </summary>
    public static bool CursorIsLocked
    {
        get
        {
            return _cursorIsLocked;
        }
        set
        {
            _cursorIsLocked = value;
            SetCursorIsLocked();
        }
    }

    private static void SetCursorIsLocked()
    {
        CursorLockMode newCursorLockModus = CursorIsLocked ? CursorLockMode.Locked : CursorLockMode.Confined;
        Cursor.lockState = newCursorLockModus;
    }

    public static void ToggleCursor()
    {
        if (!IsPaused)
        {
            CursorIsLocked = true;
        }
        else
        {
            CursorIsLocked = false;
        }
    }
}
