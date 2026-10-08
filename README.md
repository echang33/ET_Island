# Killswitch: The Dawn of AI

## Team Members

Ethan Chang  -- [echang33](https://example.com)  
Ryan Wu -- [wucru365](https://github.com/wucru365)  
Steven Tan -- [StevenTan123](https://github.com/StevenTan123)

## Game Summary

In *Killswitch*, you play as a disgruntled computer scientist named Chi Poh-Le who was laid off from his SWE job over a decade ago due to AI. In fact, the entire world population is effectively unemployed, and AI controls the world. The main culprit is Epsilon Tau, the supreme AGI system that is currently expanding its network by building data centers on remote islands in the Pacific Ocean. Deciding enough is enough, you (Chi Poh-Le) travel to the Pacific with the goal of stopping Epsilon Tau. *Years of research has revealed that one data center, in particular, holds the emergency killswitch to shut down Epsilon Tau.* You must infiltrate the data centers, reach the central control room, and pull the kill switch. But the task will not be easy. Epsilon Tau has constructed the data centers to be a complex labyrinth of fortified rooms with traps, difficult terrain, and many safeguards to prevent human infiltrators.

The gameplay mixes platforming, stealth, and puzzle-solving as you navigate the island's terrain and high-tech facilities. You will use parkour to maneuver around obstacles and avoid robotic security guards patrolling the area. Along the way, you must hack into restricted areas by solving math problems and puzzles at security doors. These act as “reverse captchas”; you must prove that you are not-human by solving them fast in order to proceed.


## Genres

Puzzle and Adventure game 

## Inspiration

### [Jailbreak](https://www.roblox.com/games/606849621/Jailbreak)


Jailbreak is a popular Roblox open-world cops-and-robbers game where players choose to be criminals or police officers. Criminals escape prison, rob locations like banks and stores, and evade capture, while police try to stop and arrest them. The robbing part is extremely similar to what we are thinking. Often these require a series of tasks to be completed, involving puzzles, and dodging lasers to get to a vault or some objective, where you then have to escape with the money. 


![JailBreak](https://entertainment-focus.com/wp-content/uploads/2020/06/Roblox_jailbreak_press_image_2250x1200.jpg)

### [Squid Game - Netflix](https://www.roblox.com/games/606849621/Jailbreak)

One of the subplots of the TV show Squid Game was Detective Hwang Jun-ho’s attempts to infiltrate the secret island where games took place. In season one, he successfully does so by sneaking onto a ferry, neutralizing a guard, stealing their outfit, and assuming their identity to blend in. While Jun-ho’s objectives were different, we took inspiration from the tension of infiltrating a secure island facility and hiding in plain sight. Rather than wearing a physical disguise to blend in, our protagonist must disguise their human-ness by solving timed math and logic puzzles at security checkpoints. At the end of season 3, Jun-ho destroys the island, just as our protagonist will aim to take down the data center island.

![Jun-ho](https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSg6klfg3Dgfnabe6uLqHyHLUEgLqcZndJyykhFeQHYuiX2CKPuK05Sfqw&s=10)

### [Current Events](https://www.datacenterdynamics.com/en/news/terrorism-elon-musk-xai-memphis-data-center/)

There has been a rise in the number of people opposing AI and datacenters across the world over the past two years. Some focus on the environmental and local effects of data centers, including electricity and water use, noise, land development, and the possibility that residents may bear higher infrastructure costs. Others are concerned about AI replacing workers, using copyrighted material without consent, increasing surveillance, or concentrating power in a small number of technology companies. Our game simulates an extreme situation where AI has gone rogue and needs to be taken down using extreme measures. 


## Gameplay

Solve puzzles inside the datacenter to prove that the player is AI. These puzzles include:
Arithmetic
- Factorizing numbers (inspired by RSA encryption)
- Manually solving small instances of NP-hard problems, such as: 
    - Finding the maximum clique in a graph
    - Finding the minimum vertex cover
    - Finding a hamiltonian path in a graph
When these puzzles are reached, a new screen will pop up which the user can type into to solve them.
- Navigate through rooms in the datacenter, solving the puzzles mentioned above along the way to unlock new areas to explore.

Some rooms will have parkour aspects that will require careful maneuvering from the player.
    - The rooms may have moving platforms, lasers, and cameras that you must evade.
    - We plan on implementing jumping (spacebar), crouching (shift), and directional movement (WASD). 

## Development Plan

### Project Checkpoint 1-2: Basic Mechanics and Scripting (Ch 5-9)

For the first checkpoint, we plan on implementing all of the movement controls (WASD, jumping, crouching). We will manually build one data center room for testing that contains some parkour aspects as well. 

We will also do research on how to procedurally generate the data center labyrinth. This part might be relatively involved if we want to have a large variety of possible rooms inside the labyrinth. The vision is that the rooms may be of different sizes, and may connect to each other on any face. A general way we may achieve this generation is to manually create some template rooms, and define a grammar with production rules (like a CFG but for graphics) that define which kinds of rooms may generate beside each other.

For the puzzles, we plan on implementing the arithmetic and factorization ones since those are the simpler ones. These puzzles will pop up on a new screen, so they are not directly integrated inside the 3D game world (although they will be activated by events from the game world). 

TODO:
* ~~Basic movement (WASD, jumping)~~
* ~~One example room/level~~
* ~~One obstacle (laser beams, deals damage to player)~~
* ~~One puzzle screen~~
* ~~Interaction that calls and displays the puzzle screen~~
* ~~One enemy (laser turret)~~

### Additions:
Not applicable

### Project Checkpoint 3-4: 3D Scenes and Models (Ch 3+4, 10)

For the next checkpoint, we plan on creating additional rooms and environmental assets, so that we can then use those building blocks to generate larger data center levels that combine multiple rooms, obstacles, and puzzles. We may experiment with basic procedural generation techniques (if time permits).

We also plan on smoothing out movement mechanics. Currently the player can jump even while in the air, so that will need to be fixed. Some of the movement and gravity also don't feel as smooth as we would like to, so we plan on refining those controls. 

We also plan on implementing more types of puzzles. Since we already have a graph UI, this will be easy to expand. For example, we can add finding Hamiltonian paths and minimum vertex cover. We also plan to connect puzzles more directly to progression through the data center. Successfully solving puzzles will allow the player to access previously restricted areas.

## Development PLan

### Project Checkpoint 1-2:
* Movement: We have implemented basic movement (WASD, jumping mechanics). In the future, crouching can also be implemented, but that can be for later on.
* Obstacles: One obstacle, static laser beams, has been implemented. Implemented collider logic where touching a laser beam deals damage to players. It is currently set to deal 100% of player damage, meaning touching a laser beam will kill you. For now, "dying" just respawns you at the beginning of the level, but can be changed to respawn at a certain checkpoint later on.
* One example level was created. It involves a hallway guarded by laser beams, leading to a larger room with enemies. 
* One stationary enemy, a laser turret, was created. It shoots laser projectiles at the player, which also deal damage and cause the player to die. Laser damage is also adjustable in the inspector.
* Puzzle system: We implemented a puzzle UI that appears when the player gets near a puzzle location in the 3D environment (currently it is a green cube at the end of the test level). The puzzle UI pauses the 3D gameplay, and prompts the user with a puzzle and textbox waiting for the user's answer.
* Graph puzzle: We implemented a graph generation and display system for graph-based puzzles. Graphs are randomly generated with a configurable number of vertices and edge density and displayed in the puzzle UI. Currently the puzzle asks the user to find a maximum clique. Once the user enters their answer, it is parsed and programmatically checked. If the user enters the wrong answer, it kills the player. If the user answers correctly, they exit the puzzle screen (but nothing more happens for now).

![Test Level](./Screenshots/TestLevel.png)
![Death Popup](./Screenshots/TestDeathScene.png)
![Turret](./Screenshots/Turret.png)
![GraphPuzzle](./Screenshots/game_clique_puzzle.png)
