using System.Collections.Generic;
using System.Linq;

// What the game-feel pass says on each level beyond the curriculum's own words (Rooms.cs): the
// one objective line, the night shift's transmission, the short names on the plates in the world
// (the full text stays in the file and on the card brought close), and the thing that pulses
// after a rehearsal. The curriculum text, the checks and the report are unchanged.
public static class Script
{
    // One line, at most eight words: what to do in this room.
    public static string Objective(Room r) => r.number switch
    {
        1 => "Find what is missing from the board.",
        2 => "Rate each risk. Control the high ones.",
        3 => "Stamp each message: safe or suspicious.",
        4 => "Two proofs on every account that matters.",
        5 => "Facts only. Then three actions.",
        6 => "Keep, reduce or remove each account.",
        7 => "Order the patches. Reachable first.",
        8 => "Zone each system.",
        9 => "Close what should never be open.",
        10 => "Encrypt it. Keep the key apart.",
        11 => "Change every default.",
        12 => "Keep what matters.",
        13 => "Isolate, copy, then clean.",
        14 => "Put the backups out of reach.",
        15 => "Treat every risk. Show the evidence.",
        16 => "Declare it. Split the roles.",
        17 => "Rotate, invalidate, revoke. In order.",
        18 => "Put the chain in order.",
        19 => "Own and date every gap.",
        _ => "Fix the room. Then lift the lockdown.",
    };

    public const string Rehearsed = "Now fix it.";
    public const string Lift = "Lift the lockdown";

    // The alarm before each room: what is happening tonight and what the player is there to do,
    // three short lines and the button. The level's motto is LUMA's line on the debrief, not here.
    public static (string header, string sub, List<string> lines, string button) Transmission(Room r)
    {
        var lines = r.number switch
        {
            1 => new[] { "23:47. An intruder alarm at the Harbor Permits Office.", "Every door is sealed. Someone is in the corridor.", "Before you lift the lockdown, list what the Risk Room protects." },
            2 => new[] { "The intruder is back, testing the doors one by one.", "The risk board rates every threat the same: Medium.", "Rate them for real before you lift the lockdown." },
            3 => new[] { "A wave of emails hit a clerk's inbox at 23:40.", "Some of them were sent by the intruder.", "Sort the post before anyone opens it." },
            4 => new[] { "A list of stolen passwords is on sale tonight.", "The intruder will try it on every account in the vault.", "Lock the accounts that matter most." },
            5 => new[] { "Two calls came in at once.", "A laptop lost on the train, and files turning unreadable on the shared drive.", "The responder needs your report now." },
            6 => new[] { "Badges at the gate still open doors for people long gone.", "A contractor left in June. Nobody took anything back.", "Check every account before the gate opens." },
            7 => new[] { "A scan found five holes in the office's systems.", "The intruder only needs one it can reach.", "Two get patched before the lockdown lifts. Pick them." },
            8 => new[] { "The intruder is in the lobby, at the public kiosk.", "From there, one flat network runs all the way to the database.", "Draw the lines before the lockdown lifts." },
            9 => new[] { "Alarms on the firewall: someone is knocking on every port.", "Tonight's rules let the public side reach the admin portal.", "Close the doors that should never be open." },
            10 => new[] { "The intruder wants a copy of the register's disk.", "The disk is encrypted. Its key sits right beside it.", "Move the keys before the lockdown lifts." },
            11 => new[] { "The office's cloud went live this week, settings as they came.", "The intruder knows the defaults better than anyone here.", "Change them before the lockdown lifts." },
            12 => new[] { "Tonight someone will sign in as an administrator, from abroad.", "Four thousand failed logins already fill the alert screen.", "Decide what gets logged, and what wakes you." },
            13 => new[] { "Ransomware on a clerk's machine. It is spreading.", "Wipe it now and the trail of how it got in is gone.", "Choose the order of every step." },
            14 => new[] { "The intruder carries ransomware and the administrator's keys.", "The backups sit on the same network, behind the same keys.", "Get them out of reach." },
            15 => new[] { "Midnight: the intruder will try every room at once.", "Every gap you left is a door it can walk through.", "Show the evidence for every control." },
            16 => new[] { "02:46. The register was copied to an address abroad.", "Omar, Lina and Dana are on the line, all talking at once.", "Declare it, and give each a role." },
            17 => new[] { "The intruder holds the administrator's password and an open session.", "It is reading the register right now.", "Cut its access, in the right order." },
            18 => new[] { "The intruder walked from a clerk's inbox to the backup vault.", "The exit stays locked until the chain is told, link by link.", "Put the links in order." },
            19 => new[] { "The night is over. The panel wants the review.", "Four gaps, four actions, and not one name blamed.", "Then the exit opens." },
            _ => new[] { r.kicker },
        };
        return ("Intruder alarm", r.room, lines.ToList(), "Enter the " + r.room);
    }

