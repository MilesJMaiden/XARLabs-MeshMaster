# XARLabs-MeshMaster

This project demonstration of procedural 3D object creation, animation, and VR interaction in Unity.

---

## Overview

**Features**  
1. **Procedural Mesh Creation**  
   - Two meshes generated at runtime:  
     - **Object A**: Pentagonal Prism  
     - **Object B**: Hexagonal Pyramid  
   - Each mesh has distinguishable front/back normals, a MeshFilter, MeshRenderer, and Material.  

2. **Lissajous Curve Movement Animation**  
   - Both objects move along independently-parameterized Lissajous curves.  
   - Curves are visualized via a toggleable LineRenderer that updates at runtime.  

3. **Colour and Rotational Animation**  
   - Object A rotates toward Object B at a configurable angular speed.  
   - Object A’s color interpolates from red (B in front) to blue (B behind).  

4. **Perlin Mesh Vertex Animation**  
   - Object A’s vertices displace along normals by Perlin noise.  
   - Noise can be toggled off to restore the original mesh.  

5. **VR Integration**  
   - Animation is anchored to the user’s view (parented under the *'XR Origin (XR Rig)/Camera Offset/Main Camera'*).  
   - Right hand-controller attracts Object A; left hand-controller attracts Object B.

---

## Setup Instructions

1. **Clone the Repository**  
   git clone https://github.com/MilesJMaiden/XARLabs-MeshMaster.git
   
2. **Open in Unity**
   - Launch Unity Hub, 'Add/Add project from disk', and select the project folder.
   - Use Unity version 6000.0.47f1 LTS or newer
  
3. **XR Packages**
   - Packages included; *XR Interaction Toolkit* and *XR Plug-in Management*.
   - To deploy on VR ensure that you have selected Android in the Build Profile and build using the '*MeshMasterVR.scene*'
   - Deploying this on other VR devices requires you to Enable additional Interaction profiles via *'Project Settings/XR Plug-in Management/OpenXR/EnabledInterctionProfiles'* This project only has the '*Meta Quest Touch Plus Controllers profile*' (Quest 3) Enabled.

4. **Assign Materials & Attractors**
   - Packages included; *XR Interaction Toolkit* and *XR Plug-in Management*.
   - Materials for used are located in '*Assets/Materials*' and are assigned via the Material field of the PentagonalPrism and HexagonalPyramid script components on either object.
   - For VR Controller Attraction, ensure that you Select both GameObjects, assign your right/left controller/hand Transforms ('*XR Origin (XR Rig)/Camera Offset/...*') to their corresponding '*Attractor Transform field*' of the Lissajous Curve Mover script component.

5. **Tweaking Parameters (Play or Edit Modes)**
   - Lissajous properties (*LissajousCurveMover.cs*): XY amplitudes, XY frequencies, phase offset (Radians), speed (Multiplier for mesh movement along curve). Additionally a curve resolution and line width to visualize the curve using a Line Renderer.
   - Object A (*PentagonalPrism.cs*): Angular (rotation speed), front/back colors, noise amplitude/frequency/speed, noise toggle
   - VR Controller Attracion (*LissajousCurveMover.cs*): range, strength, enable/disable
   
---

## Implementation Details

1. **ProceduralMesh.cs (Base Class)**
   - Awake(): Creates a child GameObject named ObjectName, adds MeshFilter & MeshRenderer, assigns m_material, then calls *BuildMesh()*.
   - BuildMesh(Mesh): Abstract—each subclass defines its own vertices & triangles.

2. **HexagonalPyramid.cs (Object B)**
   - Overrides *BuildMesh()* to generate: A 6-sided base fan (downward normals) and Six triangular sides (outward normals).
  
3. **PentagonalPrism.cs (Object A)**
   - Overrides *BuildMesh()* to generate: Bottom and top pentagon fans and Side quads split into two triangles.
   - Rotation Logic: *RotateTowardsTarget(*) uses Quaternion.RotateTowards at *m_AngularSpeed*.
   - Colour Logic: *UpdateColorBasedOnAngle()* lerps between *m_BackColor* (blue) and *m_FrontColor* (red) based on the dot product.
   - Noise Logic: *AnimateVertices()* displaces vertices along normals by Perlin noise (*m_NoiseAmplitude, m_NoiseFrequency, m_NoiseSpeed*) and when *m_EnableNoise* is false, *RestoreOriginalMesh()* resets vertices & normals to their original arrays.
  
4. **LissajousCurveMover.cs**
   - Fields: Amplitudes (*m_AmplitudeX*, *m_AmplitudeY*), frequencies (*m_FrequencyX*, *m_FrequencyY*), phase (*m_PhaseOffset*), speed (*m_Speed*), visualization toggles, attractor settings (*m_EnableAttraction*, *m_AttractorTransform*, *m_AttractionRange*, *m_AttractionStrength*).
   - *ComputeBaseCurveLocalPosition()*: Returns localPosition = initialLocal + (A·sin(a·t+δ), B·sin(b·t), 0).
   - *BlendAttraction(Vector3)*: If attractor is within m_AttractionRange, linearly interpolate between the curve position and the attractor’s local position by (1 - dist/range) * m_AttractionStrength.
   - *UpdateVisualization()*: Toggles LineRenderer.enabled by *m_ShowCurve* and regenerates the curve if any parameter has changed, caching previous values to detect changes.

---

## Assumptions & Challenges

1. **Performance Considerations**
   - The procedural and noise updates run on the main thread; performance remains smooth for simple meshes. For more complex mesh I would consider using a multi-threaded approach in conjunction with Unity DOTS or specifically the Job System in order to off-load heavy computation.
  
2. **Assumptions**
   - *Parenting Assumption*: Both the *LissajousCurveMover.cs* and its assigned attractor (controller) share the same parent (e.g., the XR camera rig), so using transform.parent.InverseTransformPoint correctly maps the attractor’s world position into the mover’s local space.

3. **Challenges**
   - *Rendering the Lissajous Curve*: Ensuring the LineRenderer accurately traces the full loop required computing the longer of the two sine periods (samplePeriod = max(2π/a, 2π/b)) so neither axis’s cycle gets cut off, while also handling dynamic resolution, width, and material changes at runtime without stuttering or frame hitches.
   - *Perlin Noise Vertex Animation*: Animating each vertex along its normal introduced the risk of cumulative mesh distortion; I solved this by caching the original vertex positions and normals on Start, displacing them each frame based solely on those originals, and restoring them instantly whenever the noise toggle is off.

---
