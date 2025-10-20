# Horror Game Player Controller - Setup Anleitung

Dieser umfassende 3D Player Controller im Stil von Phasmophobia bietet alle notwendigen Features für ein Horror-Spiel.

## Features

### Bewegung
- **WASD Bewegung** - Standardbewegung in alle Richtungen
- **Laufen/Rennen** - Halten Sie Shift zum Rennen (mit Ausdauersystem)
- **Ducken** - Drücken Sie Strg zum Ducken
- **Springen** - Leertaste zum Springen
- **Ausdauersystem** - Stamina wird beim Rennen verbraucht und regeneriert sich

### Kamera & Look
- **Maus-Look** - Freie Kamerabewegung mit der Maus
- **Head Bob** - Realistischer Kopfbob beim Gehen/Laufen
- **Angst-Effekte** - Kamera zittert bei hohem Angstlevel

### Interaktion
- **Interaktionssystem** - Drücken Sie E um mit Objekten zu interagieren
- **Reichweiten-basiert** - Objekte müssen in Reichweite sein
- **UI-Feedback** - Anzeige von Interaktions-Prompts

### Horror-Features
- **Taschenlampe** - Drücken Sie F zum Ein-/Ausschalten
- **Angst-System** - Fear Level beeinflusst Spieler-Verhalten
- **Horror-Audio** - Herzschlag, Ambient-Sounds, Scare-Effekte
- **Footsteps** - Verschiedene Footstep-Sounds je nach Untergrund

## Setup Anweisungen

### 1. Player GameObject Setup

1. Erstellen Sie ein leeres GameObject namens "Player"
2. Fügen Sie folgende Komponenten hinzu:
   - `CharacterController`
   - `AudioSource`
   - `Playercontroller` (das erstellte Script)
   - `PlayerInput` (optional, für Input System)

### 2. Character Controller Einstellungen

- **Height**: 2.0
- **Radius**: 0.5
- **Center**: (0, 1, 0)
- **Slope Limit**: 45
- **Step Offset**: 0.3

### 3. Kamera Setup

1. Erstellen Sie ein Child-GameObject namens "PlayerCamera"
2. Fügen Sie eine `Camera` Komponente hinzu
3. Position: (0, 1.6, 0) - etwa auf Augenhöhe
4. Weisen Sie diese Kamera dem `playerCamera` Feld im Playercontroller zu

### 4. Taschenlampe Setup

1. Erstellen Sie ein Child-GameObject der Kamera namens "Flashlight"
2. Fügen Sie eine `Light` Komponente hinzu:
   - **Type**: Spot
   - **Range**: 10
   - **Spot Angle**: 60
   - **Intensity**: 2
3. Position: (0.2, -0.1, 0.3)
4. Weisen Sie diese Light dem `flashlight` Feld zu

### 5. Input System Setup (Empfohlen)

#### Option A: Mit Input System Package
1. Installieren Sie das "Input System" Package über Package Manager
2. Erstellen Sie ein neues Input Action Asset:
   - Rechtsklick im Project → Create → Input Actions
3. Erstellen Sie eine Action Map namens "Player" mit folgenden Actions:
   - **Move** (Vector2) - WASD Binding
   - **Look** (Vector2) - Mouse Delta Binding  
   - **Jump** (Button) - Space Binding
   - **Run** (Button) - Left Shift Binding
   - **Crouch** (Button) - Left Ctrl Binding
   - **Flashlight** (Button) - F Binding
   - **Interact** (Button) - E Binding

#### Option B: Ohne Input System
Das Script funktiert auch ohne Input System und verwendet automatisch die Standard Unity Input Manager Eingaben.

### 6. UI Setup

1. Erstellen Sie ein Canvas für die Player UI
2. Fügen Sie das `PlayerUI` Script zum Canvas hinzu
3. Erstellen Sie folgende UI Elemente:
   - **Stamina Bar**: Image mit Fill Type "Filled"
   - **Crosshair**: Image in der Bildschirmmitte
   - **Interaction Text**: Text Element für Interaktions-Prompts
   - **Fear Overlay**: Image über den ganzen Bildschirm für Angst-Effekte

### 7. Audio Setup

1. Fügen Sie das `HorrorAudioManager` Script zu einem GameObject hinzu
2. Erstellen Sie drei AudioSource Komponenten:
   - Ambient Audio Source
   - Scare Audio Source  
   - Heartbeat Audio Source
3. Weisen Sie Audio Clips zu:
   - Footstep Sounds Array
   - Ambient Clips Array
   - Scare Clips Array
   - Heartbeat Clip

### 8. Interaktive Objekte erstellen

1. Erstellen Sie ein GameObject für ein interaktives Objekt
2. Fügen Sie das `InteractableObject` Script hinzu
3. Konfigurieren Sie die Interaction Settings
4. Optional: Verwenden Sie die `Door` oder `PickupItem` Beispiele

## Script-Abhängigkeiten

Stellen Sie sicher, dass alle Scripts im selben Projekt sind:
- `Playercontroller.cs` - Haupt-Controller
- `IInteractable.cs` - Interface für interaktive Objekte
- `InteractableObject.cs` - Basis-Klasse für Interaktionen
- `PlayerUI.cs` - UI Management
- `HorrorAudioManager.cs` - Audio-System
- `PlayerInputSetup.cs` - Input Setup Helper

## Anpassungen

### Movement Tuning
Passen Sie die Bewegungsgeschwindigkeiten im Inspector an:
- `walkSpeed`: Standard-Gehgeschwindigkeit
- `runSpeed`: Renn-Geschwindigkeit
- `crouchSpeed`: Geschwindigkeit beim Ducken

### Camera Sensitivity
- `mouseSensitivity`: Maus-Empfindlichkeit
- `upDownRange`: Vertikaler Look-Bereich

### Stamina System
- `maxStamina`: Maximale Ausdauer
- `staminaRegenRate`: Regenerations-Rate
- `staminaDrainRate`: Verbrauchs-Rate beim Rennen

### Fear System
- `maxFear`: Maximaler Angst-Level
- `fearDecayRate`: Wie schnell Angst abnimmt

## Tipps

1. **Performance**: Deaktivieren Sie Head Bob wenn nötig (`enableHeadBob = false`)
2. **Testing**: Verwenden Sie die Gizmos im Scene View zum Debuggen
3. **Audio**: Platzieren Sie verschiedene Footstep-Sounds für verschiedene Oberflächen
4. **Fear Effects**: Rufen Sie `AddFear(amount)` auf um Angst-Effekte auszulösen
5. **Customization**: Erweitern Sie `IInteractable` für eigene Interaktions-Typen

## Troubleshooting

**Probleme mit Input System?**
- Stellen Sie sicher, dass das Input System Package installiert ist
- Prüfen Sie ob die Action Map korrekt zugewiesen ist

**Kamera bewegt sich nicht?**
- Prüfen Sie ob Cursor.lockState auf Locked gesetzt ist
- Kontrollieren Sie die Mouse Sensitivity Werte

**Footsteps spielen nicht?**
- Weisen Sie Audio Clips dem footstepSounds Array zu
- Prüfen Sie ob eine AudioSource Komponente vorhanden ist

**Interaktionen funktionieren nicht?**
- Implementieren Sie das IInteractable Interface in Ihren Objekten
- Stellen Sie sicher, dass Objekte Collider haben