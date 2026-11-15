# Gridding Factors - Roadmap

This roadmap is the working plan for turning the jam vertical slice into a finished small game. It is intentionally practical: art first, then feel, then content, then polish. Dates are deliberately not fixed - the project is made by a small team, and art time is the main constraint.

## North star

A short, replayable bingo deckbuilder where the board is the build, the cards are the payoff, and every screen looks like it came from the same hand-drawn notebook.

Guiding rules:

1. **The Bingo board must matter.** If a number can be placed anywhere with no decision, the design is not finished.
2. **Readable first.** A new player should understand a cell, a buff, and a card in one glance.
3. **One visual language.** Owner-drawn ink on paper. No mixed placeholder styles in a final build.
4. **Small scope, finished.** Five encounters done well beats twenty unfinished ones.
5. **AI is a tool, not the art direction.** Generated assets are temporary and will be replaced.

## Current baseline

- Unity 6000.3.27f2, URP 2D, WebGL target, 1920x1080 reference.
- Five encounters, complete Bingo - Combat - Shop loop, win and defeat.
- One enemy template with scaled HP/damage and placeholder names.
- One Bingo board ruleset; five card types; four-card hand cap; Focus cap 6.
- Owner-drawn cards and frames; placeholder AI characters/enemies and some UI art.
- Static characters; static Bingo machine; limited UI feedback animation.
- Temporary sound mix.

## Milestone 0 - Stabilise the slice (now)

**Owner:** Long, with Ang on visual review.

- Fix runtime errors and null references.
- Remove placeholder text, duplicate labels, and unused scene objects.
- Keep the 1920x1080 and 1280x720 layouts free of clipping and overlap.
- Ensure the WebGL build starts, plays a full run, and can be restarted.
- Keep README, credits, and AI disclosure current.
- Freeze the current ruleset for playtesting.

**Done when:** a clean five-encounter run can be played start to finish without console errors.

## Milestone 1 - Replace placeholders and animate the game

This is the largest and most important milestone. It covers every unanimated or AI-placeholder element.

### 1A. Final art replacement

**Owner:** Ang Weiyi. **Support:** Long for import/specs.

Replace, in priority order:

1. Player knight: final front-facing/right-facing design, full body, animation-ready.
2. Five enemy designs + final boss: distinct silhouettes, not recolours.
3. Card artwork for every card and upgrade state.
4. Buff icon set: attack, defend, heal, focus, piercing, wildcard, shield, draw.
5. Bingo frame, cell stamps, line-clear marks, ball/machine illustration.
6. Shop interior, shopkeeper/props, menu and victory/defeat illustrations.
7. UI frames, buttons, badges, cursors, and dividers.

Rules:

- Charcoal ink on ivory, with the established coral/teal/ochre/sage/lavender palette.
- Transparent PNGs, no baked-in text.
- Consistent optical weight at 32 px for icons and at 1024 px for characters.
- Keep source files and export presets so future changes do not require redrawing from scratch.

**Done when:** no AI-generated placeholder art remains in a release build. Every asset is either owner-drawn or a documented derivative of owner art.

### 1B. Animation pass

**Owner:** Ang (poses/keyframes), Long (integration).

Characters:

- Idle breathing loop for player and each enemy.
- Attack wind-up, strike, recover.
- Hit reaction and guard reaction.
- Defeat/dissolve.
- Boss entrance and phase change.

Bingo:

- Machine shake and ball roll on draw.
- Ball drop and number reveal. (Respect the current decision to keep the draw silent unless changed.)
- Cell stamp/pop on placement.
- Line flash and Ultimate badge pop.
- Board shake on a completed line.

Cards and UI:

- Card draw, hover lift, play, discard.
- Damage/heal/shield/focus number pops.
- Shop sign sway, offer card flip, purchase stamp.
- Button press/release, panel open/close, screen transitions.

**Done when:** every major action has visible feedback, and a reduced-motion setting can disable non-essential movement.

## Milestone 2 - Sound and music

**Owner:** Long, with Ang on feel and references.

### Music

- One menu loop, one Bingo loop, one combat loop, one boss loop, one shop loop.
- Short win/lose stingers.
- Seamless loops; no clicks at the loop point.
- Keep the current CC-BY track only if the final mix still fits; otherwise replace it and update credits.

### SFX

- Draw/ball, cell place, invalid click, line complete, Ultimate ready.
- Card draw, card play, attack, shield, heal, focus, piercing.
- Enemy hit, player hit, defeat, victory.
- Shop open, buy, reject, selector open/close.
- UI click, back, confirm.

### Mix and settings

- Master, music, and SFX volume sliders; mute.
- Loudness-normalised clips; no clipping; consistent UI level.
- Audio continues correctly through phase changes; no overlapping loops.
- Credits updated for every track.

**Done when:** a full run can be played with the final mix, and every action has deliberate audio feedback.

## Milestone 3 - Enemy types and encounter identity

**Owner:** Ang (design/creature ideas), Long (AI/behaviour), both (playtest).

The current game uses one enemy template with scaled stats. Replace it with a small set of readable archetypes. Each should change how the player values Bingo cells and cards, not just how much HP it has.

### Proposed first set

| Enemy | Hook | Why it matters |
|---|---|---|
| Lucky Slime | Basic, low HP, telegraphed attack | Teaches the loop |
| Shield Guard | Gains Shield; punishes unboosted attacks | Makes Piercing and Focus valuable |
| Number Goblin | Locks a cell or rerolls the next ball | Attacks the Bingo plan, not just HP |
| Mirror Knight | Copies a buff or reflects a portion of damage | Makes buff timing matter |
| The House (boss) | Two phases; changes a board rule between phases | Final exam for the build |

