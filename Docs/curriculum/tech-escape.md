# Tech Escape: content, levels and progression

**Topic:** defending a real system. **Follows:** Applied Introduction to Information Security (tier 1), Professional
Practices in Information Security (tier 2), and for the final exit the incident response day of Cyber Security
Fundamentals together with NIST SP 800-61 Rev. 3 (tier 3). Read [README.md](README.md) first: it sets the steps every
level follows.

**What the game is, from the outline.** A technology facility in lockdown where every door is a security problem: not a
riddle, not a code taped under a chair. You are handed systems that are genuinely misconfigured, and you have to actually
fix them; then the facility runs the attack against whatever you built and shows you what got through. The final exit
stays locked until you can reconstruct the entire attack chain. The twelve rooms: Suspicious Inbox, Identity Chamber,
Password Vault, Network Maze, Firewall Control, Malware Containment, Patch Laboratory, Encryption Chamber, Operations
Center, Cloud Chamber, Backup Vault, Risk Room. Final exit: Incident Response.

**A note on which course.** The catalogue (`site/games.json`) says the game follows "Cybersecurity Fundamentals". That
course, as built in Academy OS, is an application security course for working software engineers: threat modelling, secure
coding, the OWASP Top 10, the software supply chain and incident response, on a fictional checkout service. Its five days
contain few of the rooms: phishing, firewalls, backups, network segmentation and ransomware are absent or one line. The two
information security programs on the training sheet (Capability Development Trainings: "Applied Introduction to
Information Security", intermediate, 2 to 3 days; "Professional Practices in Information Security", advanced, 3 to 4 days)
cover eleven of the twelve rooms and the exit. Both exist today as an approved sheet line (description, outcomes, hands-on
practice, certification) and a research file, not yet as decks or labs. This file follows those two, in their order, and
takes the engineer course's incident day where it meets them. The catalogue line should change (open point 1).

## Sources

| Source | What the game takes from it |
|---|---|
| Training sheet, `academy-os/Capability Development Trainings_AI(Unique Programs) (1).csv`, rows Applied Introduction to Information Security and Professional Practices in Information Security | The outcomes of tiers 1 and 2, verbatim; the five hands-on practices of each; the fictional permit-issuing service; the certifications |
| Its research, `academy-os/.../research-courses/Capability-Development-Trainings/researcher-5-information-security.md` | What each outcome traces to (ISC2 CC domains, ISO/IEC 27002:2022 themes, ISO/IEC 27005:2022, NIST CSF 2.0's six Functions, NIST SP 800-61 Rev. 3); what the market agrees on at each level; "reporting rather than responding" at the introduction level |
| Cyber Security Fundamentals, day 5, `academy-os/.../instructor-guides/cyber-security-fundamentals_2026-08-12_12-54-09/`, `generated-labs/.../07-tabletop.md`, `08-postmortem.md`, `handouts/.../Day-5-*`, `assessments/cyber-security-fundamentals_2026-08-13_11-25-04/` | The incident lifecycle and its roles; severity levels; "rotate, invalidate, revoke"; isolate without destroying evidence; rebuild or patch; the blameless review; root cause distinct from the trigger; the incident as a graph of the week's gaps |
| Cyber Security Fundamentals, days 1 to 3 (same folders) | The 5x5 risk grid and its scoring rule; "not every boundary is a firewall"; the three habits; least privilege for every principal; the A05 and A07 checklists; the cloud-metadata chain; the misconception index |
| Ethical Hacking, `academy-os/.../developed-programs/ethical-hacking-2026-08-14_14-29-05/program-overview.md`, day 4 | What a convincing phishing campaign is made of (pretext, template, landing page), for the attack the facility runs |

## What a student can do after each tier

The tier 1 and tier 2 lines are the sheet's learning outcomes, verbatim.

| Tier | Follows | The student can |
|---|---|---|
| 1. What is at risk | Applied Introduction to Information Security | Explain confidentiality, integrity and availability using examples drawn from a public-sector service. Classify the information assets of a business process by sensitivity, owner and the impact of their loss. Assess risks to those assets by rating threat, vulnerability, likelihood and impact on a simple risk matrix. Select proportionate controls for each high risk from the ISO/IEC 27002:2022 themes and the NIST CSF 2.0 Functions. Recognize phishing, credential theft and ransomware, and report a suspected incident with the facts a responder needs. |
| 2. Prove the controls work | Professional Practices in Information Security | Conduct a risk assessment following ISO/IEC 27005:2022 and record treatment decisions in a risk register. Implement and document controls from ISO/IEC 27002:2022, including the evidence an auditor will request. Prioritize vulnerabilities by severity score, exposure and asset value, and track remediation to closure. Review user and privileged access against least privilege and produce a dated, signed-off access review record. Analyze system, network and authentication logs to detect suspicious activity and decide whether an event is an incident. Apply NIST SP 800-61 Rev. 3 to contain, eradicate and recover from a simulated incident. |
| 3. The final exit | The incident day of Cyber Security Fundamentals; Professional Practices outcomes 6 and 7 | Declare an incident and its severity, and take a role. Contain a credential compromise without destroying evidence. Reconstruct the attack chain from the facility's record, with the root cause distinct from the trigger. Write a blameless post-incident review whose action items each have an owner, a due date and a done-when. |

## Words on screen

The plain words come first; the term in brackets is shown once, when its level introduces it.

| Plain words | Term | Introduced |
|---|---|---|
| kept private, kept correct, kept available | confidentiality, integrity, availability | 1.1 |
| what the service cannot work without, who owns it, what its loss would cost | information asset, owner, impact | 1.1 |
| who might do it, the weakness they would use, how likely, how bad | threat, vulnerability, likelihood, impact | 1.2 |
| the 5 by 5 grid | risk matrix | 1.2 |
| a safeguard, chosen for the risk it lowers | control | 1.2 |
| know, protect, notice, respond, recover (and decide who is responsible) | the NIST CSF 2.0 Functions: Identify, Protect, Detect, Respond, Recover, Govern | 1.2 |
| a message that pretends, to make you click or sign in | phishing | 1.3 |
| a stolen password, or a reused one | credential theft | 1.4 |
| a second proof it is you | multi-factor authentication (MFA) | 1.4 |
| software that locks your files and asks for money | ransomware | 1.5 |
| the facts a responder needs | incident report | 1.5 |
| the list of risks and what was decided about each | risk register, treatment | 2.1 |
| proof the control is in place and working | evidence | 2.1 |
| only the access the job needs | least privilege | 2.1 |
| a known weakness in software, with a score | vulnerability, severity | 2.2 |
| reachable from outside, or not | exposure | 2.2 |
| a line where what you assume about the caller changes | trust boundary | 2.3 |
| networks kept apart | segregation | 2.3 |
| allow only what is needed, and when in doubt, block | allow list, fail closed | 2.4 |
| scrambled so a thief cannot read it; the key kept apart | encryption, key management | 2.5 |
| the settings it came with | defaults | 2.6 |
| something happened; something happened that matters | event, incident | 2.7 |
| cut it off, clean it out, bring it back | contain, eradicate, recover | 2.8 |
| a copy that still works when everything else is gone | backup, last known good | 2.9 |
| how bad, from 1 to 4 | severity level | 3.1 |
| the person in charge, the person writing, the person doing | commander, scribe, engineer | 3.1 |
| change the key, end the sessions, take the access away: in that order | rotate, invalidate, revoke | 3.2 |
| the whole path the attacker took, link by link | attack chain | 3.3 |
| what set it off, and what let it happen | trigger, root cause | 3.3 |
| the review that names causes, never people | blameless post-incident review | 3.4 |

Terms that stay in the course and off the screen: STRIDE, DFD, OWASP category numbers, SBOM, SLSA, ISO clause numbers.
The standards' names (ISO/IEC 27002, NIST CSF 2.0, NIST SP 800-61) appear once each on the debrief that introduces them,
as the place the course's method comes from.

## The running case

The course's case: "a fictional permit-issuing service" in the public sector (tier 1), "a simulated organization's shared
services" (tier 2). The facility's rooms are that service's systems. What it holds, what it runs and who works there are
the game's to fill in; the course fixes only that it is a public-sector service that issues permits, that its assets are
classified by sensitivity, owner and impact, and that its people handle sensitive information without being security
staff.

## The spine: the record and the chain

Every room from tier 2 on ends the same way: the student fixes what is misconfigured, the facility runs the attack, and
**what got through is written to the facility's record** as one line: the room, the gap, what the attacker reached. A
room where nothing got through writes "held". That record is the tier 3 material: the final exit asks the student to put
the lines in order as the chain the attacker walked, link by link, and to say which link was the trigger and which were
the causes. The engineer course does the same on its last day, drawing the incident as a graph of the week's gaps; the
Capital One recitation it uses (a reachable internal address, a read of temporary credentials, a listing of storage, the
data leaving) is the model of what a chain looks like.

So the rooms are not independent puzzles. A room the student leaves with a gap is a link the attacker will use later,
and the student will have to find it again at the exit.

## Rooms to levels

The outline names the rooms; the course sets the order they open in. Tier 1 is the Introduction's method once through:
know what you have, rate the risk, meet the two attacks staff actually meet, report. Tier 2 is one room per control.

| Room | Tier.Level | Course outcome and practice |
|---|---|---|
| Risk Room | 1.1, 1.2 | Outcomes 1 to 4; practice 2 (asset register and 5x5 risk matrix for the permit service) |
| Suspicious Inbox | 1.3 | Outcome 5; practice 3 (classify sample emails as safe or suspicious, with reasons) |
| Password Vault | 1.4 | Outcome 5; practice 3 (check sample accounts for MFA and weak passwords) |
| (Malware Containment, first visit) | 1.5 | Outcome 5; practice 4 (lost laptop and ransomware tabletop, one-page reporting form) |
| Identity Chamber | 2.1 | Professional Practices outcome 4 (access review against least privilege, signed off) |
| Patch Laboratory | 2.2 | Outcome 3 (prioritize by severity, exposure and asset value; track to closure) |
| Network Maze | 2.3 | Outcome 2 (ISO/IEC 27002 technological theme: segregation of networks); trust boundaries |
| Firewall Control | 2.4 | Outcome 2 (network controls: allow only what is needed, fail closed) |
| Encryption Chamber | 2.5 | Outcome 2 (cryptography control: in transit, at rest, keys apart) |
| Cloud Chamber | 2.6 | Outcome 2; the engineer course's A05 checklist and metadata chain |
| Operations Center | 2.7 | Outcome 5; practice 3 (trace a credential-theft intrusion in authentication and network logs) |
| Malware Containment | 2.8 | Outcome 6; practice 4 (staged ransomware: contain, recover, keep an incident log) |
| Backup Vault | 2.9 | Outcome 6 (recover); the Recover Function; the ransomware scenario |
| (the register) | 2.10 | Outcomes 1 and 2; practice 1 and 5 (risk register and treatment plan; the defended review) |
| Final exit | 3.1 to 3.4 | Outcomes 6 and 7; the engineer course's day 5 |

## The progression at a glance

| Level | Question | The student fixes | Fix it (the planned mistake) |
|---|---|---|---|
| 1.1 | What is worth protecting? | the asset register | An asset nobody listed |
| 1.2 | How likely, how bad? | the risk matrix and one control per high risk | Everything rated Medium |
| 1.3 | Which of these is an attack? | the inbox sort | The one that looked right |
| 1.4 | Who can get in? | the account check | MFA for everyone but one |
| 1.5 | Capstone: Report it | the report, and three priority actions | (none) |
| 2.1 | Who should still have access? | the access list | The leaver with admin |
| 2.2 | Which hole first? | the patch order | Severity score alone |
| 2.3 | What can reach what? | the network zones | One flat network |
| 2.4 | What is allowed through? | the rules | Allow everything, at the bottom |
| 2.5 | Who can read it if they take it? | what is encrypted, and where the key is | The key next to the data |
| 2.6 | What did the defaults leave open? | the cloud settings | Public by default |
| 2.7 | Is this an event or an incident? | the logs and the alerts | The log that was never kept |
| 2.8 | How do you stop it without losing the evidence? | the containment order | Delete first |
| 2.9 | Can you get it back? | where the backups live and who can reach them | Backups on the same keys |
| 2.10 | Capstone: The register and the evidence | the whole facility | (none) |
| 3.1 | Is it an incident, and how bad? | the declaration, the roles | The commander who starts typing |
| 3.2 | What do you cut first? | the containment steps | Rotated, and stopped there |
| 3.3 | How did they get from the inbox to the vault? | the chain | The chain that stops at the person |
| 3.4 | Capstone: What changes, who, by when? | the review | (none) |

## Tier 1: What is at risk

Follows Applied Introduction to Information Security: for "professionals who handle sensitive information but do not work
in security". The method, once through on the permit service. At this level the course teaches reporting, not responding
(research: "reporting rather than responding"): a tier 1 student recognizes and reports; fixing systems starts in tier 2.

**Placement check:** name the three things security protects; rate one risk on the grid; sort three emails. A pass starts
the student at 1.5.

### Level 1.1: Risk Room: What is worth protecting?

- **Follows:** outcome 1 (confidentiality, integrity and availability "using examples drawn from a public-sector service")
  and outcome 2 (classify assets "by sensitivity, owner and the impact of their loss"); the pre-work module on the core
  vocabulary; the CSF 2.0 Function Identify.
- **The student can:** list the permit service's information assets, give each an owner and a sensitivity, and say what its
  loss would mean in terms of private, correct and available.
- **The idea:** you cannot protect what you have not listed. Every asset has an owner, a sensitivity, and a cost if it is
  leaked (private), changed (correct) or gone (available). The same asset can fail in all three ways.
- **The student fixes:** the asset register: for each system in the facility, what it holds, who owns it, how sensitive,
  and the impact of its loss in each of the three ways.
- **Fix it: An asset nobody listed.** The ready register misses one (a working copy of applicants' documents kept outside
  the main system). The facility's attack takes exactly that one. The student adds it, with an owner and an impact.
- **The attack must show:** which asset was taken, and which of the three (private, correct, available) was lost.
- **Checks:** every asset in the facility is on the register; each has an owner; each has an impact in the three terms;
  the unlisted one was found.
- **Takeaway:** You cannot protect what you have not listed, and nobody protects what nobody owns.
- **Ask back at work:** Which information do I handle that is on nobody's list?

### Level 1.2: Risk Room: How likely, how bad?

- **Follows:** outcome 3 ("rating threat, vulnerability, likelihood and impact on a simple risk matrix") and outcome 4
  ("proportionate controls for each high risk from the ISO/IEC 27002:2022 themes and the NIST CSF 2.0 Functions"); practice
  2, the 5x5 risk matrix; the engineer course's grid rule, "score first by likelihood, then by impact", and its handout:
  "You score everything Medium to avoid conflict ... if it is exploitable today with no special access, it is High".
- **The student can:** rate each risk on the 5x5 grid and choose one proportionate control for each high risk, naming which
  Function it serves.
- **The idea:** a risk is someone (threat) using a weakness (vulnerability) against an asset. Rate how likely, then how bad.
  High risks get a control; a control is chosen for the risk it lowers, and does one of: know, protect, notice, respond,
  recover.
- **The student fixes:** the risk matrix for the register from 1.1, and a control for each high risk, picked from a short
  list grouped by Function.
- **Fix it: Everything rated Medium.** The ready matrix rates every risk Medium and gives each the same cheap control. The
  facility runs the top three threats; the ones that were really High, with no fitting control, get through. The student
  re-rates by likelihood first, and picks controls that fit.
- **The attack must show:** for each risk that got through, its real rating next to the student's, and whether the chosen
  control could ever have stopped it.
- **Checks:** rated likelihood before impact; no high risk without a control; each control names its Function; nothing
  "exploitable today with no special access" rated below High.
- **Takeaway:** Rate the risk, then pick the control for the risk. Not the other way round.
- **Ask back at work:** Which risk at work do we rate Medium to avoid an argument?
- **Recall:** Name the three ways an asset can be lost.

### Level 1.3: Suspicious Inbox: Which of these is an attack?

- **Follows:** outcome 5 ("recognize phishing ... and report a suspected incident with the facts a responder needs");
  practice 3, "classify sample emails as safe or suspicious with reasons"; the engineer course's one line on it, "MFA
  optional is MFA off for the account that gets phished"; Ethical Hacking day 4 for what a campaign is made of (pretext,
  template, landing page, tracking).
- **The student can:** sort a set of messages into safe and suspicious with a reason each, and report the suspicious ones
  with the facts a responder needs.
- **The idea:** a phishing message is built to look right: the logo, the sender's name, the urgency, the familiar request.
  The reason it is suspicious is never the look; it is the ask, the address behind the name, the link behind the text,
  the pressure. A report says who got it, when, what it asked, what was clicked and what was entered.
- **The student fixes:** the inbox sort, with a reason per message, and a report for each suspicious one.
- **Fix it: The one that looked right.** The ready sort marks one message safe because everything visible is right. The
  facility runs the campaign; that message gets a click and a sign-in. The student finds the reason it was wrong (the
  address, the link, the ask) and re-sorts.
- **The attack must show:** which message got a click, what the person entered, and what the attacker now holds.
- **Checks:** every suspicious message caught, with a reason that is not "it looked wrong"; no safe message reported; each
  report has the facts a responder needs.
- **Takeaway:** The look is the attack. The ask, the address and the link are the evidence.
- **Ask back at work:** What would I do first if I had already clicked?
- **Recall:** Which do you rate first, likelihood or impact?

### Level 1.4: Password Vault: Who can get in?

- **Follows:** outcome 5 (credential theft); practice 3, "check sample accounts for multi-factor authentication and weak
  passwords"; the engineer course's A07 checklist ("MFA required for privileged accounts") and its misconception M3.5, "MFA
  prevents credential stuffing. Reality: MFA raises the cost; rate-limiting and breach-password checks are needed too".
- **The student can:** check a set of accounts for weak or reused passwords and missing MFA, and say which accounts a stolen
  password would open.
- **The idea:** a password that is short, guessable or reused elsewhere is already stolen; it just has not been used yet.
  A second proof (MFA) means the stolen password is not enough. It raises the cost; it does not make guessing impossible,
  so guesses are also limited and known-stolen passwords refused.
- **The student fixes:** the account check: for each account, the password's strength and reuse, whether MFA is on, and
  whether the account is privileged.
- **Fix it: MFA for everyone but one.** The ready setup has MFA on for every account except the administrator's, "because it
  gets in the way". The facility runs the password from 1.3's click and a list of stolen passwords against every account.
  The administrator's falls. The student turns MFA on where it matters most, and limits guesses.
- **The attack must show:** which accounts fell, and by which route: the phished password, a reused one, a guessed one.
- **Checks:** every privileged account has MFA; weak and reused passwords flagged; guesses limited; the administrator's
  account held.
- **Takeaway:** A password alone is one proof. The account that matters most needs two.
- **Ask back at work:** Which of my accounts share a password, and which one has no second proof?
- **Recall:** Name two facts a responder needs in a phishing report.

### Level 1.5: Capstone: Report it

- **Follows:** outcome 5 (ransomware; "report a suspected incident with the facts a responder needs"); practice 4, "work
  through a lost laptop and a ransomware scenario using a one-page incident reporting form"; practice 5, "a one-page risk
  and control summary for the case service with three priority actions, presented to the group".
- **The student can:** recognize the signs of ransomware and of a lost device, fill the one-page reporting form with the
  facts a responder needs, and present three priority actions for the service, each tied to a high risk and its control.
- **The task:** two scenes, in the tabletop's order: a laptop with the permit register on it is lost; then files on a shared
  drive turn unreadable and a note appears. For each, the one-page form. Then the one-page summary: the three highest risks
  from 1.2, the control for each, and why these three first.
- **Checks:** both forms have the facts a responder needs (what, when, who noticed, what is affected, what was done so far);
  the ransomware signs named; nothing on the form is a guess presented as a fact; the three actions each tie to a high
  risk and a control from the register.
- **Takeaway:** At this level the job is to notice and report well. Responding comes next.
- **Ask back at work:** Who do I call, and what will they ask me?
- **If-then plans:** two, about the student's own work.

## Tier 2: Prove the controls work

Follows Professional Practices in Information Security: "assessing risk, putting controls in place and proving they work,
monitoring logs, and handling incidents", as "case work on a simulated organization". Each room is one control from the
tier 1 register. The student is handed it misconfigured, fixes it, writes the evidence an auditor would ask for, and the
facility runs the attack. What got through goes to the record (see "The spine").

**Placement check:** given one misconfigured room and its attack, find the gap and name the control. A pass starts the
student at 2.10.

### Level 2.1: Identity Chamber: Who should still have access?

- **Follows:** outcome 4, "review user and privileged access against least privilege and produce a dated, signed-off access
  review record"; the engineer course, "Least privilege applies to every principal" and its day 5 scenario of a former
  contractor's key still working.
- **The student can:** review every account against the job it serves, remove or reduce what the job does not need, and
  sign a dated review record.
- **The idea:** access is granted for a job and almost never taken back. Review every account: does the job still exist, does
  it need this much, who approved it. The review is only done when it is written down, dated and signed.
- **The student fixes:** the access list: each account, its role, what it can reach, and the decision (keep, reduce, remove).
- **Fix it: The leaver with admin.** A contractor who left months ago still has an administrator account. The facility
  runs the attack with that account's stolen password. The student removes it and reduces two others that have more than
  their job needs.
- **The attack must show:** which account the attacker used, and what it could reach.
- **Checks:** no account for a job that no longer exists; no account with more than its job needs; a dated, signed review
  record; the record names who approved each exception.
- **Takeaway:** Only the access the job needs, reviewed on a date, signed by a name.
- **Ask back at work:** Who still has access to something because nobody took it away?
- **Recall:** What does MFA change about a stolen password?

### Level 2.2: Patch Laboratory: Which hole first?

- **Follows:** outcome 3, "prioritize vulnerabilities from scan results by severity score, exposure and asset value, and
  track remediation to closure"; practice 2, rank the findings and write remediation tickets; the engineer course's day 4
  rule, "evaluate a CVE for reachability before opening a ticket, not just presence", and its misconception M4.2, "If a CVE
  is present in your tree, it is exploitable".
- **The student can:** order a scan's findings by severity, exposure and the value of the asset, write a ticket for each with
  an owner, and track them to closure.
- **The idea:** the score on a weakness says how bad it could be, not whether anyone can reach it. A medium weakness on a
  system facing the public and holding the permit register comes before a critical one on an internal test machine.
  Severity, then exposure, then what the asset is worth.
- **The student fixes:** the patch order and the tickets.
- **Fix it: Severity score alone.** The ready order sorts by score. The critical on the test machine is fixed first; the
  medium on the public permit portal is still open when the facility runs the attack, and that is the one it uses.
- **The attack must show:** which weakness was used, where it sat, and what its score was.
- **Checks:** ordered by severity, exposure and asset value, not score alone; every ticket has an owner and a date; nothing
  reachable from outside on a high-value asset left open; closures recorded.
- **Takeaway:** A score says how bad. Exposure says whether. The asset says how much it matters.
- **Ask back at work:** Which of our known weaknesses faces the public?
- **Recall:** What makes an access review complete?

### Level 2.3: Network Maze: What can reach what?

- **Follows:** outcome 2 (controls from ISO/IEC 27002:2022, technological theme: segregation of networks, with evidence);
  the engineer course's trust boundaries ("any line where assumptions about the caller change"; "Internal network" as a
  boundary; why service-to-service calls are boundaries) and its misconception M1.5, "Trust boundaries are network
  boundaries".
