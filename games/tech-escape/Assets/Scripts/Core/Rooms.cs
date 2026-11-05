using System;
using System.Collections.Generic;
using System.Linq;

// The nineteen levels, from Docs/curriculum/tech-escape.md: the room, what the
// student fixes, the planned mistake, the attack the facility runs, the checks.
public static class Rooms
{
    static Practised Of(bool ok) => ok ? Practised.Yes : Practised.Missed;
    static Field F(string key, string label, params string[] options) => new Field { key = key, label = label, options = options };
    static Field Once(string key, string label, params string[] options) => new Field { key = key, label = label, options = options, once = true };
    static Item I(string key, string name, string detail = "") => new Item(key, name, detail);
    static Dictionary<string, int> D(params (string key, int v)[] pairs) => pairs.ToDictionary(p => p.key, p => p.v);

    // The three ways an asset can be lost, and the people, used in several rooms.
    static readonly string[] Owners = { "Nobody", "Dana, clerk", "Omar, supervisor", "Lina, IT", "Nour, finance" };
    static readonly string[] Levels = { "?", "Low", "Medium", "High" };
    static readonly string[] Controls =
    {
        "Know: the asset register", "Protect: MFA on privileged accounts", "Protect: network zones", "Protect: patch the public portal",
        "Protect: disk encryption on laptops", "Notice: alerts on admin sign-ins", "Respond: the incident playbook", "Recover: backups the attack cannot reach",
    };

