using TMPro;
using UnityEngine;

// The two fonts every screen uses, in one asset (Resources/UiTheme) so screens built
// at runtime need no scene references. Colours and shapes live in Ui. The Maaloomatiia
// brand pairs SF Pro Display with Inter; this game uses Inter for both, because SF Pro's
// licence doesn't allow embedding it in a game published on the web.
[CreateAssetMenu(menuName = "Claude Bomb/UI Theme")]
public class UiTheme : ScriptableObject
{
    public TMP_FontAsset display; // headings, buttons, numbers (Inter Bold)
    public TMP_FontAsset body;    // running text (Inter; falls back to Noto Sans JP for Japanese)
}
