# UI Step 1A verification evidence

Scene: `Assets/Scenes/GameScene.unity` (reloaded from disk; never saved).
CanvasScaler: **Scale With Screen Size**, reference **1920x1080**, **Match 0.5** — see `canvas_scaler.png`.

## Verified
- 25 Bingo cells (5x5) at 1920x1080 — `bingo_play_1920x1080.png`.
- Draw followed by a matching-cell click: ball **38**, cell accepted it and showed **"Placed number 38!"** — `bingo_placed_1920x1080.png`.
- 3-card combat hand visible in the Game view (Attack/Defend/Focus), plus the **Continue** button; combat ran through multiple rounds.
- Shop opens with three offer columns — `shop_1920x1080.png`.
- Full run completed to **VICTORY**: Encounters **5/5**, Bingo lines **12/12**, final coins **140**, player HP **3/30** — `victory.png`.

The full run was driven by a temporary editor-side auto-play helper (prefer Attack cards; buy all shop offers), then the helper was deleted. This exercises the complete loop, but it is not a manual click-through of every screen.

## Observed UI issues
- Shop: summary text overlaps the first offer, offer text overlaps the dropdowns, and all buy buttons read **"Button"** instead of labels — `shop_1920x1080.png`.
- Defeat panel: summary text is clipped on the right ("Bingo lines completed:" is cut off).
- Console: no errors, but hundreds of TMP warnings: `The character with Unicode value \u2198 ... was not found in the [LiberationSans SDF] font asset` (arrow glyphs replaced by fallback). Affects arrow characters used in UI text.

## Not verified
- 1600x900, 1280x720, 960x600 native captures. The Editor Game view reports its viewport size (e.g. 1125x995), not the selected target resolution; no valid captures at those sizes.
- Clean manual-click run without the auto-play helper.

## Notes
- A pre-existing modification to `Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset` was left untouched.
- Temporary scripts (`Assets/Editor/VerifyCapture.cs`, `Assets/Editor/AutoPlay.cs`) were removed.

## AI page backgrounds (hand-drawn comic theme)
Generated in the existing ChatGPT "Game UI" chat to match the handoff art direction, then saved to `Assets/Art/UI/Backgrounds/`:
- `main_menu.png` (1672x941)
- `victory.png` (1672x941)
- `defeat.png` (1672x941)