    public static readonly Room[] All =
    {
        // ---- Tier 1: what is at risk --------------------------------------------------
        new Room
        {
            number = 1, tier = 1, id = "1.1", room = "Risk Room", title = "What is worth protecting?",
            kicker = "You cannot protect what you have not listed, and nobody protects what nobody owns.",
            luma = "Welcome to the Harbor Permits Office. Before anything else: what does this service hold, who owns each thing, and what would its loss cost?",
            learn = "Every asset has an owner, a sensitivity, and a cost if it is leaked (kept private), changed (kept correct) or gone (kept available). " +
                    "The same asset can fail in all three ways.\n\nThe register below lists the facility's systems. One thing is not on it. Find it, add it, give it an owner.",
            fixes = "The asset register: for each system, what it holds, who owns it, how sensitive, and the impact of its loss in each of the three ways.",
            mistake = "An asset nobody listed", why = "The ready register misses the working copy of applicants' documents kept on the shared drive, outside the main system. The facility's attack takes exactly that one.",
            items = new[]
            {
                I("register", "Permit register", "Every application and decision, in the main database."),
                I("docs", "Document store", "Applicants' identity papers and proofs, in the main system."),
                I("portal", "Permit portal", "The public website where people apply and pay."),
                I("email", "Staff email", "The clerks' and supervisors' mailboxes."),
                I("backup", "Backup server", "Copies of the register and the documents."),
                new Item("shared", "Working copy of applicants' documents on the shared drive", "Kept outside the main system by the clerks, for speed.") { hidden = true },
                new Item("printer", "The lobby printer", "Prints permits for collection.") { hidden = true, decoy = true },
                new Item("menu", "The canteen menu", "On the noticeboard.") { hidden = true, decoy = true },
            },
            fields = new[] { F("owner", "Owner", Owners), F("sens", "Sensitivity", "?", "Public", "Internal", "Confidential"), F("leak", "If leaked", Levels), F("change", "If changed", Levels), F("gone", "If gone", Levels) },
            ready = D(("register/owner", 2), ("register/sens", 3), ("register/leak", 3), ("register/change", 3), ("register/gone", 3),
                      ("docs/owner", 2), ("docs/sens", 3), ("docs/leak", 3), ("docs/change", 2), ("docs/gone", 2),
                      ("portal/owner", 3), ("portal/sens", 1), ("portal/leak", 1), ("portal/change", 3), ("portal/gone", 3),
                      ("email/owner", 3), ("email/sens", 2), ("email/leak", 2), ("email/change", 2), ("email/gone", 2),
                      ("backup/owner", 3), ("backup/sens", 3), ("backup/leak", 3), ("backup/change", 3), ("backup/gone", 3),
                      ("shared/owner", 1), ("shared/sens", 3), ("shared/leak", 3), ("shared/change", 2), ("shared/gone", 2)),
            broken = D(("register/owner", 2), ("register/sens", 3), ("register/leak", 3), ("register/change", 3), ("register/gone", 3),
                       ("docs/owner", 2), ("docs/sens", 3), ("docs/leak", 3), ("docs/change", 2), ("docs/gone", 2),
                       ("portal/owner", 3), ("portal/sens", 1), ("portal/leak", 1), ("portal/change", 3), ("portal/gone", 3),
                       ("email/owner", 3), ("email/sens", 2), ("email/leak", 2), ("email/change", 2), ("email/gone", 2),
                       ("backup/owner", 3), ("backup/sens", 3), ("backup/leak", 3), ("backup/change", 3), ("backup/gone", 3)),
            live = "The attack takes whatever is not on the list. Watch which door it goes through.",
            takeaway = "You cannot protect what you have not listed, and nobody protects what nobody owns.",
            ask = "Which information do I handle that is on nobody's list?",
            attack = (st, x) =>
            {
                var a = new Attack();
                if (!st.added.Contains("shared")) a.Through("Risk Room", "an asset nobody listed", "the working copy of applicants' documents on the shared drive: copied. Kept private: lost.");
                else if (st.Pick("shared", "owner") <= 0) a.Through("Risk Room", "an asset with no owner", "the working copy on the shared drive: nobody noticed it was gone. Kept private: lost.");
                else a.Held("Risk Room", "every asset listed and owned; the attack found nothing unlisted");
                a.summary = a.AnyThrough ? "The attacker took the one thing nobody had written down." : "The register held: nothing unlisted to take.";
                return a;
            },
            check = (st, x, a) =>
            {
                var listed = new[] { "register", "docs", "portal", "email", "backup", "shared" };
                bool all = st.added.Contains("shared"), owners = listed.All(k => st.Pick(k, "owner") > 0), impacts = listed.All(k => st.Pick(k, "leak") > 0 && st.Pick(k, "change") > 0 && st.Pick(k, "gone") > 0);
                return new List<(Practised, string)>
                {
                    (Of(all && !st.added.Contains("printer") && !st.added.Contains("menu")), "Every asset in the facility is on the register"),
                    (Of(owners), "Each has an owner"),
                    (Of(impacts), "Each has an impact in the three terms"),
                    (Of(all), "The unlisted one was found"),
                };
            },
        },
        new Room
        {
            number = 2, tier = 1, id = "1.2", room = "Risk Room", title = "How likely, how bad?",
            kicker = "Rate the risk, then pick the control for the risk. Not the other way round.",
            luma = "A risk is someone using a weakness against an asset. Rate how likely, then how bad. High risks get a control that fits, and every control does one of: know, protect, notice, respond, recover.",
            learn = "A risk is someone (threat) using a weakness (vulnerability) against an asset. Rate how likely, then how bad, on the 5 by 5 grid (risk matrix). " +
                    "High risks get a control; a control is chosen for the risk it lowers, and does one of: know, protect, notice, respond, recover.\n\n" +
                    "If it is exploitable today with no special access, it is High. Rating everything Medium avoids an argument and protects nothing.",
            fixes = "The risk matrix for the register, and a control for each high risk, picked from a short list grouped by what it does.",
            mistake = "Everything rated Medium", why = "The ready matrix rates every risk 3 by 3 and gives each the same cheap control. The facility runs the top three threats; the ones that were really High, with no fitting control, get through.",
            items = new[]
            {
                I("phish", "A phishing mail to a clerk takes a password", "Threat: anyone with an email address. Weakness: people. Exploitable today, no special access."),
                I("ransom", "Ransomware on the file server", "Threat: a criminal group. Weakness: a clerk's click, a flat network."),
                I("portal", "A weakness in the public portal is used", "Threat: anyone on the internet. Weakness: an unpatched portal. Exploitable today, no special access."),
                I("laptop", "A laptop with the register is lost", "Threat: whoever finds it. Weakness: an unencrypted disk."),
                I("contractor", "A contractor's old admin account is used", "Threat: a former contractor, or whoever has their password. Weakness: access never taken back."),
                I("kiosk", "The lobby kiosk reaches the database", "Threat: a visitor. Weakness: one flat network."),
            },
            fields = new[] { F("like", "Likelihood", "1", "2", "3", "4", "5"), F("impact", "Impact", "1", "2", "3", "4", "5"), F("control", "Control", Controls) },
            ready = D(("phish/like", 4), ("phish/impact", 3), ("phish/control", 1), ("ransom/like", 2), ("ransom/impact", 4), ("ransom/control", 7),
                      ("portal/like", 4), ("portal/impact", 4), ("portal/control", 3), ("laptop/like", 2), ("laptop/impact", 3), ("laptop/control", 4),
                      ("contractor/like", 3), ("contractor/impact", 4), ("contractor/control", 1), ("kiosk/like", 2), ("kiosk/impact", 3), ("kiosk/control", 2)),
            broken = D(("phish/like", 2), ("phish/impact", 2), ("phish/control", 0), ("ransom/like", 2), ("ransom/impact", 2), ("ransom/control", 0),
                       ("portal/like", 2), ("portal/impact", 2), ("portal/control", 0), ("laptop/like", 2), ("laptop/impact", 2), ("laptop/control", 0),
                       ("contractor/like", 2), ("contractor/impact", 2), ("contractor/control", 0), ("kiosk/like", 2), ("kiosk/impact", 2), ("kiosk/control", 0)),
            recall = new Recall { question = "Name the three ways an asset can be lost.", options = new[] { "Leaked, changed, gone: private, correct, available", "Stolen, sold, forgotten", "Hacked, phished, lost" }, answer = 0, from = "1.1" },
            live = "The facility runs the three biggest threats. For each, your rating is shown next to the real one, and whether the control you picked could ever have stopped it.",
            takeaway = "Rate the risk, then pick the control for the risk. Not the other way round.",
            ask = "Which risk at work do we rate Medium to avoid an argument?",
            attack = (st, x) =>
            {
                var a = new Attack();
                var fits = new Dictionary<string, int> { ["phish"] = 1, ["ransom"] = 7, ["portal"] = 3 };
                var names = new Dictionary<string, string> { ["phish"] = "the phishing mail", ["ransom"] = "the ransomware", ["portal"] = "the portal weakness" };
                var reaches = new Dictionary<string, string> { ["phish"] = "a clerk's password", ["ransom"] = "the file server, locked", ["portal"] = "the permit register" };
                foreach (var k in fits.Keys)
                {
                    bool high = st.Pick(k, "like") >= 3 && st.Pick(k, "impact") >= 2 || st.Pick(k, "like") + st.Pick(k, "impact") >= 6;
                    bool fitting = st.Pick(k, "control") == fits[k] || (k == "ransom" && st.Pick(k, "control") == 2);
                    if (high && fitting) a.Held("Risk Room", $"{names[k]}: rated High, and {Controls[st.Pick(k, "control")].ToLowerInvariant()} stopped it");
                    else a.Through("Risk Room", $"{names[k]} rated {(high ? "High" : "Medium")} with {(st.Pick(k, "control") >= 0 ? Controls[st.Pick(k, "control")].ToLowerInvariant() : "no control")}", reaches[k] + (fitting ? "" : ": the control could never have stopped it"));
                }
                a.summary = a.AnyThrough ? "What was really High, with a control that did not fit, got through." : "Every real High had a control that fit.";
                return a;
            },
            check = (st, x, a) =>
            {
                var all = new[] { "phish", "ransom", "portal", "laptop", "contractor", "kiosk" };
                bool High(string k) => st.Pick(k, "like") + st.Pick(k, "impact") >= 6;
                bool noHighWithout = all.Where(High).All(k => st.Pick(k, "control") >= 0);
                bool notDefault = all.Where(High).All(k => st.Pick(k, "control") != 0);
                bool exploitable = st.Pick("phish", "like") >= 3 && st.Pick("portal", "like") >= 3;
                return new List<(Practised, string)>
                {
                    (Of(all.All(k => st.Pick(k, "like") >= 0 && st.Pick(k, "impact") >= 0)), "Rated likelihood and impact for every risk"),
                    (Of(noHighWithout), "No high risk without a control"),
                    (Of(notDefault), "Each control picked for the risk it lowers, with its Function"),
                    (Of(exploitable), "Nothing exploitable today rated below High"),
                };
            },
        },
        new Room
        {
            number = 3, tier = 1, id = "1.3", room = "Suspicious Inbox", title = "Which of these is an attack?",
            kicker = "The look is the attack. The ask, the address and the link are the evidence.",
            luma = "Six messages in a clerk's inbox. A phishing message is built to look right. The reason it is suspicious is never the look.",
            learn = "A phishing message is built to look right: the logo, the sender's name, the urgency, the familiar request. The reason it is suspicious is never the look; " +
                    "it is the ask, the address behind the name, the link behind the text, the pressure.\n\nA report says who got it, when, what it asked, what was clicked and what was entered.",
            fixes = "The inbox sort: safe or suspicious, with a reason per message, and a report for each suspicious one.",
            mistake = "The one that looked right", why = "The ready sort marks the helpdesk message safe because everything visible is right. The facility runs the campaign; that message gets a click and a sign-in.",
            items = new[]
            {
                I("helpdesk", "IT Helpdesk: \"Your password expires today. Sign in to keep your access.\"", "From: helpdesk@permlts-office.gov (one letter off). Link: permits-office-support.com. The logo and the name are right."),
                I("omar", "Omar: \"Can you send me the Hassan file before the 3pm meeting?\"", "From: omar.k@permits-office.gov. No link. The usual request."),
                I("courier", "Courier: \"Your parcel could not be delivered. Pay 2.50 to reschedule.\"", "From: no-reply@parcel-fee-center.net. Link to a payment page. Nobody ordered a parcel."),
                I("newsletter", "City newsletter: \"Road closures this weekend.\"", "From: news@harbor-city.gov. Link to the city site. Expected, monthly."),
                I("director", "The Director: \"I'm in a meeting, buy six gift cards and send me the codes. Urgent.\"", "From: director.permits@gmail.com. No link. Pressure, an odd ask, a personal address."),
                I("invoice", "Supplier: \"Invoice 4471 attached\" with a file to open", "From: accounts@printsupply-co.com, a real supplier. The attachment is a .zip the supplier never sends."),
            },
            fields = new[]
            {
                F("verdict", "Verdict", "Safe", "Suspicious"),
                F("reason", "The reason", "Nothing wrong", "The ask", "The address behind the name", "The link behind the text", "The pressure", "The attachment", "It looked wrong"),
                F("report", "The report", "None", "\"Looks phishy\"", "Who got it, when, what it asked, what was clicked, what was entered"),
            },
            ready = D(("helpdesk/verdict", 1), ("helpdesk/reason", 2), ("helpdesk/report", 2), ("omar/verdict", 0), ("omar/reason", 0), ("omar/report", 0),
                      ("courier/verdict", 1), ("courier/reason", 3), ("courier/report", 2), ("newsletter/verdict", 0), ("newsletter/reason", 0), ("newsletter/report", 0),
                      ("director/verdict", 1), ("director/reason", 1), ("director/report", 2), ("invoice/verdict", 1), ("invoice/reason", 5), ("invoice/report", 2)),
            broken = D(("helpdesk/verdict", 0), ("helpdesk/reason", 0), ("helpdesk/report", 0), ("omar/verdict", 0), ("omar/reason", 0), ("omar/report", 0),
                       ("courier/verdict", 1), ("courier/reason", 3), ("courier/report", 2), ("newsletter/verdict", 0), ("newsletter/reason", 0), ("newsletter/report", 0),
                       ("director/verdict", 1), ("director/reason", 1), ("director/report", 2), ("invoice/verdict", 1), ("invoice/reason", 5), ("invoice/report", 2)),
            recall = new Recall { question = "Which do you rate first, likelihood or impact?", options = new[] { "Likelihood, then impact", "Impact, then likelihood", "Both at once" }, answer = 0, from = "1.2" },
            live = "The campaign runs. Any message you marked safe gets opened. Watch which one gets a click, and what the attacker holds afterwards.",
            takeaway = "The look is the attack. The ask, the address and the link are the evidence.",
            ask = "What would I do first if I had already clicked?",
            attack = (st, x) =>
            {
                var a = new Attack();
                var bad = new Dictionary<string, (string click, string holds)>
                {
                    ["helpdesk"] = ("Dana clicked and signed in on the fake page", "Dana's password"),
                    ["courier"] = ("Dana paid the 2.50 with the office card", "the office card number"),
                    ["director"] = ("Dana bought the gift cards", "600 in gift cards"),
                    ["invoice"] = ("Dana opened the attachment", "a foothold on Dana's computer"),
                };
                foreach (var kv in bad)
                    if (st.Pick(kv.Key, "verdict") != 1) a.Through("Suspicious Inbox", $"a message marked safe: {kv.Key}", $"{kv.Value.click}. The attacker now holds {kv.Value.holds}.");
                    else a.Held("Suspicious Inbox", $"{kv.Key}: reported, nobody clicked");
                a.summary = a.AnyThrough ? "The message that looked right got the click." : "Every attack caught; nothing clicked.";
                return a;
            },
            check = (st, x, a) =>
            {
                var bad = new[] { "helpdesk", "courier", "director", "invoice" }; var good = new[] { "omar", "newsletter" };
                return new List<(Practised, string)>
                {
                    (Of(bad.All(k => st.Pick(k, "verdict") == 1 && st.Pick(k, "reason") >= 1 && st.Pick(k, "reason") <= 5)), "Every suspicious message caught, with a reason that is not \"it looked wrong\""),
                    (Of(good.All(k => st.Pick(k, "verdict") == 0)), "No safe message reported"),
                    (Of(bad.All(k => st.Pick(k, "report") == 2)), "Each report has the facts a responder needs"),
                };
            },
        },
        new Room
        {
            number = 4, tier = 1, id = "1.4", room = "Password Vault", title = "Who can get in?",
            kicker = "A password alone is one proof. The account that matters most needs two.",
            luma = "Six accounts. A password that is short, guessable or reused is already stolen; it just has not been used yet. A second proof means the stolen password is not enough.",
            learn = "A password that is short, guessable or reused elsewhere is already stolen; it just has not been used yet. A second proof it is you (multi-factor authentication, MFA) " +
                    "means the stolen password is not enough.\n\nMFA raises the cost; it does not make guessing impossible. So guesses are also limited, and known-stolen passwords refused.",
            fixes = "The account check: for each account, the password's strength and reuse, whether MFA is on, and whether guesses are limited.",
            mistake = "MFA for everyone but one", why = "MFA is on for every account except the administrator's, \"because it gets in the way\". The facility runs Dana's phished password and a list of stolen passwords against every account. The administrator's falls.",
            items = new[]
            {
                I("dana", "dana (clerk)", "Password Summer2024!, also used on a shopping site. Phished in the inbox."),
                I("omar", "omar (supervisor)", "A long password, used nowhere else."),
                I("lina", "lina.admin (administrator)", "A long password. Privileged: can change anything."),
                I("kiosk", "kiosk (the lobby kiosk)", "Password kiosk123, shared."),
                I("nour", "nour (finance)", "Password reused from a personal account that leaked last year."),
                I("backup", "backup-svc (service account)", "A long password. Privileged: reaches the backups."),
            },
            fields = new[] { F("mfa", "Second proof (MFA)", "Off", "On"), F("pw", "Password", "Keep", "Must change"), Once("guesses", "Guesses", "Unlimited", "Limited, and stolen passwords refused") },
            ready = D(("dana/mfa", 1), ("dana/pw", 1), ("omar/mfa", 1), ("omar/pw", 0), ("lina/mfa", 1), ("lina/pw", 0), ("kiosk/mfa", 0), ("kiosk/pw", 1), ("nour/mfa", 1), ("nour/pw", 1), ("backup/mfa", 1), ("backup/pw", 0), ("room/guesses", 1)),
            broken = D(("dana/mfa", 1), ("dana/pw", 0), ("omar/mfa", 1), ("omar/pw", 0), ("lina/mfa", 0), ("lina/pw", 0), ("kiosk/mfa", 1), ("kiosk/pw", 0), ("nour/mfa", 1), ("nour/pw", 0), ("backup/mfa", 1), ("backup/pw", 0), ("room/guesses", 0)),
            recall = new Recall { question = "Name two facts a responder needs in a phishing report.", options = new[] { "Who got it and what was clicked", "The logo and the font", "The time of day and the weather" }, answer = 0, from = "1.3" },
            live = "Three routes: the phished password, a reused one, a guessed one. Watch which accounts fall, and by which route.",
            takeaway = "A password alone is one proof. The account that matters most needs two.",
            ask = "Which of my accounts share a password, and which one has no second proof?",
            attack = (st, x) =>
            {
                var a = new Attack();
                bool limited = st.Pick("room", "guesses") == 1;
                if (st.Pick("dana", "mfa") == 0 && st.Pick("dana", "pw") == 0) a.Through("Password Vault", "a phished password with no second proof", "dana: signed in with the phished password"); else a.Held("Password Vault", "dana: the phished password was not enough");
                if (st.Pick("nour", "pw") == 0 && (st.Pick("nour", "mfa") == 0 || !limited)) a.Through("Password Vault", "a reused password", "nour: signed in with a password from last year's leak"); else a.Held("Password Vault", "nour: the reused password refused");
                if (st.Pick("kiosk", "pw") == 0 && !limited) a.Through("Password Vault", "a guessable password and unlimited guesses", "kiosk: guessed on the 40th try"); else a.Held("Password Vault", "kiosk: guesses ran out");
                if (st.Pick("lina", "mfa") == 0) a.Through("Password Vault", "the administrator with no second proof", "lina.admin: the stolen list had an old password of hers; it worked, and it can change anything"); else a.Held("Password Vault", "lina.admin: held, two proofs");
                a.summary = a.AnyThrough ? "The account that mattered most had one proof." : "Every privileged account held.";
                return a;
            },
            check = (st, x, a) => new List<(Practised, string)>
            {
                (Of(st.Pick("lina", "mfa") == 1 && st.Pick("backup", "mfa") == 1), "Every privileged account has MFA"),
                (Of(st.Pick("dana", "pw") == 1 && st.Pick("kiosk", "pw") == 1 && st.Pick("nour", "pw") == 1), "Weak and reused passwords flagged"),
                (Of(st.Pick("room", "guesses") == 1), "Guesses limited"),
                (Of(a != null && !a.lines.Any(l => !l.held && l.reached.StartsWith("lina.admin"))), "The administrator's account held"),
            },
        },
        new Room
        {
            number = 5, tier = 1, id = "1.5", room = "Malware Containment", title = "Capstone: Report it",
            kicker = "At this level the job is to notice and report well. Responding comes next.",
            luma = "Two scenes, one form each: a lost laptop, then files turning unreadable with a note. Then your three priority actions for the service. Facts, not guesses.",
            learn = "Software that locks your files and asks for money (ransomware) shows itself: files unreadable with new names, a note, machines slowing one after another. " +
                    "A lost device holds whatever was on it.\n\nThe one-page report says what happened, when, who noticed, what is affected, what was done so far. Nothing on it is a guess presented as a fact.",
            fixes = "The one-page form for each scene, and the one-page summary: the three highest risks, the control for each, and why these three first.",
            items = new[]
            {
                I("laptop", "Scene 1: Dana's laptop, with the permit register on it, is lost after the train home.", "Dana noticed at 19:40. The disk was not encrypted."),
                I("ransom", "Scene 2: Files on the shared drive turn unreadable, each renamed .locked, and a note appears asking for payment.", "Omar noticed at 08:10. Two more machines slowed by 08:30."),
            },
            fields = new[]
            {
                F("what", "What happened", "?", "A laptop with the register was lost; disk not encrypted", "Someone stole the laptop to sell the register", "Files on the shared drive are unreadable and renamed; a note asks for money", "A virus from the kiosk encrypted everything"),
                F("when", "When", "?", "19:40 yesterday, noticed by Dana", "Sometime last week", "08:10 today, noticed by Omar", "Probably overnight"),
                F("affected", "What is affected", "?", "The register copy on one laptop", "Every system", "The shared drive and two machines so far", "Nothing important"),
                F("done", "What was done so far", "?", "Nothing yet; the laptop password is a strong one", "The thief was reported to the police by name", "The two machines unplugged from the network; nothing deleted", "The note was paid"),
                Once("signs", "The ransomware signs", "?", "Slow wifi and a new wallpaper", "Files unreadable with new names, a note asking for money, machines slowing one after another", "A full disk"),
                Once("a1", "Priority action 1", Controls), Once("a2", "Priority action 2", Controls), Once("a3", "Priority action 3", Controls),
            },
            ready = D(("laptop/what", 1), ("laptop/when", 1), ("laptop/affected", 1), ("laptop/done", 1), ("ransom/what", 3), ("ransom/when", 3), ("ransom/affected", 3), ("ransom/done", 3), ("room/signs", 2), ("room/a1", 1), ("room/a2", 3), ("room/a3", 7)),
            recall = new Recall { question = "What does MFA change about a stolen password?", options = new[] { "It is no longer enough on its own", "It cannot be stolen", "It becomes longer" }, answer = 0, from = "1.4" },
            live = "The responder reads your form. A guess written as a fact sends them the wrong way.",
            takeaway = "At this level the job is to notice and report well. Responding comes next.",
            ask = "Who do I call, and what will they ask me?",
            attack = (st, x) =>
            {
                var a = new Attack();
                bool guess = st.Pick("laptop", "what") == 2 || st.Pick("ransom", "what") == 4 || st.Pick("laptop", "when") == 2 || st.Pick("laptop", "when") == 4 || st.Pick("ransom", "when") == 4 || st.Pick("laptop", "done") == 2;
                if (guess) a.Through("Malware Containment", "a guess presented as a fact", "the responder spent the first hour on the wrong lead"); else a.Held("Malware Containment", "the forms gave the responder facts");
                a.summary = guess ? "The report sent the responder the wrong way." : "The responder had what they needed.";
                return a;
            },
            check = (st, x, a) =>
            {
                bool facts = st.Pick("laptop", "what") == 1 && st.Pick("laptop", "when") == 1 && st.Pick("laptop", "affected") == 1 && st.Pick("laptop", "done") == 1
                          && st.Pick("ransom", "what") == 3 && st.Pick("ransom", "when") == 3 && st.Pick("ransom", "affected") == 3 && st.Pick("ransom", "done") == 3;
                var picks = new[] { st.Pick("room", "a1"), st.Pick("room", "a2"), st.Pick("room", "a3") };
                bool actions = picks.All(p => p >= 1) && picks.Distinct().Count() == 3 && picks.Contains(1) && picks.Contains(3);
                return new List<(Practised, string)>
                {
                    (Of(facts), "Both forms have the facts a responder needs"),
                    (Of(st.Pick("room", "signs") == 2), "The ransomware signs named"),
                    (Of(a != null && !a.AnyThrough), "Nothing on the form is a guess presented as a fact"),
                    (Of(actions), "The three actions each tie to a high risk and its control"),
                };
            },
        },

        // ---- Tier 2: prove the controls work -------------------------------------------
        new Room
        {
            number = 6, tier = 2, id = "2.1", room = "Identity Chamber", title = "Who should still have access?",
            kicker = "Only the access the job needs, reviewed on a date, signed by a name.",
            luma = "Access is granted for a job and almost never taken back. Review every account: does the job still exist, does it need this much, who approved it. Then sign and date it.",
            learn = "Access is granted for a job and almost never taken back. Review every account: does the job still exist, does it need this much, who approved it (least privilege). " +
                    "The review is only done when it is written down, dated and signed.",
            fixes = "The access list: each account, its role, what it can reach, and the decision: keep, reduce, remove.",
            mistake = "The leaver with admin", why = "A contractor who left in June still has an administrator account. The facility runs the attack with that account's stolen password.",
            signable = true,
            items = new[]
            {
                I("sami", "sami.contract", "Contractor, left in June. Administrator of the register."),
                I("dana", "dana", "Clerk. Reads and writes the register; views payments."),
                I("omar", "omar", "Supervisor. Approves permits; admin portal."),
                I("lina", "lina.admin", "IT. Administrator of everything."),
                I("nour", "nour", "Finance. Payments, plus full write access to the register she never uses."),
                I("intern", "intern.2026", "Summer intern. Administrator of the document store, \"to help out\"."),
            },
            fields = new[] { F("decision", "Decision", "?", "Keep", "Reduce", "Remove"), F("approved", "Approved by", "Nobody", "Omar", "Lina") },
            ready = D(("sami/decision", 3), ("sami/approved", 2), ("dana/decision", 1), ("dana/approved", 1), ("omar/decision", 1), ("omar/approved", 2), ("lina/decision", 1), ("lina/approved", 1), ("nour/decision", 2), ("nour/approved", 1), ("intern/decision", 2), ("intern/approved", 2)),
            broken = D(("sami/decision", 1), ("sami/approved", 0), ("dana/decision", 1), ("dana/approved", 0), ("omar/decision", 1), ("omar/approved", 0), ("lina/decision", 1), ("lina/approved", 0), ("nour/decision", 1), ("nour/approved", 0), ("intern/decision", 1), ("intern/approved", 0)),
            recall = new Recall { question = "What does MFA change about a stolen password?", options = new[] { "It is no longer enough on its own", "It cannot be stolen", "It becomes longer" }, answer = 0, from = "1.4" },
            live = "The attacker has the contractor's old password. Watch which door it opens.",
            takeaway = "Only the access the job needs, reviewed on a date, signed by a name.",
            ask = "Who still has access to something because nobody took it away?",
            attack = (st, x) =>
            {
                var a = new Attack();
                if (st.Pick("sami", "decision") != 3) a.Through("Identity Chamber", "a leaver's administrator account still open", "the permit register, as an administrator");
                else a.Held("Identity Chamber", "the contractor's account was gone; the stolen password opened nothing");
                if (st.Pick("intern", "decision") == 1) a.Through("Identity Chamber", "an intern with administrator rights", "the document store, with a guessed intern password");
                a.summary = a.AnyThrough ? "An account nobody took back opened the door." : "Nothing more than each job needed; nothing to use.";
                return a;
            },
            check = (st, x, a) =>
            {
                var kept = new[] { "dana", "omar", "lina" };
                return new List<(Practised, string)>
                {
                    (Of(st.Pick("sami", "decision") == 3), "No account for a job that no longer exists"),
                    (Of(st.Pick("nour", "decision") >= 2 && st.Pick("intern", "decision") >= 2), "No account with more than its job needs"),
                    (Of(st.signed), "A dated, signed review record"),
                    (Of(kept.All(k => st.Pick(k, "approved") > 0)), "The record names who approved each account kept"),
                };
            },
        },
        new Room
        {
            number = 7, tier = 2, id = "2.2", room = "Patch Laboratory", title = "Which hole first?",
            kicker = "A score says how bad. Exposure says whether. The asset says how much it matters.",
            luma = "Five weaknesses from a scan. The score says how bad it could be, not whether anyone can reach it. Order them, and give each ticket an owner and a date.",
            learn = "The score on a weakness (severity) says how bad it could be, not whether anyone can reach it (exposure). A medium weakness on a system facing the public and holding the " +
                    "permit register comes before a critical one on an internal test machine.\n\nSeverity, then exposure, then what the asset is worth. The first two in your order get fixed before the attack.",
            fixes = "The patch order, and a ticket for each with an owner and a date.",
            mistake = "Severity score alone", why = "The ready order sorts by score. The critical on the test machine is fixed first; the medium on the public permit portal is still open when the facility runs the attack, and that is the one it uses.",
            ordered = true,
            items = new[]
            {
                I("test", "Critical 9.8 on the internal test server", "Not reachable from outside. Holds test data."),
                I("portal", "Medium 6.5 on the public permit portal", "Reachable from the internet. Holds the permit register."),
                I("files", "High 8.1 on the staff file server", "Internal only. Holds the documents."),
                I("kiosk", "Low 3.2 on the lobby kiosk", "Public. Holds nothing."),
                I("admin", "High 7.5 on the admin portal", "Internal only. Administers the register."),
            },
            fields = new[] { F("owner", "Ticket owner", "Nobody", "Lina", "Dana"), F("due", "Due", "No date", "This week", "This quarter") },
            readyOrder = new[] { "portal", "admin", "files", "test", "kiosk" },
            brokenOrder = new[] { "test", "files", "admin", "portal", "kiosk" },
            ready = D(("portal/owner", 1), ("portal/due", 1), ("admin/owner", 1), ("admin/due", 1), ("files/owner", 1), ("files/due", 2), ("test/owner", 1), ("test/due", 2), ("kiosk/owner", 1), ("kiosk/due", 2)),
            broken = D(("test/owner", 1), ("test/due", 1), ("files/owner", 1), ("files/due", 1), ("admin/owner", 0), ("admin/due", 0), ("portal/owner", 0), ("portal/due", 0), ("kiosk/owner", 0), ("kiosk/due", 0)),
            recall = new Recall { question = "What makes an access review complete?", options = new[] { "Written down, dated and signed", "Everyone kept their access", "An email saying it was done" }, answer = 0, from = "2.1" },
            live = "The first two tickets get closed. Then the attack: it uses whatever reachable weakness is still open.",
            takeaway = "A score says how bad. Exposure says whether. The asset says how much it matters.",
            ask = "Which of our known weaknesses faces the public?",
            attack = (st, x) =>
            {
                var a = new Attack();
                bool portalClosed = st.Position("portal") >= 0 && st.Position("portal") < 2;
                if (!portalClosed) a.Through("Patch Laboratory", "a medium weakness on the public portal, left open", "the permit register through the portal (score 6.5, reachable from the internet)");
                else a.Held("Patch Laboratory", "the public portal was patched first; the attack found nothing reachable");
                a.summary = a.AnyThrough ? "The score was lower. The exposure was total." : "Reachable first: held.";
                return a;
            },
            check = (st, x, a) =>
            {
                var all = new[] { "test", "portal", "files", "kiosk", "admin" };
                return new List<(Practised, string)>
                {
                    (Of(st.Position("portal") < st.Position("test")), "Ordered by severity, exposure and asset value, not score alone"),
                    (Of(all.All(k => st.Pick(k, "owner") > 0 && st.Pick(k, "due") > 0)), "Every ticket has an owner and a date"),
                    (Of(st.Position("portal") >= 0 && st.Position("portal") < 2), "Nothing reachable from outside on a high-value asset left open"),
                    (Of(st.signed), "Closures recorded, dated and signed"),
                };
            },
            signable = true,
        },
        new Room
        {
            number = 8, tier = 2, id = "2.3", room = "Network Maze", title = "What can reach what?",
            kicker = "Put a line wherever what you assume about the caller changes.",
            luma = "On one flat network, whoever gets in anywhere can reach everything. Put a line wherever what you assume about the caller changes: public to internal, internal to the data.",
            learn = "On one flat network, whoever gets in anywhere can reach everything. Put a line (trust boundary) wherever what you assume about the caller changes: public to internal, " +
                    "internal to the data, one service to another. Networks kept apart (segregation). A line is a place to check, and the evidence is the check.",
            fixes = "The zones each system sits in, and what may cross between zones.",
            mistake = "One flat network", why = "The public kiosk, the staff computers and the permit database all sit on one network. The facility runs the attack from the kiosk and reaches the database.",
            signable = true,
            items = new[]
            {
                I("kiosk", "Lobby kiosk", "Anyone in the lobby."), I("portal", "Permit portal", "The public website."), I("staff", "Staff computers", "Clerks and supervisors."),
                I("admin", "Admin portal", "Administration of the register."), I("register", "Permit register", "The database."), I("docs", "Document store", "Applicants' papers."), I("backup", "Backup server", "The copies."),
            },
            fields = new[]
            {
                F("zone", "Zone", "?", "Public", "Internal", "Data"),
                Once("pi", "Public to Internal", "?", "Allowed, checked at the line", "Blocked"), Once("id", "Internal to Data", "?", "Allowed, checked at the line", "Blocked"), Once("pd", "Public to Data", "?", "Allowed", "Blocked"),
            },
            ready = D(("kiosk/zone", 1), ("portal/zone", 1), ("staff/zone", 2), ("admin/zone", 2), ("register/zone", 3), ("docs/zone", 3), ("backup/zone", 3), ("room/pi", 1), ("room/id", 1), ("room/pd", 2)),
            broken = D(("kiosk/zone", 2), ("portal/zone", 2), ("staff/zone", 2), ("admin/zone", 2), ("register/zone", 2), ("docs/zone", 2), ("backup/zone", 2), ("room/pi", 1), ("room/id", 1), ("room/pd", 1)),
            recall = new Recall { question = "Which comes first when ordering patches: the score, or whether it is reachable?", options = new[] { "Whether it is reachable, with the score and the asset's value", "The score alone", "The date it was found" }, answer = 0, from = "2.2" },
            live = "The attack starts at the kiosk. Watch its path, zone by zone, and where a line stops it.",
            takeaway = "Put a line wherever what you assume about the caller changes.",
            ask = "From the visitor network at work, what can be reached?",
            attack = (st, x) =>
            {
                var a = new Attack();
                int kz = st.Pick("kiosk", "zone"), rz = st.Pick("register", "zone");
                bool reach = kz == rz || kz == 1 && rz == 3 && st.Pick("room", "pd") == 1 || kz == 1 && rz == 2 && st.Pick("room", "pi") == 1 && rz == 2 || kz == 2 && rz == 3 && st.Pick("room", "id") == 1 && kz == 2;
                if (kz <= 0 || rz <= 0) reach = true;
                if (reach) a.Through("Network Maze", kz == rz ? "the kiosk and the database on one network" : "a line that let the public side through to the data", "the permit register, from the lobby kiosk");
                else a.Held("Network Maze", "from the kiosk, the data zone was out of reach");
                a.summary = a.AnyThrough ? "From the lobby to the database, with nothing in between." : "The line held.";
                return a;
            },
            check = (st, x, a) => new List<(Practised, string)>
            {
                (Of(st.Pick("kiosk", "zone") == 1 && st.Pick("portal", "zone") == 1 && st.Pick("register", "zone") == 3 && st.Pick("docs", "zone") == 3 && st.Pick("room", "pd") == 2), "The public zone cannot reach the data zone"),
                (Of(st.Pick("room", "pi") > 0 && st.Pick("room", "id") > 0 && st.Pick("room", "pd") > 0), "Each crossing is listed and justified"),
                (Of(st.signed), "The evidence says what was checked and when"),
            },
        },
        new Room
        {
            number = 9, tier = 2, id = "2.4", room = "Firewall Control", title = "What is allowed through?",
            kicker = "Allow by name, block the rest, log the block.",
            luma = "A rule set that ends with \"allow everything\" is no rule set. Allow what is needed, by name, with a reason. Block the rest and log it. Administration is never reachable from the public side.",
            learn = "A rule set that ends with \"allow everything\" is no rule set. Allow what is needed, by name (allow list); block the rest (fail closed); write down why each allow exists; " +
                    "log the blocks. Administration is never reachable from the public side.\n\nRules are read top to bottom; the first that matches wins.",
            fixes = "The rules on each line from the Network Maze: action, reason, and their order.",
            mistake = "Allow everything, at the bottom", why = "The ready rules allow the right things and end with an allow-all \"so nothing breaks\"; remote administration is open to the public side. The facility runs the attack through the open administration port.",
            ordered = true,
            items = new[]
            {
                I("web", "Web traffic (443) from the public side to the portal", ""), I("staffadmin", "Staff computers to the admin portal", ""),
                I("remote", "Remote administration (3389) from the public side to the admin portal", ""), I("backups", "Data zone to the backup server, nightly", ""),
                I("all", "Everything else, from anywhere to anywhere", ""),
            },
            fields = new[] { F("action", "Action", "?", "Allow", "Block and log"), F("reason", "Reason", "None", "Needed by the service", "\"So nothing breaks\"") },
            readyOrder = new[] { "web", "staffadmin", "backups", "remote", "all" },
            brokenOrder = new[] { "web", "staffadmin", "remote", "backups", "all" },
            ready = D(("web/action", 1), ("web/reason", 1), ("staffadmin/action", 1), ("staffadmin/reason", 1), ("backups/action", 1), ("backups/reason", 1), ("remote/action", 2), ("remote/reason", 0), ("all/action", 2), ("all/reason", 0)),
            broken = D(("web/action", 1), ("web/reason", 1), ("staffadmin/action", 1), ("staffadmin/reason", 1), ("remote/action", 1), ("remote/reason", 2), ("backups/action", 1), ("backups/reason", 1), ("all/action", 1), ("all/reason", 2)),
            recall = new Recall { question = "Where do you put a line on a network?", options = new[] { "Wherever what you assume about the caller changes", "Only at the internet", "Between every two computers" }, answer = 0, from = "2.3" },
            live = "The attack tries the administration port from the public side, then anything an allow-all lets through.",
            takeaway = "Allow by name, block the rest, log the block.",
            ask = "What rule did we add \"so nothing breaks\", and is it still there?",
            attack = (st, x) =>
            {
                var a = new Attack();
                if (st.Pick("remote", "action") == 1) a.Through("Firewall Control", "remote administration open to the public side", "the admin portal over port 3389");
                else if (st.Pick("all", "action") == 1 && st.Position("all") >= 0) a.Through("Firewall Control", "an allow-all at the bottom", "the admin portal: the last rule let it through");
                else a.Held("Firewall Control", "3389 from the public side blocked and logged");
                a.summary = a.AnyThrough ? "The open port was the front door." : "Blocked, and the block is in the log.";
                return a;
            },
            check = (st, x, a) => new List<(Practised, string)>
            {
                (Of(st.Pick("all", "action") == 2), "No allow-all"),
                (Of(new[] { "web", "staffadmin", "backups" }.All(k => st.Pick(k, "action") != 1 || st.Pick(k, "reason") == 1)), "Each allow has a reason"),
                (Of(st.Pick("remote", "action") == 2), "Administration unreachable from the public side"),
                (Of(st.Pick("all", "action") == 2 && st.Position("all") == 4), "Blocks are logged, by the last rule"),
                (Of(st.Pick("web", "action") == 1 && st.Pick("staffadmin", "action") == 1 && st.Pick("backups", "action") == 1), "The service still works"),
            },
        },
        new Room
        {
            number = 10, tier = 2, id = "2.5", room = "Encryption Chamber", title = "Who can read it if they take it?",
            kicker = "Encrypted data with the key beside it is not encrypted.",
            luma = "Encryption means a thief who takes the data cannot read it. That holds only if the key is somewhere the thief did not also take.",
            learn = "Encryption means a thief who takes the data cannot read it (scrambled; the key kept apart). That holds only if the key is somewhere the thief did not also take. " +
                    "Data in transit is encrypted so nobody on the way reads it; data at rest so a stolen disk or backup is unreadable; the key lives apart, with its own access.",
            fixes = "What is encrypted, where, and where each key is kept.",
            mistake = "The key next to the data", why = "The ready setup encrypts the permit database and keeps the key in a file on the same server; the backups are not encrypted at all. The facility copies the server's disk, and a backup.",
            signable = true,
            items = new[]
            {
                I("register", "Permit register, at rest", "The database on its server."), I("docs", "Document store, at rest", "Applicants' papers."),
                I("backup", "The backups", "Copies of both."), I("transit", "Portal traffic across the public line", "Applicants' data on the way in."),
            },
            fields = new[] { F("enc", "Encrypted", "?", "No", "Yes"), F("key", "The key kept", "n/a", "On the same server", "In the key service, with its own access") },
            ready = D(("register/enc", 2), ("register/key", 2), ("docs/enc", 2), ("docs/key", 2), ("backup/enc", 2), ("backup/key", 2), ("transit/enc", 2), ("transit/key", 0)),
            broken = D(("register/enc", 2), ("register/key", 1), ("docs/enc", 1), ("docs/key", 0), ("backup/enc", 1), ("backup/key", 0), ("transit/enc", 2), ("transit/key", 0)),
            recall = new Recall { question = "What should the last rule in a rule set be?", options = new[] { "Block the rest, and log it", "Allow everything, so nothing breaks", "There is no last rule" }, answer = 0, from = "2.4" },
            live = "The attacker copies the register's disk and a backup. Watch whether either can be read.",
            takeaway = "Encrypted data with the key beside it is not encrypted.",
            ask = "If someone walked out with our backup, could they read it?",
            attack = (st, x) =>
            {
                var a = new Attack();
                if (st.Pick("register", "enc") != 2 || st.Pick("register", "key") == 1) a.Through("Encryption Chamber", st.Pick("register", "enc") != 2 ? "the register not encrypted" : "the register's key on the same server", "the register's disk, copied and read: the key was in a file beside it");
                else a.Held("Encryption Chamber", "the register's disk copied, unreadable; the key was elsewhere");
                if (st.Pick("backup", "enc") != 2) a.Through("Encryption Chamber", "backups not encrypted", "a backup, copied and read in full");
                else if (st.Pick("backup", "key") == 1) a.Through("Encryption Chamber", "the backups' key beside them", "a backup, read with the key stored next to it");
                else a.Held("Encryption Chamber", "a backup copied, unreadable");
                a.summary = a.AnyThrough ? "Taken, and readable." : "Taken, and useless to the thief.";
                return a;
            },
            check = (st, x, a) => new List<(Practised, string)>
            {
                (Of(st.Pick("register", "enc") == 2 && st.Pick("docs", "enc") == 2), "The sensitive assets are encrypted at rest"),
                (Of(st.Pick("transit", "enc") == 2), "Traffic across the public line is encrypted"),
                (Of(st.Pick("register", "key") == 2 && st.Pick("docs", "key") == 2 && st.Pick("backup", "key") != 1), "No key on the same system as its data"),
                (Of(st.Pick("backup", "enc") == 2), "Backups encrypted"),
                (Of(st.signed), "Evidence of each, dated and signed"),
            },
        },
        new Room
        {
            number = 11, tier = 2, id = "2.6", room = "Cloud Chamber", title = "What did the defaults leave open?",
            kicker = "A default is a decision somebody else made for you.",
            luma = "A cloud service arrives with settings chosen for convenience, not for the permit service. Each one is a door nobody decided to open.",
            learn = "A cloud service arrives with settings chosen for convenience, not for the permit service (the settings it came with: defaults). Storage that is public by default, " +
                    "credentials that came with the box, internal addresses any service can reach: each is a door nobody decided to open.",
            fixes = "The cloud settings: storage visibility, default credentials, which services may reach the internal address, which have more rights than their job.",
            mistake = "Public by default", why = "A storage area holding applicants' documents is readable by anyone with the address, and the web service can reach the internal address that hands out temporary credentials. The facility runs the chain.",
            signable = true,
            items = new[]
            {
                I("storage", "Document storage visibility", "Applicants' documents."), I("creds", "The administrator credentials that came with the account", ""),
                I("metadata", "The web service's reach to the internal credentials address", ""), I("rights", "The web service's rights", ""),
            },
            fields = new[] { F("value", "Setting", "?", "As it came: public / default / reachable / everything", "Changed: private / rotated / cut / only its job") },
            ready = D(("storage/value", 2), ("creds/value", 2), ("metadata/value", 2), ("rights/value", 2)),
            broken = D(("storage/value", 1), ("creds/value", 1), ("metadata/value", 1), ("rights/value", 1)),
            recall = new Recall { question = "Where must the key never be?", options = new[] { "On the same system as the data it protects", "In a key service", "With its own access" }, answer = 0, from = "2.5" },
            live = "The chain: a reachable internal address, a read of temporary credentials, a listing of storage, the data leaving. Each link uses one setting.",
            takeaway = "A default is a decision somebody else made for you.",
            ask = "What did we switch on and never configure?",
            attack = (st, x) =>
            {
                var a = new Attack();
                if (st.Pick("storage", "value") == 1) a.Through("Cloud Chamber", "storage public by default", "the applicants' documents, read by anyone with the address");
                else a.Held("Cloud Chamber", "the documents' storage was private");
                if (st.Pick("metadata", "value") == 1) { a.Through("Cloud Chamber", "the internal credentials address reachable from the web service", "temporary credentials, read through the web service");
                    if (st.Pick("rights", "value") == 1) a.Through("Cloud Chamber", "a web service with more rights than its job", "every storage area listed, and the register copied out"); }
                else a.Held("Cloud Chamber", "the internal address was out of the web service's reach; the chain stopped at its first link");
                if (st.Pick("creds", "value") == 1) a.Through("Cloud Chamber", "default administrator credentials", "the account's console, with the password from the manual");
                a.summary = a.AnyThrough ? "Every link was a default nobody changed." : "The chain stopped at its first link.";
                return a;
            },
            check = (st, x, a) => new List<(Practised, string)>
            {
                (Of(st.Pick("storage", "value") == 2), "No public storage of sensitive assets"),
                (Of(st.Pick("creds", "value") == 2), "Default credentials rotated"),
                (Of(st.Pick("metadata", "value") == 2), "The internal address unreachable from services that do not need it"),
                (Of(st.Pick("rights", "value") == 2), "Each service's rights limited to its job"),
                (Of(st.signed), "Evidence of each"),
            },
        },
        new Room
        {
            number = 12, tier = 2, id = "2.7", room = "Operations Center", title = "Is this an event or an incident?",
            kicker = "You can only investigate what you logged.",
            luma = "A log you never kept cannot be read later. An alert on everything is an alert on nothing. Keep the logs that answer who signed in, from where, and what they touched.",
            learn = "A log you never kept cannot be read later. An alert on everything is an alert on nothing. Keep the logs that answer \"who signed in, from where, and what did they touch\", keep private data out of them, " +
                    "alert on what is rare and bad, and when it fires, trace it.\n\nAn event is something that happened; an incident is something that happened that matters.",
            fixes = "Which logs are kept and which alerts raise, then the decision on the replayed intrusion.",
            mistake = "The log that was never kept", why = "The administration portal keeps no sign-in log, and the alert rule fires on every failed login. The facility replays the credential theft; the sign-in that mattered cannot be seen and the real alert drowns.",
            items = new[]
            {
                I("adminlog", "Log: admin portal sign-ins", "Who signed in, from where."), I("accesslog", "Log: register access", "What was read and exported."),
                I("netlog", "Log: crossings between zones", ""), I("cardlog", "Log: full card numbers in the payment log", "Private data."), I("doclog", "Log: the contents of applicants' documents", "Private data."),
                I("failalert", "Alert: every failed login", "About 4,000 a day."), I("abroadalert", "Alert: an admin sign-in from a new country at night", "Rare."), I("exportalert", "Alert: a register export over 1,000 rows", "Rare."), I("printeralert", "Alert: a printer goes offline", ""),
            },
            fields = new[] { F("on", "Kept / raised", "?", "Off", "On"), Once("decision", "The replayed sign-in", "?", "An event: a sign-in, nothing more", "An incident: an admin signed in from abroad at night and exported the register", "Cannot tell") },
            ready = D(("adminlog/on", 2), ("accesslog/on", 2), ("netlog/on", 2), ("cardlog/on", 1), ("doclog/on", 1), ("failalert/on", 1), ("abroadalert/on", 2), ("exportalert/on", 2), ("printeralert/on", 1), ("room/decision", 2)),
            broken = D(("adminlog/on", 1), ("accesslog/on", 2), ("netlog/on", 1), ("cardlog/on", 2), ("doclog/on", 1), ("failalert/on", 2), ("abroadalert/on", 1), ("exportalert/on", 1), ("printeralert/on", 2), ("room/decision", 3)),
            recall = new Recall { question = "Name one setting a cloud service comes with that must be changed.", options = new[] { "Storage public by default, or the credentials that came with it", "The colour of the console", "The time zone" }, answer = 0, from = "2.6" },
            live = "The replay runs with the logs as they are. What you kept, you can see. What you did not, is gone.",
            takeaway = "You can only investigate what you logged.",
            ask = "If someone signed in as me last night, would anything show it?",
            attack = (st, x) =>
            {
                var a = new Attack();
                bool seen = st.Pick("adminlog", "on") == 2 && st.Pick("accesslog", "on") == 2;
                bool alerted = st.Pick("abroadalert", "on") == 2 || st.Pick("exportalert", "on") == 2;
                bool drowned = st.Pick("failalert", "on") == 2;
                if (!seen) a.Through("Operations Center", "the admin sign-in log never kept", "an admin sign-in from abroad at 02:14, and an export of 12,000 rows, with nothing to show it");
                else a.Held("Operations Center", "the sign-in at 02:14 and the export at 02:31 are in the logs");
                if (!alerted || drowned) a.Through("Operations Center", drowned ? "the real alert drowned in 4,000 failed logins" : "no alert on what was rare and bad", "the export ran to the end; nobody was woken");
                else a.Held("Operations Center", "the alert fired once, on the sign-in from abroad");
                if (st.Pick("cardlog", "on") == 2 || st.Pick("doclog", "on") == 2) a.Through("Operations Center", "private data in the logs", "card numbers and documents, read out of the log itself");
                a.summary = a.AnyThrough ? "What was not logged did not happen, as far as anyone could see." : "Seen, alerted, traced.";
                return a;
            },
            check = (st, x, a) => new List<(Practised, string)>
            {
                (Of(st.Pick("adminlog", "on") == 2 && st.Pick("accesslog", "on") == 2 && st.Pick("netlog", "on") == 2), "Sign-in, access and network logs kept for the sensitive assets"),
                (Of(st.Pick("cardlog", "on") == 1 && st.Pick("doclog", "on") == 1), "No private data in the logs"),
                (Of((st.Pick("abroadalert", "on") == 2 || st.Pick("exportalert", "on") == 2) && st.Pick("failalert", "on") == 1 && st.Pick("printeralert", "on") == 1), "An alert that fired on the intrusion and not on the noise"),
                (Of(st.Pick("adminlog", "on") == 2 && st.Pick("accesslog", "on") == 2 && st.Pick("netlog", "on") == 2), "The trace is complete from the first sign-in to the data touched"),
                (Of(st.Pick("room", "decision") == 2), "The decision, event or incident, is right and gives a reason"),
            },
        },
        new Room
        {
            number = 13, tier = 2, id = "2.8", room = "Malware Containment", title = "How do you stop it without losing the evidence?",
            kicker = "Cut it off, keep a copy, then clean. Deletion feels like containment; it destroys the evidence.",
            luma = "Cutting a machine off stops the spread. Wiping it destroys the only record of what happened. Put the steps in order.",
            learn = "Cutting a machine off stops the spread (contain). Wiping it destroys the only record of what happened. Cut it off first, keep a copy, then clean out (eradicate) and bring back (recover). " +
                    "A machine that was taken over is rebuilt from a known-good state, not patched in place. Every step gets a time in the incident log.",
            fixes = "The containment order, as steps, with the log entry for each. One of the two last steps is wrong: rebuild, or patch in place.",
            mistake = "Delete first", why = "The ready playbook wipes the infected machine as step one. The spread stops, and the record of how it got in is gone, so the record for the exit reads \"unknown\".",
            ordered = true,
            items = new[]
            {
                I("isolate", "Isolate the machine from the network", ""), I("snapshot", "Take a copy of its disk and memory", ""), I("wipe", "Wipe the machine", ""),
                I("rebuild", "Rebuild it from a known-good image", ""), I("patch", "Patch it in place and keep it", ""), I("log", "Write the time and the step in the incident log", "Every step."),
            },
            fields = new[] { F("do", "Do it", "?", "Yes", "Skip") },
            readyOrder = new[] { "isolate", "snapshot", "log", "wipe", "rebuild", "patch" },
            brokenOrder = new[] { "wipe", "isolate", "patch", "log", "snapshot", "rebuild" },
            ready = D(("isolate/do", 1), ("snapshot/do", 1), ("log/do", 1), ("wipe/do", 1), ("rebuild/do", 1), ("patch/do", 2)),
            broken = D(("wipe/do", 1), ("isolate/do", 1), ("patch/do", 1), ("log/do", 2), ("snapshot/do", 2), ("rebuild/do", 2)),
            recall = new Recall { question = "What is the difference between an event and an incident?", options = new[] { "An incident is something that happened that matters", "An incident is older", "There is none" }, answer = 0, from = "2.7" },
            live = "The ransomware spreads while your steps run, in your order. Watch what evidence survives.",
            takeaway = "Cut it off, keep a copy, then clean. Deletion feels like containment; it destroys the evidence.",
            ask = "Who at work would wipe the machine first, and who would stop them?",
            attack = (st, x) =>
            {
                var a = new Attack();
                bool isolateFirst = st.Position("isolate") == 0 && st.Pick("isolate", "do") == 1;
                bool copyBeforeWipe = st.Pick("snapshot", "do") == 1 && (st.Pick("wipe", "do") != 1 || st.Position("snapshot") < st.Position("wipe"));
                if (!isolateFirst) a.Through("Malware Containment", "the machine not isolated first", "two more machines, locked while the first was being handled");
                else a.Held("Malware Containment", "isolated first: the spread stopped at one machine");
                if (!copyBeforeWipe) a.Through("Malware Containment", "wiped before a copy was kept", "the entry point: unknown. The record for the exit has a hole.");
                else a.Held("Malware Containment", "a copy kept: the entry point is in the record (the helpdesk message, opened at 08:02)");
                if (st.Pick("patch", "do") == 1 && st.Pick("rebuild", "do") != 1) a.Through("Malware Containment", "a taken-over machine patched in place", "the same machine, locked again three days later");
                a.summary = a.AnyThrough ? "Containment that destroyed the evidence, or let the spread run." : "Stopped, and the evidence survived.";
                return a;
            },
            check = (st, x, a) => new List<(Practised, string)>
            {
                (Of(st.Position("isolate") == 0 && st.Pick("isolate", "do") == 1), "Isolated before anything else"),
                (Of(st.Pick("snapshot", "do") == 1 && (st.Pick("wipe", "do") != 1 || st.Position("snapshot") < st.Position("wipe"))), "A copy kept before cleaning"),
                (Of(st.Pick("rebuild", "do") == 1 && st.Pick("patch", "do") != 1), "Rebuild chosen over patch"),
                (Of(st.Pick("log", "do") == 1), "An incident log with a time for each step"),
                (Of(a != null && !a.lines.Any(l => l.reached.StartsWith("the entry point: unknown"))), "The entry point recorded"),
            },
        },
        new Room
        {
            number = 14, tier = 2, id = "2.9", room = "Backup Vault", title = "Can you get it back?",
            kicker = "A backup the attacker can reach is not a backup.",
            luma = "A backup is the copy that still works when everything else is gone. It is only that if the thing that took everything else cannot take it too.",
            learn = "A backup is the copy that still works when everything else is gone (the last known good). It is only that if the thing that took everything else cannot take it too: " +
                    "not on the same network, not under the same administrator's key, and restored once to prove it restores.",
            fixes = "Where the backups live, who can reach them, how often, and the restore test.",
            mistake = "Backups on the same keys", why = "The ready backups sit on the same network as the file server and are reachable with the same administrator credentials. The facility runs the ransomware with those credentials; the backups are encrypted with everything else.",
            signable = true,
            items = new[]
            {
                I("where", "Where the backups live", ""), I("who", "Who can reach them", ""), I("often", "How often", ""), I("enc", "Encrypted", "From the Encryption Chamber."), I("test", "A restore test", ""),
            },
            fields = new[] { F("value", "Setting", "?", "As it is: same network / the administrator's credentials / weekly / no / never tested", "Changed: behind its own line / its own access / nightly / yes / tested and dated") },
            ready = D(("where/value", 2), ("who/value", 2), ("often/value", 2), ("enc/value", 2), ("test/value", 2)),
            broken = D(("where/value", 1), ("who/value", 1), ("often/value", 1), ("enc/value", 1), ("test/value", 1)),
            recall = new Recall { question = "What is the first step of containment?", options = new[] { "Isolate the machine", "Wipe the machine", "Call the press" }, answer = 0, from = "2.8" },
            live = "The ransomware runs with the administrator's credentials. Watch whether the backups survive, and whether the restore brings the service back.",
            takeaway = "A backup the attacker can reach is not a backup.",
            ask = "When did we last restore something, on purpose?",
            attack = (st, x) =>
            {
                var a = new Attack();
                if (st.Pick("where", "value") == 1 || st.Pick("who", "value") == 1) a.Through("Backup Vault", st.Pick("where", "value") == 1 ? "backups on the same network" : "backups under the administrator's credentials", "the backups, encrypted with everything else");
                else a.Held("Backup Vault", "the backups were out of reach");
                if (st.Pick("test", "value") != 2) a.Through("Backup Vault", "a restore never tested", "the restore failed on the night: a setting nobody had tried");
                else a.Held("Backup Vault", "the restore brought the service back in four hours");
                a.summary = a.AnyThrough ? "Everything else was gone, and so was the copy." : "Everything else was gone. The copy was not.";
                return a;
            },
            check = (st, x, a) => new List<(Practised, string)>
            {
                (Of(st.Pick("where", "value") == 2), "Backups unreachable from the zones the attack ran in"),
                (Of(st.Pick("who", "value") == 2), "Their own access, not the administrator's"),
                (Of(st.Pick("enc", "value") == 2), "Encrypted"),
                (Of(st.Pick("test", "value") == 2 && st.signed), "A restore tested and dated"),
            },
        },
        new Room
        {
            number = 15, tier = 2, id = "2.10", room = "Risk Room", title = "Capstone: The register and the evidence",
            kicker = "A control nobody can show evidence for is a control nobody can be sure of.",
            luma = "The whole facility: every risk with a treatment decision, every control with the evidence an auditor would ask for. Then the attack runs through every room at once.",
            learn = "The risk register, built from tiers 1 and 2: each risk, its rating, the decision (treat, accept, avoid, share) and the control. Then the audit: for each control, the evidence " +
                    "(a signed access review, a rule set with reasons, a dated restore test). Then the whole-facility attack, every room at once, with the record written for the exit.",
            fixes = "The register: a treatment decision per risk, and the evidence for each control.",
            items = new[]
            {
                I("phish", "A phishing mail takes a clerk's password", "Control: MFA on privileged accounts."), I("ransom", "Ransomware on the file server", "Control: backups the attack cannot reach."),
                I("portal", "A weakness in the public portal", "Control: the patch order."), I("contractor", "A leaver's account used", "Control: the signed access review."),
                I("kiosk", "The kiosk reaches the database", "Control: network zones and the rules."), I("laptop", "A lost laptop", "Control: disk encryption."),
            },
            fields = new[] { F("treat", "Decision", "?", "Treat", "Accept", "Avoid", "Share"), F("evidence", "Evidence", "None", "The room's signed, dated record", "\"It is done, trust me\"") },
            ready = D(("phish/treat", 1), ("phish/evidence", 1), ("ransom/treat", 1), ("ransom/evidence", 1), ("portal/treat", 1), ("portal/evidence", 1), ("contractor/treat", 1), ("contractor/evidence", 1), ("kiosk/treat", 1), ("kiosk/evidence", 1), ("laptop/treat", 1), ("laptop/evidence", 1)),
            recall = new Recall { question = "What makes a backup a backup?", options = new[] { "The attack cannot reach it, and a restore was tested", "It is nightly", "It is large" }, answer = 0, from = "2.9" },
            live = "Every room at once. Whatever you closed holds; whatever you left open is a link the attacker walks, and a line in the record.",
            takeaway = "A control nobody can show evidence for is a control nobody can be sure of.",
            ask = "Which control at work could I prove is working, today, with a document?",
            attack = (st, x) =>
            {
                var a = new Attack();
                // The rooms as the student left them: each one's attack, again.
                foreach (var r in All.Where(r => r.tier == 2 && r.number < 15))
                {
                    var rs = x.Of(r.id);
                    if (rs == null) { a.Through(r.room, "the room was never done", "whatever that room protects"); continue; }
                    var sub = r.attack(rs, x);
                    foreach (var l in sub.lines) a.lines.Add(l);
                }
                a.summary = a.AnyThrough ? "The attacker walked every gap you left. The record has the chain." : "Every room held. The record is clean: the exit will have a story to tell anyway.";
                return a;
            },
            check = (st, x, a) =>
            {
                var all = new[] { "phish", "ransom", "portal", "contractor", "kiosk", "laptop" };
                return new List<(Practised, string)>
                {
                    (Of(all.All(k => st.Pick(k, "treat") > 0)), "Every high risk has a treatment decision"),
                    (Of(all.All(k => st.Pick(k, "evidence") == 1)), "Every control has evidence that would satisfy an auditor"),
                    (Of(a != null && !a.AnyThrough), "The whole-facility attack got through nowhere"),
                    (Of(a != null), "Whatever got through is in the record with the room and the gap"),
                };
            },
        },

        // ---- Tier 3: the final exit -------------------------------------------------------
        new Room
        {
            number = 16, tier = 3, id = "3.1", room = "Operations Center", title = "Is it an incident, and how bad?",
            kicker = "Declare it, rate it, split the roles. Then act.",
            luma = "The facility is under attack for real. The first minutes decide the rest: declare it, say how bad, and split the work. The commander who starts typing stops commanding.",
            learn = "The first minutes decide the rest. Declare it, say how bad on the scale (severity 1 to 4; data exposed is a 1, declared in under 15 minutes), and split the work: one person commands, " +
                    "one writes everything down with times (the scribe), one does the technical work (the engineer). The commander who starts typing stops commanding.",
            fixes = "The declaration and the roles.",
            mistake = "The commander who starts typing", why = "The ready scene has Lina commanding and fixing the firewall at once, nobody writing anything down, and the severity never set. The facility keeps moving; the record has holes.",
            items = new[] { I("omar", "Omar, supervisor", ""), I("lina", "Lina, IT administrator", ""), I("dana", "Dana, clerk", "") },
            fields = new[]
            {
                F("role", "Role", "?", "Commander: decides, does no technical work", "Scribe: writes everything down, with times", "Engineer: does the technical work", "None"),
                Once("sev", "Severity", "?", "1: data exposed; declare within 15 minutes", "2: a system down, no data", "3: one user affected", "4: an anomaly worth tracking"),
                Once("why", "Because", "?", "The register was exported to an address abroad at 02:31", "A printer is offline", "Dana cannot sign in"),
            },
            ready = D(("omar/role", 1), ("lina/role", 3), ("dana/role", 2), ("room/sev", 1), ("room/why", 1)),
            broken = D(("omar/role", 4), ("lina/role", 1), ("dana/role", 4), ("room/sev", 0), ("room/why", 0)),
            recall = new Recall { question = "What is the evidence for an access review?", options = new[] { "A dated, signed record naming who approved what", "An email", "A spreadsheet nobody signed" }, answer = 0, from = "2.1" },
            live = "The clock runs. Watch what each role is doing, and what gets written down.",
            takeaway = "Declare it, rate it, split the roles. Then act.",
            ask = "If this happened to us, who is in charge, and who writes it down?",
            attack = (st, x) =>
            {
                var a = new Attack();
                var roles = new[] { st.Pick("omar", "role"), st.Pick("lina", "role"), st.Pick("dana", "role") };
                bool three = roles.Contains(1) && roles.Contains(2) && roles.Contains(3);
                if (st.Pick("room", "sev") != 1) a.Through("Operations Center", "severity not declared, or wrong", "forty minutes passed before anyone treated it as data exposed");
                else a.Held("Operations Center", "SEV-1 declared at 02:46: data exposed");
                if (!three) a.Through("Operations Center", "the roles not split", roles.Contains(2) ? "the commander fixed the firewall while nobody decided" : "nobody wrote anything down; the record has holes");
                else a.Held("Operations Center", "one commands, one writes, one works");
                if (st.Pick("lina", "role") == 1) a.Through("Operations Center", "the commander started typing", "the commander fixed the firewall and stopped commanding");
                a.summary = a.AnyThrough ? "The first minutes were lost." : "Declared, rated, split. The record has every minute.";
                return a;
            },
            check = (st, x, a) =>
            {
                var roles = new[] { st.Pick("omar", "role"), st.Pick("lina", "role"), st.Pick("dana", "role") };
                return new List<(Practised, string)>
                {
                    (Of(st.Pick("room", "sev") == 1 && st.Pick("room", "why") == 1), "Severity declared within the time limit and justified by the facts"),
                    (Of(roles.Contains(1) && roles.Contains(2) && roles.Contains(3)), "Three roles assigned"),
                    (Of(roles.Contains(2)), "The scribe's log has times"),
                    (Of(st.Pick("lina", "role") != 1), "The commander made no technical change"),
                };
            },
        },
        new Room
        {
            number = 17, tier = 3, id = "3.2", room = "Password Vault", title = "What do you cut first?",
            kicker = "Rotate, invalidate, revoke. In that order, all three.",
            luma = "A stolen credential is contained in three steps, in order: change it, end every session using it, take the access away. Rotating alone leaves the attacker's open sessions open.",
            learn = "A stolen credential is contained in three steps in order: change it (rotate), end every session using it (invalidate), take the access away (revoke). Rotating alone leaves the attacker's " +
                    "open sessions open. The runbook says do, never \"consider\". Evidence is kept before anything is removed.",
            fixes = "The containment steps, as a runbook anyone could follow.",
            mistake = "Rotated, and stopped there", why = "The ready runbook rotates the administrator's password and ends. The attacker's session from the Operations Center trace is still open, and the facility shows it still reading.",
            ordered = true,
            items = new[]
            {
                I("snapshot", "Keep a copy of the session logs and the admin host", "Evidence first."), I("rotate", "Change the administrator's password", ""),
                I("invalidate", "End every session using it", ""), I("revoke", "Take the account's access away", ""),
            },
            fields = new[] { F("do", "Runbook line", "?", "Do it", "\"Consider\" it", "Skip") },
            readyOrder = new[] { "snapshot", "rotate", "invalidate", "revoke" },
            brokenOrder = new[] { "rotate", "snapshot", "invalidate", "revoke" },
            ready = D(("snapshot/do", 1), ("rotate/do", 1), ("invalidate/do", 1), ("revoke/do", 1)),
            broken = D(("rotate/do", 1), ("snapshot/do", 3), ("invalidate/do", 3), ("revoke/do", 2)),
            recall = new Recall { question = "Which role writes everything down?", options = new[] { "The scribe", "The commander", "The engineer" }, answer = 0, from = "3.1" },
            live = "The attacker's access, step by step, as each of your steps lands.",
            takeaway = "Rotate, invalidate, revoke. In that order, all three.",
            ask = "If my password were stolen tonight, who could end my sessions?",
            attack = (st, x) =>
            {
                var a = new Attack();
                bool rot = st.Pick("rotate", "do") == 1, inv = st.Pick("invalidate", "do") == 1, rev = st.Pick("revoke", "do") == 1, snap = st.Pick("snapshot", "do") == 1 && st.Position("snapshot") == 0;
                bool order = rot && inv && rev && st.Position("rotate") < st.Position("invalidate") && st.Position("invalidate") < st.Position("revoke");
                if (!snap) a.Through("Password Vault", "evidence not kept first", "the session logs, overwritten by the password change");
                else a.Held("Password Vault", "the logs and the host copied before anything changed");
                if (!rot) a.Through("Password Vault", "the password never changed", "the attacker signed in again at 03:10");
                if (!inv) a.Through("Password Vault", "sessions never ended", "the attacker's open session kept reading the register after the password changed");
                if (!rev) a.Through("Password Vault", "access never revoked", "a second token the attacker had minted still worked");
                if (order && snap) a.Held("Password Vault", "rotated, invalidated, revoked: the attacker's access ended at 02:58");
                a.summary = a.AnyThrough ? "Rotated and stopped there: the open session read on." : "All three, in order. The access ended.";
                return a;
            },
            check = (st, x, a) =>
            {
                bool order = st.Pick("rotate", "do") == 1 && st.Pick("invalidate", "do") == 1 && st.Pick("revoke", "do") == 1 && st.Position("rotate") < st.Position("invalidate") && st.Position("invalidate") < st.Position("revoke");
                return new List<(Practised, string)>
                {
                    (Of(order), "Rotate, invalidate, revoke, in that order"),
                    (Of(st.Pick("snapshot", "do") == 1 && st.Position("snapshot") == 0), "Evidence kept before anything is removed"),
                    (Of(new[] { "snapshot", "rotate", "invalidate", "revoke" }.All(k => st.Pick(k, "do") != 2)), "The runbook has no \"consider\""),
                    (Of(a != null && !a.AnyThrough), "The attacker's access ends"),
                };
            },
        },
        new Room
        {
            number = 18, tier = 3, id = "3.3", room = "Network Maze", title = "How did they get from the inbox to the vault?",
            kicker = "The click is the trigger. The gaps are the causes. Fix the gaps.",
            luma = "The attacker did not do one thing. They did a chain. Put the links in order, name the room and the gap for each, and mark which was the trigger and which were the causes.",
            learn = "The attacker did not do one thing. They did a chain (the attack chain): a click let them sign in, no second proof let the sign-in work, a flat network let them reach the database, " +
                    "an open port let them administer it, a reachable backup let them lock it.\n\nThe click is the trigger. The gaps are the causes. A chain that ends at \"the clerk clicked\" has stopped one link too early.",
            fixes = "The chain: the links in order, each with its room and its gap, the trigger and the causes marked.",
            mistake = "The chain that stops at the person", why = "The ready chain is one link long: \"a clerk clicked the message\". The exit stays locked. Continue: what did the click give, what let that work, what let that reach.",
            ordered = true,
            items = new[]
            {
                I("click", "A clerk opened the helpdesk message and signed in on the fake page", ""), I("mfa", "The stolen password worked: the administrator had no second proof", ""),
                I("flat", "From the clerk's computer, the database was on the same network", ""), I("port", "Remote administration was open, and the admin portal was taken", ""),
                I("backup", "The backups sat on the same keys, and were locked with everything else", ""),
            },
            fields = new[] { F("kind", "This link is", "?", "The trigger", "A cause: a control gap", "A person's fault") },
            readyOrder = new[] { "click", "mfa", "flat", "port", "backup" },
            brokenOrder = new[] { "click", "backup", "port", "flat", "mfa" },
            ready = D(("click/kind", 1), ("mfa/kind", 2), ("flat/kind", 2), ("port/kind", 2), ("backup/kind", 2)),
            broken = D(("click/kind", 3), ("mfa/kind", 0), ("flat/kind", 0), ("port/kind", 0), ("backup/kind", 0)),
            recall = new Recall { question = "What is the second containment step?", options = new[] { "Invalidate: end every session", "Rotate: change the password", "Revoke: take the access away" }, answer = 0, from = "3.2" },
            live = "Each link lights the room it ran through as you place it. The exit opens when the chain is whole.",
            takeaway = "The click is the trigger. The gaps are the causes. Fix the gaps.",
            ask = "The last time something went wrong at work, did the story stop at a person?",
            attack = (st, x) =>
            {
                var a = new Attack();
                var right = new[] { "click", "mfa", "flat", "port", "backup" };
                bool inOrder = right.Select(k => st.Position(k)).SequenceEqual(new[] { 0, 1, 2, 3, 4 });
                if (!inOrder) a.Through("Network Maze", "links out of order", "the exit stays locked: the chain does not read as the attacker walked it");
                else a.Held("Network Maze", "the chain reads link by link, inbox to vault");
                if (st.Pick("click", "kind") == 3 || right.Any(k => st.Pick(k, "kind") == 3)) a.Through("Network Maze", "a person named as the cause", "the chain stops at the person; the gaps stay open");
                a.summary = a.AnyThrough ? "A chain that stops at a person leaves every gap open." : "The chain is whole. The exit unlocks.";
                return a;
            },
            check = (st, x, a) =>
            {
                var right = new[] { "click", "mfa", "flat", "port", "backup" };
                return new List<(Practised, string)>
                {
                    (Of(right.Select(k => st.Position(k)).SequenceEqual(new[] { 0, 1, 2, 3, 4 })), "Every link in the record is in the chain, in order"),
                    (Of(right.All(k => st.Pick(k, "kind") > 0)), "Each link names its room and its gap"),
                    (Of(st.Pick("click", "kind") == 1 && right.Skip(1).All(k => st.Pick(k, "kind") != 1)), "The trigger is marked, and is not called the cause"),
                    (Of(right.Skip(1).Any(k => st.Pick(k, "kind") == 2)), "At least one cause is a control gap from tier 2"),
                    (Of(right.All(k => st.Pick(k, "kind") != 3)), "No link names a person as the cause"),
                };
            },
        },
        new Room
        {
            number = 19, tier = 3, id = "3.4", room = "Operations Center", title = "Capstone: What changes, who, by when?",
            kicker = "The review names causes, never people, and ends in actions with a name and a date on each.",
            luma = "The blameless review: the timeline, the trigger, the root causes, and an action per gap with an owner, a due date and a done-when. Then the exit opens.",
            learn = "The review that names causes, never people (the blameless post-incident review): a timeline that matches the scribe's log, the trigger distinct from the root causes, " +
                    "and an action per gap, each with an owner, a due date and a done-when. The three priority actions trace back to the tier 1 register.",
            fixes = "The review: an action per gap, with owner, due date and done-when, in blameless words.",
            items = new[]
            {
                I("mfa", "Gap: no second proof on the administrator's account", ""), I("flat", "Gap: one flat network from the clerks to the database", ""),
                I("port", "Gap: remote administration open to the public side", ""), I("backup", "Gap: backups on the same keys", ""),
            },
            fields = new[]
            {
                F("owner", "Owner", "Nobody", "Lina", "Omar", "Nour"), F("due", "Due", "No date", "This week", "This month"),
                F("done", "Done when", "?", "Every privileged account signs in with two proofs / the kiosk cannot reach the data zone / 3389 blocked and logged from the public side / a restore from the separate vault succeeds", "\"When it is fixed\""),
                F("words", "The sentence", "?", "\"The control was missing; here is the action\"", "\"Dana should have known better\""),
                Once("trigger", "The trigger", "?", "The clerk's click on the helpdesk message", "The administrator's missing second proof", "Dana"),
                Once("cause", "A root cause", "?", "Controls missing in four rooms", "The clerk", "Bad luck"),
            },
            ready = D(("mfa/owner", 1), ("mfa/due", 1), ("mfa/done", 1), ("mfa/words", 1), ("flat/owner", 1), ("flat/due", 2), ("flat/done", 1), ("flat/words", 1),
                      ("port/owner", 1), ("port/due", 1), ("port/done", 1), ("port/words", 1), ("backup/owner", 1), ("backup/due", 2), ("backup/done", 1), ("backup/words", 1), ("room/trigger", 1), ("room/cause", 1)),
            recall = new Recall { question = "What is the trigger of the chain, and what are the causes?", options = new[] { "The click is the trigger; the control gaps are the causes", "The clerk is the cause", "The attacker is the trigger and the cause" }, answer = 0, from = "3.3" },
            live = "The facilitator reads your review as the panel. Every action needs a name, a date and a done-when.",
            takeaway = "The review names causes, never people, and ends in actions with a name and a date on each.",
            ask = "After our last incident, what changed, who owned it, and is it done?",
            attack = (st, x) =>
            {
                var a = new Attack();
                var gaps = new[] { "mfa", "flat", "port", "backup" };
                foreach (var g in gaps)
                    if (st.Pick(g, "owner") == 0 || st.Pick(g, "due") == 0 || st.Pick(g, "done") != 1) a.Through("Operations Center", $"an action without an owner, a date or a done-when: {g}", "the gap stays open after the review");
                if (gaps.Any(g => st.Pick(g, "words") == 2) || st.Pick("room", "cause") == 2 || st.Pick("room", "trigger") == 3) a.Through("Operations Center", "a sentence that blames a person", "the review names a person, and the next incident finds the same gaps");
                if (!a.AnyThrough) a.Held("Operations Center", "every cause has an owned, dated action; nobody is blamed. The exit opens.");
                a.summary = a.AnyThrough ? "The review left gaps open, or blamed a person." : "The exit is open.";
                return a;
            },
            check = (st, x, a) =>
            {
                var gaps = new[] { "mfa", "flat", "port", "backup" };
                return new List<(Practised, string)>
                {
                    (Of(st.Pick("room", "trigger") == 1 && st.Pick("room", "cause") == 1), "The trigger and the root causes are distinct"),
                    (Of(gaps.All(g => st.Pick(g, "done") == 1)), "Every cause has an action with a done-when"),
                    (Of(gaps.All(g => st.Pick(g, "owner") > 0 && st.Pick(g, "due") > 0)), "Every action has an owner and a due date"),
                    (Of(gaps.All(g => st.Pick(g, "words") == 1) && st.Pick("room", "cause") != 2), "No sentence blames a person"),
                    (Of(st.Pick("mfa", "owner") > 0 && st.Pick("flat", "owner") > 0 && st.Pick("backup", "owner") > 0), "The priority actions trace back to the tier 1 register"),
                };
            },
        },
    };

    public static int Count => All.Length;
    public static Room Get(int number) => number >= 1 && number <= All.Length ? All[number - 1] : null;
    public static readonly string[] TierNames = { "What is at risk", "Prove the controls work", "The final exit" };
}
