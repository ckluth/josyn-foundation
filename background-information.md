# About

## Der größere Kontext

- wir befinden uns in der frühen design- und entwicklungs-phase eines systems namens JOSYN. JOSYN steht für "JobSystem Next"
- wir befinden uns kurz vor der ziellinie des ersten milestones. 

## goal: erster milestone

- der erste milestone ist ein reifer PoC, der zum ersten mal kollegen vorgestellt und dikutiert werden kann.
- der gesamte PoC beinhaltet weitere repos, die hier nicht zur diskussion stehen, das dieses repo hier autark und agnostisch gegenüber dem system ist.

## abschluss-kriterien

- alle drei "logischen" unter-repos haben ein gleichförmigkeit in der struktur.
- alle drei könnten als eigenständiges repo weiterbetrieben werden.
- jedes "unter-repo" hat eine eigenständige gute autarke README, die die Nutzung und Funktionalität beschreibt
- das mutil-repo hat eine README, die den größeren Zusammenhang beschreibt und die für den Show-Case der ersten Vorstellung des PoC gedacht ist.

## content des repos

- dieses repo ist ein multi-repo mit drei "logischen"unter-repos, die die drei kern-buildingblocks von JOSYN darstellen.
- die drei unter-repos sind die drei kern-buildingblocks von JOSYN
- jedes unter-repo erzeugt ein NuGet-Paket; für den Poc in einem Folder ""..\..\local packages""

1. JOSYN.Foundation.ResultPattern

- keine abhängigkeiten zu anderen buildingblocks
- die kern-komponente des systems.
- implementiert das result pattern, das durchgängige design- und entwicklungs-prinzip von JOSYN.

2. JOSYN.Foundation.PropertyBag

- eine eher leichtgewichtiger mit helper-charakter
- dient der serialisierung/deserialisierung im kontext der systems.
- wird innerhalb dieses repos nicht referenziert.
- referneziert JOSYN.Foundation.ResultPattern

3. JOSYN.Foundation.JIP

- die schwergewichtigste und kritischste komponente.
- implementiert ein ipc-protokoll namens JIP: JOSYN Inter Process Communication Protocol.
- wird innerhalb dieses repos nicht referenziert.
- referenziert JOSYN.Foundation.ResultPattern

## additional information (not really relevant)

- JIP und PorpertyBag werden im system von einer Applications-Protokoll-Implementierung namens JAP (Josyn Application Protocoll) genutzt.




