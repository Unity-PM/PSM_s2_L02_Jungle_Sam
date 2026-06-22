# Jungle Sam - mission and story

Ten plik scala fabule, mission flow, roadmap vertical slice i szczegoly etapow. Zastapil rozbite dokumenty z `AI_CODEX/Docs` oraz pojedyncze dokumenty etapow w `Assets/JungleSam/Docs`.

## Setup fabularny

Vertical slice rozgrywa sie w zalanej, odcietej strefie tropikalnej. Black Orchid prowadzila tam badania biologiczne powiazane z obca technologia. Grom Division zabezpieczala teren, ale eksperyment wymknal sie spod kontroli. Infekcja rozprzestrzenila sie na personel badawczy, zolnierzy i ludzi ze strefy.

Klimat:

- dzungla,
- zalane drogi,
- nabrzeze i statek,
- opuszczony kosciol i cmentarz,
- domki i szklarnie,
- kompleks Black Orchid,
- final z UFO.

## Frakcje i zagrozenia

### Black Orchid

Tajna organizacja badawczo-militarna. Oficjalnie bada skazenie i anomalie biologiczne, w praktyce probuje wykorzystac infekcje jako bron terenowa i technologie kontroli biologicznej.

### Grom Division

Jednostka wojskowa wyslana do zabezpieczenia strefy, ochrony personelu Black Orchid i utrzymania kordonu. Ich slady to checkpointy, skrzynie z amunicja, barykady, radio, polegli zolnierze i sprzet ewakuacyjny.

### Infekcja

Infekcja pochodzi ze zrodla powiazanego z UFO. Reaguje na tkanki, wode, roslinnosc i sygnaly obiektu. Objawy:

- agresja,
- degradacja swiadomosci,
- deformacje ciala,
- przyspieszona mutacja.

### Zombie

Zombie to zainfekowani ludzie:

- personel Black Orchid,
- zolnierze Grom Division,
- cywile lub pracownicy strefy.

### MutantStalker

Silniejsza mutacja albo wynik eksperymentu bojowego Black Orchid. Ma pelnic role mini-bossa i sygnalu eskalacji.

### UFO

Ukryte zrodlo kryzysu i finalowy element vertical slice. W finale obiekt aktywuje beam, blokuje gracza i rozpoczyna abdukcje.

## Aktualny vertical slice

Aktualnie najwazniejszy fragment zaczyna sie przy statku/nabrzezu.

Gracz znajduje radio Grom Division przy malej lodce. Podniesienie radia powinno:

1. pokazac prompt interakcji,
2. zaktualizowac objective,
3. uruchomic pierwsza arene przy statku,
4. zamknac/utrzymac blockery areny podczas walki,
5. po ukonczeniu fal otworzyc przejscie,
6. aktywowac checkpoint po arenie.

Radio nie powinno byc niszczone przez `Destroy(gameObject)`, bo musi dac sie przywrocic po smierci gracza przed ukonczeniem areny.

## Plan etapow

### Etap 1: Start przy statku

Objective:

```text
Znajdz droge z nabrzeza i przedostan sie przez zalana droge.
```

Flow:

- start przy statku, wraku albo punkcie desantu,
- pierwsza mala fala zombie,
- gracz uczy sie ruchu, strzelania, ammo pickupow,
- radio Grom Division uruchamia wazny encounter arenowy,
- checkpoint po oczyszczeniu przejscia.

Checkpointy:

- `Checkpoint_Start_Boat` aktywny od startu,
- `Checkpoint_DockExit` aktywowany po ukonczeniu etapu nabrzeza.

### Etap 2: Kosciol i cmentarz

Objective:

```text
Przeszukaj teren kosciola i znajdz przejscie w strone zabudowan.
```

Flow:

- wejscie przez brame cmentarza albo prog kosciola,
- srednia fala zombie z cmentarza, alejek i wnetrza,
- dokument wojskowy lub story pickup moze aktualizowac cel,
- po oczyszczeniu odblokowanie przejscia do domkow, budynkow i szklarni.

Checkpoint:

- `Checkpoint_ChurchExit` aktywowany po oczyszczeniu kosciola/cmentarza i odblokowaniu wyjscia.

Przykladowe obiekty sceny:

```text
MissionStage_02_ChurchCemetery
Trigger_ChurchAreaEntered
INT_Church_MilitaryDocument
Encounter_ChurchCemetery
Checkpoint_ChurchExit
```

Reset po smierci przed ukonczeniem:

- encounter kosciola zatrzymuje fale,
- zywi przeciwnicy sa usuwani,
- liczniki spawnow wracaja do stanu startowego,
- checkpoint koncowy nie jest aktywowany.

### Etap 3: Kompleks Black Orchid / szklarnie

Objective:

```text
Wejdz do kompleksu Black Orchid i odszukaj zrodlo skazenia.
```

Flow:

- wejscie przez brame obozu albo szklarnie,
- walka z personelem badawczym, zainfekowanymi zolnierzami i MutantStalkerem,
- lokacja pokazuje kontenery, namioty, szklarnie, slady eksperymentow,
- gracz odkrywa zwiazek infekcji z UFO.

Checkpoint:

- `Checkpoint_BlackOrchidExit` po ukonczeniu glownego encountera kompleksu i otwarciu drogi do finalu.

### Etap 4: Finalna lokacja UFO

Objective:

```text
Dotrzyj do zrodla sygnalu i zabezpiecz finalowa lokacje.
```

Flow:

- gracz wychodzi na finalowa wyspe albo do budynku pod UFO,
- aktywuje sie sygnal i swiatlo nad lokacja,
- ostatnia horda z kilku kierunkow,
- opcjonalny powrot MutantStalkera,
- po walce przejscie do sekwencji UFO.

Checkpoint:

- `Checkpoint_UFOFinal` przed finalowa walka albo po zabezpieczeniu wejscia do areny UFO, ale nie w srodku aktywnej hordy.

### Etap 5: Abdukcja

Objective:

```text
Przetrwaj kontakt z obiektem.
```

Flow:

- po finalowej walce gracz wchodzi pod UFO,
- beam blokuje ruch i strzelanie,
- kamera moze skierowac sie ku UFO,
- dzwiek narasta,
- fade out,
- koniec vertical slice.

Po aktywacji beamu normalny fail state smierci powinien byc zablokowany. Sekwencja przejmuje flow i prowadzi do zakonczenia.

## Lista checkpointow

```text
Checkpoint_Start_Boat
Checkpoint_DockExit
Checkpoint_ChurchExit
Checkpoint_BlackOrchidExit
Checkpoint_UFOFinal
```

Zasady:

- checkpointy aktywuja sie po ukonczeniu waznych etapow,
- nie aktywuja sie w srodku aktywnej walki,
- po smierci gracz wraca do ostatniego aktywnego checkpointu,
- aktywny encounter resetuje sie po smierci,
- pickupy na MVP nie musza sie resetowac,
- ammo moze byc podbite do minimum tylko w sytuacji anty-softlockowej.

## Proponowana organizacja sceny

```text
MissionRoot
MissionStage_01_DockStart
MissionStage_02_ChurchCemetery
MissionStage_03_BlackOrchidGreenhouses
MissionStage_04_UFOFinalArea
MissionStage_05_Abduction
```

Encountery:

```text
Encounter_DockStart
Encounter_ChurchCemetery
Encounter_BlackOrchidCamp
Encounter_UFOFinalHorde
```

Spawn groupy:

```text
SpawnGroup_Zombie_Civilians
SpawnGroup_Zombie_GromSoldiers
SpawnGroup_Zombie_BlackOrchidStaff
SpawnPoint_MutantStalker_Greenhouse
```

Final:

```text
UFO_FinalObject
UFO_BeamVolume
UFO_AbductionCameraTarget
Trigger_Stage05_AbductionBeam
```

## Kryteria ukonczenia vertical slice

- Gracz moze przejsc trase od statku do finalowej lokacji UFO.
- Kazdy etap ma objective, trigger i encounter.
- Kazda walka ma zasoby potrzebne do kontynuowania gry.
- MutantStalker pojawia sie jako moment eskalacji.
- Smierc respawnuje gracza przy ostatnim checkpointcie bez reloadu sceny.
- Aktywny encounter resetuje sie po smierci.
- Finalowy beam blokuje gracza i konczy vertical slice abdukcja.

## Testy misji

### Radio i pierwsza arena

1. Podnies radio.
2. Sprawdz objective i notification.
3. Sprawdz start areny.
4. Zgin przed ukonczeniem.
5. Potwierdz:
   - radio wraca,
   - objective cofa sie,
   - arena resetuje fale,
   - blockery nie zostaja w stanie po sukcesie.
6. Ukoncz arene.
7. Potwierdz:
   - checkpoint po arenie aktywny,
   - bramy otwarte,
   - po smierci radio nie wraca.

### Kosciol

1. Wejdz do triggera kosciola/cmentarza.
2. Sprawdz, czy spawn pointy nie tworza przeciwnikow na oczach gracza.
3. Ukoncz encounter.
4. Sprawdz objective i checkpoint `Checkpoint_ChurchExit`.
5. Zgin przed ukonczeniem i potwierdz reset encountera.

### Black Orchid

1. Wejdz do kompleksu.
2. Sprawdz objective.
3. Przetestuj walke z MutantStalkerem.
4. Zgin podczas encountera.
5. Potwierdz despawn przeciwnikow i reset walki.

### UFO final

1. Wejdz do finalowej lokacji.
2. Sprawdz widocznosc UFO/swiatla/sygnalu.
3. Ukoncz finalowa walke.
4. Wejdz w beam.
5. Potwierdz blokade ruchu i strzelania.
6. Potwierdz fade out i zakonczenie vertical slice.