- **The student can:** split the facility's network into zones so that what the public can reach cannot reach what holds
  the data, and write the evidence that it is so.
- **The idea:** on one flat network, whoever gets in anywhere can reach everything. Put a line wherever what you assume
  about the caller changes: public to internal, internal to the data, one service to another. A line is a place to check,
  and the evidence is the check.
- **The student fixes:** the zones and what may cross between them.
- **Fix it: One flat network.** The public kiosk, the staff computers and the permit database all sit on one network. The
  facility runs the attack from the kiosk and reaches the database. The student puts the lines in.
- **The attack must show:** the attacker's path from where it started to what it reached, zone by zone.
- **Checks:** the public zone cannot reach the data zone; each crossing is listed and justified; the evidence says what
  was checked and when.
- **Takeaway:** Put a line wherever what you assume about the caller changes.
- **Ask back at work:** From the visitor network at work, what can be reached?
- **Recall:** Which comes first when ordering patches: the score, or whether it is reachable?

### Level 2.4: Firewall Control: What is allowed through?

- **Follows:** outcome 2 (network controls, with evidence); the engineer course's A05 checklist, "Unused features, ports,
  and services turned off", admin reachable only from inside, the third habit, "Fail closed, log the reason, never leak
  the cause", and its day 1 question whose point is that "not every boundary is a firewall".
