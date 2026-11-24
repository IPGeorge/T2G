## T2G Framework

**T2G (Text-to-Game)** is an instruction-driven framework for LLM-assisted game development. It aims to decouple semantic interpretation, game state management, and game engine execution, enabling more flexible, consistent, and reusable game development workflows.

The framework introduces **structured instructions** as an intermediate representation between user requests and game engine operations, and **GameDesc** as a persistent representation of the evolving game state. A dedicated resolution mechanism connects semantic descriptions to game resources, while engine-specific executors perform the actual operations and return execution results.

<p align="center"><img width="600" height="420" alt="image" src="https://github.com/user-attachments/assets/1e862242-6b73-42eb-8711-babcafcfa8e1" /></p>

By separating these responsibilities, T2G aims to support:

- **LLM Independence:** Different language models or interpretation methods can be used without changing the execution pipeline.
- **State Continuity:** Persistent game representations support iterative development across sessions.
- **Resource Flexibility:** Semantic descriptions can be resolved to different game assets.
- **Reusability and Portability:** Structured game descriptions and instructions can be reused for game reconstruction and adaptation to different engines.

The current prototype is implemented with Unity and includes the T2G Assistant, T2G Execution plugin, and Asset Library Service (SQLite, Python).


## Repository Structure

This repository contains the T2G prototype application, Unity project, and supporting asset library service used in the experiments.

| Folder / File | Description |
|---|---|
| `/T2G_Assistant` | Windows standalone T2G Assistant application. |
| `/T2G_Assistant_project` | Unity project containing the T2G Assistant source code. |
| `/AssetLibrary` | Game assets used to simulate an asset library for the experiments. |
| `/AssetLibraryService` | Asset Library Service and asset data sheet containing information about available assets. Additional assets can be registered by updating the data sheet. Run `BuildAssetsDB.bat` to generate the asset database, then `StartService.bat` to start the service. |
| `README.md` | This documentation file. |