    // The room's one-off decisions, as a card of their own: its name and its line.
    public static string ConsoleName(Room r) => r.number switch
    {
        4 => "Sign-in rules", 5 => "The summary", 8 => "Crossings", 12 => "The sign-in", 16 => "Declaration", 19 => "Trigger and cause", _ => "The room",
    };
    public static string ConsoleLine(Room r) => r.number switch
    {
        4 => "How many guesses an account allows, and whether known stolen passwords are refused.",
        5 => "The ransomware signs, and the three priority actions for the service.",
        8 => "What may cross from one zone to the next.",
        12 => "The replayed sign-in: an event, or an incident?",
        16 => "How bad it is, and the fact that says so.",
        19 => "What set it off, and what let it happen.",
        _ => "",
    };

    // The short name on a token's plate, at rest. The full name and the detail show on the card
    // brought close. Where the number is the lesson (the patch scores) it stays on the plate.
    static readonly Dictionary<(int, string), string> Names = new Dictionary<(int, string), string>
    {
        [(1, "register")] = "Register", [(1, "docs")] = "Documents", [(1, "portal")] = "Portal", [(1, "email")] = "Email", [(1, "backup")] = "Backups",
        [(1, "shared")] = "Shared drive", [(1, "printer")] = "Printer", [(1, "menu")] = "Menu",
        [(2, "phish")] = "Phishing", [(2, "ransom")] = "Ransomware", [(2, "portal")] = "Portal flaw", [(2, "laptop")] = "Lost laptop", [(2, "contractor")] = "Old admin", [(2, "kiosk")] = "Kiosk",
        [(3, "helpdesk")] = "Helpdesk", [(3, "omar")] = "Omar", [(3, "courier")] = "Courier", [(3, "newsletter")] = "Newsletter", [(3, "director")] = "Director", [(3, "invoice")] = "Invoice",
        [(4, "dana")] = "dana", [(4, "omar")] = "omar", [(4, "lina")] = "lina.admin", [(4, "kiosk")] = "kiosk", [(4, "nour")] = "nour", [(4, "backup")] = "backup-svc",
        [(5, "laptop")] = "Lost laptop", [(5, "ransom")] = "Locked files",
        [(6, "sami")] = "sami.contract", [(6, "dana")] = "dana", [(6, "omar")] = "omar", [(6, "lina")] = "lina.admin", [(6, "nour")] = "nour", [(6, "intern")] = "intern.2026",
        [(7, "test")] = "Test server 9.8", [(7, "portal")] = "Public portal 6.5", [(7, "files")] = "File server 8.1", [(7, "kiosk")] = "Kiosk 3.2", [(7, "admin")] = "Admin portal 7.5",
        [(8, "kiosk")] = "Kiosk", [(8, "portal")] = "Portal", [(8, "staff")] = "Staff PCs", [(8, "admin")] = "Admin portal", [(8, "register")] = "Register", [(8, "docs")] = "Documents", [(8, "backup")] = "Backups",
        [(9, "web")] = "Internet → Portal · Web (443)", [(9, "staffadmin")] = "Staff → Admin", [(9, "remote")] = "Internet → Admin · Remote Desktop (3389)", [(9, "backups")] = "Data → Backup · nightly", [(9, "all")] = "Anything → Anywhere",
        [(10, "register")] = "Register", [(10, "docs")] = "Documents", [(10, "backup")] = "Backups", [(10, "transit")] = "Portal traffic",
        [(11, "storage")] = "Storage", [(11, "creds")] = "Admin password", [(11, "metadata")] = "Internal address", [(11, "rights")] = "Web rights",
        [(12, "adminlog")] = "Sign-ins", [(12, "accesslog")] = "Access", [(12, "netlog")] = "Crossings", [(12, "cardlog")] = "Cards", [(12, "doclog")] = "Documents",
        [(12, "failalert")] = "Failed logins", [(12, "abroadalert")] = "Admin abroad", [(12, "exportalert")] = "Big export", [(12, "printeralert")] = "Printer offline",
        [(13, "isolate")] = "Isolate", [(13, "snapshot")] = "Copy disk", [(13, "wipe")] = "Wipe", [(13, "rebuild")] = "Rebuild", [(13, "patch")] = "Patch in place", [(13, "log")] = "Log the step",
        [(14, "where")] = "Where", [(14, "who")] = "Who reaches", [(14, "often")] = "How often", [(14, "enc")] = "Encrypted", [(14, "test")] = "Restore test",
        [(15, "phish")] = "Phishing", [(15, "ransom")] = "Ransomware", [(15, "portal")] = "Portal flaw", [(15, "contractor")] = "Leaver's account", [(15, "kiosk")] = "Kiosk", [(15, "laptop")] = "Lost laptop",
        [(16, "omar")] = "Omar", [(16, "lina")] = "Lina", [(16, "dana")] = "Dana",
        [(17, "snapshot")] = "Keep a copy", [(17, "rotate")] = "Rotate", [(17, "invalidate")] = "Invalidate", [(17, "revoke")] = "Revoke",
        [(18, "click")] = "The click", [(18, "mfa")] = "No second proof", [(18, "flat")] = "Flat network", [(18, "port")] = "Open port", [(18, "backup")] = "Same keys",
        [(19, "mfa")] = "No second proof", [(19, "flat")] = "Flat network", [(19, "port")] = "Open port", [(19, "backup")] = "Same keys",
    };
    public static string ItemName(Room r, Item i) => Names.TryGetValue((r.number, i.key), out var s) ? s : i.name;

