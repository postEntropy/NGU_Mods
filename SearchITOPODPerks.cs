using BepInEx;
using UnityEngine;
using HarmonyLib;
using System.Linq;
using System;

[BepInPlugin("com.leo.searchitopodperks", "ITOPOD Perk Search", "1.0.0.0")]
[BepInProcess("NGUIdle.exe")]
public class SearchITOPODPerks : BaseUnityPlugin
{
    public static SearchITOPODPerks Instance { get; private set; }

    public string _searchString = "";
    public bool _visible = false;
    public Rect _windowRect = new Rect(400f, 50f, 220f, 60f);

    public ItopodPerkController perkController;

    private static Character _character;
    public static Character GameCharacter
    {
        get
        {
            if (_character == null)
            {
                _character = UnityEngine.Object.FindObjectsOfType<Character>()
                                               .FirstOrDefault(c => c.name == "Character");
            }
            return _character;
        }
    }

    public void Awake()
    {
        Instance = this;
        var harmony = new Harmony("com.leo.searchitopodperks");
        harmony.PatchAll();
        Logger.LogInfo("SearchITOPODPerks was loaded and patches applied!");
    }

    public void Update()
    {
        Character currentChar = GameCharacter;
        const int ID_DO_MENU_ITOPOD = 41;
        bool isItopodScreenActive = currentChar != null && currentChar.menuID == ID_DO_MENU_ITOPOD;

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (isItopodScreenActive)
            {
                this.perkController = UnityEngine.Object.FindObjectOfType<ItopodPerkController>();
                _visible = !_visible;
            }
        }

        if (_visible && !isItopodScreenActive)
        {
            _visible = false;
        }
    }

    public void OnGUI()
    {
        if (!_visible) return;

        GUI.backgroundColor = new Color(0f, 0f, 0f, 0.9f);
        _windowRect = new Rect(_windowRect.x, _windowRect.y, 350f, 0f);

        _windowRect = GUILayout.Window(GetHashCode(), _windowRect, id =>
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Search:", GUILayout.Width(50f));

            GUI.SetNextControlName("SearchField");
            string newSearch = GUILayout.TextField(_searchString);
            GUI.FocusControl("SearchField");

            if (GUILayout.Button("X", GUILayout.Width(25f)))
            {
                _visible = false;
                _searchString = "";
                RefreshPerks();
                return;
            }

            GUILayout.EndHorizontal();

            if (GUI.changed && newSearch != _searchString)
            {
                _searchString = newSearch.ToLowerInvariant().Trim();
                RefreshPerks();
            }

            GUI.DragWindow();

        }, "ITOPOD Perks Search");
    }

    public void RefreshPerks()
    {
        if (perkController == null) return;

        if (!string.IsNullOrEmpty(_searchString))
        {
            foreach (var uiController in perkController.perkControllers)
            {
                int perkId = uiController.id;
                string name = perkController.perkName[perkId].ToLowerInvariant().Trim();
                bool isMatch = name.Contains(_searchString);

                if (isMatch)
                {
                    uiController.itemGraphic.color = Color.white;
                    uiController.itemBorder.color = Color.green;
                }
                else
                {
                    uiController.itemGraphic.color = Color.grey;
                    uiController.itemBorder.color = Color.grey;
                }
            }
        }
        else
        {
            foreach (var uiController in perkController.perkControllers)
            {
                uiController.updateGraphic();
            }
        }
    }
}
