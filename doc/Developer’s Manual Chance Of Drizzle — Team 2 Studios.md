## 1. Overview
**Project Summary:** Chance of Drizzle is a fast-paced third-person roguelike action shooter game.  Battle through changing maps, difficult enemies and enjoy exploring new powerups and hazards.  Each game will introduce new challenges and play styles required to win and make it to the escape pod.  
**Project goals:** Create a fully playable gameplay loop including
* Procedural enemy waves and item drops
* a progression mechanic to increase difficult over time
* prefab driven level generation for a new feel each play.  
## 2. Environment setup (exact, step-by-step)
1. **Editor & version**: Unity 6.000.2.6f2 LTS (Most recent at time of creation).  
2. **Install Unity Hub**: Install and open Hub.  
	https://public-cdn.cloud.unity3d.com/hub/prod/UnityHubSetup.exe
3. **Install Unity Editor**: Use Hub → Installs → select/install Unity 6.000.2.6f2 LTS
4. **Clone project**:
```bash
mkdir <project-folder>
git clone https://github.com/sashasagebd/chance-of-drizzle.git
cd <project-folder>
```
Then open with Unity Hub.  
5. **Build instructions**: Current version includes multiple scenes, not yet integrated together.   
6. **Common fixes**: Mismatched version to an earlier 6.000 version.
## 3. Repo structure (high level)
- `/Assets` - Unity assets and scripts individual folders as well
- `/Assets/Scripts` - C# source code needed by others
-  /Assets/Scenes  - Individual scenes as complete
- `/ProjectSettings` - project config
- `/Docs` - diagrams/manuals/other deliverables
-  `/Docs/<individualusers>` - individual uploads
## 4. Context diagram & class overview
Sebastian - [Class Diagram](https://github.com/sashasagebd/chance-of-drizzle/blob/1cd7d90e1114e309d6e07fc25e1aa0f07fc1d95e/doc/individual/Kynan/Class%20Diagram.png) ----- [Code Example](https://github.com/sashasagebd/chance-of-drizzle/blob/1cd7d90e1114e309d6e07fc25e1aa0f07fc1d95e/Assets/Scripts/Player/Health.cs)
Alija [Class Diagram](https://github.com/sashasagebd/chance-of-drizzle/blob/1cd7d90e1114e309d6e07fc25e1aa0f07fc1d95e/doc/individual/Alija/ClassDiagram.drawio.html) ------------ [Code Example](https://github.com/sashasagebd/chance-of-drizzle.git)
Erik [Class Diagram](https://github.com/sashasagebd/chance-of-drizzle/blob/1cd7d90e1114e309d6e07fc25e1aa0f07fc1d95e/doc/individual/Erik/classdiagram.png) ------------- [Code Example](https://github.com/sashasagebd/chance-of-drizzle.git)
Kynan [Class Diagram](https://github.com/sashasagebd/chance-of-drizzle/blob/1cd7d90e1114e309d6e07fc25e1aa0f07fc1d95e/doc/individual/Kynan/Class%20Diagram.png) ---------- [Code Example](https://github.com/sashasagebd/chance-of-drizzle/blob/1cd7d90e1114e309d6e07fc25e1aa0f07fc1d95e/Assets/Scripts/Enemies_TL5/EnemyHub.cs)
Owen [Class Diagram](https://github.com/sashasagebd/chance-of-drizzle/blob/1cd7d90e1114e309d6e07fc25e1aa0f07fc1d95e/doc/individual/Owen/ClassDiagramReal.PNG) ---------- [Code Example](https://github.com/sashasagebd/chance-of-drizzle/blob/1cd7d90e1114e309d6e07fc25e1aa0f07fc1d95e/Assets/Scripts/HudMenus/MenuController.cs)
Sasha [Class Diagram](https://github.com/sashasagebd/chance-of-drizzle/blob/1cd7d90e1114e309d6e07fc25e1aa0f07fc1d95e/doc/individual/Sasha/SB_Class.png) ---------- [Code Example](https://github.com/sashasagebd/chance-of-drizzle.git)
## 5. oral exam tasks
### Creating a Prefab
1. Build GameObject in Hierarchy including any components 
2. Drag from Hierarchy to `Assets/Prefabs` or individual assets folder.  
3. Use prefab in script objects to keep settings previously made.

### Design Patterns GRASP
- A link to where to find patterns can be found on canvas 3383 page modules > Patterns GRASP and GOF - Weeks 11-12 "ish" > Patterns
- Provides tools for designing software following standardized established object oriented programming principals. 

GRASP is a set of nine principles object oriented design first written in Craig Larmans book in 1997
	* Controller - Handle system events or coordination workflow
	* Creator - Let a class create objects it requires 
	* Indirection - uses an intermediate object to reduce coupling
	* Information expert - Assign responsibility to the class with required information
	* low coupling - Minimizes dependencies between classes
	* high cohesion - Keep class responsibilities as main focus and related
	* polymorphism - Assign behavior variations to different types
	* protected variations - Protects stable sections of the system from changes in works in progress
	* pure fabrication - Create helper classes to to get better design principles 

### Exam Checklist (latest)
- Demo run, prefab creation, show patterns, code walkthrough.

## 6. Coding standards
[Coding Standard PDF found on github](https://github.com/sashasagebd/chance-of-drizzle/blob/61a5bb582f37edbacf60a34209b1adb2b80452b6/doc/CodingStandardsV1.1.pdf)
## 7. Build & release notes
major changes here

## 8. Contacts
Erik TL3
Alija TL6
Sasha TL1
Owen TL4
Sebastian TL2
Kynan TL5

