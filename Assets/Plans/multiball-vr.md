# Project Overview
- **Game Title**: Multiball VR
- **High-Level Concept**: A physics-driven VR experience where throwing a grabbable ball at a wall shatters it into multiple smaller, bouncy child balls. The greater the impact velocity, the more balls emerge and ricochet dynamically around the arena.
- **Players**: Single Player (VR)
- **Inspiration / Reference Games**: Ricochet VR, Breakout / Multiball arcade pinball mechanics, Physics Playground VR
- **Tone / Art Direction**: Clean, vibrant minimalist VR arcade with high-contrast materials, responsive physics, and dynamic collision feedback.
- **Target Platform**: Standalone VR (Meta Quest / PCVR / OpenXR standalone)
- **Screen Orientation / Resolution**: Stereoscopic VR (Native HMD Resolution @ 72/90/120 Hz)
- **Render Pipeline**: Universal Render Pipeline (URP)

---

# Game Mechanics

## Core Gameplay Loop
1. **Grab & Prepare**: The player grabs an interactive ball from a pedestal or spawner near their position using VR hand tracking or motion controllers.
2. **Throw**: The player hurls the ball towards the back wall.
3. **Impact & Force Detection**: On collision with the wall, the impact intensity is measured using collision relative velocity (`collision.relativeVelocity.magnitude`).
4. **Dynamic Division**:
   - If impact speed is below a minimum threshold (`minImpactSpeed`, e.g., 2.0 m/s), the ball bounces off normally without splitting.
   - If impact speed exceeds the threshold, the ball splits into smaller child balls. The number of balls scales dynamically between `minSplitCount` (e.g., 2) and `maxSplitCount` (e.g., 8) based on impact velocity.
   - Child balls are scaled down (e.g., 60% of parent scale) and assigned adjusted mass and physics parameters.
   - Child balls inherit the reflected trajectory with a randomized conical spread and velocity boost proportional to the hit force.
   - Particle spark/flash VFX and audio feedback trigger at the contact point.
   - The original ball is destroyed.
5. **Cascading / Chain Interaction**: Spawned smaller balls remain grabbable and can also split on subsequent high-speed impacts down to a configured minimum generation/size (`maxGeneration` = 2 or 3).
6. **Continuous Spawning & Cleanup**: The ball pedestal automatically dispenses a new full-size ball after the current one is thrown, ensuring an uninterrupted gameplay loop. Smaller balls automatically fade/despawn after a set lifetime or when falling off-bounds to maintain optimal VR framerates.

## Controls and Input Methods
- **VR Motion Controllers & Hands (XR Interaction Toolkit & XR Hands)**:
  - **Grip / Trigger (Controller)** or **Pinch / Grab Gesture (Hands)**: Pick up and hold balls within reach via `XRGrabInteractable`.
  - **Natural Throwing Motion**: Release grip/pinch mid-swing to throw the ball; velocity tracking handles angular and linear momentum with throw smoothing.
  - **Locomotion / Turn**: Standard snap/continuous turn and optional teleportation/smooth move to reposition within the play space.

---

# UI & Visual Feedback
- **In-World Floating Impact Indicator**: A subtle, stylish world-space hit speed / ball count readout that briefly pops up near the collision point (e.g., "SPEED: 14 m/s | x6 SPLIT!").
- **Ball Spawner Pedestal**: Visual spawn ring/cradle indicating when a new ball is ready to be picked up.
- **VFX Particle Bursts**: Bright glowing collision burst using URP particle system when balls divide.

---

# Key Asset & Context

### Existing Scene Context
- **Active Scene**: `Assets/Scenes/SampleScene.unity`
- **XR Rig**: `XR Origin Hands (XR Rig)` configured with XR Interaction Toolkit 3.6.0 and XR Hands 1.8.1.
- **Environment**: `Floor` (BoxCollider) and `Wall_Back` (BoxCollider).
- **Physics Material**: `Interactables Bouncy.physicMaterial` (high bounciness).
- **Existing Ball Prefab**: `Assets/VRTemplateAssets/Prefabs/Interactables/Sphere Interactable.prefab` with `XRGrabInteractable` and `XRGeneralGrabTransformer`.

