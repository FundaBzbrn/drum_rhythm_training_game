# Drum Rhythm Training Game

This project is a 2D drum rhythm training game developed by Funda Bozburun as part of the "Game Design and Programming" course in the Software Engineering Department at Istanbul Topkapi University.

## About The Project

"Drum Rhythm Training Game" is a Unity project aimed at helping players improve their rhythm skills, timing abilities, and reflexes in an entertaining way. Players attempt to follow rhythms by hitting the correct drums at the right time, either with keyboard keys (A, S, K, L) or the mouse, based on on-screen cues.

### Core Features

*   **3 Different Difficulty Levels:**
    *   **Level 1:** An introduction to the game with a slow tempo and wide timing windows.
    *   **Level 2:** Increased difficulty with a faster tempo and narrower timing windows.
    *   **Level 3:** High tempo, tight timing windows, and a **multi-hit (chord)** mechanic металл requiring players to hit two different drums simultaneously.
*   **Dynamic Scoring System:** Feedback and scoring based on hit accuracy, such as "Perfect," "Good," "OK," and "Miss."
*   **Combo System:** Consecutive successful hits (Perfect or Good) increase the combo counter, فيلم rewarding extra bonus points after a certain threshold.
*   **Keyboard and Mouse Support:** Players can play using either the keyboard (defaults: A, S, K, L) or the mouse.
*   **Sound Settings:** The volume of each drum (Kick, Snare, Hi-Hat, Tom) can be adjusted individually in the Settings menu, and these preferences are saved.
*   **Visual and Auditory Feedback:** Flame effects on successful hits, sound effects, and on-screen indicators for score and combo.
*   **Comprehensive Menu System:** Includes Main Menu, Help, Settings, Credits, and Asset Showroom screens.

## Development Environment

*   **Game Engine:** Unity 
*   **Programming Language:** C#
*   **Platform:** Windows

## Setup and Gameplay

1.  Run the `.exe` (for Windows) or the respective executable file located in the project's `Build` folder.
2.  Click the "BAŞLA" (START) button from the main menu to begin the game.
3.  **Controls:**
    *   **Mouse:** Click on the drum cues appearing on the screen.
    *   **Keyboard:**
        *   Kick Drum: `A`
        *   Snare Drum: `S`
        *   Hi-Hat: `K`
        *   Tom Drum: `L`
4.  The goal is to hit the correct drum(s) at the right time to achieve the highest score.

## Project Structure (Key Scripts)

*   **`GameManager.cs`**: Manages the core game logic, level progression, scoring, combos, input detection, and cue display.
*   **`DrumHitDetector.cs`**: Attached to each drum object, responsible for triggering sound/visual effects upon being hit and communicating with the `GameManager`.
*   **`MainMenuManager.cs`**: Handles the functionality of buttons in the main menu and scene transitions.
*   **`OptionsManager.cs`**: Manages sound settings in the Settings menu (utilizing AudioMixer and PlayerPrefs).
*   **`LevelData.cs`, `BeatEvent.cs`, `DrumKeyMapping.cs`**: Serializable classes used to configure game data such as level settings, rhythm events, and key mappings.

## Future Plans and Improvements (Future Work)

*   Introduction of new game mechanics (e.g., hold notes).
*   More diverse visual themes and drum skins.
*   Enhanced animations and hit effects.
*   A detailed tutorial section.
*   Player statistics and progress tracking.
*   Ability to customize key bindings in the Settings menu.
*   Adaptation for mobile platforms.
  
Acknowledgements...
