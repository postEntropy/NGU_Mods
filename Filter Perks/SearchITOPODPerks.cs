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

    private const int ID_DO_MENU_ITOPOD = 41;
    private static Character _character;

    private GUIStyle _windowStyle;
    private bool _setFocus = false;

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

    private Texture2D CreateSolidColorTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
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
        bool isItopodScreenActive = currentChar != null && currentChar.menuID == ID_DO_MENU_ITOPOD;

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (isItopodScreenActive)
            {
                _visible = !_visible;
                if (_visible)
                {
                    this.perkController = UnityEngine.Object.FindObjectOfType<ItopodPerkController>();
                    _setFocus = true;
                }
            }
        }

        if (_visible && !isItopodScreenActive)
        {
            _visible = false;
            _searchString = "";
            RefreshPerks();
        }
    }

    public void OnGUI()
    {
        if (!_visible) return;

        var sf = GameCharacter.tooltip.canvas.scaleFactor;
        _windowRect = new Rect(724 * sf, 105 * sf, 200 * sf, 20 * sf);

        if (_windowStyle == null)
        {
            _windowStyle = new GUIStyle("box");
            _windowStyle.normal.background = CreateSolidColorTexture(new Color(0.1f, 0.1f, 0.1f, 0.95f));
            _windowStyle.padding = new RectOffset(5, 5, 5, 5);
        }

        string newSearch;
        GUILayout.BeginArea(_windowRect, _windowStyle);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Search:", GUILayout.Width(50f));

        GUI.SetNextControlName("SearchField");
        newSearch = GUILayout.TextField(_searchString);

        if (_setFocus)
        {
            GUI.FocusControl("SearchField");
            _setFocus = false;
        }

        if (GUILayout.Button("X", GUILayout.Width(25f)))
        {
            _visible = false;
            _searchString = "";
            RefreshPerks();
            return;
        }
        GUILayout.EndHorizontal();

        GUILayout.EndArea();

        if (newSearch != _searchString)
        {
            _searchString = newSearch.ToLowerInvariant().Trim();
            RefreshPerks();
        }
    }

    public void RefreshPerks()
    {
        if (perkController == null) return;

        if (!string.IsNullOrEmpty(_searchString))
        {
            foreach (var uiController in perkController.perkControllers)
            {
                int perkId = uiController.id;
                if (perkId >= 0 && perkId < perkController.perkName.Count)
                {
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
