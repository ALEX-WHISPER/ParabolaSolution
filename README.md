# Parabolic Trajectory Launcher

## Main Features

- Target Detector
    + Locate the parabola destination position from above (See Projector.prefab,  TargetDetectorControl.cs)

- Parabola Generator(See LaunchArc.prefab, LaunchArcRenderer.cs)
    + Calculate the points of parabola trajectory
    + Generate the parabola line with those points using LineRenderer component

- Parabolic Movement(See TestCarrot_UnDestructible.prefab)
    + Emit a bullet which moves along that parabola (See MissileBehaviour.cs)
    + The moving bullet will rotate constantly to aim at its target during the movement to simulate the realistic parabolic flight(Demo: CharacterParaMotion.unity, Code: CharacterParaMove.cs)

- Target Projector(See Projector.prefab)
    + Using Projector in Unity StandardAssets to follow and display the target point as an UI element

- Interaction    
To see the whole simulation, open and play: ThrowSimulation.unity. 
You can adjust the shooting angle by using joystick in the right bottom corner, and emit a carrot missile by hitting the shoot button in the left bottom corner.