- **The student can:** write rules that allow only what the service needs across each line, block everything else, and log
  what was blocked.
- **The idea:** a rule set that ends with "allow everything" is no rule set. Allow what is needed, by name; block the rest;
  write down why each allow exists; log the blocks. Administration is never reachable from the public side.
- **The student fixes:** the rules on each line from 2.3.
- **Fix it: Allow everything, at the bottom.** The ready rules allow the right things and end with an allow-all "so nothing
  breaks"; remote administration is open to the public side. The facility runs the attack through the open administration
  port. The student replaces the last rule with block-and-log and closes administration from outside.
- **The attack must show:** the port and the rule the attacker came through.
- **Checks:** no allow-all; each allow has a reason; administration unreachable from the public side; blocks are logged;
  the service still works.
- **Takeaway:** Allow by name, block the rest, log the block.
- **Ask back at work:** What rule did we add "so nothing breaks", and is it still there?
- **Recall:** Where do you put a line on a network?

### Level 2.5: Encryption Chamber: Who can read it if they take it?

- **Follows:** outcome 2 (cryptography control, with evidence); the engineer course's A02 ("TLS 1.2 minimum", encryption
  with a managed key service), its rule that backup dumps "inherit the sensitivity of the most sensitive column", and its
  day 2, "what not to hand-roll".
