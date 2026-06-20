# Jungle Sam - Mission Flow

## Etap 1: Start przy statku

**Objective text:** Znajdź drogę z nabrzeża i przedostań się przez zalaną drogę.

**Trigger:** Gracz rozpoczyna poziom przy statku, wraku lub prowizorycznym punkcie desantu. Po wejściu w pierwszy obszar trigger aktywuje początkową falę zombie i pierwszy komunikat celu.

**Encounter:** Mała fala zainfekowanych cywilów i pojedynczych żołnierzy Grom Division. Walka powinna uczyć rytmu hordy, zbierania amunicji oraz poruszania się po zalanym terenie.

**Reward / unlock:** Podstawowa broń, pierwsze ammo pickupy, opcjonalnie otwarcie przejścia przez barykadę lub zalaną drogę.

**Checkpoint:** `Checkpoint_Start_Boat` jest aktywny od startu poziomu. `Checkpoint_DockExit` aktywuje się dopiero po ukończeniu początkowej fali i opuszczeniu nabrzeża, nie w trakcie walki.

**Respawn po śmierci:** Przed aktywacją `Checkpoint_DockExit` gracz wraca do `Checkpoint_Start_Boat`. Po aktywacji wyjścia z nabrzeża wraca do `Checkpoint_DockExit`.

**Reset po śmierci:** Jeśli śmierć nastąpi w encounterze startowym, aktywna fala zostaje wyczyszczona: żywi przeciwnicy z `Encounter_DockStart` są despawnowani, a encounter wraca do stanu gotowego do ponownego uruchomienia. Gracz odzyskuje pełne HP, zachowuje broń i amunicję z minimalnym progiem anty-softlockowym.

**Uwagi do rozmieszczenia w Unity:** Start gracza ustawić na czytelnym punkcie przy statku z widokiem na dalszą drogę. Trigger fali powinien znajdować się po kilku metrach od spawnu, aby gracz miał czas odczytać kierunek. Skrzynie Grom Division mogą prowadzić linią nawigacyjną do wyjścia z nabrzeża.

## Etap 2: Kościół i cmentarz

**Objective text:** Przeszukaj teren kościoła i znajdź przejście w stronę zabudowań.

**Trigger:** Wejście przez bramę cmentarza lub przekroczenie progu opuszczonego kościoła aktywuje encounter.

**Encounter:** Średnia fala zombie pojawiająca się z cmentarza, bocznych alejek i wnętrza kościoła. Wrogowie mogą wychodzić zza nagrobków i z ciemnych wejść, tworząc presję z kilku stron.

**Reward / unlock:** Nowa broń lub większy zapas amunicji, checkpoint, otwarcie przejścia do domków, budynków i szklarni.

**Checkpoint:** Etap zaczyna się najczęściej z aktywnym `Checkpoint_DockExit`. `Checkpoint_ChurchExit` aktywuje się po oczyszczeniu kościoła i cmentarza oraz po odblokowaniu drogi do zabudowań, nie podczas aktywnej fali.

**Respawn po śmierci:** Przed ukończeniem encountera kościoła gracz wraca do `Checkpoint_DockExit`. Po aktywacji checkpointu końcowego wraca do `Checkpoint_ChurchExit`.

**Reset po śmierci:** Aktywny `Encounter_ChurchCemetery` zostaje zatrzymany, żywi przeciwnicy z tej fali są usuwani, a encounter resetuje liczniki spawnów i stan ukończenia. Pickupy zebrane przed śmiercią nie muszą wracać na MVP.

**Uwagi do rozmieszczenia w Unity:** Kościół powinien być dominantą wizualną i orientacyjną. Cmentarz może działać jako arena z osłonami niskiej wysokości. Spawn pointy ukryć za nagrobkami, ścianami i w wejściach, unikając pojawiania się przeciwników bezpośrednio przed kamerą.

## Etap 3: Kompleks Black Orchid / szklarnie

**Objective text:** Wejdź do kompleksu Black Orchid i odszukaj źródło skażenia.

**Trigger:** Gracz przekracza bramę obozu Black Orchid, wchodzi do szklarni albo aktywuje punkt śledztwa przy kontenerze badawczym.

**Encounter:** Większa walka z mieszanką zombie personelu badawczego, zainfekowanych żołnierzy oraz pierwszym mocnym wejściem MutantStalkera. Encounter powinien wymuszać ruch między budynkami, alejkami szklarni i otwartymi fragmentami obozu.

**Reward / unlock:** Dostęp do finałowej ścieżki, mocniejsza broń lub większe zasoby, fabularna informacja o UFO jako prawdziwym źródle infekcji.

**Checkpoint:** Etap startuje z `Checkpoint_ChurchExit`. `Checkpoint_BlackOrchidExit` aktywuje się po ukończeniu głównego encountera kompleksu, rozwiązaniu presji MutantStalkera i otwarciu ścieżki do finałowej lokacji.

