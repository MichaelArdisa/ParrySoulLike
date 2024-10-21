# Feline Parrymaster

<p align="center">
  <img src="https://github.com/user-attachments/assets/cbabf260-05d9-4b70-9d50-ddbdad679838" alt="Feline Parrymaster Gameplay" style="width: 600px;">
</p>

## 🔴 About This Project
<p align="justify">This project was developed as a way for me and my team to dive into the third-person action genre. Through this project, I gained valuable insights into the intricacies of game development, from crafting engaging combat mechanics to creating dynamic code for the purpose of futureproofing.</p>

<br>

## 📋 Project Info

<b> Developed with Unity 2022 </b>

| **Role** | **Name** | **Development Time** |
| - | - | - |
| Game Programmer | Michael Ardisa | 1 week |
| 3D Character Artist | Yohanes Duns Scotus Aerotri Tunyanan | 1 week |
| Game Designer | Steven Putra Adicandra | 1 week |

<details>
  <summary> <b>My Contribution (Game programmer)</b> </summary>
  
  - Core mechanics
  - Bug Fixing
  
</details>

<br>

## ♦️About Game
<p align="justify">Feline Parrymaster is a third-person action-adventure game set in a mystical world. You play as a legendary sword-wielding cat who battles fierce enemies like orcs, using unique parrying skills to deflect both melee and projectile attacks with a mix of fast-paced combat. The game challenges players to master the art of parrying and become a true feline swordmaster.</p>

<br>

## 🎮 Gameplay
<p align="justify">Feline Parrymaster is all about mastering two types of parries: projectile parry and close-range parry! Play as a sword-wielding cat, deflecting enemy attacks with precise timing to defeat fierce foes like orcs. The game challenges you to perfect your parry skills and counter with powerful strikes.</p>

<br>

## ⚙️ Game Mechanics I Created
### Projectile Parry
![parryProjectile](https://github.com/user-attachments/assets/6d80f566-8dda-4725-94a9-0cb04950b986)

- Logic is located within the `PCombat.cs` script
- It functions similarly to a normal attack but targets projectiles instead of enemies.
- When the player performs this action, the system looks for incoming projectiles in the area.
- Upon hitting a projectile, the force of the projectile is reversed.
- The projectile is then launched back at the enemy who originally fired it.

### Combo System

<p align="justify">
  <img src="https://github.com/user-attachments/assets/c4bdbab3-71fc-4c87-9a76-369f1f9d8fe0">
  <img src="https://github.com/user-attachments/assets/29b074ff-56c0-4045-b221-4428a2f9e90f" style="width: 40%;">
</p>

- Logic is located within the `PCombat.cs` script
- The code is dynamic, allowing for flexibility in the number of combos the player can perform.
- A list of classes called "Combos" is used to handle the combo system, with the length of the list determining the amount of moves available to the player.
- The system functions by starting a timer each time the player performs a normal attack.
- If the player doesn't attack within a certain time, the combo is reset to the first move.
- If the player continues attacking within the given time, the combo progresses to the next move in the list.
- Once the combo reaches the final move, the sequence resets back to the first move, allowing the player to start the combo over.
  
<br>

## 📜 Scripts

|  Script       | Description                                                  |
| - | - |
| `PCombat.cs`  | Manages the logic behind the player's combat (projectile parry, melee parry, combo system). |
| `PMove.cs`  | Manages all player movements (run, dodge, jump, idle). |
| `EBehaviour.cs`  | Responsible for how the enemies behave around the player. |
| `ECombat.cs`  | Manages the logic behind the enemies' combat. |
| `etc`  |

<br>

## 🕹️ Controls

| **Key Binding** | **Function** |
|:-|:-|
| W, A, S, D | Basic movement |
| Left-Click | Basic Attack |
| Right-Click | Projectile Parry |
| E | Close Range Parry |
| L-Shift | Dodge |

<br>

## 💻 Setup

This game is still in beta, a playable version will be available soon!
