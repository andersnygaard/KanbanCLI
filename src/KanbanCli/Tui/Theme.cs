namespace KanbanCli.Tui;

/// <summary>
/// Centralized color palette for the entire TUI.
/// Inspired by Nord (https://www.nordtheme.com/) mapped to ConsoleColor.
///
/// To customize the app's look, edit ONLY this file.
///
/// Nord mapping (approximate):
///   nord0  #F8F9FB  → White        (bright background)
///   nord1  #ECEFF4  → Gray         (muted foreground)
///   nord3  #8B98B3  → DarkGray     (subtle text, borders)
///   nord4  #434C5E  → DarkGray     (dark foreground on light)
///   nord5  #3B4252  → Black        (dark background)
///   nord6  #2E3440  → Black        (darkest background)
///   nord7  #D08770  → DarkYellow   (aurora orange — accents)
///   nord8  #B48EAD  → Magenta      (aurora purple)
///   nord9  #5E81AC  → DarkCyan     (frost blue)
///   nord11 #BF616A  → Red          (aurora red — errors)
///   nord13 #81A1C1  → Cyan         (frost light blue — primary)
///   nord14 #A3BE8C  → Green        (aurora green — success)
///   nord15 #8FBCBB  → DarkCyan     (frost teal)
/// </summary>
public static class Theme
{
    // ── Board Frame ──────────────────────────────────────────────
    public static readonly ConsoleColor BoardBorder = ConsoleColor.DarkGray;          // nord3
    public static readonly ConsoleColor BoardTitleBg = ConsoleColor.DarkBlue;         // nord9-ish
    public static readonly ConsoleColor BoardTitleFg = ConsoleColor.White;            // nord0

    // ── Column Headers ───────────────────────────────────────────
    public static readonly ConsoleColor ColumnHeaderSelectedFg = ConsoleColor.White;       // nord0
    public static readonly ConsoleColor ColumnHeaderSelectedBg = ConsoleColor.DarkBlue;    // nord9-ish
    public static readonly ConsoleColor ColumnHeaderFg = ConsoleColor.Yellow;              // nord13-ish warm
    public static readonly ConsoleColor ColumnHeaderBg = ConsoleColor.Black;               // nord6

    // ── Column Body ──────────────────────────────────────────────
    public static readonly ConsoleColor ColumnBg = ConsoleColor.Black;                // nord6
    public static readonly ConsoleColor ColumnEmptyText = ConsoleColor.DarkGray;      // nord3
    public static readonly ConsoleColor ScrollIndicator = ConsoleColor.DarkYellow;    // nord7

    // ── Selected Column Highlight ────────────────────────────────
    public static readonly ConsoleColor SelectedBorder = ConsoleColor.Cyan;           // nord13

    // ── Task Cards ───────────────────────────────────────────────
    public static readonly ConsoleColor CardSelectedBg = ConsoleColor.DarkCyan;       // nord9
    public static readonly ConsoleColor CardSelectedFg = ConsoleColor.White;          // nord0
    public static readonly ConsoleColor CardBg = ConsoleColor.Black;                  // nord6
    public static readonly ConsoleColor CardFg = ConsoleColor.Gray;                   // nord1
    public static readonly ConsoleColor CardLabel = ConsoleColor.DarkYellow;          // nord7

    // ── Status Bar ───────────────────────────────────────────────
    public static readonly ConsoleColor StatusBarBg = ConsoleColor.DarkGray;          // nord3
    public static readonly ConsoleColor StatusBarFg = ConsoleColor.White;             // nord0

    // ── Detail Panel ─────────────────────────────────────────────
    public static readonly ConsoleColor DetailBorder = ConsoleColor.DarkGray;         // nord3
    public static readonly ConsoleColor DetailHeading = ConsoleColor.Cyan;            // nord13
    public static readonly ConsoleColor DetailFieldLabel = ConsoleColor.DarkCyan;     // nord9
    public static readonly ConsoleColor DetailFieldValue = ConsoleColor.White;        // nord0
    public static readonly ConsoleColor DetailMuted = ConsoleColor.DarkGray;          // nord3

    // ── Status Workflow ──────────────────────────────────────────
    public static readonly ConsoleColor WorkflowActiveFg = ConsoleColor.White;        // nord0
    public static readonly ConsoleColor WorkflowActiveBg = ConsoleColor.DarkBlue;     // nord9-ish
    public static readonly ConsoleColor WorkflowInactive = ConsoleColor.DarkGray;     // nord3

    // ── Labels ───────────────────────────────────────────────────
    public static readonly ConsoleColor LabelBracket = ConsoleColor.DarkYellow;       // nord7
    public static readonly ConsoleColor LabelText = ConsoleColor.White;               // nord0

    // ── Dialogs ──────────────────────────────────────────────────
    public static readonly ConsoleColor DialogBorder = ConsoleColor.DarkGray;         // nord3
    public static readonly ConsoleColor DialogTitle = ConsoleColor.White;             // nord0
    public static readonly ConsoleColor DialogPrompt = ConsoleColor.Cyan;             // nord13
    public static readonly ConsoleColor DialogText = ConsoleColor.White;              // nord0
    public static readonly ConsoleColor DialogListNumber = ConsoleColor.DarkGray;     // nord3
    public static readonly ConsoleColor DialogListItem = ConsoleColor.White;          // nord0
    public static readonly ConsoleColor DialogHeader = ConsoleColor.DarkGreen;        // nord14-ish

    // ── Confirm / Danger Dialog ──────────────────────────────────
    public static readonly ConsoleColor DangerBorder = ConsoleColor.Red;              // nord11
    public static readonly ConsoleColor DangerText = ConsoleColor.Red;               // nord11

    // ── Feedback ─────────────────────────────────────────────────
    public static readonly ConsoleColor Success = ConsoleColor.Green;                 // nord14
    public static readonly ConsoleColor Error = ConsoleColor.Red;                     // nord11

    // ── Keybinding Hints ─────────────────────────────────────────
    public static readonly ConsoleColor HintKey = ConsoleColor.DarkYellow;            // nord7
    public static readonly ConsoleColor HintText = ConsoleColor.DarkGray;             // nord3

    // ── Markdown Rendering ───────────────────────────────────────
    public static readonly ConsoleColor MdHeading = ConsoleColor.Cyan;                // nord13
    public static readonly ConsoleColor MdSubheading = ConsoleColor.DarkCyan;         // nord9
    public static readonly ConsoleColor MdBold = ConsoleColor.White;                  // nord0
    public static readonly ConsoleColor MdCode = ConsoleColor.DarkYellow;             // nord7
    public static readonly ConsoleColor MdLiteral = ConsoleColor.White;               // nord0
    public static readonly ConsoleColor MdCheckboxChecked = ConsoleColor.Green;       // nord14
    public static readonly ConsoleColor MdCheckboxUnchecked = ConsoleColor.DarkGray;  // nord3
    public static readonly ConsoleColor MdBullet = ConsoleColor.DarkYellow;           // nord7
}
