# Killswitch: The Dawn of AI

## Team Members

Ethan Chang  -- [echang33](https://example.com)  
Ryan Wu -- [wucru365](https://github.com/wucru365)  
Steven Tan -- [StevenTan123](https://github.com/StevenTan123)

## Game Summary

In *Killswitch*, you play as a disgruntled computer scientist named Chi Poh-Le who was laid off from his SWE job over a decade ago due to AI. In fact, the entire world population is effectively unemployed, and AI controls the world. The main culprit is Epsilon Tau, the supreme AGI system that is currently expanding its network by building data centers on remote islands in the Pacific Ocean. Deciding enough is enough, you (Chi Poh-Le) travel to the Pacific with the goal of stopping Epsilon Tau. You must infiltrate the data centers, reach the central control room, and pull the kill switch. But the task will not be easy. Epsilon Tau has constructed the data centers to be a complex labyrinth of fortified rooms with traps, difficult terrain, and many safeguards to prevent human infiltrators.

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
- Some rooms will have parkour aspects that will require careful maneuvering from the player.
    - The rooms may have moving platforms, lasers, and cameras that you must evade.
    - We plan on implementing jumping (spacebar), crouching (shift), and directional movement (WASD). 

## Development Plan

### Project Checkpoint 1-2: Basic Mechanics and Scripting (Ch 5-9)

For the first checkpoint, we plan on implementing all of the movement controls (WASD, jumping, crouching). We will manually build one data center room for testing that contains some parkour aspects as well. 

We will also do research on how to procedurally generate the data center labyrinth. This part might be relatively involved if we want to have a large variety of possible rooms inside the labyrinth. The vision is that the rooms may be of different sizes, and may connect to each other on any face. A general way we may achieve this generation is to manually create some template rooms, and define a grammar with production rules (like a CFG but for graphics) that define which kinds of rooms may generate beside each other.

For the puzzles, we plan on implementing the arithmetic and factorization ones since those are the simpler ones. These puzzles will pop up on a new screen, so they are not directly integrated inside the 3D game world (although they will be activated by events from the game world). 
