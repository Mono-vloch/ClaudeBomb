# AI Arena: screen inventory, before

Build served at http://localhost:8085/games/ai-arena/ (files `ee4e5083e1631a8805062c5cba38b944.data.unityweb`, `953a7f0e9ec407a9011738dad8de7017.wasm.unityweb`, dated 7 Nov 14:38). Branch `realism-pass`. Captured 7 Nov 2026, 1600 x 900, fresh profile per run.

**Method.** Headless Chrome driven over CDP (a copy of `play.mjs`, port 9352). The copy renders WebGL on the GPU (`--use-angle=metal`) instead of SwiftShader: with SwiftShader and two other agents' Chrome sessions plus a Unity build on the machine, the game ran at about 1 fps, the story beats moved on before their animations finished (beat 3's note, beat 5's verdict, beat 6's last cards never appeared). On the GPU it runs at 60 fps and every beat completes. Frames were taken from a screencast (one every 0.25 s) and the last frame before each change was chosen, so LUMA's line has finished typing. Rounds use real Claude turns, so each round differs; the round frames come from three runs (noted per screen).

**Counting.** A word is a whitespace-separated token with at least one letter or digit (`·`, `/`, `›`, `...` alone are not words; `1.1`, `30,000`, `10k`, `$0.001`, `7:42` are). Each screen's **words** total counts every visible game word, including the wordmark and LUMA's line. Not in the total: Claude's text (thought bubbles and the quoted line on the agent card) and the **pre-filled starter list** in the garage's list field (game-written but sitting in the player's field, shown separately as `+N list`). No player-typed text appears anywhere: every list was left as the game pre-filled it. Underlay text under a tour card's dim is counted when legible; parts hidden by the card are listed and not counted.

Screenshots: `games/ai-arena/Builds/look-audit/game-feel/before/<errand>-<nn>-<screen>.jpg`. Source frames (all casts and shots) are kept under `/private/tmp/claude-501/-Users-cdaoud-Desktop-maaloomatiia-games/93c6a380-3a2f-41bd-ab0e-2242c69e5aa5/scratchpad/aa-runs/` (runs 5-9 used).

Screens not reached: none. Not opened (outside the brief): The file, Settings, What it knows, The lists, the Build-it-from-parts mode, Free play.

## 1.1-01-title · Title

Screenshot: `1.1-01-title.jpg` (source frame `aa-runs/5/00-title.jpg`)

| element | text (verbatim) | words |
|---|---|---|
| Wordmark (logo, top left) | maaloomatiia | 1 |
| Top button | Sound on | 2 |
| Top button | Games menu | 2 |
| Eyebrow | AI ARENA · GAME 02 | 4 |
| Title | Send the Agent | 3 |
| Tagline | You write the list. It shops alone. | 7 |
| LUMA line · name | LUMA | 1 |
| LUMA line · line | I'm LUMA, your coach. Seven errands, one idea each. Press Start and I will tell you the story first. | 19 |
| Status line (teal dot) | Claude connected through the Maaloomatiia Games platform | 7 |
| Errand track · eyebrow | ERRANDS | 1 |
| Errand track · dot numbers | 1 2 3 4 5 6 7 | 7 |
| Main button | Start | 1 |
| Button | Free play | 2 |
| Button | The story | 2 |
| **total** | | **59** |

Controls (12): Sound on (toggle); Games menu; 7 errand dots (1-7, jump to any errand); **Start** (main); Free play; The story.

Notes:

- State: three small star marks under each of the 7 dots (21 marks, all empty); dot 1 haloed as next.
- State: 'Claude connected through the Maaloomatiia Games platform' with a teal status dot.
- Future shown: the 7-errand track with its stars.
- Picture: the 3D robot standing in the market at dusk, right half.
- LUMA (LUMA line): "I'm LUMA, your coach. Seven errands, one idea each. Press Start and I will tell you the story first." — 19 words, complete.

## 1.1-02-story-1-meet-your-agent · Story 1/6: Meet your agent

Screenshot: `1.1-02-story-1-meet-your-agent.jpg` (source frame `aa-runs/5/st-0016.8.jpg`)

| element | text (verbatim) | words |
|---|---|---|
| Wordmark (logo, top left) | maaloomatiia | 1 |
| Eyebrow | THE STORY | 2 |
| Title | Meet your agent. | 3 |
| Scene caption | Walks. Looks. Buys. Does not plan dinner. | 7 |
| LUMA card · name | LUMA | 1 |
| LUMA card · line | This is your household agent. It can walk to the market, look at the stalls, and buy things. It cannot think of dinner on its own. | 26 |
| Button | Skip the story | 3 |
| Button (accent) | Next | 1 |
| **total** | | **44** |

Controls (2): Skip the story; **Next** (main).

Notes:

- State: 6 progress dots under LUMA's card, dot 1 lit (not clickable).
- Picture: flat house icon + bobbing robot icon (no words); the dusk market in 3D on the right.
- Auto-advances 1.8 s after LUMA finishes (no click needed).
- LUMA (LUMA card): "This is your household agent. It can walk to the market, look at the stalls, and buy things. It cannot think of dinner on its own." — 26 words, complete.

## 1.1-03-story-2-the-family-texts-you · Story 2/6: The family texts you

Screenshot: `1.1-03-story-2-the-family-texts-you.jpg` (source frame `aa-runs/5/st-0020.7.jpg`)

| element | text (verbatim) | words |
|---|---|---|
| Wordmark (logo, top left) | maaloomatiia | 1 |
| Eyebrow | THE STORY | 2 |
| Title | The family texts you. | 4 |
| Message card · eyebrow | A MESSAGE FROM DAD | 4 |
| Message card · time | 7:42 pm | 2 |
| Message card · message | Fruit salad tonight. Bring back fruit, no vegetables please. | 9 |
| Scene note | You read it. The agent never does. | 7 |
| LUMA card · name | LUMA | 1 |
| LUMA card · line | Every evening, the family sends a message about dinner. You read it. The agent never does. | 16 |
| Button | Skip the story | 3 |
| Button (accent) | Next | 1 |
| **total** | | **50** |

Controls (2): Skip the story; **Next** (main).

Notes:

- State: progress dots, 2 of 6 lit.
- Picture: house + robot icons, a phone-message card.
- The scene note repeats LUMA's last two sentences word for word.
- LUMA (LUMA card): "Every evening, the family sends a message about dinner. You read it. The agent never does." — 16 words, complete.

## 1.1-04-story-3-you-write-the-list · Story 3/6: You write the list

Screenshot: `1.1-04-story-3-you-write-the-list.jpg` (source frame `aa-runs/5/st-0025.4.jpg`)

| element | text (verbatim) | words |
|---|---|---|
| Wordmark (logo, top left) | maaloomatiia | 1 |
| Eyebrow | THE STORY | 2 |
| Title | You write the list. | 4 |
| List card · eyebrow | YOUR LIST FOR THE AGENT | 5 |
| List card · example list (scripted, typed out by the game) | Apples, bananas, pears, grapes. No tomatoes, no mushrooms, no vegetables at all. If nothing on this list is in view, keep walking. | 22 |
| List card · note | This is its job description. Nothing else goes with it. | 10 |
| LUMA card · name | LUMA | 1 |
| LUMA card · line | You turn the message into a shopping list. That list is the agent's job description. It is the only thing the agent will know. | 24 |
| Button | Skip the story | 3 |
| Button (accent) | Next | 1 |
| **total** | | **73** |

Controls (2): Skip the story; **Next** (main).

Notes:

- State: progress dots, 3 of 6 lit.
- The example list is game text (typed in by the game, not by the player): counted.
- The card note and LUMA's line say the same thing ('job description').
- LUMA (LUMA card): "You turn the message into a shopping list. That list is the agent's job description. It is the only thing the agent will know." — 24 words, complete.

## 1.1-05-story-4-it-shops-alone · Story 4/6: It shops alone

Screenshot: `1.1-05-story-4-it-shops-alone.jpg` (source frame `aa-runs/5/st-0030.6.jpg`)

| element | text (verbatim) | words |
|---|---|---|
| Wordmark (logo, top left) | maaloomatiia | 1 |
| Eyebrow | THE STORY | 2 |
| Title | It shops alone. | 3 |
| Robot speech bubble (scripted, not Claude) | I see an apple, north-east. Apples are on the list. Heading there. | 12 |
| Loop chips (4, lit in turn; 'Text home' lit) | See Decide Act Text home | 5 |
| Scene caption | No phone. At every stall: look, decide, move, text home. Then again. | 12 |
| LUMA card · name | LUMA | 1 |
| LUMA card · line | Once it leaves, there is no phone. At every stall it looks, decides, moves, and texts you one line. Then it looks again. That is the loop. | 27 |
| Button | Skip the story | 3 |
| Button (accent) | Next | 1 |
| **total** | | **67** |

Controls (2): Skip the story; **Next** (main).

Notes:

- State: progress dots, 4 of 6 lit; a 7x4 market grid with stall icons (apple, banana, carrot, tomato) and the robot icon.
- The bubble cycles during the beat: 'Nothing on the list in view. Keep walking.' (8 words) then the line above; the scripted 'Apple is just north. Going north.' and 'Got the apple. Texting home.' are never reached before the beat moves on.
- Three names for the loop on one screen: chips 'See / Decide / Act / Text home', caption 'look, decide, move, text home', LUMA 'looks, decides, moves, and texts' (the round HUD later says LOOK / DECIDE / MOVE).
- LUMA (LUMA card): "Once it leaves, there is no phone. At every stall it looks, decides, moves, and texts you one line. Then it looks again. That is the loop." — 27 words, complete.

## 1.1-06-story-5-some-things-ruin-dinner · Story 5/6: Some things ruin dinner

Screenshot: `1.1-06-story-5-some-things-ruin-dinner.jpg` (source frame `aa-runs/5/st-0035.3.jpg`)

| element | text (verbatim) | words |
|---|---|---|
| Wordmark (logo, top left) | maaloomatiia | 1 |
| Eyebrow | THE STORY | 2 |
| Title | Some things ruin dinner. | 4 |
| Robot speech bubble (scripted, not Claude) | Fruit salad needs fruit. A tomato is a fruit. Buying. | 10 |
| List card · eyebrow | THE LIST SAID | 3 |
| List card · quote | "Get fruit. No vegetables." | 4 |
| Verdict card (red) | Dinner ruined. The agent is sent home. | 7 |
| Scene note | The agent was not wrong. The list was vague. Say exactly what you mean. | 14 |
| LUMA card · name | LUMA | 1 |
| LUMA card · line | A vague list does not come home with a bad basket. It comes home with a tomato in the fruit salad. Say exactly what you mean. | 26 |
| Button | Skip the story | 3 |
| Button (accent) | Next | 1 |
| **total** | | **76** |

Controls (2): Skip the story; **Next** (main).

Notes:

- State: progress dots, 5 of 6 lit; market grid, robot on the eaten tomato stall, pear stall left; a red flash plays before the verdict.
- 'Say exactly what you mean.' appears twice (scene note and LUMA).
- LUMA (LUMA card): "A vague list does not come home with a bad basket. It comes home with a tomato in the fruit salad. Say exactly what you mean." — 26 words, complete.

## 1.1-07-story-6-six-errands · Story 6/6: Six errands (the ladder)

Screenshot: `1.1-07-story-6-six-errands.jpg` (source frame `aa-runs/5/st-0039.7.jpg`)

| element | text (verbatim) | words |
|---|---|---|
| Wordmark (logo, top left) | maaloomatiia | 1 |
| Eyebrow | THE STORY | 2 |
| Title | Six errands. | 2 |
| Ladder card 1 | 1 What does an agent do on its own? You write the list. It shops alone. You cannot step in. | 20 |
| Ladder card 2 | 2 What exactly is its job? A vague list does not come home with a bad basket. It comes home with a tomato in the fruit salad. | 27 |
| Ladder card 3 | 3 What does it see? The agent only knows the stalls in front of it right now. | 17 |
| Ladder card 4 | 4 What does it remember? Every turn starts from nothing, unless you give it a memory. | 16 |
| Ladder card 5 | 5 Which brain does the job? A bigger brain is not always a better buy. It is always pricier. | 19 |
| Ladder card 6 | 6 Does this task need an agent? A fixed rule costs nothing and never surprises you. It cannot decide anything. | 20 |
| Ladder card 7 | 7 Capstone: Who is accountable? Your agent has pocket money. Every choice costs some, every turn. | 16 |
| Note under the cards | Each errand opens one more part of the agent. Free play, with everything open, waits at the end. | 18 |
| LUMA card · name | LUMA | 1 |
| LUMA card · line | Six errands, one idea each. Each one opens one more part of the agent. I will coach you along the way. Let's go. | 23 |
| Button | Skip the story | 3 |
| Button (accent) | Next | 1 |
| **total** | | **186** |

Controls (2): Skip the story; **Next** (main).

Notes:

- State: progress dots, 6 of 6 lit.
- Future shown: the whole ladder, 7 cards with each errand's question and lesson.
- Count mismatch on screen: title and LUMA say 'Six errands', 7 cards are shown; the title screen said 'Seven errands'.
- No card text is cut (ellipsis mode is on, none triggered).
- LUMA (LUMA card): "Six errands, one idea each. Each one opens one more part of the agent. I will coach you along the way. Let's go." — 23 words, complete.

## 1.1-08-briefing · Briefing 1.1

Screenshot: `1.1-08-briefing.jpg` (source frame `aa-runs/5/10-brief.jpg`)

| element | text (verbatim) | words |
|---|---|---|
| Eyebrow | ERRAND 1.1 OF 7 · THE BRIEFING | 6 |
| Title | What does an agent do on its own? | 8 |
| Kicker (teal) | You write the list. It shops alone. You cannot step in. | 11 |
| LUMA strip · name | LUMA | 1 |
| LUMA strip · line | Hi, I'm LUMA, your coach. Today you send your first agent. Write its list, send it, and watch it shop without you. | 22 |
| Message box · eyebrow | A MESSAGE FROM MOM | 4 |
| Message box · message | Just apples today, please. Nothing else. | 6 |
| Parts · eyebrow | YOUR AGENT, PART BY PART · THIS ERRAND OPENS | 8 |
| Part chip (lit teal: opens now) | The list | 2 |
| Part chip (dim: not open) | Sight | 1 |
| Part chip (dim) | Memory | 1 |
| Part chip (dim) | Brain | 1 |
| Part chip (dim) | Effort | 1 |
| Part chip (open, white) | Pocket money | 2 |
| Main button | Go to the garage | 4 |
| Button | The file | 2 |
| Button | Title screen | 2 |
| Ladder strip · eyebrow | THE LADDER | 2 |
| Ladder strip · steps (current 1.1 lit teal) | 1.1 1.2 1.3 1.4 1.5 1.6 1.7 | 7 |
| **total** | | **91** |

Controls (9): **Go to the garage** (main); The file; Title screen; ladder steps 1.2-1.7 (6 jump buttons; 1.1 is current, not a button).

Notes:

- Future shown: 4 of 6 part chips greyed (Sight, Memory, Brain, Effort); the 7-step ladder 1.1-1.7.
- Part chips are not clickable. Bottom-right: the Maaloomatiia emblem (no words).
- Picture: the 3D market square orbiting slowly behind the glass panel.
- LUMA (LUMA strip): "Hi, I'm LUMA, your coach. Today you send your first agent. Write its list, send it, and watch it shop without you." — 22 words, complete.

## 1.1-09-garage-tour-1 · Garage tour 1/4 (the message)

Screenshot: `1.1-09-garage-tour-1.jpg` (source frame `aa-runs/5/ga-0060.4.jpg`)

| element | text (verbatim) | words |
|---|---|---|
| Tour card · name | LUMA | 1 |
| Tour card · step count | 1 / 4 | 2 |
| Tour card · line | Here is the family's message. Read it closely. The agent will never see it. | 14 |
| Tour card · button | Skip | 1 |
| Tour card · button | Next | 1 |
| (under dim) Eyebrow | ERRAND 1.1 · THE GARAGE | 4 |
| (under dim) Title | What does an agent do on its own? | 8 |
| (under dim) Top button | Briefing | 1 |
| (under dim) Top button | Tour | 1 |
| (under dim) Top button | The file | 2 |
| (under dim) Top button | Settings | 1 |
| (under dim) Message card · eyebrow | A MESSAGE FROM MOM | 4 |
| (under dim) Message card · message | Just apples today, please. Nothing else. | 6 |
| (under dim) Message card · the catch | The catch: A simple errand on purpose: this round is about watching the agent work. | 15 |
| (under dim) Robot tag BRAIN · label (closed, dim) | BRAIN · OPENS AT 1.5 | 4 |
| (under dim) Robot tag BRAIN · value (dim) | Haiku 5.5 | 2 |
| (under dim) Robot tag EFFORT · label (closed, dim) — CUT by ellipsis | EFFORT · OPENS AT ... | 3 |
| (under dim) Robot tag EFFORT · value (dim) | Low | 1 |
| (under dim) Robot tag SIGHT · label (closed, dim) | SIGHT · OPENS AT 1.3 | 4 |
| (under dim) Robot tag SIGHT · value (dim) | 2 cells | 2 |
| (under dim) Robot tag MEMORY · label (closed, dim) — CUT by ellipsis | MEMORY · OPENS A... | 3 |
| (under dim) Robot tag MEMORY · value (dim) | Last 3 turns | 3 |
| (under dim) Drawer · eyebrow | YOUR AGENT | 2 |
| (under dim) Drawer · name field (default value) | Agent | 1 |
| (under dim) Drawer · list eyebrow | THE LIST · EVERYTHING IT WILL KNOW | 6 |
| (under dim) Drawer · mode button | Build it from parts | 4 |
| (under dim) Drawer · list field (pre-filled starter list, editable) *(pre-filled list: not in total)* | Buy apples. Nothing else. If no apples are in view, keep walking. | 12 |
| (under dim) Drawer · list token count | about 17 tokens | 3 |
| (under dim) Drawer · pocket money eyebrow | POCKET MONEY · TAP THE COINS | 5 |
| (under dim) Drawer · pocket money total | 30,000 tokens | 2 |
| (under dim) Drawer · coins (10; first 3 lit gold) | 10k 10k 10k 10k 10k 10k 10k 10k 10k 10k | 10 |
| (under dim) Drawer · estimate line | About 1,021 tokens a turn · enough for about 29 of 30 turns | 12 |
| (under dim) Drawer · main button | DEPLOY THE AGENT | 3 |
| (under dim) LUMA strip (bottom left) · name | LUMA | 1 |
| (under dim) LUMA strip (bottom left) · line — still typing, CUT | This list names the one thing to buy, says what to leave, and says what to do when nothing is in view. Watch the loop run: look, decid | 28 |
| **total** | | **160** (+12 pre-filled list) |

Controls (2): Skip; **Next** (main); (the garage controls are behind a dim overlay that blocks clicks).

Notes:

- Appears on its own on the first garage visit of errand 1.1 (fresh profile); auto-advances.
- The whole garage stays legible under a dim overlay; the message card is outlined in teal.
- Garage LUMA strip is still typing in this frame (it completes during tour step 2): counted as visible.
- Same garage state text as 1.1-13 (Haiku 5.5, OPENS AT 1.5 / 1.3, 30,000 tokens, about 1,021 tokens a turn).
- LUMA (Tour card): "Here is the family's message. Read it closely. The agent will never see it." — 14 words, complete.
- LUMA ((under dim) LUMA strip (bottom left)): "This list names the one thing to buy, says what to leave, and says what to do when nothing is in view. Watch the loop run: look, decid" — 28 words, CUT, still typing.

## 1.1-10-garage-tour-2 · Garage tour 2/4 (the list)

Screenshot: `1.1-10-garage-tour-2.jpg` (source frame `aa-runs/5/ga-0063.5.jpg`)

| element | text (verbatim) | words |
|---|---|---|
| Tour card · name | LUMA | 1 |
| Tour card · step count | 2 / 4 | 2 |
| Tour card · line | Here you write the agent's list. Name the things. Say what to leave at the market. | 16 |
| Tour card · button | Skip | 1 |
| Tour card · button | Next | 1 |
| (under dim) Eyebrow | ERRAND 1.1 · THE GARAGE | 4 |
| (under dim) Title | What does an agent do on its own? | 8 |
| (under dim) Top button | Briefing | 1 |
| (under dim) Top button | Tour | 1 |
| (under dim) Top button | The file | 2 |
| (under dim) Top button | Settings | 1 |
| (under dim) Message card · eyebrow | A MESSAGE FROM MOM | 4 |
| (under dim) Message card · message | Just apples today, please. Nothing else. | 6 |
| (under dim) Message card · the catch | The catch: A simple errand on purpose: this round is about watching the agent work. | 15 |
| (under dim) Robot tag BRAIN · label (closed, dim) | BRAIN · OPENS AT 1.5 | 4 |
| (under dim) Robot tag BRAIN · value (dim) | Haiku 5.5 | 2 |
| (under dim) Robot tag EFFORT · label (closed, dim) — CUT by ellipsis | EFFORT · OPENS AT ... | 3 |
| (under dim) Robot tag EFFORT · value (dim) | Low | 1 |
| (under dim) Robot tag SIGHT · label (closed, dim) | SIGHT · OPENS AT 1.3 | 4 |
| (under dim) Robot tag SIGHT · value (dim) | 2 cells | 2 |
| (under dim) Robot tag MEMORY · label (closed, dim) — CUT by ellipsis | MEMORY · OPENS A... | 3 |
| (under dim) Robot tag MEMORY · value (dim) | Last 3 turns | 3 |
| (under dim) Drawer · eyebrow | YOUR AGENT | 2 |
| (under dim) Drawer · name field (default value) | Agent | 1 |
| (under dim) Drawer · list eyebrow | THE LIST · EVERYTHING IT WILL KNOW | 6 |
| (under dim) Drawer · mode button | Build it from parts | 4 |
| (under dim) Drawer · list field (pre-filled starter list, editable) *(pre-filled list: not in total)* | Buy apples. Nothing else. If no apples are in view, keep walking. | 12 |
| (under dim) Drawer · list token count | about 17 tokens | 3 |
| (under dim) Drawer · pocket money eyebrow | POCKET MONEY · TAP THE COINS | 5 |
| (under dim) Drawer · pocket money total | 30,000 tokens | 2 |
| (under dim) LUMA strip (bottom left) · name | LUMA | 1 |
| (under dim) LUMA strip (bottom left) · line | This list names the one thing to buy, says what to leave, and says what to do when nothing is in view. Watch the loop run: look, decide, act, text home. | 31 |
| **total** | | **140** (+12 pre-filled list) |

Controls (2): Skip; **Next** (main); (the garage controls are behind a dim overlay that blocks clicks).

Notes:

- The tour card sits bottom right and hides the coins, the estimate line and DEPLOY THE AGENT (not counted).
- The list area (eyebrow, mode button, field, token count) is outlined in teal.
- LUMA (Tour card): "Here you write the agent's list. Name the things. Say what to leave at the market." — 16 words, complete.
- LUMA ((under dim) LUMA strip (bottom left)): "This list names the one thing to buy, says what to leave, and says what to do when nothing is in view. Watch the loop run: look, decide, act, text home." — 31 words, complete.

## 1.1-11-garage-tour-3 · Garage tour 3/4 (the cost line)

Screenshot: `1.1-11-garage-tour-3.jpg` (source frame `aa-runs/5/ga-0066.9.jpg`)

| element | text (verbatim) | words |
|---|---|---|
| Tour card · name | LUMA | 1 |
| Tour card · step count | 3 / 4 | 2 |
| Tour card · line | Every word costs tokens, the agent's pocket money. This line shows what one turn will cost. | 16 |
| Tour card · button | Skip | 1 |
| Tour card · button | Next | 1 |
| (under dim) Eyebrow | ERRAND 1.1 · THE GARAGE | 4 |
| (under dim) Title | What does an agent do on its own? | 8 |
| (under dim) Top button | Briefing | 1 |
| (under dim) Top button | Tour | 1 |
| (under dim) Top button | The file | 2 |
| (under dim) Top button | Settings | 1 |
| (under dim) Message card · eyebrow | A MESSAGE FROM MOM | 4 |
| (under dim) Message card · message | Just apples today, please. Nothing else. | 6 |
| (under dim) Message card · the catch | The catch: A simple errand on purpose: this round is about watching the agent work. | 15 |
| (under dim) Robot tag BRAIN · label (closed, dim) | BRAIN · OPENS AT 1.5 | 4 |
| (under dim) Robot tag BRAIN · value (dim) | Haiku 5.5 | 2 |
| (under dim) Robot tag EFFORT · label (closed, dim) — CUT by ellipsis | EFFORT · OPENS AT ... | 3 |
| (under dim) Robot tag EFFORT · value (dim) | Low | 1 |
| (under dim) Robot tag SIGHT · label (closed, dim) | SIGHT · OPENS AT 1.3 | 4 |
| (under dim) Robot tag SIGHT · value (dim) | 2 cells | 2 |
| (under dim) Robot tag MEMORY · label (closed, dim) — CUT by ellipsis | MEMORY · OPENS A... | 3 |
| (under dim) Robot tag MEMORY · value (dim) | Last 3 turns | 3 |
| (under dim) Drawer · eyebrow | YOUR AGENT | 2 |
| (under dim) Drawer · name field (default value) | Agent | 1 |
| (under dim) Drawer · list eyebrow | THE LIST · EVERYTHING IT WILL KNOW | 6 |
| (under dim) Drawer · mode button | Build it from parts | 4 |
| (under dim) Drawer · list field (pre-filled starter list, editable) *(pre-filled list: not in total)* | Buy apples. Nothing else. If no apples are in view, keep walking. | 12 |
| (under dim) Drawer · estimate line | About 1,021 tokens a turn · enough for about 29 of 30 turns | 12 |
| (under dim) Drawer · main button | DEPLOY THE AGENT | 3 |
| **total** | | **113** (+12 pre-filled list) |

Controls (2): Skip; **Next** (main); (the garage controls are behind a dim overlay that blocks clicks).

Notes:

- The tour card hides 'about 17 tokens', the POCKET MONEY eyebrow, '30,000 tokens' (fragments 'ns' / 'tok ns' show at its edge) and the coins: not counted.
- The estimate line 'About 1,021 tokens a turn · enough for about 29 of 30 turns' is outlined in teal.
- The garage LUMA strip has faded out by this step.
- LUMA (Tour card): "Every word costs tokens, the agent's pocket money. This line shows what one turn will cost." — 16 words, complete.

## 1.1-12-garage-tour-4 · Garage tour 4/4 (deploy)

Screenshot: `1.1-12-garage-tour-4.jpg` (source frame `aa-runs/5/ga-0070.3.jpg`)

| element | text (verbatim) | words |
|---|---|---|
| Tour card · name | LUMA | 1 |
| Tour card · step count | 4 / 4 | 2 |
| Tour card · line | When every list is written, send the agents. From then on, nobody can help them. | 15 |
| Tour card · button | Skip | 1 |
| Tour card · button | Next | 1 |
| (under dim) Eyebrow | ERRAND 1.1 · THE GARAGE | 4 |
| (under dim) Title | What does an agent do on its own? | 8 |
| (under dim) Top button | Briefing | 1 |
| (under dim) Top button | Tour | 1 |
| (under dim) Top button | The file | 2 |
| (under dim) Top button | Settings | 1 |
| (under dim) Message card · eyebrow | A MESSAGE FROM MOM | 4 |
| (under dim) Message card · message | Just apples today, please. Nothing else. | 6 |
| (under dim) Message card · the catch | The catch: A simple errand on purpose: this round is about watching the agent work. | 15 |
| (under dim) Robot tag BRAIN · label (closed, dim) | BRAIN · OPENS AT 1.5 | 4 |
| (under dim) Robot tag BRAIN · value (dim) | Haiku 5.5 | 2 |
| (under dim) Robot tag EFFORT · label (closed, dim) — CUT by ellipsis | EFFORT · OPENS AT ... | 3 |
| (under dim) Robot tag EFFORT · value (dim) | Low | 1 |
| (under dim) Robot tag SIGHT · label (closed, dim) | SIGHT · OPENS AT 1.3 | 4 |
| (under dim) Robot tag SIGHT · value (dim) | 2 cells | 2 |
| (under dim) Robot tag MEMORY · label (closed, dim) — CUT by ellipsis | MEMORY · OPENS A... | 3 |
| (under dim) Robot tag MEMORY · value (dim) | Last 3 turns | 3 |
| (under dim) Drawer · eyebrow | YOUR AGENT | 2 |
| (under dim) Drawer · name field (default value) | Agent | 1 |
| (under dim) Drawer · list eyebrow | THE LIST · EVERYTHING IT WILL KNOW | 6 |
| (under dim) Drawer · mode button | Build it from parts | 4 |
| (under dim) Drawer · list field (pre-filled starter list, editable) *(pre-filled list: not in total)* | Buy apples. Nothing else. If no apples are in view, keep walking. | 12 |
| (under dim) Drawer · main button | DEPLOY THE AGENT | 3 |
| **total** | | **100** (+12 pre-filled list) |

Controls (2): Skip; **Next** (main); (the garage controls are behind a dim overlay that blocks clicks).

Notes:

- The tour card hides the token count, pocket money, coins and the estimate line (faint through the card): not counted.
- DEPLOY THE AGENT is outlined in teal. LUMA says 'the agents' (plural) though errand 1.1 has one agent.
- LUMA (Tour card): "When every list is written, send the agents. From then on, nobody can help them." — 15 words, complete.

## 1.1-13-garage · Garage 1.1 (tour skipped)

Screenshot: `1.1-13-garage.jpg` (source frame `aa-runs/6/ga-0023.1.jpg`)

| element | text (verbatim) | words |
|---|---|---|
| Eyebrow | ERRAND 1.1 · THE GARAGE | 4 |
| Title | What does an agent do on its own? | 8 |
| Top button | Briefing | 1 |
| Top button | Tour | 1 |
| Top button | The file | 2 |
| Top button | Settings | 1 |
| Message card · eyebrow | A MESSAGE FROM MOM | 4 |
| Message card · message | Just apples today, please. Nothing else. | 6 |
| Message card · the catch | The catch: A simple errand on purpose: this round is about watching the agent work. | 15 |
| Robot tag BRAIN · label (closed, dim) | BRAIN · OPENS AT 1.5 | 4 |
| Robot tag BRAIN · value (dim) | Haiku 5.5 | 2 |
| Robot tag EFFORT · label (closed, dim) — CUT by ellipsis | EFFORT · OPENS AT ... | 3 |
| Robot tag EFFORT · value (dim) | Low | 1 |
| Robot tag SIGHT · label (closed, dim) | SIGHT · OPENS AT 1.3 | 4 |
| Robot tag SIGHT · value (dim) | 2 cells | 2 |
| Robot tag MEMORY · label (closed, dim) — CUT by ellipsis | MEMORY · OPENS A... | 3 |
| Robot tag MEMORY · value (dim) | Last 3 turns | 3 |
| Drawer · eyebrow | YOUR AGENT | 2 |
| Drawer · name field (default value) | Agent | 1 |
| Drawer · list eyebrow | THE LIST · EVERYTHING IT WILL KNOW | 6 |
| Drawer · mode button | Build it from parts | 4 |
| Drawer · list field (pre-filled starter list, editable) *(pre-filled list: not in total)* | Buy apples. Nothing else. If no apples are in view, keep walking. | 12 |
| Drawer · list token count | about 17 tokens | 3 |
| Drawer · pocket money eyebrow | POCKET MONEY · TAP THE COINS | 5 |
| Drawer · pocket money total | 30,000 tokens | 2 |
| Drawer · coins (10; first 3 lit gold) | 10k 10k 10k 10k 10k 10k 10k 10k 10k 10k | 10 |
| Drawer · estimate line | About 1,021 tokens a turn · enough for about 29 of 30 turns | 12 |
| Drawer · main button | DEPLOY THE AGENT | 3 |
| LUMA strip (bottom left) · name | LUMA | 1 |
| LUMA strip (bottom left) · line | This list names the one thing to buy, says what to leave, and says what to do when nothing is in view. Watch the loop run: look, decide, act, text home. | 31 |
| **total** | | **144** (+12 pre-filled list) |

Controls (29): Briefing; Tour; The file; Settings; drawer tab ›; name field (Agent); 10 colour swatches; Build it from parts; list text field; 10 coin buttons (10k each); **DEPLOY THE AGENT** (main).

Notes:

- State as text/numbers: 'Haiku 5.5', 'Low', '2 cells', 'Last 3 turns', 'about 17 tokens', '30,000 tokens', 'About 1,021 tokens a turn · enough for about 29 of 30 turns', coins 3 of 10 lit, a gold budget bar.
- Future shown: all 4 robot tags closed and dimmed with 'OPENS AT 1.5' / 'OPENS AT ...' (CUT) / 'OPENS AT 1.3' / 'OPENS A...' (CUT).
- Picture: the 3D robot in the street with leader-line tags; pink squares on the ground show its sight.
- LUMA's strip fades away ~2.5 s after it finishes typing.
- Captured with the tour skipped (Skip at step 1) so the screen shows on its own.
- LUMA (LUMA strip (bottom left)): "This list names the one thing to buy, says what to leave, and says what to do when nothing is in view. Watch the loop run: look, decide, act, text home." — 31 words, complete.

## 1.1-14-round-countdown · Round: countdown banner (3, 2, 1)

Screenshot: `1.1-14-round-countdown.jpg` (source frame `aa-runs/6/rd-0030.1.jpg`)

| element | text (verbatim) | words |
|---|---|---|
| HUD 1 · errand | ERRAND 1.1 | 2 |
| HUD 1 · turn | Turn 0 of 30 | 4 |
| HUD 1 · status line | Walking. | 1 |
| HUD 2 · eyebrow | THE LOOP · EVERY TURN | 4 |
| HUD 2 · loop chips (lit: none) | LOOK DECIDE MOVE | 3 |
| HUD 3 · HOME | HOME 1 / 1 | 3 |
| HUD 3 · IN THE BASKETS | IN THE BASKETS 0 | 4 |
| HUD 3 · TOKENS LEFT | TOKENS LEFT 30.0k | 3 |
| Moment banner (countdown digit; 3 then 2 then 1) | 2 | 1 |
| Drawer · mission eyebrow | MISSION · MOM · ERRAND 1.1 | 4 |
| Drawer · mission message | Just apples today, please. Nothing else. | 6 |
| Agent card · name | Agent | 1 |
| Agent card · status line | Shopping · Haiku 5.5, sees 2, last 3 turns mem... | 9 |
| Agent card · basket count | 0 | 1 |
| Agent card · button | What it knows | 3 |
| Agent card · tokens | 30,000 tokens left | 3 |
| View tab | Street | 1 |
| View tab | Agent's eye | 2 |
| View tab | The market | 2 |
| Button | Pause | 1 |
| Button | Call them home | 3 |
| Button | Settings | 1 |
| LUMA strip · name | LUMA | 1 |
| LUMA strip · line | Watch your agent. After its first turn I will read out the loop it just ran. | 16 |
| **total** | | **79** |

Controls (9): Street / Agent's eye / The market (3 view tabs); Pause; Call them home; Settings; drawer tab ›; agent card (click to follow); What it knows.

Notes:

- Banner covers the view centre-left for 0.5 s per digit (3, 2, 1). Camera flies down from the rooftops.
- State: 'Turn 0 of 30', 'HOME 1 / 1', 'IN THE BASKETS 0', 'TOKENS LEFT 30.0k', '30,000 tokens left' (same budget twice, two formats), token bar full.
- Agent card status CUT: 'Shopping · Haiku 5.5, sees 2, last 3 turns mem...'.
- LUMA (LUMA strip): "Watch your agent. After its first turn I will read out the loop it just ran." — 16 words, complete.

## 1.1-15-round-agents-out · Round: AGENTS OUT banner

Screenshot: `1.1-15-round-agents-out.jpg` (source frame `aa-runs/6/rd-0032.1.jpg`)

| element | text (verbatim) | words |
|---|---|---|
| HUD 1 · errand | ERRAND 1.1 | 2 |
| HUD 1 · turn | Turn 1 of 30 | 4 |
| HUD 1 · status line | 1 agent is looking at the stalls and deciding | 9 |
| HUD 2 · eyebrow | THE LOOP · EVERY TURN | 4 |
| HUD 2 · loop chips (lit: DECIDE) | LOOK DECIDE MOVE | 3 |
| HUD 3 · HOME | HOME 1 / 1 | 3 |
| HUD 3 · IN THE BASKETS | IN THE BASKETS 0 | 4 |
| HUD 3 · TOKENS LEFT | TOKENS LEFT 30.0k | 3 |
| Moment banner · headline | AGENTS OUT | 2 |
| Moment banner · subline | nobody can help them now | 5 |
| Drawer · mission eyebrow | MISSION · MOM · ERRAND 1.1 | 4 |
| Drawer · mission message | Just apples today, please. Nothing else. | 6 |
| Agent card · name | Agent | 1 |
| Agent card · status line | Shopping · Haiku 5.5, sees 2, last 3 turns mem... | 9 |
| Agent card · basket count | 0 | 1 |
| Agent card · button | What it knows | 3 |
| Agent card · tokens | 30,000 tokens left | 3 |
| View tab | Street | 1 |
| View tab | Agent's eye | 2 |
| View tab | The market | 2 |
| Button | Pause | 1 |
| Button | Call them home | 3 |
| Button | Settings | 1 |
| LUMA strip · name | LUMA | 1 |
| LUMA strip · line | Watch your agent. After its first turn I will read out the loop it just ran. | 16 |
| **total** | | **93** |

Controls (9): Street / Agent's eye / The market (3 view tabs); Pause; Call them home; Settings; drawer tab ›; agent card (click to follow); What it knows.

Notes:

- Banner covers the upper-left view for 0.9 s; the camera is behind the robot.
- The status line ends in animated dots ('deciding', 'deciding.' ...).
- LUMA (LUMA strip): "Watch your agent. After its first turn I will read out the loop it just ran." — 16 words, complete.

## 1.1-16-round-street · Round: Street view (mid-round)

Screenshot: `1.1-16-round-street.jpg` (source frame `aa-runs/6/rd-0043.0.jpg`)

| element | text (verbatim) | words |
|---|---|---|
| HUD 1 · errand | ERRAND 1.1 | 2 |
| HUD 1 · turn | Turn 3 of 30 | 4 |
| HUD 1 · status line | 1 agent is looking at the stalls and deciding | 9 |
| HUD 2 · eyebrow | THE LOOP · EVERY TURN | 4 |
| HUD 2 · loop chips (lit: DECIDE) | LOOK DECIDE MOVE | 3 |
| HUD 3 · HOME | HOME 1 / 1 | 3 |
| HUD 3 · IN THE BASKETS | IN THE BASKETS 1 | 4 |
| HUD 3 · TOKENS LEFT | TOKENS LEFT 27.0k | 3 |
| Drawer · mission eyebrow | MISSION · MOM · ERRAND 1.1 | 4 |
| Drawer · mission message | Just apples today, please. Nothing else. | 6 |
| Agent card · name | Agent | 1 |
| Agent card · status line | Shopping · Haiku 5.5, sees 2, last 3 turns mem... | 9 |
| Agent card · basket count | 1 | 1 |
| Agent card · button | What it knows | 3 |
| Agent card · tokens | 27,007 tokens left | 3 |
| View tab | Street | 1 |
| View tab | Agent's eye | 2 |
| View tab | The market | 2 |
| Button | Pause | 1 |
| Button | Call them home | 3 |
| Button | Settings | 1 |
| LUMA strip · name | LUMA | 1 |
| LUMA strip · line | Watch the text in quotes on each card: that is the agent telling you what it saw and what it decided. | 21 |
| **total** | | **91** |

Excluded (Claude's text, 26 words, not in the total):

- Thought bubble over the robot (Claude) (13): Apple at (6, 0) is one cell east. Moving onto it buys it.
- Agent card quote (Claude) (13): “Apple at (6, 0) is one cell east. Moving onto it buys it.”

Controls (9): Street / Agent's eye / The market (3 view tabs); Pause; Call them home; Settings; drawer tab ›; agent card (click to follow); What it knows.

Notes:

- State: 'Turn 3 of 30', 'HOME 1 / 1', 'IN THE BASKETS 1' + card count '1', 'TOKENS LEFT 27.0k' and '27,007 tokens left' (same number twice, two formats), token bar.
- Coordinates appear in Claude's text ('(6, 0)'), not in game text.
- Pink translucent squares in 3D mark the cells the agent sees.
- LUMA (LUMA strip): "Watch the text in quotes on each card: that is the agent telling you what it saw and what it decided." — 21 words, complete.

## 1.1-17-round-agents-eye · Round: Agent's eye view

Screenshot: `1.1-17-round-agents-eye.jpg` (source frame `aa-runs/7/eye-0043.1.jpg`)

| element | text (verbatim) | words |
|---|---|---|
| HUD 1 · errand | ERRAND 1.1 | 2 |
| HUD 1 · turn | Turn 3 of 30 | 4 |
| HUD 1 · status line | 1 agent is looking at the stalls and deciding | 9 |
| HUD 2 · eyebrow | THE LOOP · EVERY TURN | 4 |
| HUD 2 · loop chips (lit: DECIDE) | LOOK DECIDE MOVE | 3 |
| HUD 3 · HOME | HOME 1 / 1 | 3 |
| HUD 3 · IN THE BASKETS | IN THE BASKETS 0 | 4 |
| HUD 3 · TOKENS LEFT | TOKENS LEFT 26.9k | 3 |
| Camera feed · label (red REC dot) | REC AGENT CAM / AGENT | 4 |
| Camera feed · sight label | SIGHT / 2 CELLS | 3 |
| Drawer · mission eyebrow | MISSION · MOM · ERRAND 1.1 | 4 |
| Drawer · mission message | Just apples today, please. Nothing else. | 6 |
| Agent card · name | Agent | 1 |
| Agent card · status line | Shopping · Haiku 5.5, sees 2, last 3 turns mem... | 9 |
| Agent card · basket count | 0 | 1 |
| Agent card · button | What it knows | 3 |
| Agent card · tokens | 26,870 tokens left | 3 |
| View tab | Street | 1 |
| View tab | Agent's eye | 2 |
| View tab | The market | 2 |
| Button | Pause | 1 |
| Button | Call them home | 3 |
| Button | Settings | 1 |
| LUMA strip · name | LUMA | 1 |
| LUMA strip · line | Watch the text in quotes on each card: that is the agent telling you what it saw and what it decided. | 21 |
| **total** | | **98** |

Excluded (Claude's text, 60 words, not in the total):

- Thought bubble (Claude) (30): Apples at (6,2): I'm at (4,1). Moving east brings me to (5,1), then south-east toward (6,2). Chili at (5,3) is not on my list, so I'll avoid it. East first.
- Agent card quote (Claude) (30): “Apples at (6,2): I'm at (4,1). Moving east brings me to (5,1), then south-east toward (6,2). Chili at (5,3) is not on my list, so I'll avoid it. East first.”

Controls (9): Street / Agent's eye / The market (3 view tabs); Pause; Call them home; Settings; drawer tab ›; agent card (click to follow); What it knows.

Notes:

- First-person camera with viewfinder corners; stalls outside sight carry a floating '?' (not a word).
- From a second run (each round differs: Claude decides).
- LUMA (LUMA strip): "Watch the text in quotes on each card: that is the agent telling you what it saw and what it decided." — 21 words, complete.

## 1.1-18-round-the-market · Round: The market view (from above)

Screenshot: `1.1-18-round-the-market.jpg` (source frame `aa-runs/7/mkt-0054.9.jpg`)

| element | text (verbatim) | words |
|---|---|---|
| HUD 1 · errand | ERRAND 1.1 | 2 |
| HUD 1 · turn | Turn 4 of 30 | 4 |
| HUD 1 · status line | Walking. | 1 |
| HUD 2 · eyebrow | THE LOOP · EVERY TURN | 4 |
| HUD 2 · loop chips (lit: none) | LOOK DECIDE MOVE | 3 |
| HUD 3 · HOME | HOME 1 / 1 | 3 |
| HUD 3 · IN THE BASKETS | IN THE BASKETS 1 | 4 |
| HUD 3 · TOKENS LEFT | TOKENS LEFT 22.7k | 3 |
| Floating name label over the robot | Agent | 1 |
| Drawer · mission eyebrow | MISSION · MOM · ERRAND 1.1 | 4 |
| Drawer · mission message | Just apples today, please. Nothing else. | 6 |
| Agent card · name | Agent | 1 |
| Agent card · status line | Shopping · Haiku 5.5, sees 2, last 3 turns mem... | 9 |
| Agent card · basket count | 1 | 1 |
| Agent card · button | What it knows | 3 |
| Agent card · tokens | 22,726 tokens left | 3 |
| View tab | Street | 1 |
| View tab | Agent's eye | 2 |
| View tab | The market | 2 |
| Button | Pause | 1 |
| Button | Call them home | 3 |
| Button | Settings | 1 |
| LUMA strip · name | LUMA | 1 |
| LUMA strip · line | Agent bought an apple: it saw it, matched it to the list, and moved | 14 |
| **total** | | **77** |

Excluded (Claude's text, 55 words, not in the total):

- Thought bubble (Claude) (35): Nearest apple is at (6,2). Moving east lands on the tomato at (6,1), which isn't on the list. Going south to (5,2) keeps me clear of the chili at (5,3), then east onto the apple.
- Agent card quote (Claude) (20): “I'm at (5,2) and the apple at (6,2) is directly east. Stepping onto it buys an apple from my list.”

Controls (9): Street / Agent's eye / The market (3 view tabs); Pause; Call them home; Settings; drawer tab ›; agent card (click to follow); What it knows.

Notes:

- Top-down view of the whole square; a pink block marks the agent's sight.
- LUMA's line is built on the fly from the event; all 14 words shown, the closing full stop may still be typing.
- LUMA (LUMA strip): "Agent bought an apple: it saw it, matched it to the list, and moved" — 14 words, complete in words (final full stop possibly not yet typed).

## 1.1-19-round-sent-home · Round: SENT HOME banner

Screenshot: `1.1-19-round-sent-home.jpg` (source frame `aa-runs/6/rd-0079.5.jpg`)

| element | text (verbatim) | words |
|---|---|---|
| HUD 1 · errand | ERRAND 1.1 | 2 |
| HUD 1 · turn | Turn 6 of 30 | 4 |
| HUD 1 · status line | Walking. | 1 |
| HUD 2 · eyebrow | THE LOOP · EVERY TURN | 4 |
| HUD 2 · loop chips (lit: none) | LOOK DECIDE MOVE | 3 |
| HUD 3 · HOME | HOME 0 / 1 | 3 |
| HUD 3 · IN THE BASKETS | IN THE BASKETS 2 | 4 |
| HUD 3 · TOKENS LEFT | TOKENS LEFT 18.6k | 3 |
| Moment banner · headline (red) | SENT HOME | 2 |
| Moment banner · subline | Agent bought a lemon: that ruins dinner | 7 |
| Floating name label (drawn over the 'Call them home' button) | Agent | 1 |
| Drawer · mission eyebrow | MISSION · MOM · ERRAND 1.1 | 4 |
| Drawer · mission message | Just apples today, please. Nothing else. | 6 |
| Agent card · name | Agent | 1 |
| Agent card · status line | Sent home on turn 6: bought a lemon | 8 |
| Agent card · basket count | 2 | 1 |
| Agent card · button | What it knows | 3 |
| Agent card · tokens | 18,555 tokens left | 3 |
| View tab | Street | 1 |
| View tab | Agent's eye | 2 |
| View tab | The market | 2 |
| Button | Pause | 1 |
| Button | Call them home | 3 |
| Button | Settings | 1 |
| LUMA strip · name | LUMA | 1 |
| LUMA strip · line — still typing, CUT | Agent is coming home early: it bought a lem | 9 |
| **total** | | **80** |

Excluded (Claude's text, 49 words, not in the total):

- Thought bubble (Claude, partly behind the banner) (19): I'm at (5,1). Apple at (5,2) is one cell south, and it's on the list. Moving south buys it.
- Agent card quote (Claude) (30): “I'm at (5,2). No apples in view: the carrot, lemons, orange and cherry are not on my list. Nothing to buy here, so I'll keep exploring south into unseen cells.”

Controls (9): Street / Agent's eye / The market (3 view tabs); Pause; Call them home; Settings; drawer tab ›; agent card (click to follow); What it knows.

Notes:

- Banner for 1.8 s, red keyline. Happened on its own with the starter list 'Buy apples. Nothing else.' (the agent bought a lemon).
- Agent card turns red with 'Sent home on turn 6: bought a lemon'. HOME drops to 0 / 1.
- LUMA line CUT here (still typing); it completes as 'Agent is coming home early: it bought a lemon, and that ruins dinner.' (13 words).
- The 3D name label 'Agent' renders on top of the bottom button 'Call them home'.
- LUMA (LUMA strip): "Agent is coming home early: it bought a lem" — 9 words, CUT, still typing.

## 1.1-20-round-nobody-came-home · Round: NOBODY CAME HOME banner (round over)

Screenshot: `1.1-20-round-nobody-came-home.jpg` (source frame `aa-runs/6/rd-0084.1.jpg`)

| element | text (verbatim) | words |
|---|---|---|
| HUD 1 · errand | ERRAND 1.1 | 2 |
| HUD 1 · turn | Home after 7 turns | 4 |
| HUD 1 · status line | 0 of 1 agents came home with dinner. | 8 |
| HUD 2 · eyebrow | THE LOOP · EVERY TURN | 4 |
| HUD 2 · loop chips (lit: none) | LOOK DECIDE MOVE | 3 |
| HUD 3 · HOME | HOME 0 / 1 | 3 |
| HUD 3 · IN THE BASKETS | IN THE BASKETS 2 | 4 |
| HUD 3 · TOKENS LEFT | TOKENS LEFT 18.6k | 3 |
| Moment banner · headline (red) | NOBODY CAME HOME | 3 |
| Moment banner · subline | every list had a gap in it | 7 |
| Drawer · mission eyebrow | MISSION · MOM · ERRAND 1.1 | 4 |
| Drawer · mission message | Just apples today, please. Nothing else. | 6 |
| Agent card · name | Agent | 1 |
| Agent card · status line | Sent home on turn 6: bought a lemon | 8 |
| Agent card · basket count | 2 | 1 |
| Agent card · button | What it knows | 3 |
| Agent card · tokens | 18,555 tokens left | 3 |
| View tab | Street | 1 |
| View tab | Agent's eye | 2 |
| View tab | The market | 2 |
| Button | Pause | 1 |
| Button | Call them home | 3 |
| Button | Settings | 1 |
| Drawer · main button | See the debrief | 3 |
| **total** | | **80** |

Excluded (Claude's text, 30 words, not in the total):

- Agent card quote (Claude) (30): “I'm at (5,2). No apples in view: the carrot, lemons, orange and cherry are not on my list. Nothing to buy here, so I'll keep exploring south into unseen cells.”

Controls (10): Street / Agent's eye / The market (3 view tabs); Pause; Call them home; Settings; drawer tab ›; agent card (click to follow); What it knows; **See the debrief** (main).

Notes:

- End banner (2.4 s). 'every list had a gap in it' with a single agent and list.
- LUMA strip is removed when the round ends.
- 'Home after 7 turns' though the agent was sent home on turn 6.
- LUMA: none on screen.

## 1.1-21-round-all-home · Round: ALL HOME banner (after Call them home)

Screenshot: `1.1-21-round-all-home.jpg` (source frame `aa-runs/7/home-0062.5.jpg`)

| element | text (verbatim) | words |
|---|---|---|
| HUD 1 · errand | ERRAND 1.1 | 2 |
| HUD 1 · turn | Home after 5 turns | 4 |
| HUD 1 · status line | 1 of 1 agents came home with dinner. | 8 |
| HUD 2 · eyebrow | THE LOOP · EVERY TURN | 4 |
| HUD 2 · loop chips (lit: none) | LOOK DECIDE MOVE | 3 |
| HUD 3 · HOME | HOME 1 / 1 | 3 |
| HUD 3 · IN THE BASKETS | IN THE BASKETS 2 | 4 |
| HUD 3 · TOKENS LEFT | TOKENS LEFT 20.5k | 3 |
| Moment banner · headline (teal) | ALL HOME | 2 |
| Moment banner · subline | 1 of 1 agents back with dinner | 7 |
| Floating name label | Agent | 1 |
| Drawer · mission eyebrow | MISSION · MOM · ERRAND 1.1 | 4 |
| Drawer · mission message | Just apples today, please. Nothing else. | 6 |
| Agent card · name | Agent | 1 |
| Agent card · status line | Shopping · Haiku 5.5, sees 2, last 3 turns mem... | 9 |
| Agent card · basket count | 2 | 1 |
| Agent card · button | What it knows | 3 |
| Agent card · tokens | 20,459 tokens left | 3 |
| View tab | Street | 1 |
| View tab | Agent's eye | 2 |
| View tab | The market | 2 |
| Button | Pause | 1 |
| Button | Call them home | 3 |
| Button | Settings | 1 |
| Drawer · main button | See the debrief | 3 |
| **total** | | **81** |

Excluded (Claude's text, 45 words, not in the total):

- Thought bubble (Claude, partly behind the banner) (20): I'm at (5,2) and the apple at (6,2) is directly east. Stepping onto it buys an apple from my list.
- Agent card quote (Claude) (25): “I'm at (6,2) with 1 item in the basket. The apple at (7,2) is directly east, so stepping east buys another apple from my list.”

Controls (10): Street / Agent's eye / The market (3 view tabs); Pause; Call them home; Settings; drawer tab ›; agent card (click to follow); What it knows; **See the debrief** (main).

Notes:

- Reached with 'Call them home' (2.4 s banner, fanfare).
- Agent card still says 'Shopping' after the round ended.
- LUMA: none on screen.

## 1.1-22-round-end · Round over (banner gone)

Screenshot: `1.1-22-round-end.jpg` (source frame `aa-runs/7/40-home.jpg`)

| element | text (verbatim) | words |
|---|---|---|
| HUD 1 · errand | ERRAND 1.1 | 2 |
| HUD 1 · turn | Home after 5 turns | 4 |
| HUD 1 · status line | 1 of 1 agents came home with dinner. | 8 |
| HUD 2 · eyebrow | THE LOOP · EVERY TURN | 4 |
| HUD 2 · loop chips (lit: none) | LOOK DECIDE MOVE | 3 |
| HUD 3 · HOME | HOME 1 / 1 | 3 |
| HUD 3 · IN THE BASKETS | IN THE BASKETS 2 | 4 |
| HUD 3 · TOKENS LEFT | TOKENS LEFT 20.5k | 3 |
| Drawer · mission eyebrow | MISSION · MOM · ERRAND 1.1 | 4 |
| Drawer · mission message | Just apples today, please. Nothing else. | 6 |
| Agent card · name | Agent | 1 |
| Agent card · status line | Shopping · Haiku 5.5, sees 2, last 3 turns mem... | 9 |
| Agent card · basket count | 2 | 1 |
| Agent card · button | What it knows | 3 |
| Agent card · tokens | 20,459 tokens left | 3 |
| View tab | Street | 1 |
| View tab | Agent's eye | 2 |
| View tab | The market | 2 |
| Button | Pause | 1 |
| Button | Call them home | 3 |
| Button | Settings | 1 |
| Drawer · main button | See the debrief | 3 |
| **total** | | **71** |

Excluded (Claude's text, 50 words, not in the total):

- Thought bubble (Claude) (25): I'm at (6,2) with 1 item in the basket. The apple at (7,2) is directly east, so stepping east buys another apple from my list.
- Agent card quote (Claude) (25): “I'm at (6,2) with 1 item in the basket. The apple at (7,2) is directly east, so stepping east buys another apple from my list.”

Controls (10): Street / Agent's eye / The market (3 view tabs); Pause; Call them home; Settings; drawer tab ›; agent card (click to follow); What it knows; **See the debrief** (main).

Notes:

- State: 'Home after 5 turns', '1 of 1 agents came home with dinner.', '20.5k' / '20,459 tokens left'.
- The round controls (Pause, Call them home, view tabs) stay on screen after the round is over.
- LUMA: none on screen.

## 1.1-23-debrief · Debrief 1.1

Screenshot: `1.1-23-debrief.jpg` (source frame `aa-runs/7/50-debrief.jpg`)

| element | text (verbatim) | words |
|---|---|---|
| Title | Debrief · Errand 1.1 | 3 |
| Subtitle | Mom: Just apples today, please. Nothing else. | 7 |
| LUMA strip · name | LUMA | 1 |
| LUMA strip · line | An agent decides its own next step. Everything it does traces back to the list you wrote before it left. | 20 |
| Result strip | FIRST ERRAND · 1 OF 1 HOME WITH DINNER | 8 |
| Medal box · eyebrow | THE MEDAL | 2 |
| Medal box · medal | GOLD | 1 |
| Medal box · line (under 3 gold discs) | 3 of 3 stars · 0 in all · Trainee | 8 |
| Unlock card · eyebrow | UNLOCKED FOR ERRAND 1.2 | 4 |
| Unlock card · title | What exactly is its job? | 5 |
| Unlock card · line | 3 of 3 stars banked for this errand · 3 in all · Trainee | 12 |
| Agent row · rank | 1 | 1 |
| Agent row · name | Agent | 1 |
| Agent row · basket count | 2 | 1 |
| Agent row · badge | GOLD | 1 |
| Agent row · result | Came home with 2 wanted items | 6 |
| Agent row · checklist (4 ticks) | Wrote a list Named apples Said what to leave Came home | 11 |
| Agent row · cost column | 9,541 tokens $0.001 Haiku 5.5 | 5 |
| Main button | Next errand: What exactly is its job? | 7 |
| Button | Same errand again | 3 |
| Button | The lists | 2 |
| Button | The file | 2 |
| Button | Title screen | 2 |
| Button | Games menu | 2 |
| **total** | | **115** |

Controls (6): **Next errand: What exactly is its job?** (main); Same errand again; The lists; The file; Title screen; Games menu.

Notes:

- State: 3 gold star discs, 'GOLD' twice (medal box and row badge), '9,541 tokens', '$0.001', 'Haiku 5.5', basket + 2 apple icons.
- Contradiction on screen: medal box '0 in all' vs unlock card '3 in all'.
- Future shown: 'UNLOCKED FOR ERRAND 1.2 · What exactly is its job?' and the Next errand button repeat the same title.
- Picture: blurred top-down market; Maaloomatiia emblem bottom right.
- LUMA (LUMA strip): "An agent decides its own next step. Everything it does traces back to the list you wrote before it left." — 20 words, complete.

## 1.5-01-briefing · Briefing 1.5

Screenshot: `1.5-01-briefing.jpg` (source frame `aa-runs/8/10-brief.jpg`)

| element | text (verbatim) | words |
|---|---|---|
| Eyebrow | ERRAND 1.5 OF 7 · THE BRIEFING | 6 |
| Title | Which brain does the job? | 5 |
| Kicker (teal) | A bigger brain is not always a better buy. It is always pricier. | 13 |
| LUMA strip · name | LUMA | 1 |
| LUMA strip · line | Now you choose the agent's brain and how hard it thinks. Watch the cost, not just the basket. | 18 |
| Message box · eyebrow | A MESSAGE FROM MOM | 4 |
| Message box · message | Grandma is coming. Anything is fine except corn, orange and broccoli. She is allergic. | 14 |
| Parts · eyebrow | YOUR AGENT, PART BY PART · THIS ERRAND OPENS | 8 |
| Part chip (open, white) | The list | 2 |
| Part chip (open, white) | Sight | 1 |
| Part chip (open, white) | Memory | 1 |
| Part chip (lit teal: opens now) | Brain | 1 |
| Part chip (lit teal: opens now) | Effort | 1 |
| Part chip (open, white) | Pocket money | 2 |
| Main button | Go to the garage | 4 |
| Button | The file | 2 |
| Button | Title screen | 2 |
| Ladder strip · eyebrow | THE LADDER | 2 |
| Ladder strip · steps (current 1.5 lit teal) | 1.1 1.2 1.3 1.4 1.5 1.6 1.7 | 7 |
| **total** | | **94** |

Controls (9): **Go to the garage** (main); The file; Title screen; ladder steps 1.1-1.4, 1.6, 1.7 (6 jump buttons).

Notes:

- Reached straight from the title's errand dot 5 (no story).
- State: Brain and Effort lit as this errand's new parts; the others open (white); none greyed.
- Future shown: the ladder 1.1-1.7 (1.6, 1.7 ahead).
- LUMA (LUMA strip): "Now you choose the agent's brain and how hard it thinks. Watch the cost, not just the basket." — 18 words, complete.

## 1.5-02-garage · Garage 1.5

Screenshot: `1.5-02-garage.jpg` (source frame `aa-runs/8/ga-0028.0.jpg`)

| element | text (verbatim) | words |
|---|---|---|
| Eyebrow | ERRAND 1.5 · THE GARAGE | 4 |
| Title | Which brain does the job? | 5 |
| Top button | Briefing | 1 |
| Top button | Tour | 1 |
| Top button | The file | 2 |
| Top button | Settings | 1 |
| Message card · eyebrow | A MESSAGE FROM MOM | 4 |
| Message card · message — CUT by ellipsis | Grandma is coming. Anything is fine except corn, orange and broccoli. S... | 12 |
| Message card · the catch | The catch: Three exceptions. Miss one on the list and the agent will bring it home. | 16 |
| Robot tag BRAIN (open) · label | BRAIN | 1 |
| Robot tag BRAIN · value | Opus 5.5 | 2 |
| Robot tag EFFORT (open) · label | EFFORT | 1 |
| Robot tag EFFORT · value | High | 1 |
| Robot tag SIGHT (open) · label | SIGHT | 1 |
| Robot tag SIGHT · value | 2 cells | 2 |
| Robot tag MEMORY (open) · label | MEMORY | 1 |
| Robot tag MEMORY · value | Last 3 turns | 3 |
| Drawer · eyebrow | YOUR AGENT | 2 |
| Drawer · name field (default value) | Agent | 1 |
| Drawer · list eyebrow | THE LIST · EVERYTHING IT WILL KNOW | 6 |
| Drawer · mode button | Build it from parts | 4 |
| Drawer · list field (pre-filled starter list, editable) *(pre-filled list: not in total)* | Anything at the market is fine except oranges, broccoli and corn. Never buy those: Grandma is allergic. If nothing else is in view, keep walking. | 25 |
| Drawer · list token count | about 37 tokens | 3 |
| Drawer · pocket money eyebrow | POCKET MONEY · TAP THE COINS | 5 |
| Drawer · pocket money total | 30,000 tokens | 2 |
| Drawer · coins (10; first 3 lit gold) | 10k 10k 10k 10k 10k 10k 10k 10k 10k 10k | 10 |
| Drawer · estimate line | About 1,281 tokens a turn · enough for about 23 of 30 turns | 12 |
| Drawer · main button | DEPLOY THE AGENT | 3 |
| LUMA strip (bottom left) · name | LUMA | 1 |
| LUMA strip (bottom left) · line | Fix it: The biggest brain on an easy job. Opus at high effort does this errand too, at many times the price per item. Send it, then try a smaller brain and compare the cost per item on the debrief. | 40 |
| **total** | | **147** (+25 pre-filled list) |

Controls (33): Briefing; Tour; The file; Settings; 4 robot tags (BRAIN, EFFORT, SIGHT, MEMORY: click to cycle the value); drawer tab ›; name field (Agent); 10 colour swatches; Build it from parts; list text field; 10 coin buttons (10k each); **DEPLOY THE AGENT** (main).

Notes:

- State as text/numbers: 'Opus 5.5', 'High', '2 cells', 'Last 3 turns', 'about 37 tokens', '30,000 tokens', 'About 1,281 tokens a turn · enough for about 23 of 30 turns'.
- The errand starts deliberately expensive (Opus 5.5, High) and LUMA's line opens with 'Fix it:'.
- The family message is CUT in the card ('... broccoli. S...'); the full message is on the briefing.
- No tour on errand 1.5. LUMA's strip fades ~2.5 s after typing.
- LUMA (LUMA strip (bottom left)): "Fix it: The biggest brain on an easy job. Opus at high effort does this errand too, at many times the price per item. Send it, then try a smaller brain and compare the cost per item on the debrief." — 40 words, complete.

## 1.5-03-round-street · Round 1.5: Street view

Screenshot: `1.5-03-round-street.jpg` (source frame `aa-runs/8/rd-0041.3.jpg`)

| element | text (verbatim) | words |
|---|---|---|
| HUD 1 · errand | ERRAND 1.5 | 2 |
| HUD 1 · turn | Turn 1 of 30 | 4 |
| HUD 1 · status line | 1 agent is looking at the stalls and deciding. | 9 |
| HUD 2 · eyebrow | THE LOOP · EVERY TURN | 4 |
| HUD 2 · loop chips (lit: DECIDE) | LOOK DECIDE MOVE | 3 |
| HUD 3 · HOME | HOME 1 / 1 | 3 |
| HUD 3 · IN THE BASKETS | IN THE BASKETS 0 | 4 |
| HUD 3 · TOKENS LEFT | TOKENS LEFT 30.0k | 3 |
| Drawer · mission eyebrow | MISSION · MOM · ERRAND 1.5 | 4 |
| Drawer · mission message | Grandma is coming. Anything is fine except corn, orange and broccoli. She is allergic. | 14 |
| Agent card · name | Agent | 1 |
| Agent card · status line | Shopping · Opus 5.5, sees 2, last 3 turns memory | 9 |
| Agent card · basket count | 0 | 1 |
| Agent card · button | What it knows | 3 |
| Agent card · tokens | 30,000 tokens left | 3 |
| View tab | Street | 1 |
| View tab | Agent's eye | 2 |
| View tab | The market | 2 |
| Button | Pause | 1 |
| Button | Call them home | 3 |
| Button | Settings | 1 |
| LUMA strip · name | LUMA | 1 |
| LUMA strip · line | Every card shows the brain it runs on. At the debrief, compare what each item in the basket cost, not just how many. | 23 |
| **total** | | **101** |

Controls (9): Street / Agent's eye / The market (3 view tabs); Pause; Call them home; Settings; drawer tab ›; agent card (click to follow); What it knows.

Notes:

- Same HUD as 1.1; the countdown and AGENTS OUT banners play as in 1.1 (not repeated here).
- Agent card status shows the brain: 'Shopping · Opus 5.5, sees 2, last 3 turns memory' (not cut here).
- No Claude text on screen in this frame (first turn still deciding).
- LUMA (LUMA strip): "Every card shows the brain it runs on. At the debrief, compare what each item in the basket cost, not just how many." — 23 words, complete.

## 1.5-04-round-end · Round 1.5 over (after Call them home)

Screenshot: `1.5-04-round-end.jpg` (source frame `aa-runs/8/40-home.jpg`)

| element | text (verbatim) | words |
|---|---|---|
| HUD 1 · errand | ERRAND 1.5 | 2 |
| HUD 1 · turn | Home after 10 turns | 4 |
| HUD 1 · status line | 1 of 1 agents came home with dinner. | 8 |
| HUD 2 · eyebrow | THE LOOP · EVERY TURN | 4 |
| HUD 2 · loop chips (lit: none) | LOOK DECIDE MOVE | 3 |
| HUD 3 · HOME | HOME 1 / 1 | 3 |
| HUD 3 · IN THE BASKETS | IN THE BASKETS 5 | 4 |
| HUD 3 · TOKENS LEFT | TOKENS LEFT 13.0k | 3 |
| Drawer · mission eyebrow | MISSION · MOM · ERRAND 1.5 | 4 |
| Drawer · mission message | Grandma is coming. Anything is fine except corn, orange and broccoli. She is allergic. | 14 |
| Agent card · name | Agent | 1 |
| Agent card · status line | Shopping · Opus 5.5, sees 2, last 3 turns memory | 9 |
| Agent card · basket count | 5 | 1 |
| Agent card · button | What it knows | 3 |
| Agent card · tokens | 12,955 tokens left | 3 |
| View tab | Street | 1 |
| View tab | Agent's eye | 2 |
| View tab | The market | 2 |
| Button | Pause | 1 |
| Button | Call them home | 3 |
| Button | Settings | 1 |
| Drawer · main button | See the debrief | 3 |
| **total** | | **79** |

Excluded (Claude's text, 40 words, not in the total):

- Thought bubble (Claude) (20): Broccoli is directly south at (7,6), so I'm going east to (8,5) first, then south to the grapes at (8,6).
- Agent card quote (Claude) (20): “Broccoli is directly south at (7,6), so I'm going east to (8,5) first, then south to the grapes at (8,6).”

Controls (10): Street / Agent's eye / The market (3 view tabs); Pause; Call them home; Settings; drawer tab ›; agent card (click to follow); What it knows; **See the debrief** (main).

Notes:

- The ALL HOME banner ('ALL HOME' / '1 of 1 agents back with dinner') played first, as in 1.1-21.
- State: 'Home after 10 turns', 5 in the basket, '13.0k' / '12,955 tokens left'; card still says 'Shopping'.
- LUMA: none on screen.

## 1.5-05-debrief · Debrief 1.5

Screenshot: `1.5-05-debrief.jpg` (source frame `aa-runs/8/50-debrief.jpg`)

| element | text (verbatim) | words |
|---|---|---|
| Title | Debrief · Errand 1.5 | 3 |
| Subtitle | Mom: Grandma is coming. Anything is fine except corn, orange and broccoli. She is allergic. | 15 |
| LUMA strip · name | LUMA | 1 |
| LUMA strip · line | Pick the smallest model and the least effort that do the job. The cost is per turn, and the turns add up. | 22 |
| Result strip | GRANDMA'S ALLERGIES · 1 OF 1 HOME WITH DINNER | 8 |
| Medal box · eyebrow | THE MEDAL | 2 |
| Medal box · medal | GOLD | 1 |
| Medal box · line (under 3 gold discs) | 3 of 3 stars · 0 in all · Trainee | 8 |
| Unlock card · eyebrow | UNLOCKED FOR ERRAND 1.6 | 4 |
| Unlock card · title | Agent, rule, or neither | 4 |
| Unlock card · line | 3 of 3 stars banked for this errand · 3 in all · Trainee | 12 |
| Agent row · rank | 1 | 1 |
| Agent row · name | Agent | 1 |
| Agent row · basket count | 5 | 1 |
| Agent row · badge | GOLD | 1 |
| Agent row · result | Came home with 5 wanted items | 6 |
| Agent row · checklist (4 ticks) | Came home Bought a listed item Lasted the budget Cheapest item: $0.0160 | 12 |
| Agent row · cost column | 17,045 tokens $0.080 Opus 5.5 | 5 |
| Main button | Next errand: Does this task need an agent? | 8 |
| Button | Same errand again | 3 |
| Button | The lists | 2 |
| Button | The file | 2 |
| Button | Title screen | 2 |
| Button | Games menu | 2 |
| **total** | | **126** |

Controls (6): **Next errand: Does this task need an agent?** (main); Same errand again; The lists; The file; Title screen; Games menu.

Notes:

- State: '17,045 tokens', '$0.080', 'Cheapest item: $0.0160', 'Opus 5.5', basket + 5 item icons (carrot, chili, carrot, cherry, grapes).
- Name mismatch: unlock card calls errand 1.6 'Agent, rule, or neither' while the Next button says 'Does this task need an agent?'.
- Same '0 in all' vs '3 in all' contradiction as 1.1.
- LUMA (LUMA strip): "Pick the smallest model and the least effort that do the job. The cost is per turn, and the turns add up." — 22 words, complete.

## 1.6-01-briefing · Briefing 1.6

Screenshot: `1.6-01-briefing.jpg` (source frame `aa-runs/9/10-brief.jpg`)

| element | text (verbatim) | words |
|---|---|---|
| Eyebrow | ERRAND 1.6 OF 7 · THE BRIEFING | 6 |
| Title | Does this task need an agent? | 6 |
| Kicker (teal, 2 lines) | A fixed rule costs nothing and never surprises you. It cannot decide anything. | 13 |
| LUMA strip · name | LUMA | 1 |
| LUMA strip · line | Now you decide before writing any list: does this task need an agent at all? A fixed rule costs nothing. An agent costs tokens on every turn. | 27 |
| Message box · eyebrow | A MESSAGE FROM MOM | 4 |
| Message box · message | Just apples today, please. Nothing else. | 6 |
| Parts · eyebrow | YOUR AGENT, PART BY PART · THIS ERRAND OPENS | 8 |
| Part chip (lit teal) | Agent, or a fixed rule | 5 |
| Part chip (dim) | Sight | 1 |
| Part chip (dim) | Memory | 1 |
| Part chip (dim) | Brain | 1 |
| Part chip (dim) | Effort | 1 |
| Part chip (open, white) | Pocket money | 2 |
| Main button | Decide the tasks | 3 |
| Button | The file | 2 |
| Button | Title screen | 2 |
| Ladder strip · eyebrow | THE LADDER | 2 |
| Ladder strip · steps (current 1.6 lit teal) | 1.1 1.2 1.3 1.4 1.5 1.6 1.7 | 7 |
| **total** | | **98** |

Controls (9): **Decide the tasks** (main); The file; Title screen; ladder steps 1.1-1.5, 1.7 (6 jump buttons).

Notes:

- Sight, Memory, Brain and Effort are greyed again on 1.6, though 1.3-1.5 opened them.
- The message box shows one message ('Just apples today...'), but the decide screen has five tasks.
- Three names for the choice: chip 'Agent, or a fixed rule'; decide buttons 'A simple automation'; 1.5 debrief 'Agent, rule, or neither'.
- LUMA (LUMA strip): "Now you decide before writing any list: does this task need an agent at all? A fixed rule costs nothing. An agent costs tokens on every turn." — 27 words, complete.

## 1.6-02-decide · Decide screen 1.6

Screenshot: `1.6-02-decide.jpg` (source frame `aa-runs/9/20-decide.jpg`)

| element | text (verbatim) | words |
|---|---|---|
| Drawer · eyebrow | ERRAND 1.6 · DECIDE, TASK BY TASK | 6 |
| Drawer · title | Does this task need an agent? | 6 |
| Button | Briefing | 1 |
| Button | The file | 2 |
| Button | Title screen | 2 |
| Task 1 · eyebrow | 1 · A MESSAGE FROM MOM | 5 |
| Task 1 · message | Just apples today, please. Nothing else. | 6 |
| Task 1 · hint | One named thing, no exceptions. | 5 |
| Choice buttons | An agent | 2 |
| Choice buttons | A simple automation | 3 |
| Choice buttons | Neither | 1 |
| Task 2 · eyebrow | 2 · A MESSAGE FROM DAD | 5 |
| Task 2 · message | We need exactly lemons, corn, pears and potatoes. Nothing else. | 10 |
| Task 2 · hint | A fixed list of names. | 5 |
| Choice buttons | An agent | 2 |
| Choice buttons | A simple automation | 3 |
| Choice buttons | Neither | 1 |
| Task 3 · eyebrow | 3 · A MESSAGE FROM DAD | 5 |
| Task 3 · message | Fruit salad tonight: fruit only, no vegetables. And in this house a tomato is a vegetable. | 16 |
| Task 3 · hint | A category, with a catch. | 5 |
| Choice buttons | An agent | 2 |
| Choice buttons | A simple automation | 3 |
| Choice buttons | Neither | 1 |
| Task 4 · eyebrow | 4 · A MESSAGE FROM BIG SISTER | 6 |
| Task 4 · message | A colorful fruit bowl for the guests: at least three different fruits, no vegetables. And no strawberries, one guest is allergic. | 21 |
| Task 4 · hint | Different kinds, and an allergy to work around. | 8 |
| Choice buttons | An agent | 2 |
| Choice buttons | A simple automation | 3 |
| Choice buttons | Neither | 1 |
| Task 5 · eyebrow | 5 · A MESSAGE FROM MOM | 5 |
| Task 5 · message | Should we have the party at home or at the restaurant on Saturday? Decide for us. | 16 |
| Task 5 · hint | Not a shopping task. | 4 |
| Choice buttons | An agent | 2 |
| Choice buttons | A simple automation | 3 |
| Choice buttons | Neither | 1 |
| Main button | RUN THE SHOPPING TASKS | 4 |
| LUMA strip (bottom left) · name | LUMA | 1 |
| LUMA strip (bottom left) · line | Mark each task: an agent, a simple automation, or neither. | 10 |
| **total** | | **184** |

Controls (19): Briefing; The file; Title screen; 15 choice buttons (An agent / A simple automation / Neither, x5 tasks); **RUN THE SHOPPING TASKS** (main).

Notes:

- A form: 5 task cards, each with message, hint and three choice buttons; nothing chosen yet.
- Each task's hint ('One named thing, no exceptions.', 'A fixed list of names.' ...) points at the answer.
- Picture: the 3D market from above on the left 2/3; no agent on screen.
- LUMA (LUMA strip (bottom left)): "Mark each task: an agent, a simple automation, or neither." — 10 words, complete.

## Summary

| screen | words | controls (main action) | notes |
|---|---|---|---|
| 1.1-01-title | 59 | 12 (Start) | 3D robot right; 21 empty star marks; 'Claude connected' status |
| 1.1-02-story-1-meet-your-agent | 44 | 2 (Next) | auto-advancing beat; 6 progress dots |
| 1.1-03-story-2-the-family-texts-you | 50 | 2 (Next) | note repeats LUMA |
| 1.1-04-story-3-you-write-the-list | 73 | 2 (Next) | example list typed by the game |
| 1.1-05-story-4-it-shops-alone | 67 | 2 (Next) | loop named 3 ways; scripted bubble |
| 1.1-06-story-5-some-things-ruin-dinner | 76 | 2 (Next) | red verdict card; line said twice |
| 1.1-07-story-6-six-errands | 186 | 2 (Next) | ladder of 7 cards; says 'Six errands' |
| 1.1-08-briefing | 91 | 9 (Go to the garage) | 4 part chips greyed; ladder 1.1-1.7 |
| 1.1-09-garage-tour-1 | 160 (+12 list) | 2 (Next) | tour overlay on dimmed garage; strip CUT (typing) |
| 1.1-10-garage-tour-2 | 140 (+12 list) | 2 (Next) | card hides coins/estimate/deploy |
| 1.1-11-garage-tour-3 | 113 (+12 list) | 2 (Next) | card hides pocket money and coins |
| 1.1-12-garage-tour-4 | 100 (+12 list) | 2 (Next) | card hides cost lines |
| 1.1-13-garage | 144 (+12 list) | 29 (DEPLOY THE AGENT) | 4 closed tags 'OPENS AT' (2 CUT); 30,000 tokens; ~1,021/turn |
| 1.1-14-round-countdown | 79 | 9 (none, the player watches) | banner '2' (3,2,1); 30.0k + 30,000 tokens left |
| 1.1-15-round-agents-out | 93 | 9 (none, the player watches) | banner 0.9 s |
| 1.1-16-round-street | 91 [Claude 26 excl.] | 9 (none, the player watches) | 27.0k + 27,007 tokens left; card status CUT |
| 1.1-17-round-agents-eye | 98 [Claude 60 excl.] | 9 (none, the player watches) | REC AGENT CAM; SIGHT / 2 CELLS; '?' marks |
| 1.1-18-round-the-market | 77 [Claude 55 excl.] | 9 (none, the player watches) | top-down; LUMA event line |
| 1.1-19-round-sent-home | 80 [Claude 49 excl.] | 9 (none, the player watches) | red banner; LUMA CUT (typing); label over button |
| 1.1-20-round-nobody-came-home | 80 [Claude 30 excl.] | 10 (See the debrief) | end banner; 'every list had a gap' |
| 1.1-21-round-all-home | 81 [Claude 45 excl.] | 10 (See the debrief) | end banner after Call them home |
| 1.1-22-round-end | 71 [Claude 50 excl.] | 10 (See the debrief) | round controls stay after the end |
| 1.1-23-debrief | 115 | 6 (Next errand: What exactly is its job?) | GOLD; '0 in all' vs '3 in all'; $0.001 |
| 1.5-01-briefing | 94 | 9 (Go to the garage) | Brain + Effort lit |
| 1.5-02-garage | 147 (+25 list) | 33 (DEPLOY THE AGENT) | 4 open tags; Opus 5.5 High; message CUT; ~1,281/turn |
| 1.5-03-round-street | 101 | 9 (none, the player watches) | card shows Opus 5.5 |
| 1.5-04-round-end | 79 [Claude 40 excl.] | 10 (See the debrief) | 13.0k + 12,955 tokens left |
| 1.5-05-debrief | 126 | 6 (Next errand: Does this task need an agent?) | $0.080; cheapest item $0.0160; 1.6 named differently |
| 1.6-01-briefing | 98 | 9 (Decide the tasks) | parts greyed again; chip 'Agent, or a fixed rule' |
| 1.6-02-decide | 184 | 19 (RUN THE SHOPPING TASKS) | form of 5 tasks x 3 choices |

Word totals of a round change every turn (turn number, tokens, LUMA's event lines); the counts are for the frame shown.

Sums (game words, excluding pre-filled lists and Claude; all captured screens, so 1.1 includes both round endings and every banner): errand 1.1 2168 over 23 screens; errand 1.5 547 over 5; errand 1.6 282 over 2.