**Respawn po śmierci:** Podczas walki w kompleksie gracz wraca do `Checkpoint_ChurchExit`. Po pełnym ukończeniu etapu i aktywacji wyjścia wraca do `Checkpoint_BlackOrchidExit`.

**Reset po śmierci:** `Encounter_BlackOrchidCamp` resetuje aktualną falę, usuwa żywe zombie personelu, żołnierzy i aktywnego MutantStalkera powiązanego z tym encounterem. Bronie gracza zostają zachowane, a amunicja jest uzupełniana tylko do minimalnego progu, jeśli spadła poniżej niego.

**Uwagi do rozmieszczenia w Unity:** Kompleks powinien mieć wyraźne znaki Black Orchid, kontenery, namioty, szklarniowe tunele i ślady eksperymentów. MutantStalker potrzebuje przestrzeni do pościgu, więc główna arena powinna mieć pętle ruchu, boczne przejścia i kilka przeszkód zamiast jednego wąskiego korytarza.

## Etap 4: Finalna lokacja UFO

**Objective text:** Dotrzyj do źródła sygnału i zabezpiecz finałową lokację.

**Trigger:** Gracz wychodzi na finałową wyspę albo wchodzi do budynku pod UFO. Aktywuje się sygnał, światło nad lokacją i ostatnia fala przeciwników.

**Encounter:** Kulminacyjna horda z kilku kierunków, opcjonalnie z powrotem MutantStalkera jako presją końcową. Tempo powinno być intensywne, ale czytelne: gracz ma zrozumieć, że dociera do centrum anomalii.

**Reward / unlock:** Zakończenie walki, aktywacja sekwencji UFO, ostatni checkpoint przed abdukcją.

**Checkpoint:** Etap zaczyna się z `Checkpoint_BlackOrchidExit`. `Checkpoint_UFOFinal` aktywuje się po dotarciu do finałowej areny i przed rozpoczęciem właściwego finałowego encountera albo bezpośrednio po zabezpieczeniu wejścia do areny. Nie powinien aktywować się w środku ostatniej hordy.

**Respawn po śmierci:** Przed aktywacją `Checkpoint_UFOFinal` gracz wraca do `Checkpoint_BlackOrchidExit`. Po aktywacji finałowego checkpointu i przed beamem wraca do `Checkpoint_UFOFinal`.

**Reset po śmierci:** `Encounter_UFOFinalHorde` zostaje wyczyszczony z żywych przeciwników i przygotowany do ponownego startu. Jeśli MutantStalker wraca w finale, jego instancja także jest despawnowana i resetowana razem z encounterem.

**Uwagi do rozmieszczenia w Unity:** UFO powinno być widoczne nad lokacją albo stopniowo odsłaniane po zakończeniu walki. Arena finałowa powinna mieć centralny punkt dla beamu, miejsca na spawn hordy poza polem widzenia i bezpieczny obszar, w którym gracz zostanie zatrzymany przez sekwencję końcową.

## Etap 5: Abdukcja

**Objective text:** Przetrwaj kontakt z obiektem.

**Trigger:** Po pokonaniu finałowej fali gracz wchodzi w wyznaczoną strefę pod UFO lub skryptowana sekwencja aktywuje beam po krótkim opóźnieniu.

**Encounter:** Brak pełnej walki albo krótka kontrolowana presja ostatnich zombie. Głównym wydarzeniem jest utrata kontroli: beam blokuje gracza, dźwięk narasta, obraz przechodzi w światło UFO.

**Reward / unlock:** Koniec vertical slice, ekran podsumowania lub przejście do czarnego ekranu z komunikatem kontynuacji.

**Checkpoint:** Sekwencja korzysta z `Checkpoint_UFOFinal` jako ostatniego normalnego checkpointu przed beamem. Po aktywacji beamu nie ustawiamy nowego checkpointu fail-state, bo sekwencja ma zakończyć vertical slice.

**Respawn po śmierci:** Przed rozpoczęciem beamu gracz wraca do `Checkpoint_UFOFinal`. Po rozpoczęciu sekwencji abdukcji normalna śmierć jest blokowana i nie uruchamia standardowego respawnu.

**Reset po śmierci:** Przed beamem reset dotyczy tylko ewentualnego aktywnego finałowego encountera. Po aktywacji beamu nie resetujemy encountera jako fail state; sekwencja UFO przejmuje flow, blokuje śmierć i prowadzi do zakończenia.

**Uwagi do rozmieszczenia w Unity:** Punkt abdukcji oznaczyć niewidzialnym triggerem w centrum finałowej lokacji. Sekwencja powinna blokować ruch i strzelanie dopiero po jasnym sygnale wizualnym. Kamera może delikatnie skierować się ku UFO, a potem przejść w fade out.
