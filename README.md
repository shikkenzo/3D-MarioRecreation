# 3D-MarioRecreation
Super Mario 64 recreation made in Unity for learning more about some of its mechanics.

## Video
[![Image](https://img.youtube.com/vi/oUViJ8xMhJc/0.jpg)](https://youtu.be/oUViJ8xMhJc) 

## Features
- Player Movement / Animator FSM:
   - Idle ![Image](https://github.com/shikkenzo/3D-MarioRecreation/blob/main/Resources/Idle.gif)
   - Walk ![Image](https://github.com/shikkenzo/3D-MarioRecreation/blob/main/Resources/Walk.gif)
   - Run ![Image](https://github.com/shikkenzo/3D-MarioRecreation/blob/main/Resources/Run.gif)
   - Single Jump ![Image](https://github.com/shikkenzo/3D-MarioRecreation/blob/main/Resources/SingleJump.gif)
   - Double Jump (sequence) ![Image](https://github.com/shikkenzo/3D-MarioRecreation/blob/main/Resources/DoubleJump.gif)
   - Triple Jump (sequence) ![Image](https://github.com/shikkenzo/3D-MarioRecreation/blob/main/Resources/TripleJump.gif)
   - Long Jump ![Image](https://github.com/shikkenzo/3D-MarioRecreation/blob/main/Resources/LongJump.gif)
   - Wall Jump ![Image](https://github.com/shikkenzo/3D-MarioRecreation/blob/main/Resources/WallJump.gif)
   - Punch 1 ![Image](https://github.com/shikkenzo/3D-MarioRecreation/blob/main/Resources/SinglePunch.gif)
   - Punch 2 (sequence) ![Image](https://github.com/shikkenzo/3D-MarioRecreation/blob/main/Resources/DoublePunchCombo.gif)
   - Punch 3 (sequence) ![Image](https://github.com/shikkenzo/3D-MarioRecreation/blob/main/Resources/PunchCombo.gif)
   - Hit ![Image](https://github.com/shikkenzo/3D-MarioRecreation/blob/main/Resources/Hit.gif)
   - Die ![Image](https://github.com/shikkenzo/3D-MarioRecreation/blob/main/Resources/Die%20(debug).gif)
- Special Idle: Mario will change his Idle animation after not inputing camera or player movement for 10 seconds ![Image](https://github.com/shikkenzo/3D-MarioRecreation/blob/main/Resources/SpecialIdle.gif)
- Camera:
   - Moves around the player based on mouse input ![Image](https://github.com/shikkenzo/3D-MarioRecreation/blob/main/Resources/CameraMovement.gif)
   - Avoids entering map geometry ![Image](https://github.com/shikkenzo/3D-MarioRecreation/blob/main/Resources/CameraCollision.gif)
   - Goes back to default position after not inputing for 5 seconds ![Image](https://github.com/shikkenzo/3D-MarioRecreation/blob/main/Resources/CameraReturn.gif)
- Platforms:
   - Moving Platform ![Image](https://github.com/shikkenzo/3D-MarioRecreation/blob/main/Resources/MovingPlatform.gif)
   - Physics bridge ![Image](https://github.com/shikkenzo/3D-MarioRecreation/blob/main/Resources/PhysicsBridge.gif)
- Coins ![Image](https://github.com/shikkenzo/3D-MarioRecreation/blob/main/Resources/Coins.gif)
- Stars (restore health) ![Image](https://github.com/shikkenzo/3D-MarioRecreation/blob/main/Resources/Star.gif) 
- Goomba 
   - Enemies can be killed by jumping on them or punching them ![Image](https://github.com/shikkenzo/3D-MarioRecreation/blob/main/Resources/Goomba.gif)
- Koopa
   - Drops shell on death
   - Shell can be thrown and will damage enemies and player if they are hit before it stops ![Image](https://github.com/shikkenzo/3D-MarioRecreation/blob/main/Resources/Koopa.gif)
- Simple Checkpoint System
- "Restart" system using Interfaces

## Tools
- Unity 6000.2.6f2
    - Package: Visual Studio Editor
    - Package: Unity UI

## Controls
- Movement: W, A, S, D / Left Thumbstick
- Run: Left Shift / Left Trigger
- Jump: Spacebar / 'Cross' Button (PS4)
- Punch / Throw Shell: 'O' Button (PS4)