- **The student can:** decide what is encrypted in transit and at rest, keep the key apart from the data, and show the
  evidence.
- **The idea:** encryption means a thief who takes the data cannot read it. That holds only if the key is somewhere the
  thief did not also take. Data in transit is encrypted so nobody on the way reads it; data at rest so a stolen disk or
  backup is unreadable; the key lives apart, with its own access.
- **The student fixes:** what is encrypted, where, and where each key is kept.
- **Fix it: The key next to the data.** The ready setup encrypts the permit database and keeps the key in a file on the same
  server; the backups are not encrypted at all. The facility runs the attack: the server's disk is copied, and so is a
  backup. The student moves the key and encrypts the backups.
- **The attack must show:** what was copied, and whether the attacker could read it.
- **Checks:** the sensitive assets from 1.1 are encrypted at rest; traffic across the public line is encrypted; no key on
  the same system as its data; backups encrypted; evidence of each.
- **Takeaway:** Encrypted data with the key beside it is not encrypted.
- **Ask back at work:** If someone walked out with our backup, could they read it?
- **Recall:** What should the last rule in a rule set be?

### Level 2.6: Cloud Chamber: What did the defaults leave open?

- **Follows:** outcome 2 (controls with evidence); the engineer course's A05 checklist, "Default credentials rotated",
  "Cloud storage buckets private by default", and its cloud-metadata chain (a reachable internal address, a read of
  temporary credentials, a listing of storage, the data leaving). The sheet says the Introduction's certification "needs
  further study" on networking and cloud; this room is the thinnest in the course (open point 3).
