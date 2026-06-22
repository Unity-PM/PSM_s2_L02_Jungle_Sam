# Menu, mission intro and pause

## Flow: Nowa gra -> Mission Intro -> Gameplay

`MainMenuController.NewGame()` nadal tworzy nowy zapis JSON użytkownika, ale nie ładuje już gameplayu od razu.

Flow:

- użytkownik klika `Nowa gra`,
- stary save użytkownika jest usuwany,
- powstaje nowy `SaveGameData`,
- save jest zapisywany do JSON,
- `MainMenuPanel` jest ukrywany,
- `MissionIntroController.Show(gameplaySceneName)` pokazuje briefing,
- gameplay ładuje się dopiero po kliknięciu `StartMissionButton`.

Aktualna scena gameplay: `Scene_A`.

## Continue

`ContinueGame()` nie pokazuje briefingu. To powrót do istniejącego zapisu, więc odczytuje save JSON i ładuje scenę zapisaną w `SaveGameData.sceneName`.

## Mission Intro

Prefab:

- `Assets/JungleSam/UI/MissionIntro/Prefabs/PF_MissionIntroPanel.prefab`

Kontroler:

- `Assets/JungleSam/Core/UI/MissionIntro/MissionIntroController.cs`

Sprite:

- `Assets/JungleSam/UI/WybórMisji.png`

Prefab ma strukturę:

- `Canvas_MissionIntro`
- `MissionIntroRoot`
- `MissionIntroBackground`
- `StartMissionButton`
- `BackButton`

`MissionIntroBackground` jest pełnoekranowym tłem z raycast target, żeby blokować kliknięcia w menu pod spodem. `StartMissionButton` i `BackButton` są transparentnymi przyciskami Unity UI nałożonymi na narysowane obszary w sprite.

W `Scene_MainMenu` prefab jest już wstawiony, a `MainMenuController.missionIntroController` jest podpięty.

## Pause Menu

Prefab:

- `Assets/JungleSam/UI/Pause/Prefabs/PF_PauseMenu.prefab`

Kontroler:

- `Assets/JungleSam/Core/UI/Pause/PauseMenuController.cs`

Sprite:

- `Assets/JungleSam/UI/Pauza.png`

Prefab ma strukturę:

- `Canvas_PauseMenu`
- `PauseRoot`
- `PauseBackground`
- `ResumeButton`
- `SettingsButton`
- `CheckpointButton`
- `ExitToMenuButton`

`PauseBackground` jest pełnoekranowym tłem z raycast target. Przyciski są transparentne i działają przez `PauseMenuController`.

W `Scene_A` prefab jest już wstawiony. `PlayerControlLock`, `playerRoot` i `CheckpointManager` są autowykrywane, a jeśli auto-wire nie znajdzie referencji, można podpiąć je ręcznie w Inspectorze.

## InputActionReference dla pauzy

`PauseMenuController` nie używa `UnityEngine.Input.GetKeyDown`.

Aby pauza działała z klawiatury/pada:

- otwórz instancję `Canvas_PauseMenu` w scenie gameplay,
- znajdź `PauseMenuController`,
- w polu `Pause Action` przypisz `InputActionReference` dla akcji pauzy, np. `Esc` / `Start`,
- akcja powinna mieć typ `Button`,
- kontroler zasubskrybuje `performed += TogglePause` w `OnEnable`.

Jeśli `Pause Action` nie jest podpięte, pauzę nadal można wywołać z UI, Context Menu albo publicznych metod.

## Przyciski transparentne

Sprite ma już narysowane teksty i ramki, dlatego prawdziwe przyciski mają prawie przezroczysty `Image`.

Ważne ustawienia:

- `Image.raycastTarget = true`,
- `Button.interactable = true`,
- alpha może być bliskie `0`, ale nie musi być całkowicie widoczne,
- highlight może mieć subtelny zielony tint.

Nie trzeba dublować tekstu TMP, jeśli tekst jest wypalony w sprite.

## Cursor i pauza

Podczas pauzy:

- `Time.timeScale = 0`,
- `Cursor.visible = true`,
- `Cursor.lockState = CursorLockMode.None`,
- `PlayerControlLock.SetLocked(true)`.

Po wznowieniu:

- `Time.timeScale = 1`,
- `Cursor.visible = false`,
- `Cursor.lockState = CursorLockMode.Locked`,
- `PlayerControlLock.SetLocked(false)`.

Jeśli Death UI albo Story Popup są aktywne, `PauseMenuController` ignoruje otwarcie pauzy, żeby nie naruszać death/checkpoint/story flow.

## Return To Checkpoint

`ReturnToCheckpoint()` nie przeładowuje sceny.

Kontroler używa:

- `CheckpointManager.RespawnPlayer(playerRoot)`

Jeśli `CheckpointManager` albo `playerRoot` nie są dostępne, metoda wypisze `Debug.LogWarning` i nie będzie wymuszać ryzykownego fallbacku.

## Build Settings

W Build Settings powinny być włączone:

- `Assets/JungleSam/Scenes/Menu/Scene_MainMenu.unity`
- `Assets/ThirdParty/Flooded_Grounds/Scenes/Scene_A.unity`

`Scene_MainMenu` powinna być pierwsza, jeśli build ma startować od menu.

## Test Mission Intro

1. Uruchom `Scene_MainMenu`.
2. Zaloguj się.
3. Kliknij `Nowa gra`.
4. Sprawdź, czy save JSON został utworzony.
5. Gameplay nie powinien załadować się od razu.
6. Powinien pokazać się Mission Intro.
7. Kliknij `Rozpocznij misję`.
8. Powinna załadować się scena `Scene_A`.
9. `Continue` w menu powinno ładować gameplay bez Mission Intro.

## Test Pause

1. Uruchom `Scene_A`.
2. Wywołaj `PauseMenuController.Pause()` z Context Menu albo podpiętej akcji inputu.
3. Gra zatrzymuje się, a cursor jest widoczny.
4. `Wznów` zamyka pauzę, `Time.timeScale` wraca do `1`, cursor wraca do locked.
5. `Wyjdź do menu` ładuje `Scene_MainMenu`.
6. `Powrót do checkpointu` używa `CheckpointManager.RespawnPlayer(playerRoot)`.
7. Console bez `NullReferenceException`.
