# 📋 SETUP INSTRUKCJA - Ammo Pickups & UI

## 🎯 Co zostało zrobione

✅ **AmmoPack.cs** - Skrypt paczek amunicji  
✅ **WeaponBase.AddAmmo()** - Metoda uzupełniania amunicji  
✅ **AmmoUI.cs** - Wyświetlanie amunicji na ekranie  

---

## 🔧 Setup w Unity

### Krok 1: Przygotowanie Prefabów Paczek

1. W **Assets/Prefabs** znajdź prefaby amunicji (pistolet i karabin)
2. Na każdym prefabie wykonaj:

   **a) Collider**
   - Zaznacz prefab
   - Inspector → dodaj **Sphere Collider** (jeśli brakuje)
   - Ustaw **Is Trigger = ON** ✓
   - Adjust size aby otaczał model

   **b) Rigidbody**
   - Zaznacz prefab
   - Inspector → dodaj **Rigidbody** (jeśli brakuje)
   - Ustaw **Is Kinematic = ON** ✓
   - Ustaw **Gravity = OFF** ✓

   **c) AmmoPack.cs**
   - Zaznacz prefab
   - Inspector → Add Component
   - Szukaj **AmmoPack**
   - Skonfiguruj:
     - **Ammo Amount**: 
       - Pistolet: `9`
       - Karabin: `30`
     - **Respawn Time**: `5`
     - **Rotation Speed**: `45` (powolna rotacja)
     - **Bobbing Speed**: `2`
     - **Bobbing Height**: `0.5`

3. Ustaw Tag na prefabie:
   - Inspector → Tag dropdown
   - Jeśli nie ma tagu "Player" → utwórz go
   - Player (gracz) musi mieć Tag: **"Player"**

### Krok 2: Rozmieszczenie Paczek na Mapie

1. W scenie **Main Game**
2. Przeciągnij prefaby paczek z Assets/Prefabs do sceny
3. Umieść je na mapie w strategicznych miejscach
4. Upewnij się że są na NavMesh (dla widoczności)

### Krok 3: Setup UI

1. W Canvas sceny (jeśli brakuje, utwórz: Right Click → UI → Canvas)
2. Utwórz Text dla amunicji:
   - Right Click na Canvas → UI → Text - TextMeshPro
   - Zmień nazwę na **"AmmoText"**
   - Inspector → Text Mesh Pro - wpisz placeholder: "0/30"
   - Zmień rozmiar fonta na ~40
   - Ustaw kolor na biały lub żółty

3. Dodaj skrypt AmmoUI do Canvas lub osobnego GameObject'u:
   - Zaznacz Canvas lub UI parent
   - Add Component → AmmoUI
   - Przypisz **AmmoText** do pola "Ammo Text"

4. Parametry AmmoUI:
   - **Offset X**: `30`
   - **Offset Y**: `30`

### Krok 4: Sprawdzenie Player Tagu

1. Zaznacz gracza (Player GameObject)
2. Inspector → Tag dropdown
3. Ustaw na **"Player"** (musi się zgadzać z AmmoPack.cs)

---

## 🎮 Test

1. Uruchom grę (Play)
2. Zbliż się do paczki amunicji
3. Powinna się ona:
   - ✅ Obracać (Y axis)
   - ✅ Pulsować up-down
   - ✅ Zniknąć po zbieraniu (automatycznie)
   - ✅ Pojawić się ponownie po 5 sekundach
4. W lewym dolnym rogu ekranu powinna być: **"30/30"** (lub inna ilość)
5. Po strzale: **"29/30"**
6. Po podniesienia paczki: powinna wrócić do **"30/30"** (lub +9 dla pistoletu)

---

## 📊 Konfiguracja Paczek

### Pistolet Ammo Pack
```
Ammo Amount: 9
Respawn Time: 5s
```

### Karabin Ammo Pack
```
Ammo Amount: 30
Respawn Time: 5s
```

---

## 🐛 Debugowanie

**Jeśli paczka się nie zbiera:**
- ✓ Sprawdź Tag "Player" na graczu
- ✓ Sprawdź czy Collider na paczce ma **Is Trigger = ON**
- ✓ Sprawdź Console za errorami

**Jeśli UI nie pokazuje się:**
- ✓ Sprawdź czy AmmoText istnieje i jest assigned
- ✓ Sprawdź czy Canvas jest active
- ✓ Sprawdź czy PlayerController ma public currentWeapon

**Jeśli paczka nie respawnia:**
- ✓ Sprawdź Respawn Time w AmmoPack
- ✓ Uruchom grę i czekaj 5 sekund

---

## ✅ Checklist Setup

- [ ] Obie paczki mają Sphere Collider (Is Trigger = ON)
- [ ] Obie paczki mają Rigidbody (Is Kinematic = ON)
- [ ] Obie paczki mają AmmoPack.cs skrypt
- [ ] Obie paczki mają różne Ammo Amount (9 i 30)
- [ ] Gracz ma Tag "Player"
- [ ] Canvas ma AmmoUI skrypt
- [ ] AmmoText jest przypisany w AmmoUI
- [ ] Paczki rozmieszczone na mapie
- [ ] Build kompiluje się bez błędów

---

## 🎬 Zachowanie w Grze

```
┌─────────────────────────────┐
│ Gracz zbliża się do paczki  │
└──────────────┬──────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│ Trigger OnTriggerEnter ()           │
├─────────────────────────────────────┤
│ 1. Sprawdź czy to Player            │
│ 2. Pobierz WeaponBase               │
│ 3. Dodaj amunicję: AddAmmo(9 lub 30)│
│ 4. Wyłącz paczkę (Disable)          │
└──────────────┬──────────────────────┘
               │
               ▼
┌──────────────────────────────┐
│ Paczka znika (5 sekund)      │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│ Timer odpala: Respawn()      │
│ Paczka pojawia się na nowo   │
└──────────────────────────────┘
```

---

## 📌 Ważne Notatki

- AmmoPack.cs automatycznie dodaje do **aktualnie wybranej broni**
- UI aktualizuje się każdą klatkę (Update)
- Offset 30 pixeli sprawdza się dobrze w 1920x1080, ale możesz go zmienić w zależności od rozdzielczości
- Respawn timer liczy się nawet gdy gracz nie obserwuje paczki

---

Gotowe do testowania! 🚀