- **The student can:** find what the cloud account's defaults left open, close it, and show the evidence.
- **The idea:** a cloud service arrives with settings chosen for convenience, not for the permit service. Storage that is
  public by default, credentials that came with the box, internal addresses any service can reach: each is a door nobody
  decided to open.
- **The student fixes:** the cloud settings: storage visibility, default credentials, which services may reach the internal
  address, which have more rights than their job.
- **Fix it: Public by default.** A storage area holding applicants' documents is readable by anyone with the address, and a
  web service can reach the internal address that hands out temporary credentials. The facility runs the metadata chain.
  The student makes storage private, rotates the default credentials and cuts the web service's reach.
- **The attack must show:** each link of the chain in order, and which setting each one used.
- **Checks:** no public storage of sensitive assets; default credentials rotated; the internal address unreachable from
  services that do not need it; each service's rights limited to its job; evidence of each.
- **Takeaway:** A default is a decision somebody else made for you.
- **Ask back at work:** What did we switch on and never configure?
- **Recall:** Where must the key never be?

### Level 2.7: Operations Center: Is this an event or an incident?

- **Follows:** outcome 5, "analyze system, network and authentication logs to detect suspicious activity and decide
  whether an event is an incident"; practice 3, "trace a credential-theft intrusion" in authentication and network logs;
  the engineer course's day 5 "bad alert vs good alert", its A09 misconception M3.8 ("A09 is about verbose logs. Reality:
  the absence of the right logs"), and its rule to keep private data out of logs.