    // A group's label over a set of tokens (the Operations Center's logs and alerts).
    public static string Group(Room r, Item i) => r.number == 12 ? (i.key == "failalert" ? "ALERTS" : i.key == "adminlog" ? "LOGS" : null) : null;

    // The switch's short name, on its plate while the card is close.
    static readonly Dictionary<string, string> FieldNames = new Dictionary<string, string>
    {
        ["If leaked"] = "Leaked", ["If changed"] = "Changed", ["If gone"] = "Gone", ["Second proof (MFA)"] = "MFA", ["The reason"] = "Reason", ["The report"] = "Report",
        ["What happened"] = "What", ["What is affected"] = "Affected", ["What was done so far"] = "Done so far", ["The ransomware signs"] = "Signs",
        ["Priority action 1"] = "1st", ["Priority action 2"] = "2nd", ["Priority action 3"] = "3rd", ["Ticket owner"] = "Owner",
        ["Public to Internal"] = "Public → Internal", ["Internal to Data"] = "Internal → Data", ["Public to Data"] = "Public → Data", ["The key kept"] = "Key",
        ["Kept / raised"] = "On", ["The replayed sign-in"] = "The sign-in", ["Do it"] = "Do it", ["Runbook line"] = "Runbook", ["This link is"] = "This link",
        ["Done when"] = "Done when", ["The sentence"] = "Words", ["The trigger"] = "Trigger", ["A root cause"] = "Root cause", ["Because"] = "Because",
    };
    public static string FieldName(Field f) => FieldNames.TryGetValue(f.label, out var s) ? s : f.label;