### Work items

- Add enemy intents/telegraphs: "Attack 5", "Shield 3", "Lock a cell".
- Add enemy status icons and a compact intent display.
- Give each encounter a unique Bingo constraint or starting board modification.
- Add encounter intro cards with name, portrait, and one-line rule.
- Add boss phases and a phase-change animation.
- Keep enemy names and numbers data-driven, not hardcoded.

**Done when:** encounters 1-5 feel different in decisions, not only in HP/damage numbers.

## Milestone 4 - Balance and economy

**Owner:** Long (numbers), Ang (feel), both (playtests).

### Balance targets

- A first run should be winnable but not guaranteed; target roughly a **40-60% win rate** for new players after the tutorial.
- A run should last about **15-25 minutes** once animation is in.
- No single card, buff, or Bingo line should dominate every run.
- The shop should present a real choice, not an obvious best buy.
- RNG should create stories, not dead turns.

### Work items

- Build a playtest spreadsheet for HP, damage, coins, card values, buff values, and win rate.
- Review the current curve: 30 player HP, 12/16/20/24/36 enemy HP, 2/2/3/4/5 attack, 10 starting coins, 20-60 coin rewards.
- Review shop prices: card 10/20, condition 35, buff 20.
- Review four-card hand cap, Focus cap 6, and the U2 extra-draw cap.
- Add bad-luck protection: reroll rules, no dead draw, or a small pity system.
- Add difficulty options after the base run is stable.
- Playtest with fresh players and record where they get confused or stop.

**Done when:** three internal playtests produce comparable win rates and no obvious dominant strategy.

## Milestone 5 - UX, accessibility, and content

**Owner:** Long, with Ang on visual hierarchy.

### UX

- Tooltips/glossary for cell rules, buffs, Focus, Shield, Piercing, and Ultimates.
- A clear "why can I not place this?" message.
- Card and buff previews before purchase.
- Run summary with encounters cleared, lines completed, and final deck.
- Pause menu and restart confirmation.
- Save/continue for a run.

### Accessibility

- Colour-blind-safe rule cues: shape/pattern plus colour.
- Reduced motion toggle.
- Text scale and high-contrast option.
- Master/music/SFX volume and mute.
- Keyboard/controller navigation; visible focus states.
- Test at 1280x720, 1920x1080, and ultrawide.

### Content

- More Bingo conditions and buff cells.
- More cards and upgrade paths.
- Daily seed and run history.
- Achievements/unlocks only if they do not bloat scope.

**Done when:** a new player can learn the game without external explanation, and the UI is usable without colour alone.

## Milestone 6 - Release and live updates

**Owner:** Long, with Ang on assets and messaging.

- Final WebGL build on itch.io with a clear page, controls, credits, and AI disclosure.
- Version tags: 0.1 jam build, 0.2 art/animation, 0.3 enemies/audio, 0.4 balance/UX, 1.0 release.
- Patch cadence: small fixes as needed; one content/art milestone per update.
- Collect feedback through itch comments and a short form.
- Keep the roadmap public and mark items Done / In progress / Cut.
- Never ship a build with missing credits or unlicensed audio.

**Done when:** 1.0 is playable end to end, credits are complete, and the page accurately describes the game.

## How we will work

### Weekly

- Ang: one art target (character, card, icon set, or animation pass).
- Long: one systems/UI target plus a playable build.
- Both: one short playtest and a written note of what felt good or confusing.

### Per milestone

1. Pick the milestone.
2. Write a one-page brief with references and acceptance criteria.
3. Implement in small commits.
4. Playtest the build, not the editor scene.
5. Update README/roadmap and archive the build.

### Art handoff

- Ang supplies transparent PNGs and source files.
- Naming: category_subject_state.png (for example enemy_goblin_idle.png).
- Long imports, slices, animates, and keeps a manifest of what replaced what.
- No placeholder is deleted until its replacement is verified in-game.

## Priority board

| Priority | Item | Owner | Status |
|---|---|---|---|
| P0 | Runtime stability and clean UI | Long | In progress |
| P0 | Replace AI placeholder characters/enemies | Ang | Next |
| P0 | Final buff icon set and card art | Ang | Next |
| P1 | Character and Bingo animations | Ang/Long | Planned |
| P1 | Final SFX and music mix | Long | Planned |
| P1 | Enemy archetypes and intents | Long/Ang | Planned |
| P2 | Balance pass and difficulty | Long | Planned |
| P2 | Tooltips, save, accessibility | Long | Planned |
| P3 | More cards, conditions, daily seed | Both | Later |
| P3 | Achievements and meta-progression | Both | Later |

## Scope guard

We are **not** doing:

- multiplayer, online services, or accounts;
- 3D models or a genre change;
- runtime generative AI;
- monetisation or ads;
- a large content treadmill before the core five encounters feel finished.

If a feature does not make the Bingo-to-combat decision better, it waits.

## Open questions

- Should the board persist across the whole run or reset partially at the boss?
- How much should the player see of the next enemy before committing a Bingo placement?
- Is four cards the right hand cap after animation and enemy intents are added?
- Do we want a daily seeded run, or is a standard run plus difficulty modes enough?
- Which two enemy archetypes are the best use of Ang's art time first?

These are answered by playtests, not by adding more systems.