- **The student can:** decide which logs to keep and which alerts to raise, then read the logs to trace an intrusion and
  decide whether it is an incident.
- **The idea:** a log you never kept cannot be read later. An alert on everything is an alert on nothing. Keep the logs
  that answer "who signed in, from where, and what did they touch", alert on what is rare and bad, and when it fires,
  trace it: an event is something that happened; an incident is something that happened that matters.
- **The student fixes:** which logs are kept and which alerts raise.
- **Fix it: The log that was never kept.** The administration portal keeps no sign-in log, and the alert rule fires on every
  failed login. The facility replays the credential theft from tiers 1 and 2; the student cannot see the sign-in that
  mattered and the real alert drowns. The student turns the right log on, tunes the alert, replays, and traces the
  intrusion from the click to the data.
- **The attack must show:** the replay, with the logs as they are: what can and cannot be seen.
- **Checks:** sign-in, access and network logs kept for the sensitive assets; no private data in the logs; an alert that
  fired on the intrusion and not on the noise; the trace is complete from the first sign-in to the data touched; the
  decision (event or incident) is right and gives a reason.
- **Takeaway:** You can only investigate what you logged.
- **Ask back at work:** If someone signed in as me last night, would anything show it?
- **Recall:** Name one setting a cloud service comes with that must be changed.

### Level 2.8: Malware Containment: How do you stop it without losing the evidence?

- **Follows:** outcome 6, "apply the NIST SP 800-61 Rev. 3 recommendations to contain, eradicate and recover from a
  simulated incident"; practice 4, "handle a staged ransomware incident ... through containment, recovery and
  communication, keeping an incident log"; the engineer course's day 5: "Isolate Without Destroying Evidence", "Rebuild vs
  Patch", "Deactivate, then snapshot, then delete after 24 hours", and its misconception M5.2, "Delete compromised resources
  immediately. Reality: snapshot first".
- **The student can:** put the containment steps in an order that stops the spread without destroying the evidence, decide
  rebuild or patch, and keep an incident log.
- **The idea:** cutting a machine off stops the spread. Wiping it destroys the only record of what happened. Cut it off
  first, keep a copy, then clean. A machine that was taken over is rebuilt from a known-good state, not patched in place.
