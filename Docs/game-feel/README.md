# Game-feel pass: the design

Step 1 of `Docs/game-feel-pass-prompt.md`: the design for each game, before any code. One doc
per game, and this page for what the three share.

- [Robot Racing](robot-racing.md): level 1.1 "Who is driving?" and level 2.3 "What earns points?"
- [AI Arena](ai-arena.md): errand 1.1 "What does an agent do on its own?" and errand 1.5 "Which
  brain does the job?" (AI Arena has no tier 2: its seven errands are all tier 1, so 1.5, the
  errand with the most parts, stands in for it), plus errand 1.6's decide screen
- [Tech Escape](tech-escape.md): level 1.1 "What is worth protecting?" (Risk Room) and level 2.4
  "What is allowed through?" (Firewall Control)

Each doc lists, for every screen of those levels, what is on it today (every word and control,
counted from the current web build) and the sketch of what replaces it. The screenshots of
today's screens are in `games/<id>/Builds/look-audit/game-feel/before/`.

## How the "before" was counted

Each game's current web build (7 November 2026, after the voice and the warm-up were removed)
was played in Chrome, level by level, and every screen captured once its text had finished
typing. A word is a whitespace-separated token with a letter or a digit in it ("1.1", "30,000"
and "10k" are words; "·" is not). Left out of the counts, as the prompt asks: what the player
types, and what Claude writes (AI Arena's thought bubbles and card quotes). A control is
anything the player can press, type in or drag.

The yardstick, counted the same way on Claude Bomb's frames:

| Claude Bomb | Words | Controls |
|---|---|---|
| Briefing ("Incoming transmission") | 78: the camera feed's labels 10, the header 8, four story lines 60 | 1 (Skip) |
| Play screen | 68 at the frame counted, of which 22 are a passing LUMA chat line and 17 her card; 46 without the passing line | 4 groups (Manuals, the camera pad of 5, pause, the settings gear) + the main action (the chat box and send) |

Claude Bomb itself is over the prompt's limits (50 on a briefing, 30 on a play screen, 15 words
a LUMA line: its tour line is 21). The three games are held to the prompt's limits, not to
Claude Bomb's counts.

## What the three games share after the pass

**The menu.** One icon in the top-right corner and Esc open the game menu, as in Claude Bomb
(`GameMenu.cs`): Resume, The file, The briefing (replays the transmission), How to play (the
tour, where a game has one), Settings (sound, and each game's own: speed, turns, the class),
This level again, Title screen, Games menu. The game pauses while it is open. Every button that
sits on a play, briefing or debrief screen today and is not the screen's own action moves here.

**The objective line.** One line at the top left, at most 8 words, the same on the briefing,
the play screen and the run: what to do, not what the level is called. The level's question
(its title) stays in the file and on the title's level map.

**LUMA.** Text only, since the owner removed the text-to-speech on 7 November (the prompt's
"spoken and subtitled" becomes "subtitled"). One line at a time, at most 15 words, shown whole
(it fades in, it is never typed out mid-sentence), held for about a third of a second a word
plus a second and a half, then gone. She speaks when something happens, not to explain a
screen. Her lines are taken word for word from what she says today: one sentence of her intro,
her live line, the level's takeaway or a moment line, whichever fits the moment and is 15 words
or fewer. Her full lines and the curriculum text stay in the file, unchanged. Where no sentence
of hers is short enough, she says nothing.

**The briefing is a transmission.** Claude Bomb's shape: the world on the left in its camera
feed, on the right a header (who is calling), three or four short lines typed in, and one
button. No chips, no cards, no ladder, no part grid. At most 50 words with the feed's labels.
The lines are new short text written from the level's story; the doc shows them for 1.1 and the
tier-2 level, and the other levels' lines will be written the same way and listed in the look
audit for the owner to change.

**Parts arrive when they open.** Nothing shows the future: no "OPENS AT", no grey chips, no
ladder off the title. When a level opens a part, it arrives in the world the first time the
player reaches the setup screen (the part is fitted to the car or the agent, the lever lights
on the console), with a two-word banner ("NEW: SIGHT") and, where she has one, LUMA's line.

**The debrief is a moment, then a door.** One big result in the world, the medal and its stars,
LUMA's one line, "Next" and a quiet "Again". Everything else (the chart, the checks, the runs or
agents table, the costs, the questions' review) sits behind one "Details" control, closed by
default. At most 30 words with Details closed.

**Questions become actions.** Where a level asks questions today (Robot Racing's debrief
questions, AI Arena's decide screen), each question is one action on the world: tap the part
that answers it, drop the note in the right place. The answers are stored as they are today, so
the checks and the platform's report do not change.

**Tension.** Each game gets one thing to watch that runs down while it plays, shown by the
world: Robot Racing's attempt budget as a fuel gauge, AI Arena's dinner time as the market
clock with the family at the window, Tech Escape's attacker on the corridor monitor coming
closer. Never a loss on its own; it is there to care about.

**What does not change.** What each level teaches, its checks, its scoring, the platform's
progress reports, the curriculum text in the file, LUMA's lines (shortened only by choosing one
of her own sentences), the exact words the agents read (AI Arena's system prompt), the models,
materials, lighting and post. Actions write the same state the forms write today, so every
check reads the same values.

## How the after will be measured

The screenshot walk gains a counter: on every screen, the visible words of every visible TMP
text (minus the player's own text and Claude's replies) and the controls on screen, logged in
the player log and written to a table per game. Limits: play screens 30 words and 3 controls
plus the main action, briefings 50 words, debriefs 30 words with Details closed, every LUMA line
complete and 15 words or fewer, and no "OPENS AT", ladder, coordinates, seeds or probabilities
on a play or briefing screen. Then the two fresh reviewers of the prompt (the player test on
level 1.1's screenshots, the Claude Bomb test on the side-by-sides), and Phase 7 in
`Docs/look-audit.md`.
