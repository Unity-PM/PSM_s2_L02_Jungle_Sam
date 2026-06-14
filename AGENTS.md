# Jungle Sam - Agent Instructions

Project:
Unity 6.3 URP C# single-player FPS Horde Shooter inspired by Serious Sam/Escape from Tarkov/Call of Duty Zombies.

Current context:
- Existing WeaponBase.cs uses raycast shooting.
- Ammo system: magazine + reserve ammo.
- Weapons support semi-auto and full-auto through WeaponData.isAutomatic.
- Weapon animator triggers: Shoot, Reload, ReloadEnd, Inspect.
- Weapon animator bools: IsWalking, IsRunning.
- Zombie EnemyAI uses NavMeshAgent.
- Zombie chase, rotation, attack and death animation are currently working.
- Do not break existing zombie EnemyAI behavior.

Coding rules:
- Use [SerializeField] private fields.
- Do not rename existing public classes, public methods, serialized fields, or ScriptableObject fields unless explicitly requested.
- Do not modify unrelated systems.
- Do not edit Unity .meta files manually.
- Do not make scene/prefab changes directly unless explicitly requested.
- After code changes, explain required Unity Inspector setup.
- After code changes, explain Play Mode test steps.

Architecture:
- Player uses CharacterController.
- Enemy AI uses NavMeshAgent.
- WeaponBase uses raycast and WeaponData ScriptableObject.
- WaveSpawner handles enemy waves.
- Prefer modular scripts.
- Avoid large manager classes doing everything.

Do not touch unless task explicitly requires it:
- Assets/Scripts/Weapon/WeaponBase.cs
- Assets/Scripts/Weapon/WeaponData.cs
- Assets/Scripts/Enemy/EnemyAI.cs
- Assets/Scripts/Player/PlayerController.cs
- Assets/Scripts/Player/PlayerStats.cs

Preferred folders:
- Assets/Scripts/Spawning
- Assets/Scripts/Environment
- Assets/Scripts/UI
- Assets/Scripts/Managers

After every task, summarize:
1. changed files,
2. what was added/modified,
3. Inspector setup,
4. Unity Play Mode test steps,
5. suggested commit message.Prze