- **The student fixes:** the containment order, as steps, with the log entry for each.
- **Fix it: Delete first.** The ready playbook wipes the infected machine as step one. The facility runs the ransomware: the
  spread stops, and the record of how it got in is gone, so the record for tier 3 reads "unknown". The student reorders:
  isolate, snapshot, then clean, and the entry point is kept.
- **The attack must show:** the spread over time, and what evidence survived the student's order.
- **Checks:** isolated before anything else; a copy kept before cleaning; rebuild chosen over patch for the taken-over
  machine; an incident log with a time for each step; the entry point recorded.
- **Takeaway:** Cut it off, keep a copy, then clean. Deletion feels like containment; it destroys the evidence.
- **Ask back at work:** Who at work would wipe the machine first, and who would stop them?
- **Recall:** What is the difference between an event and an incident?

### Level 2.9: Backup Vault: Can you get it back?

- **Follows:** outcome 6 (recover); the CSF 2.0 Function Recover, named in outcome 4 of the Introduction; the ransomware
  scenario of both programs; the engineer course's recovery as redeploying "the last known-good" state and its rule that
  backups inherit the sensitivity of what they hold. The courses do not teach a backup rule by name (open point 4).
- **The student can:** place the backups where the attack cannot reach them, restrict who can reach them, test that a
  restore works, and show the evidence.
- **The idea:** a backup is the copy that still works when everything else is gone. It is only that if the thing that took
  everything else cannot take it too: not on the same network, not under the same administrator's key, and restored once
  to prove it restores.
- **The student fixes:** where the backups live, who can reach them, how often, and the restore test.
- **Fix it: Backups on the same keys.** The ready backups sit on the same network as the file server and are reachable with
  the same administrator credentials. The facility runs the ransomware from 2.8 with those credentials; the backups are
  encrypted with everything else. The student moves them behind their own line and their own access, and runs the restore.
- **The attack must show:** whether the backups survived, and whether the restore brought the service back.
- **Checks:** backups unreachable from the zones the attack ran in; their own access, not the administrator's; encrypted
  (from 2.5); a restore tested and dated.
- **Takeaway:** A backup the attacker can reach is not a backup.
- **Ask back at work:** When did we last restore something, on purpose?
- **Recall:** What is the first step of containment?

### Level 2.10: Capstone: The register and the evidence

- **Follows:** outcome 1, "conduct a risk assessment following ISO/IEC 27005:2022 and record treatment decisions in a risk
  register"; outcome 2, the evidence "an internal or external auditor will request"; practice 1 (register and treatment
  plan) and practice 5, a "control improvement plan, defended before a review panel".
- **The student can:** bring the whole facility to a risk register with a treatment decision per risk, show the evidence for
  every control, and defend it.
- **The task:** the register, built from tiers 1 and 2: each risk, its rating, the decision (treat, accept, avoid, share) and
  the control. Then the facility's audit: for each control, the student points to the evidence (a signed access review,
  a rule set with reasons, a dated restore test). Then the whole-facility attack, every room at once, with the record
  written for tier 3.
- **The facilitator:** can ask for any control's evidence, and decides whether it stands.
- **Checks:** every high risk has a treatment decision; every control has evidence that would satisfy an auditor; the
  whole-facility attack got through nowhere the student had closed; whatever got through is in the record with the room
  and the gap.
- **Takeaway:** A control nobody can show evidence for is a control nobody can be sure of.
- **Ask back at work:** Which control at work could I prove is working, today, with a document?
- **If-then plans:** two, about the student's own work.

## Tier 3: The final exit

Follows the engineer course's day 5 and the sheet's outcomes 6 and 7. The facility is now under attack for real: the
whole chain, through whatever the student left open in tier 2, and the record from 2.10 is what the student has to read.
NIST SP 800-61 Rev. 3's phases, as the course's handout puts them: "Preparation, Detection and Analysis, Containment,
Eradication and Recovery, Post-Incident. Not linear; you loop."

**Placement check:** given a run of the record, order the links and name the trigger and the root cause. A pass starts the
student at 3.4.

### Level 3.1: Is it an incident, and how bad?

- **Follows:** the engineer course's severity levels (SEV-1, "Customer data exposed or exfiltrated", declared in under 15
  minutes, down to SEV-4, "Anomaly worth tracking"); the tabletop's three roles, commander, scribe and engineer; its
  misconceptions M5.1, "The on-call engineer is in charge of an incident", and the handout's "You start typing as IC".
- **The student can:** declare the incident, set its severity from what is known, and take the commander's role without
  doing the engineer's work.
- **The idea:** the first minutes decide the rest. Declare it, say how bad on the scale, and split the work: one person
  commands, one writes everything down with times, one does the technical work. The commander who starts typing stops
  commanding.
- **Fix it: The commander who starts typing.** The ready scene has the commander fixing the firewall while nobody records
  what is happening and the severity is never set. The facility keeps moving; the record has holes. The student assigns the
  roles and declares.
- **The run must show:** the clock, the facts known so far, and what each role is doing.
- **Checks:** severity declared within the time limit and justified by the facts known; three roles assigned; the scribe's
  log has times; the commander made no technical change.
- **Takeaway:** Declare it, rate it, split the roles. Then act.
- **Ask back at work:** If this happened to us, who is in charge, and who writes it down?
- **Recall:** What is the evidence for an access review?

### Level 3.2: What do you cut first?

