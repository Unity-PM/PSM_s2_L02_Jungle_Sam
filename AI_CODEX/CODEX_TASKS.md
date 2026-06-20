# CODEX_TASKS.md - Jungle Sam

## Done on 2026-06-19

- Reorganized project folders under `Assets/JungleSam` and `Assets/ThirdParty`.
- Moved first-party scripts, prefabs, scenes, settings, UI sprites and ScriptableObjects into `Assets/JungleSam`.
- Moved Flooded Grounds and imported model packs into `Assets/ThirdParty`.
- Updated `ProjectSettings/EditorBuildSettings.asset`.
- Added MutantStalker gameplay enemy.
- Added MutantStalker animator controller builder.
- Generated/used `AC_MutantStalker_Gameplay.controller`.
- Added shared `IDamageable` interface.
- Updated `WeaponBase` to damage `IDamageable` first and fallback to `EnemyAI`.
- Added MutantStalker action locks for attack, hit reaction and rage.
- Updated MutantStalker animator wrapper to guard locomotion against action states and death.

## Immediate Unity verification

- Let Unity recompile and reimport.
- Check Console for compiler errors, missing scripts and missing references.
- Open `Assets/ThirdParty/Flooded_Grounds/Scenes/Scene_A.unity`.
- Confirm Build Settings scenes:
  - `Assets/ThirdParty/Flooded_Grounds/Scenes/Scene_A.unity`
  - `Assets/JungleSam/Scenes/Test/World.unity`
- Test `WeaponBase` damage against old zombie `EnemyAI`.
- Test `WeaponBase` damage against `MutantStalkerAI`.
- Test MutantStalker action states:
  - chase,
  - attack,
  - hit reaction,
  - rage,
  - death.
- Confirm no sliding after attack/gethit/rage.

## Next tasks

- Create or finalize MutantStalker prefab in a stable first-party folder.
- Add attack Animation Events calling `DealDamageToTarget` if animation-event damage is used.
- If sliding persists, inspect MonsterMutant7 clips and Avatar/Rig import.
- Create a first-party gameplay scene based on Flooded Grounds instead of editing the third-party scene directly.
- Update older `Dokumentacja/*` files that still mention `Assets/Scripts` paths.
