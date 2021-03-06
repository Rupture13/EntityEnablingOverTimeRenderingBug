# Entity Enabling Over Time Rendering Bug
Unity DOTS reference project demonstrating a visual bug that occurs when entities are enabled over time.

The bug occurs when entities in a subscene are disabled when the subscene loads, and are enabled over time.<br>
When doing so, entities of the same archetype seem to have an issue where they use the bounding box of the first entity of that archetype, resulting in entities becoming invisible when the firstly enabled entity of that archetype is out of view. This issue does, for some reason, not apply to entities of the archetype of the very first entity enabled in the subscene.

[![Click to see the video](https://user-images.githubusercontent.com/31402211/112824682-21fcc680-908b-11eb-9258-990cfbb2678e.png)](https://user-images.githubusercontent.com/31402211/112812789-cc6ded00-907d-11eb-8a0c-f9c246ee124e.mp4)

To experience the bug, go into play mode and fly around with the camera, following the onscreen instructions.
<br>
<br>
<br>
## Editor and package versions
Unity Editor version is `2020.1.17f1`.

Important package versions include:
```
"com.unity.dots.editor":                       "0.12.0-preview.6",
"com.unity.entities":                          "0.17.0-preview.41",
"com.unity.render-pipelines.high-definition":  "9.0.0-preview.71",
"com.unity.rendering.hybrid":                  "0.11.0-preview.42",
"com.unity.scriptablebuildpipeline":           "1.16.1",
```
*(other package versions can be viewed in the `/Packages/manifest.json` file)*

This project also uses the **Hybrid Renderer V2**. <br>
*(this is enabled by adding `ENABLE_HYBRID_RENDERER_V2` to the Scripting Define symbols in the project's Player settings)*
<br>
<br>
<br>
## Project layout
The project is a simple Unity project.<br>
The code of interest is located in `Assets/Scripts/Systems/SceneLoadSystem.cs` and is supplemented by explanatory comments.