### New & Modified Assets
1. **`Assets/Scripts/BallImpactSplitter.cs`**:
   - Core division script attached to the ball prefab.
   - Properties:
     - `wallLayer` / `wallTag`: Filter for valid splitting surfaces (e.g., tag `"Wall"` or layer).
     - `minImpactSpeed` (float, e.g. 2.5 m/s): Minimum velocity to trigger split.
     - `maxImpactSpeed` (float, e.g. 15.0 m/s): Velocity threshold for maximum ball output.
     - `minSplitCount` (int, e.g. 2): Minimum child balls on split.
     - `maxSplitCount` (int, e.g. 8): Maximum child balls on hard throw.
     - `childBallPrefab` (GameObject): Reference to the splittable ball prefab.
     - `scaleMultiplier` (float, default 0.65f): Scale reduction per generation.
     - `currentGeneration` (int, default 0): Current generation index.
     - `maxGenerations` (int, default 2): Maximum times a ball can further divide.
     - `splitVFXPrefab` (GameObject): Particle effect spawned on split.
     - `lifetime` (float, default 20f for child balls): Auto-despawn timer for performance.
     - `spreadAngle` (float, default 45f): Conical dispersion angle for child ball ejection.
2. **`Assets/Scripts/BallSpawner.cs`**:
   - Pedestal spawner script that maintains an active ball on the pedestal.
   - Monitors when the spawned ball is grabbed/moved away and spawns a replacement after a customizable delay (e.g., 1.5 seconds).
3. **`Assets/Scripts/ImpactPopup.cs` (Optional polish)**:
   - Floating text popup showing impact speed and split count.
4. **`Assets/Prefabs/Ball_Splittable.prefab`**:
   - Configured prefab with `XRGrabInteractable`, `Rigidbody` (Continuous Dynamic collision detection mode), `SphereCollider` with Bouncy Physics Material, and `BallImpactSplitter`.
5. **`Assets/Prefabs/VFX_ImpactBurst.prefab`**:
   - Lightweight URP particle burst for split impact.
6. **`Assets/Prefabs/Ball_Spawner_Pedestal.prefab`**:
   - Simple visual pedestal with spawn point transform.

---

# Implementation Steps

| Step | Description | Assigned Role | Dependencies | Parallelizable |
| :--- | :--- | :--- | :--- | :--- |
| **Step 1** | **Create Ball Division Logic (`BallImpactSplitter.cs`)**<br>Implement velocity-based collision detection, relative velocity mapping to ball count, reflection physics calculation with conical spread, generation tracking, and auto-cleanup. | developer | None | No |
| **Step 2** | **Create Ball Spawner System (`BallSpawner.cs`)**<br>Implement ball dispenser on pedestal with grab detection / distance check, respawn delay timer, and spawn limits. | developer | None | Yes |
| **Step 3** | **Create Impact VFX & Audio / Visual Feedback**<br>Create `VFX_ImpactBurst` particle prefab using URP particle system and optional impact sound synthesis/audio affordance. | developer | None | Yes |
| **Step 4** | **Create and Configure Ball Prefab**<br>Create `Ball_Splittable.prefab` with `Rigidbody` (`CollisionDetectionMode.ContinuousDynamic`), `XRGrabInteractable` (`throwVelocityScale = 1.5`), `SphereCollider` with `Interactables Bouncy.physicMaterial`, and `BallImpactSplitter`. | developer | Step 1, Step 3 | No |
| **Step 5** | **Scene Setup & Environment Configuration**<br>1. Ensure `Wall_Back` is tagged/configured as `Wall` with bouncy physics material.<br>2. Place `BallSpawner` pedestal in front of `XR Origin Hands`.<br>3. Add out-of-bounds cleanup zone / reset plane below the floor. | developer | Step 2, Step 4 | No |
| **Step 6** | **Validation & Playmode Verification**<br>Test ball throwing physics at varying velocities, verify proportional ball division (2 to 8+ balls), test VR grab and hand-tracking compatibility, and confirm performance stability with multiple active balls. | developer | Step 5 | No |

---

# Verification & Testing
1. **Low-Speed Throw Test**: Toss ball softly against the wall (< 2.5 m/s). Verify the ball bounces cleanly off the wall without splitting.
2. **Medium-Speed Throw Test**: Throw ball at moderate speed (~6–8 m/s). Verify it splits into 3–4 smaller balls with outward bouncing velocity.
3. **High-Speed Throw Test**: Throw ball at high speed (> 12 m/s). Verify maximum ball count (6–8 balls) spawns with satisfying wide spread and VFX burst.
4. **Recursive Division Test**: Pick up one of the spawned smaller balls and throw it hard against the wall. Verify it divides again if under `maxGenerations` limit.
5. **Continuous Spawner Test**: After throwing the ball, verify a new ball spawns at the pedestal after ~1.5s delay.
6. **VR Grab & Throw Feel**: Verify that release velocity tracking feels natural with VR controllers and hand tracking.
7. **Performance & Memory**: Ensure spawned balls despawn after their lifetime and no memory leaks or physics jitter occur.
