using System.Collections.Generic;

// Short forms of the curriculum's check labels, for the debrief's lamps: the full
// label stays in the file and in the report the platform keeps. Anything not
// listed here is cut at its colon or its fifth word.
public static class Labels
{
    static readonly Dictionary<string, string> Short = new Dictionary<string, string>
    {
        ["Administration unreachable from the public side"] = "Admin closed to the public side",
        ["An alert that fired on the intrusion and not on the noise"] = "An alert on the intrusion, not the noise",
        ["An incident log with a time for each step"] = "A timed incident log",
        ["At least one cause is a control gap from tier 2"] = "A cause from a tier 2 gap",
        ["Backups unreachable from the zones the attack ran in"] = "Backups out of the attack's reach",
        ["Blocks are logged, by the last rule"] = "Blocks logged by the last rule",
        ["Both forms have the facts a responder needs"] = "Both forms carry the facts",
        ["Each control picked for the risk it lowers, with its Function"] = "Each control fits its risk",
        ["Each crossing is listed and justified"] = "Each crossing listed and justified",
        ["Each has an impact in the three terms"] = "An impact in the three terms",
        ["Each link names its room and its gap"] = "Each link names room and gap",
        ["Each report has the facts a responder needs"] = "Each report carries the facts",
        ["Each service's rights limited to its job"] = "Each service limited to its job",
        ["Every action has an owner and a due date"] = "Every action owned and dated",
        ["Every asset in the facility is on the register"] = "Every asset on the register",
        ["Every cause has an action with a done-when"] = "Every cause has a done-when",
        ["Every control has evidence that would satisfy an auditor"] = "Evidence for every control",
        ["Every high risk has a treatment decision"] = "Every high risk treated",
        ["Every link in the record is in the chain, in order"] = "The chain complete, in order",
        ["Every privileged account has MFA"] = "MFA on every privileged account",
        ["Every suspicious message caught, with a reason that is not \"it looked wrong\""] = "Every suspicious message caught, with a reason",
        ["Every ticket has an owner and a date"] = "Every ticket owned and dated",
        ["Evidence kept before anything is removed"] = "Evidence kept first",
        ["Evidence of each, dated and signed"] = "Evidence, dated and signed",
        ["No account for a job that no longer exists"] = "No account for a gone job",
        ["No account with more than its job needs"] = "No account above its job",
        ["No high risk without a control"] = "No high risk uncontrolled",
        ["No key on the same system as its data"] = "No key beside its data",
        ["No private data in the logs"] = "No private data in logs",
        ["No public storage of sensitive assets"] = "Sensitive storage not public",
        ["Nothing exploitable today rated below High"] = "Exploitable today rated High",
        ["Nothing on the form is a guess presented as a fact"] = "No guess presented as fact",
        ["Nothing reachable from outside on a high-value asset left open"] = "No reachable high-value gap left",
        ["Ordered by severity, exposure and asset value, not score alone"] = "Ordered by severity, exposure, value",
        ["Rated likelihood and impact for every risk"] = "Every risk rated",
        ["Rotate, invalidate, revoke, in that order"] = "Rotate, invalidate, revoke, in order",
        ["Severity declared within the time limit and justified by the facts"] = "Severity declared in time, justified",
        ["Sign-in, access and network logs kept for the sensitive assets"] = "Sign-in, access, network logs kept",
        ["The commander made no technical change"] = "The commander made no change",
        ["The decision, event or incident, is right and gives a reason"] = "Event or incident: right, with a reason",
        ["The evidence says what was checked and when"] = "Evidence says what and when",
        ["The internal address unreachable from services that do not need it"] = "Internal address closed to the rest",
        ["The priority actions trace back to the tier 1 register"] = "Priorities trace to the register",
        ["The public zone cannot reach the data zone"] = "Public zone cannot reach data",
        ["The record names who approved each account kept"] = "Each kept account has an approver",
        ["The runbook has no \"consider\""] = "No \"consider\" in the runbook",
        ["The sensitive assets are encrypted at rest"] = "Sensitive assets encrypted at rest",
        ["The three actions each tie to a high risk and its control"] = "Three actions tied to high risks",
        ["The trace is complete from the first sign-in to the data touched"] = "The trace complete, sign-in to data",
        ["The trigger and the root causes are distinct"] = "Trigger and root causes distinct",
        ["The trigger is marked, and is not called the cause"] = "The trigger marked, not the cause",
        ["The whole-facility attack got through nowhere"] = "The attack got through nowhere",
        ["Their own access, not the administrator's"] = "Own access, not the admin's",
        ["Traffic across the public line is encrypted"] = "Public-line traffic encrypted",
        ["Whatever got through is in the record with the room and the gap"] = "What got through is in the record",
    };

    public static string Of(string full)
    {
        if (string.IsNullOrEmpty(full)) return "";
        if (Short.TryGetValue(full, out var s)) return s;
        int colon = full.IndexOf(':');
        if (colon > 0 && colon <= 30) return full.Substring(0, colon);
        var words = full.Split(' ');
        return words.Length <= 6 ? full : string.Join(" ", words, 0, 6) + "…";
    }

    // The first sentence or two of a longer line, for a coach strip: at least sixty
    // characters, whole sentences, the rest left to the file or the debrief.
    public static string Lead(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        int at = 0;
        while (at < text.Length)
        {
            int dot = text.IndexOf(". ", at);
            if (dot < 0) return text;
            at = dot + 1;
            if (at >= 60) return text.Substring(0, at);
        }
        return text;
    }
}