- **Follows:** the engineer course's day 5: "rotate / invalidate / revoke" matched to the kind of access, "Order matters:
  rotate first, invalidate second, revoke third"; its runbook rule, "no 'consider' language"; its misconception M5.3,
  "Rotate the credential is enough".
- **The student can:** contain a credential compromise in the right order, without destroying evidence, and write the steps
  as a runbook anyone could follow.
- **The idea:** a stolen credential is contained in three steps in order: change it (rotate), end every session using it
  (invalidate), take the access away (revoke). Rotating alone leaves the attacker's open sessions open. The runbook says do,
  never "consider".
- **Fix it: Rotated, and stopped there.** The ready runbook rotates the administrator's password and ends. The attacker's
  session from 2.7's trace is still open, and the facility shows it still reading. The student adds the second and third
  steps, in order, and keeps the snapshot rule from 2.8.
- **The run must show:** the attacker's access, step by step, as each containment step lands.
- **Checks:** rotate, invalidate, revoke, in that order; evidence kept before anything is removed; the runbook has no
  "consider"; the attacker's access ends.
- **Takeaway:** Rotate, invalidate, revoke. In that order, all three.
- **Ask back at work:** If my password were stolen tonight, who could end my sessions?
- **Recall:** Which role writes everything down?

### Level 3.3: How did they get from the inbox to the vault?

- **Follows:** the engineer course's last day, the incident drawn as a graph of the week's gaps, and its Capital One
  recitation as the model of a chain; the postmortem lab's rubric, "root cause distinct from immediate trigger"; the
  misconceptions M5.5, "Postmortem should identify the person", and M5.6, "The five whys always finds the root cause"; the
  sheet's outcome 7, "root causes, control gaps".
- **The student can:** reconstruct the attack chain from the record, in order, each link naming the room and the gap, and
  separate the trigger from the root causes.
- **The idea:** the attacker did not do one thing. They did a chain: a click let them sign in, no second proof let the
  sign-in work, a flat network let them reach the database, an open port let them administer it, a reachable backup let
  them lock it. The click is the trigger. The gaps are the causes. A chain that ends at "the clerk clicked" has stopped
  one link too early.
- **The task:** the record from 2.10 and the logs from 2.7, shuffled. The student orders the links, names the room and the
  gap for each, and marks the trigger and the causes.
- **Fix it: The chain that stops at the person.** The ready chain is one link long: "a clerk clicked the message". The exit
  stays locked. The student continues: what did the click give, what let that work, what let that reach.
- **The run must show:** the facility, with each link lighting the room it ran through as the student places it.
- **Checks:** every link in the record is in the chain, in order; each link names its room and its gap; the trigger is marked
  and is not called the cause; at least one cause is a control gap from tier 2; no link names a person as the cause.
- **Takeaway:** The click is the trigger. The gaps are the causes. Fix the gaps.
- **Ask back at work:** The last time something went wrong at work, did the story stop at a person?
- **Recall:** What is the second containment step?

### Level 3.4: Capstone: What changes, who, by when?

- **Follows:** the sheet's outcome 7, "a post-incident review that identifies root causes, control gaps and corrective
  actions with named owners"; practice 5, "an incident report and control improvement plan, defended before a review
  panel"; the engineer course's postmortem lab: a timeline, root cause distinct from the trigger, contributing factors, and
  action items "each having an owner, a due date, and a done-when criterion"; its blameless "Say this, not that".
- **The student can:** write the blameless review of the incident and defend it: timeline, trigger, root causes, control
  gaps, and an action per gap with an owner, a due date and a done-when.
- **The task:** the review, from the chain in 3.3 and the scribe's log from 3.1. Then the exit opens, and the facilitator
  reads the review as the panel.
- **Checks:** the timeline matches the log; the trigger and the root causes are distinct; every cause has an action; every
  action has an owner, a due date and a done-when; no sentence blames a person; the three priority actions trace back to
  the tier 1 register.
- **Takeaway:** The review names causes, never people, and ends in actions with a name and a date on each.
- **Ask back at work:** After our last incident, what changed, who owned it, and is it done?
- **If-then plans:** two, about the student's own work.

## Not taught by the game

What stays with the courses: threat modelling with data-flow diagrams and STRIDE, secure coding and the injection classes,
the OWASP Top 10 by category, software supply chain (SBOM, signing, provenance), the governance programs (ISMS, Statement
of Applicability, maturity, KPIs, the board); any real tool by name.

## Open points

1. **The catalogue line.** `site/games.json` gives Tech Escape the course "Cybersecurity Fundamentals". It should name
   Applied Introduction to Information Security (with Professional Practices for the second tier), or the game and the
   course will not match.
2. **Both sheet programs are unbuilt.** The game would be their first materials. The permit-issuing service should be
   defined once, for the course and the game together, so the register, the accounts and the rooms are the same case.
3. **Cloud Chamber is the thinnest room.** The Introduction's certification note says networking and cloud "need further
   study"; the room leans on the engineer course's checklist and chain.
4. **Backup Vault** has no named rule in the courses; the level teaches it through the Recover Function and the ransomware
   scenario, and names no rule.
5. **The engineer course's day 5 disagrees with itself** on the scenario (three versions), the roles (commander, scribe
   and engineer in the lab; commander, scribe and comms on a slide) and the runbook's sections. This file uses the lab's
   roles; the scenario is the game's own chain.
6. **Room order.** The outline lists the Risk Room last; the course starts there. This file follows the course.
