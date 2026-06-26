# Jungle Sam - auth and save system

## Cel

System menu wykorzystuje lokalne pliki JSON jako zewnętrzny zapis danych MVP. Dzięki temu projekt spełnia wymaganie zapisu i odczytu danych poza pamięcią runtime aplikacji, a jednocześnie zostawia prostą ścieżkę do późniejszego backendu online.

## Gdzie zapisują się pliki

Unity zapisuje pliki w `Application.persistentDataPath`.

Pliki Jungle Sam:

- `Application.persistentDataPath/JungleSam/UsersDatabase.json`
- `Application.persistentDataPath/JungleSam/Saves/{userId}_save.json`

Na Windows typowa ścieżka ma postać:

- `%USERPROFILE%/AppData/LocalLow/<CompanyName>/Jungle Sam/JungleSam/UsersDatabase.json`
- `%USERPROFILE%/AppData/LocalLow/<CompanyName>/Jungle Sam/JungleSam/Saves/{userId}_save.json`

## Rejestracja

`MainMenuController.Register()` pobiera `username` i `password` z pól UI, a następnie wywołuje `LocalJsonAuthService.Register()`.

Rejestracja:

- odrzuca pustą nazwę użytkownika,
- odrzuca puste hasło,
- sprawdza, czy użytkownik już istnieje,
- generuje `userId`,
- generuje sól hasła,
- zapisuje hash PBKDF2 zamiast hasła plaintext,
- zapisuje użytkownika do `UsersDatabase.json`,
- ustawia `AuthSession.CurrentUser`.

## Logowanie

`MainMenuController.Login()` wywołuje `LocalJsonAuthService.Login()`.

Logowanie:

- odczytuje `UsersDatabase.json`,
- znajduje użytkownika po nazwie,
- liczy hash wpisanego hasła z zapisaną solą,
- porównuje hash z zapisanym hashem,
- aktualizuje `lastLoginUtc`,
- ustawia `AuthSession.CurrentUser`.

Hasła nie są zapisywane plaintextem.

## Nowa gra

`MainMenuController.NewGame()` wymaga zalogowanego użytkownika.

Flow:

- usuwa istniejący save użytkownika,
- tworzy nowy `SaveGameData`,
- zapisuje go do `Saves/{userId}_save.json`,
- ładuje `gameplaySceneName`.

Domyślny save startuje od:

- `checkpointId = Checkpoint_Start_Boat`
- `missionStage = DockStart`
- `currentObjective = Znajdź źródło sygnału`
- `health = 100`
- `armor = 100`
- `ammo762 = 120`
- `ammo9mm = 48`

## Kontynuuj

Po zalogowaniu `MainMenuController` ustawia:

- `PlayerNameText = OPERATOR: {username}`
- `ContinueButton.interactable = saveService.HasSave(userId)`

`ContinueGame()` sprawdza zapis użytkownika. Jeśli istnieje, odczytuje JSON, przekazuje go do `SaveLoadContext` i ładuje scenę z save'a. Jeśli pliku nie ma, pokazuje komunikat `Brak zapisu gry`.

Po załadowaniu gameplayu `GameplaySaveLoader` zużywa `SaveLoadContext.PendingSave` i aplikuje podstawowy stan:

- checkpoint przez `CheckpointManager.RespawnPlayer(playerRoot)`,
- zdrowie przez `PlayerHealth`,
- pancerz przez `PlayerStats`,
- tekst celu przez `UIManager` / `GameplayHUDController`.

Pola broni, amunicji, ukończonych encounterów i zebranych pickupów są zapisane w modelu danych, ale ich pełne odtworzenie wymaga osobnej integracji z systemami gameplayu.

## Przygotowanie pod backend

Kod menu używa interfejsów:

- `IAuthService`
- `ISaveGameService`

Aktualnie `MainMenuController` tworzy:

- `LocalJsonAuthService`
- `LocalJsonSaveGameService`

W przyszłości można podmienić implementacje na:

- `BackendAuthService`
- `BackendSaveGameService`

Placeholdery backendowe są dodane, ale celowo nie wykonują requestów HTTP. Kiedy backend będzie gotowy, wystarczy zaimplementować te klasy i podać je kontrolerowi zamiast lokalnych usług JSON.

## Wymagania zaliczeniowe

System spełnia:

- logowanie użytkownika,
- rejestrację użytkownika,
- zapis danych zewnętrznych do pliku JSON,
- odczyt danych zewnętrznych z pliku JSON,
- lokalny zapis stanu gry przypisany do użytkownika,
- architekturę gotową do późniejszej zamiany lokalnych plików na backend online.
