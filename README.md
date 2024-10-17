# ParrySoulLike
 
## 🔴 About This Project
<p align="justify">CyberShield was originally developed as a submission for the Indie Game Ignite (IGI) Showcase and the GameToday Indie Game Competition, but we decided to expand the game even after the competition to further explore its potential. </p>

<br>

## 📋 Project Info

| **Role** | **Team Size** | **Development Time** | **Engine** |
|:-|:-|:-|:-|
| Game Programmer | 3 | 2 weeks | Unity 2022|

<br>

## 👤 Meet the Team
- Michael Ardisa (Programmer)
- Steven Putra Adicandra (Designer)
- Duns Scotus Aerotri Tunyanan (3D Artist)

<br>

## ♦️About Game
<p align="justify">CyberShield is an isometric top-down action RPG where players can battle a variety of cyber threats like malware, trojans, and other digital dangers in the computer world. The game introduces a unique gameplay experience by fusing cybersecurity concepts with fast-paced action mechanics.</p>

<br>

## 🎮 Gameplay
<p align="justify">Players engage in intense combat, dodging and countering threats using weapons like machete, katana, and hammer. As they progress, they will face tougher enemies and adapt to new challenges. The game currently features 2 levels, and we plan to add more in future updates.</p>

<br>

## ⚙️ Game Mechanics I Created
<!-- ### Dash Mechanic
<p align="justify">The dash mechanic in this game works by increasing the player's velocity, allowing them to change direction mid-dash rather than being locked into a straight line. The visual impact of the dash is enhanced by a trail effect, created using the Trail Renderer component. To make the dash feel smoother, the trail time is gradually reduced through a coroutine when the dash ends, giving the trail a retracting effect.</p>

```
void Update()
{
    ...        
    // dash
    if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && !CD && activeScene.name != "MainMenu & Shop")
    {
        trail.enabled = true;
        trail.time = 0.5f;

        isDashing = true;
        moveSpeed = moveSpeed * dashSpeed;
        Invoke(nameof(dashReset), 0.2f);

        CD = true;
        Invoke(nameof(coolDown), dashCD + 0.2f);
    }
    ...
}

private void dashReset()
{
    moveSpeed = moveSpeed / dashSpeed;
    isDashing = false;
    StartCoroutine(trailReduce());
}

IEnumerator trailReduce()
{
    while (trail.time > 0)
    {
        trail.time = trail.time - 0.01f;
        yield return new WaitForSeconds(0.01f);
    }

    trail.time = 0f;
}
```

### Scriptable Objects Utilization for Weapon Data
<p align="justify">Scriptable objects here are used to store key weapon data within the 'Resources' folder, providing a flexible way to manage and modify weapon attributes. This approach makes adding new weapons efficient and straightforward — simply create a new weapon asset file and adjust its data as needed.</p>

```
[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class Weapon : ScriptableObject
{
    public string weaponName;
    public Vector3 atkPos;
    public float atkRange;
    public int atkDamage;
    public float atkRate;
    public float atkDelay;
}
```
-->

<br>

## 📜 Scripts

|  Script       | Description                                                  |
| ------------------- | ------------------------------------------------------------ |
| `Weapon.cs` | Scriptable object class used to determine which data needs to be stored. |
| `EBehaviour.cs`  | Responsible for how the enemies behave around the player. |
| `PMove.cs`  | Manages all isometric (skewed) player movements. |
| `ECombat.cs`  | Manages the logic behind the enemies' combat. |
| `PCombat.cs`  | Manages the logic behind the player's combat. |
| `etc`  |

<br>

## 🕹️ Controls

| **Key Binding** | **Function** |
|:-|:-|
| W, A, S, D | Basic movement |
| Left-Click | Attack |
| L-Shift | Dash |

<br>

## 💻 Setup

This game is still in beta, a playable version will be available soon!
