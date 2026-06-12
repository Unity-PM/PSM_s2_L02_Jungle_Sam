# Project Jungle Horde (MVP)

An intensive, arcade Horde Shooter / FPS game inspired by the classic gameplay of the *Serious Sam* series. The project focuses on high-performance rendering of enemy crowds, fluid retro-styled movement, and a decoupled, scalable architecture using modern Unity 6 features.

---

## 🛠️ Tech Stack & Tools

* **Game Engine:** Unity 6.3 (LTS)
* **Render Pipeline:** Universal Render Pipeline (URP)
* **Language:** C# (SOLID principles, Event-Driven Architecture)
* **Input System:** Unity New Input System Package
* **Navigation:** Advanced NavMesh Surfaces


---

## 🚀 Key Features & Gameplay Architecture

* **Snappy FPS Controller:** Custom-built character controller utilizing the New Input System. Features high-velocity jumping, responsive airborne control, and sprinting mechanics tailored for fast-paced arcade combat.
* **Performance-Optimized AI Hordes:** Enemy AI built on `NavMeshAgent` with a custom **AI Throttling system** (tick rate updates instead of per-frame calculations). This ensures smooth CPU performance even with 50+ active entities on screen.
* **Event-Driven Wave Manager:** The spawner communicates with enemies via static C# Actions (`OnEnemyDied`). It completely eliminates expensive runtime operations like `GameObject.Find` or `GetComponent` in `Update` loops.
* **Robust Weapon Pipeline:** Implemented a modular weapon hierarchy that prevents Blender-to-Unity axis/scale overriding via structural container layers (`Weapon_Scale_Root` -> `Rotation_Fix`).
* **Dynamic UI with Time-Zone Mapping:** Includes a unique HUD feature displaying real-time Polish local time (CET/CEST) by converting system UTC data independently of the player's local hardware settings.

---

## 📈 Current Project Status (What's Implemented?)

### 1. Gameplay & Controller
* Fully functional FPS movement with customized snappy gravity simulation.
* Camera clipping prevention with fine-tuned Near Clip Planes (0.01) for clean weapon mesh rendering.
* Locked 16:9 aspect ratio scaling environment.

### 2. Weapons & Ammo System
* **Pistol & 7.62x39mm Assault Rifle:** Base configuration complete with custom structural containers.
* Integrated asset animations: *Idle, Walk, Run, Shoot (loopable for Full-Auto), Reload, Inspect*.
* Functional Ammo Pickups mapped to specific weapon ammunition types.

### 3. AI & Game Loop
* Basic Zombie AI with automated **Mecanim Blend Tree** integration (Idle/Walk/Run blending based on physical velocity).
* Proximity-based attack triggers and state control.
* Scalable Wave Spawner loop utilizing event-driven entity tracking.