    // The option's short form on the value plate. Some settings read differently per item (the
    // cloud's "as it came" is public for the storage and default for the password).
    static readonly Dictionary<string, string> Options = new Dictionary<string, string>
    {
        ["Dana, clerk"] = "Dana", ["Omar, supervisor"] = "Omar", ["Lina, IT"] = "Lina", ["Nour, finance"] = "Nour",
        ["Know: the asset register"] = "The register", ["Protect: MFA on privileged accounts"] = "MFA", ["Protect: network zones"] = "Network zones", ["Protect: patch the public portal"] = "Patch portal",
        ["Protect: disk encryption on laptops"] = "Disk encryption", ["Notice: alerts on admin sign-ins"] = "Admin alerts", ["Respond: the incident playbook"] = "The playbook", ["Recover: backups the attack cannot reach"] = "Safe backups",
        ["The address behind the name"] = "The address", ["The link behind the text"] = "The link", ["It looked wrong"] = "Looked wrong",
        ["\"Looks phishy\""] = "Looks phishy", ["Who got it, when, what it asked, what was clicked, what was entered"] = "The facts",
        ["Limited, and stolen passwords refused"] = "Limited",
        ["A laptop with the register was lost; disk not encrypted"] = "Laptop lost, no encryption", ["Someone stole the laptop to sell the register"] = "Stolen to sell",
        ["Files on the shared drive are unreadable and renamed; a note asks for money"] = "Files locked, a note", ["A virus from the kiosk encrypted everything"] = "Kiosk virus",
        ["19:40 yesterday, noticed by Dana"] = "19:40, Dana", ["Sometime last week"] = "Last week", ["08:10 today, noticed by Omar"] = "08:10, Omar", ["Probably overnight"] = "Overnight",
        ["The register copy on one laptop"] = "One laptop's copy", ["The shared drive and two machines so far"] = "Drive + two machines",
        ["Nothing yet; the laptop password is a strong one"] = "Nothing yet", ["The thief was reported to the police by name"] = "Thief named to police",
        ["The two machines unplugged from the network; nothing deleted"] = "Two machines unplugged", ["The note was paid"] = "Note paid",
        ["Slow wifi and a new wallpaper"] = "Slow wifi, wallpaper", ["Files unreadable with new names, a note asking for money, machines slowing one after another"] = "Locked, a note, slowing", ["A full disk"] = "Full disk",
        ["Allowed, checked at the line"] = "Checked", ["Block and log"] = "Block + log", ["Needed by the service"] = "Needed", ["\"So nothing breaks\""] = "So nothing breaks",
        ["On the same server"] = "Same server", ["In the key service, with its own access"] = "Key service",
        ["An event: a sign-in, nothing more"] = "An event", ["An incident: an admin signed in from abroad at night and exported the register"] = "An incident",
        ["The room's signed, dated record"] = "Signed record", ["\"It is done, trust me\""] = "Trust me",
        ["Commander: decides, does no technical work"] = "Commander", ["Scribe: writes everything down, with times"] = "Scribe", ["Engineer: does the technical work"] = "Engineer",
        ["1: data exposed; declare within 15 minutes"] = "SEV-1 data exposed", ["2: a system down, no data"] = "SEV-2 system down", ["3: one user affected"] = "SEV-3 one user", ["4: an anomaly worth tracking"] = "SEV-4 anomaly",
        ["The register was exported to an address abroad at 02:31"] = "Register exported abroad", ["A printer is offline"] = "Printer offline", ["Dana cannot sign in"] = "Dana locked out",
        ["\"Consider\" it"] = "Consider it", ["A cause: a control gap"] = "A control gap",
        ["\"When it is fixed\""] = "When it is fixed", ["\"The control was missing; here is the action\""] = "Control missing: action", ["\"Dana should have known better\""] = "Dana should have known",
        ["The clerk's click on the helpdesk message"] = "The click", ["The administrator's missing second proof"] = "Missing second proof", ["Controls missing in four rooms"] = "Four missing controls",
    };
    static readonly Dictionary<(int, string, int), string> PerItem = new Dictionary<(int, string, int), string>
    {
        [(11, "storage", 1)] = "Public", [(11, "storage", 2)] = "Private", [(11, "creds", 1)] = "As delivered", [(11, "creds", 2)] = "Rotated",
        [(11, "metadata", 1)] = "Reachable", [(11, "metadata", 2)] = "Cut", [(11, "rights", 1)] = "Everything", [(11, "rights", 2)] = "Only its job",
        [(14, "where", 1)] = "Same network", [(14, "where", 2)] = "Own line", [(14, "who", 1)] = "Admin's password", [(14, "who", 2)] = "Own access",
        [(14, "often", 1)] = "Weekly", [(14, "often", 2)] = "Nightly", [(14, "enc", 1)] = "No", [(14, "enc", 2)] = "Yes", [(14, "test", 1)] = "Never", [(14, "test", 2)] = "Tested, dated",
        [(19, "mfa", 1)] = "Two proofs everywhere", [(19, "flat", 1)] = "Kiosk cut from data", [(19, "port", 1)] = "3389 blocked, logged", [(19, "backup", 1)] = "Restore from vault works",
    };
    public static string Option(Room r, string itemKey, Field f, int index)
    {
        if (index < 0 || index >= f.options.Length) return "?";
        if (PerItem.TryGetValue((r.number, itemKey, index), out var p) && (f.key == "value" || f.key == "done")) return p;
        string o = f.options[index];
        if (Options.TryGetValue(o, out var s)) return s;
        int colon = o.IndexOf(':');
        if (colon > 0 && colon <= 24) return o.Substring(0, colon);
        var words = o.Split(' ');
        return words.Length <= 4 ? o : string.Join(" ", words, 0, 4) + "…";
    }

