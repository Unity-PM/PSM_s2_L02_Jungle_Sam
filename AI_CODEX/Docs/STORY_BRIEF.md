# Jungle Sam - Story Brief

## Setup fabularny

Vertical slice gry rozgrywa się w zalanej, odciętej strefie tropikalnej, gdzie wojskowo-biologiczny eksperyment wymknął się spod kontroli. Teren obejmuje nabrzeże, wrak lub statek ewakuacyjny, zalane drogi, opuszczony kościół z cmentarzem, rozproszone domki, szklarnie oraz obóz badawczy Black Orchid.

Gracz trafia do strefy po załamaniu operacji zabezpieczającej. Komunikacja jest szczątkowa, ślady walki są świeże, a w kolejnych lokacjach widać, że infekcja rozeszła się szybciej, niż zakładali wojskowi i naukowcy.

## Black Orchid

Black Orchid to tajna frakcja badawczo-militarna odpowiedzialna za eksperymenty biologiczne prowadzone w zalanej strefie. Oficjalnie ich placówka miała badać skażenie środowiskowe i anomalie biologiczne. W praktyce Black Orchid próbowała przekształcić odkryte źródło infekcji w technologię kontroli biologicznej oraz broń terenową.

Ich obóz i szklarnie są pełne śladów pośpiesznej ewakuacji: dokumenty, kontenery badawcze, porzucony sprzęt laboratoryjny, prowizoryczne blokady i ciała personelu. Black Orchid wiedziała o powiązaniu infekcji z UFO, ale zataiła skalę ryzyka.

## Grom Division

Grom Division to wojskowa jednostka wysłana do zabezpieczenia strefy, ochrony personelu Black Orchid i utrzymania kordonu. Początkowo traktowali sytuację jako kryzys biologiczny, później jako operację bojową przeciwko zainfekowanym.

Ich obecność powinna być widoczna w lokacjach przez checkpointy, skrzynie z amunicją, barykady, ślady ewakuacji oraz poległych żołnierzy. Część przeciwników zombie to byli żołnierze Grom Division.

## Infekcja

Infekcja pochodzi ze źródła powiązanego z UFO. Nie jest zwykłą chorobą: reaguje na tkanki, wodę, lokalną roślinność i sygnały emitowane przez obiekt nad strefą. Zalanie terenu przyspieszyło rozprzestrzenianie, przenosząc materiał biologiczny przez drogi, kanały i nabrzeże.

Objawy infekcji obejmują agresję, degradację świadomości, deformacje ciała oraz przyspieszoną mutację. W pobliżu finałowej lokacji efekty są silniejsze: więcej narośli, nietypowe oświetlenie, zakłócenia dźwięku i zwiększona liczba agresywnych jednostek.

## UFO

UFO jest ukrytym źródłem całego kryzysu i finałowym elementem vertical slice. Obiekt nie musi być od początku w pełni widoczny, ale jego obecność powinna być sugerowana przez światło, dźwięk, anomalie biologiczne, ślady po eksperymentach Black Orchid oraz zachowanie zainfekowanych.

W finale UFO pojawia się nad wyspą lub budynkiem, blokuje gracza wiązką energii i rozpoczyna abdukcję. To zakończenie ma działać jako cliffhanger i zapowiedź pełnej gry.

## Zombie

Zombie to zainfekowany personel Black Orchid, żołnierze Grom Division oraz cywile lub pracownicy ze strefy. Nie są magicznymi nieumarłymi, lecz ofiarami biologiczno-obcego skażenia. Ich wygląd może zależeć od pochodzenia:

- personel badawczy: kombinezony, identyfikatory, elementy laboratoryjne,
- żołnierze: fragmenty opancerzenia, hełmy, oporządzenie,
- ludzie ze strefy: ubrania cywilne, robocze lub tropikalne.

Ich rola w vertical slice to presja hordy, budowanie skali katastrofy i prowadzenie gracza przez kolejne areny walki.

## MutantStalker

MutantStalker to silniejsza mutacja albo wynik eksperymentu bojowego Black Orchid. Może być dawnym żołnierzem lub testowym nosicielem poddanym mocniejszej ekspozycji na materiał z UFO. W przeciwieństwie do zwykłych zombie powinien sprawiać wrażenie inteligentniejszego, szybszego i bardziej celowego.

MutantStalker pełni rolę mini-bossa oraz sygnału, że Black Orchid nie tylko badała infekcję, ale próbowała ją wykorzystać militarnie. Jego pojawienie się najlepiej powiązać z kompleksem Black Orchid, szklarniami albo dojściem do finałowej lokacji.

## Cel gracza

Celem gracza jest przedarcie się przez zalaną strefę, przeżycie kolejnych fal zainfekowanych, odnalezienie źródła anomalii i dotarcie do finałowego punktu ewakuacji lub centrum sygnału. Po drodze gracz odkrywa, że kryzys nie jest tylko efektem nieudanego eksperymentu biologicznego, ale kontaktem z obcą technologią.

## Śmierć gracza i checkpointy

Śmierć gracza w vertical slice jest traktowana jako lokalne cofnięcie do ostatniego bezpiecznego punktu operacji, a nie jako pełny restart sceny. Na MVP gracz po śmierci respawnuje się w tej samej scenie przy ostatnim aktywnym checkpoincie. Dzięki temu tempo horde shootera zostaje utrzymane, a testowanie etapów jest szybsze.

Checkpointy reprezentują zabezpieczone punkty marszruty Grom Division albo obszary oczyszczone przez gracza. Powinny aktywować się po ukończeniu ważnych etapów, nigdy w środku aktywnej walki. Proponowane checkpointy fabularno-progresyjne:

- `Checkpoint_Start_Boat`
- `Checkpoint_DockExit`
- `Checkpoint_ChurchExit`
- `Checkpoint_BlackOrchidExit`
- `Checkpoint_UFOFinal`

Po respawnie gracz odzyskuje pełne HP. Bronie zostają zachowane, a amunicja również zostaje zachowana, z wyjątkiem sytuacji softlockowej: jeśli zapas amunicji spadł poniżej minimalnego progu, system uzupełnia go do minimalnej wartości. Pickupy na MVP nie muszą się resetować.

Aktywny encounter powinien zostać zresetowany po śmierci. Żywi przeciwnicy z aktualnego encountera zostają usunięci albo despawnowani, a walka uruchamia się ponownie po wejściu gracza w odpowiedni flow etapu.

## Zakończenie vertical slice

W finałowej lokacji gracz dociera na wyspę albo do budynku pod UFO. Po ostatnim starciu z hordą i/lub MutantStalkerem obiekt aktywuje beam. Sterowanie zostaje ograniczone lub zablokowane, otoczenie rozświetla się obcym światłem, dźwięk narasta, a gracz zostaje uniesiony i porwany.

Po aktywacji beamu UFO śmierć nie działa już jako normalny fail state. Sekwencja przejmuje kontrolę, blokuje standardowy loop walki i prowadzi do zakończenia vertical slice.

Vertical slice kończy się w momencie abdukcji, bez wyjaśnienia dalszych losów bohatera.
