# Scenes Configuration Guide

TERRAGRAV uses two core scenes for Phase 1:

1. **`Boot.unity`**: Entry point scene initializing global services, logging, and transitioning to the main Game scene.
2. **`Game.unity`**: Main local 2.5D gameplay scene.

## Game.unity Hierarchy Setup:
```
[Game.unity]
├── [Managers]                 (GameManager, InputManager)
├── [Arena]                    (ArenaBuilder, TerritoryGrid, TerritoryRenderer, GridVisualizer)
├── [GameSetup]                (GameSetup)
├── Main Camera                (GameCameraController, Camera with Orthographic projection)
├── Directional Light          (Rotation: 50, -30, 0, Soft Shadows)
├── UI_Canvas                  (Canvas, CanvasScaler, GraphicRaycaster)
│   ├── GameHUD                (GameHUD, TextMeshPro elements)
│   └── VirtualJoystick        (VirtualJoystick, Image components)
└── EventSystem                (EventSystem, InputSystemUIInputModule)
```