Theme: warm ivory paper (#F6F4EF), charcoal ink outlines (#20272B), muted coral (#E79B88), teal (#78AFA7), ochre (#D7BA75), lavender (#B8ACCD); hand-drawn comic illustration, no neon/glossy 3D.

`VictoryPanel` and `DefeatPanel` use them as full-screen background Images, and their summary text is set to charcoal for contrast. Buttons were not touched. The main-menu image is an asset only; there is no main-menu scene/panel yet.

## Cards (owner line art + theme)
Source line art: `~/Downloads/Gridding_Factors/Attack.jpg`, `Defend.png`, `Focus.jpg`, `Box.jpg`. Heal and Piercing were regenerated in ChatGPT using Attack/Defend/Focus as an explicit style reference: adult stylized knight, bold ink on flat grey, not chibi, then colorized sage/lavender.

Card reference decision:
- Reviewed Pokémon, Hearthstone, Slay the Spire and Magic layouts.
- Pokémon's multi-attack block is unnecessary for Gridding Factors' single-effect cards.
- Adopted a Hearthstone / Slay the Spire style: full-color card, artwork on top, title, large effect number, one-line rules text. The value is the effect magnitude, not a cost.
- Descriptions: "Deal X damage", "Gain X Shield", "+X next Attack", "Restore X HP", "Deal X damage / Ignores Shield".

In-project `Assets/Art/UI/Cards/`:
- `attack.png`, `defend.png`, `focus.png`, `heal.png`, `piercing.png` — 1000x1000 charcoal ink on theme tints (blush coral, mint, sand, sage, lavender), paper grain and soft ink shadow.
- `Art/` — 210:165 card crops used in the artwork window.
- `LineArt/` — transparent charcoal versions.
- `card_frame.png` — ivory frame, 60px 9-slice border.

`CardButton.prefab` is 210x290: per-type tint layer, full-width artwork (210x165), frame overlay, title, large value, description. `CardView` binds tint, art, value, title and description from `CardInstance` by `CardType`; all five sprites are assigned. Final layout mockup: `card_layout_final.png`.

Piercing effects: owner-updated sketch, stray dot removed, hand-drawn speed/impact lines added around the shield, then colorized lavender. Knight and existing lines untouched.

`BingoCell.prefab` has a transparent `BoxFrame` overlay using the box line art (rule colors and clicks unchanged).

## UI layout pass
- Global HUD: Encounter top-left, Coins top-right, 24pt; RunHUD moved above all panels.
- Shop: 3 columns of 420x560 panels at x=250/750/1250, 26pt titles, 18pt previews, 372x56 dropdowns and buy buttons, compact summary/status, no duplicate coins.
- Bingo: 620x620 grid left, 320x600 powerball column right, HP/Focus above it, feedback strip below the grid.
- Combat: player/enemy status top corners, battle log top center, hand bottom center, Continue right.
- Victory/Defeat: short 3-line summaries in a 900x300 centered block.
- Background set to warm ivory; panel placeholders made transparent.

## Shop target selector
- Dropdowns are hidden. Buy on a rule/buff offer opens a 5x5 Bingo selector with eligible cells highlighted; clicking a cell buys the upgrade.
- Buy on a card upgrade opens a card list selector; adding a new card still buys directly.
- Selector panel is built at runtime by `ShopTargetOverlay`, 760x700, with Cancel.
- Side padding increased: HUD 96px, shop columns at x=300/750/1200 (300px margins), Bingo group centered with 425px margins, combat status at 120px margins.

## Box art and inner padding
- Bingo cells now use `card_frame.png` (the Box art) as the cell background, tinted by rule color; the old thin `BoxFrame` overlay is disabled.
- Rule text is inset 8px horizontally and 28px vertically, 18pt white; buff text 16pt white.
- Shop offer panels get a runtime 9-sliced `box_frame_line.png` overlay (transparent interior). Content sits at x=40, title y=78, preview y=145, buy button 80px from the bottom, so nothing touches the frame.
- Card frame overlay also switched to the transparent `box_frame_line.png` so the art is no longer washed out.

## Gameplay fixes
- `BingoRuleType.Any` now matches every number and displays **ANY**; the shop condition upgrade is placeable.
- Reroll now draws a number that has at least one placeable cell; if no such number remains, it ends the phase instead of wasting the reroll.
- Cell buffs trigger on placement, stack additively; row completion grants separate Ultimate charges (horizontal attack multiplier, vertical blocks, diagonal card multiplier/heal).
- Centre `LUCKY` wildcard has no stat buff; it only matches any number.
- Build settings scene path updated to `GameScene.unity`.

## Main menu, tutorial, piercing
- `MainMenuUI` builds a runtime main menu over the scene at startup: ivory art background, title, PLAY, HOW TO PLAY. Time is paused until the tutorial is finished.
- The tutorial has 3 pages: draw/place, bingo buffs and Ultimates, combat/shop. PLAY opens it automatically; HOW TO PLAY opens it from the menu.
- Piercing is now one guaranteed card in the opening hand (DeckManager inserts it first), and its CardView gets an ULTIMATE badge and lavender frame glow. Shop 2 can still add a second copy.
- Scene is now `GameScene.unity`; build settings path updated.

## Audio
- BGM: `Assets/Resources/Audio/bgm.mp3` — "Lost in the forest" by TOOPS, CC-BY 4.0.
- SFX: draw, place, card, card draw, bingo/win, buy, lose, UI click — Kenney CC0, Listener CC0, bingo machine CC0; see `Assets/Resources/Audio/CREDITS.txt`.
- `AudioManager` loads clips from Resources, loops BGM at low volume, hooks scene buttons (including inactive panels) and runtime selector/menu buttons.

## Box assignment / shop art
- Soft box: normal cards and Bingo cells (the angular frame's bands were too heavy at 100–210px and squeezed text).
- Angular box: shop offer panels, 9-sliced with 100px border; content inset 100px, buy buttons 110px from bottom.
- Buttons remain clean colored pills; the angular corners do not scale to 52–60px height.
- Shop drawing used as a 12%-opacity full-screen line-art backdrop behind the offer panels.
- Piercing uses the new owner drawing, colorized lavender; CardView still adds the ULTIMATE badge and lavender frame glow.

## Selectable cells, frames, no stretching
- Eligible Bingo cells: 18% brighter rule colour, pulsing gold outline, 1.045 scale pulse, animated highlight alpha.
- Soft box stays on cards/Bingo cells. Angular box is now a dedicated 420x560 shop frame (aspect-correct, no stretch), with 100px inner padding.
- Shop drawing sits behind the offer panels at 12% opacity with preserveAspect.
- Buttons get a 320x64 hand-drawn frame (9-sliced, 16px border), so they are framed without distorting the corner art.
- Card frame overlay is 9-sliced; card art is untouched.
- New Piercing drawing colorized lavender, ULTIMATE badge/glow retained.

## Shop theme pass
- Shop offer boxes switched back to the soft box (exact 420x560 frame), with pale coral/teal/ochre tints.
- Hand-drawn SHOP sign from the owner drawing sits above the offers.
- Shop drawing is a 12% background with preserveAspect.
- Buy labels fit inside the new 320x64 button frame; card/button/card frames are 9-sliced, shop frame is exact aspect (no stretching).
- Selectable Bingo cells use the pulsing gold outline/scale effect.

## Focus HUD, buff icons, characters, buttons, larger cards
- Focus_Buff.jpeg knocked out to alpha, trimmed with 6% padding, 512x512; HUD icon replaces the Focus text.
- Focus badge is live from CombatManager.PlayerFocus; hidden at 0/1, number 2-6, cap 6.
- Five 512px buff icons: attack, defend, heal, piercing, wildcard; shown in a 40px HUD legend with labels.
- Player/enemy character art generated with ChatGPT from the card knight style, knocked out to 1024 alpha; player mirrored to face right, enemy faces left.
- Buttons use Box.jpeg as a single-outline 320x64 9-sliced sprite (16px border). The 90px border was too crowded at 56px, so the line-only fallback was used.
- Cards are 240x330, artwork clipped to 216x170 (12px inset, 420:330), frame overlay above art, 18px hand spacing; hand panel 1300x340.
- FocusCard displayName typo fixed to 'Focus'.

### Evidence
- focus_badge_states.png (0/1/2/6)
- bingo_hud_1920x1080.png / bingo_hud_1280x720.png
- combat_1920x1080.png / combat_1280x720.png
- shop_1920x1080.png / shop_1280x720.png
- buttons_box_proof.png

These are 1920/1280 layout proofs rendered from the project sprites. A fresh native Unity play-mode capture was blocked when the editor was restarted for this pass; compile is clean (0 errors) but these should be treated as layout proofs, not native captures.
## Combat layout pass
- Player side: HP bar with shield/focus rows at the left edge, beside the 4-card hand.
- Player faces right; buff strip sits immediately to the player's right.
- Enemy HP/shield panel at top-right; enemy character faces left.
- Hand capped at 4 cards; 4 x 240px cards with 18px spacing fit the 1060px hand.
- Card title/value/description and all combat labels increased; dead space removed.
- New proof: combat_layout_1920x1080.png / combat_layout_1280x720.png.
- New scripts: CombatHudUI.cs; CombatManager and BingoAttackBuffs expose read-only UI state; hand cap changed from 5 to 4.
## Combat cleanup (latest reference)
- Player/enemy health are now just labelled bars, with no surrounding panel box.
- Shield and Focus are folded into the player buff box as buff entries.
- Buff boxes sit at the sides of the card row: player left, enemy right.
- Buff boxes and individual icons are hidden whenever their value is zero.
- Removed the 'MAX 4 CARDS' hint; hand cap remains 4.
- Character facing art was left unchanged.
- New proofs: combat_clean_1920x1080.png / combat_clean_1280x720.png.
- Player character art mirrored horizontally so the player faces right toward the enemy; enemy still faces left. Updated player_character.png and its Resources copy.
## Information cleanup
- Removed the combat buff-box headings and the 'MAX 4 CARDS' hint.
- Removed Bingo buff legend labels; icons only.
- Removed the main-menu footer tagline.
- Hid unused 'New Text' placeholders and duplicate shop summary/coin labels.
- Health is now only a label plus bar, with no surrounding panel.
- Buff boxes sit at the card edges and hide when empty; individual zero-value icons also hide.
- Fixed the buff-count NullReferenceException by creating the count text as a separate child object.
- Proofs: combat_minimal_1920x1080.png / combat_minimal_1280x720.png.