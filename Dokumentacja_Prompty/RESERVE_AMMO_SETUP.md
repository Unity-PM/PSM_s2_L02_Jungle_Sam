# 🔫 Reserve Ammo System - SETUP

## ✅ Co zostało zrobione

- ✅ WeaponData - dodane `maxReserveAmmo` 
- ✅ WeaponBase - dodane `_reserveAmmo` logika
- ✅ Reload - teraz konsumuje reserve ammo
- ✅ AddAmmo - dodaje do schowka, nie magazynku
- ✅ AmmoUI - wyświetla format `11 (90)` 

---

## 📋 Setup w WeaponData

Otwórz każdy WeaponData (Pistolet, Karabin):

### **Pistolet**
- `Max Ammo`: 30 (magazynek)
- `Max Reserve Ammo`: 90 (schowek) ← **ZMIEŃ TO**

### **Karabin**
- `Max Ammo`: 30 (magazynek)
- `Max Reserve Ammo`: 240 (schowek) ← **ZMIEŃ TO**

---

## 🎮 Jak działa system

### Przed zmianą:
```
WeaponBase.AddAmmo(9)  →  currentAmmo += 9  ✗ (BŁĘDNIE)
```

### Po zmianie (POPRAWNIE):
```
WeaponBase.AddAmmo(9)  →  reserveAmmo += 9  ✓
```

### Reload:
```
Reload() → transfer z schowka do magazynku
reserveAmmo -= ammoLoaded
currentAmmo += ammoLoaded
```

---

## 📊 Wyświetlanie UI

**Format:** `11 (90)` 
- `11` = aktualne naboje w magazynku
- `90` = aktualne naboje w schowku

**Przykładowe sekwencje:**

| Akcja | Display |
|-------|---------|
| Start | 30 (90) |
| Strzał x5 | 25 (90) |
| Reload | 30 (85) |
| Pickup +9 | 30 (94) |
| Strzały | 25 (94) |
| Reload | 30 (89) |

---

## 🔧 Zmiana wartości Reserve Ammo

1. W **Project** → Find WeaponData (Pistolet)
2. Zaznacz i otwórz w Inspector
3. Zmień:
   - **Max Reserve Ammo**: `90`
4. Powtórz dla Karabinu: `240`

---

## 🎯 Test

1. Uruchom grę
2. UI powinien pokazywać: `30 (90)` lub `30 (240)`
3. Po strzale: `29 (90)`
4. Po przeładowaniu: `30 (85)` (przeniósł 5 z schowka)
5. Po podnesieniu paczki: `30 (94)` (dodał 9 do schowka)

---

## 📌 Ważne

- Reserve ammo **nie zmienia się podczas strzału** (zmienia się tylko magazynek)
- Reload **przesyła amunicję ze schowka do magazynku**
- Pickup **uzupełnia schowek**, nie magazynek
- Jeśli schowek = 0, nie możesz przeładować

---

Gotowe! 🚀
