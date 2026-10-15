# Original title-promotion challenge: UI and animation evidence

This is static source/serialized-asset evidence, not an observation of the original app running.

## Target and extraction

- Original prefab: `Assets/GameRes/NewUI/Challenge/prefabs/challengeUI.prefab`.
- Catalog bundle: `eae0a856c24d80a0f8dadadbae8b1545.bundle`, dependencies `1a5cfbfeb47584991ba7f74e94333037.bundle` and `4cac0588bfa397c0a6d0ea1103b18b17.bundle`.
- Extracted 9-node prefab retained at `UI/Prefabs/Challenge/challengeUI.json`.
- The six sprites exactly match existing project PNG pixels and have existing metadata. No sprite-key remapping is needed.
- Its owning serialized file has no AnimationClip, AnimatorController, or AnimatorOverrideController assets. The prefab nodes have no Animator, Animation, Spine, or custom animation component.
- `Tools/export_promotion_ui.py` performs isolated incremental extraction. It does not invoke the destructive full exporter and never writes live project Assets.

## Transitions and result UI

- `Reference/DecompiledCSharp/DownItem.cs:2066` OpenChallenge immediately hides side ads/permanent controls/growth badge, hides the enter button, shows exit, initializes and enables challengeUI. No curtain or cutscene call is made.
- First entry shows the original dialog `60秒内显示搜集足够多的金钱!`, sets timeScale to zero, then resumes timeScale when dismissed. First availability has a separate guide `点击进入挑战,获得等级晋升!`.
- `DownItem.cs:2132` ChallengeVictory immediately enables the native victoryPanel with the original success artwork and reward-grid prefab. It sets the dismissal timer to 3 seconds. There is no result scale/fade/tween method or serialized animation component on the native result nodes.
- `DownItem.cs:2196` ChallengeFail immediately enables the original failPanel and sets its dismissal timer to 5 seconds.
- `DownItem.cs:1146` Update invokes the result mask's click event at the timer deadline; result mask Button components are disabled in the original prefab, but their onClick can still be explicitly invoked. Confirm controls invoke the same events.
- The success ad-labelled control calls FakeVideoManager; source grants the same prepared reward quantities as normal confirmation, despite its double-reward label. It does not call CheckChallenge after ad success. This is observed source behavior, not a proposed improvement.
- `UIManager.cs:1558` reward confirmation calls original resource-flight animation. Each currency type 1/2 has 15 particles; other item types have 8. One type uses its own fly prefab; mixed types use fly/all. Stagger delay 0..0.4s, scale-pop 0.18..0.3s with ease enum value27, hold 0..0.2s, upward drift 0.35..0.55s with ease enum value3, then fly to target 0.55..0.85s with ease enum value5. Preserve numeric enum values against the shipped DOTween mapping (standard mapping27=OutBack,3=OutSine,5=InQuad). Item IDs 1 and 3 fly to gold text; every other ID flies to diamond text, exactly as source.

## Geometry

Preserve the full original DownPanel hierarchy (nodes 66..78). The parent challengePanel is anchored at (.5,1), positioned (-600,430), size (100,100). Its rotation is **zero**. The last exported rt value -5.603819847 is local z position, not rotation.

The enter button is at (457,547), size (599,225), with original Spine glow behind its static sprite. Exit is at (69,564), size (275,159). Victory/fail artwork and controls are already in `Assets/Resources/UI/Prefabs/Common/DownPanel.json`.

challengeUI root position is (0,-36), size (100,100). Countdown title at (597,649), native size (457,71), font42. Time text offset (109.4,0), size (200,50). Progress background/bar at (597,557), size (772,96). Counter background at (597,481), size (528,80), font44.6. Follow endpoint x=-360+720*fill, y=0. Goal flag at (995,566), size (141,142).

## Enter-button Spine

`Reference/Spine/Uieff_ty_btn_challenge_3ef2b7d6` contains the original Spine3.8.75 JSON/atlas/PNG and decoded attachments. DownPanel node67 starts `idle`, looping, timescale1, scaled time. The clip duration is .5s; original render order is attachment1 on bone1, attachment1 on bone2, attachment2 on bone3. Full tracks are retained in the JSON; reproduce them rather than substituting a generic pulse.

## Tab lifecycle

- `UpgradePanel.cs:623..716`: item tab calls ShowDownItem, all other tabs call HideDownItem.
- `UIManager.cs:1013..1047`: entire DownItem root is SetActive(false/true), retaining child activeSelf. It is instantiated under Popup layer.
- ChallengeManager.Update is therefore suspended while on another tab; no time ticks there.
- ChallengeManager.ChallengeAddCoin checks challengeUI.gameObject.activeSelf, not activeInHierarchy, so its subscribed gain event can still collect income while the ancestor is hidden.
- DownItem result countdown also suspends while another tab hides its root. Returning to Item preserves it. ChallengeManager has no OnEnable/OnDisable resets.

## Scope limits

The title-promotion feature does not invoke PlayCutsceneTransition. References to that method occur in world-change/rebirth paths. The separate Uieff_challenge_curtain asset and auction code are not evidence for a curtain in this feature. No original runtime recording was available in this audit, so static evidence establishes the exported layout, explicit callbacks, tracks, and timing; it cannot verify unspecified engine/plugin rendering details beyond those assets.