    // The value on a row of the card brought close: the option in full when it fits a line,
    // its short form otherwise.
    public static string Long(Room r, string itemKey, Field f, int index)
    {
        if (index < 0 || index >= f.options.Length) return "?";
        if (PerItem.TryGetValue((r.number, itemKey, index), out var p) && (f.key == "value" || f.key == "done")) return p;
        string o = f.options[index].Replace("\"", "");
        return o.Length <= 34 ? o : Option(r, itemKey, f, index);
    }

    // A choice shown red rather than teal: the one that closes a door, or marks a danger.
    public static bool Danger(Field f, int index) => index >= 0 && index < f.options.Length &&
        (f.options[index] == "Block and log" || f.options[index] == "Suspicious" || f.options[index] == "Remove" || f.options[index] == "Must change");

    // The things the planned mistake lives in: they pulse after a rehearsal.
    public static string[] Mistake(Room r) => r.number switch
    {
        1 => new[] { "shared" }, 2 => new[] { "phish", "portal" }, 3 => new[] { "helpdesk" }, 4 => new[] { "lina" }, 6 => new[] { "sami" }, 7 => new[] { "portal" },
        8 => new[] { "kiosk", "register" }, 9 => new[] { "remote", "all" }, 10 => new[] { "register", "backup" }, 11 => new[] { "storage", "metadata" }, 12 => new[] { "adminlog", "failalert" },
        13 => new[] { "wipe", "snapshot" }, 14 => new[] { "where", "who" }, 16 => new[] { "lina" }, 17 => new[] { "snapshot", "invalidate" }, 18 => new[] { "click" }, _ => new string[0],
    };
}
