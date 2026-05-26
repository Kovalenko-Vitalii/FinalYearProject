# Final Year Project – Survival Exploration Game

A final year software development project built in **Unity** using **C#**.  
This project focuses on designing and implementing a modular gameplay architecture for a survival / exploration game, including player systems, inventory, save/load, quests, status effects, UI, and AI.

## Overview

The project was developed as part of my BSc (Hons) in Software Systems Development.  
Its main goal was not only to build a playable game prototype, but also to apply software engineering principles such as modular design, event-driven systems, state management, and reusable game systems.

## Main Features

- Additive scene loading with a persistent core scene
- Central gameplay orchestration and game state flow
- Save / load system with slot-based persistence
- Inventory and equipment system
- Item interactions, world containers, and pickups
- Quest and note system with progression tracking
- Status effects and player condition simulation
- Player stats: health, hunger, hydration, energy, stamina, temperature
- AI system with state machine behaviour
- Modal UI system for gameplay, inventory, quests, saves, and settings

## Tech Stack

- **Language:** C#
- **Engine:** Unity
- **Version Control:** Git / GitHub

## Architecture Highlights

The project is structured around several core systems:

- **GameplayOrchestrator** – controls high-level game flow
- **SceneLoader** – handles additive scene loading and scene transitions
- **SaveManager / SaveRegistry** – manages save slots and world state restoration
- **InventoryManager** – handles inventory, equipment, and held items
- **QuestManager** – event-driven quest progression system
- **StatusEffectManager** – runtime effect management and stat modifiers
- **PlayerStatManager** – player vitals, resistances, stamina, and temperature logic
- **AIManager / MonsterBrain** – AI lifecycle and state machine logic
- **CanvasSwitcher / ModalStack** – UI screen management

## Software Engineering Focus

This project was used to practise and demonstrate:

- Object-oriented programming
- Separation of responsibilities
- Event-driven architecture
- Save / load serialization
- State machines
- Reusable and extensible gameplay systems
- Unity-based system design

## Repository Purpose

This repository is intended to showcase my final year project work and my practical experience with:

- C# application logic
- Unity gameplay programming
- system architecture
- debugging and iteration during development

## How to Run

1. Clone this repository
2. Open the project in Unity
3. Use the Unity version specified in `ProjectSettings/ProjectVersion.txt`
4. Load the main project scene and run the game in the editor

## Author

**Vitalii Kovalenko**  
Junior Software Developer / Graduate Software Engineer  
Email: vitkovolenko@gmail.com
