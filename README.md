# 1tontools

This is the repository for in-house Unity/C# development tools created at 1TON Games.
https://github.com/nosenfield/1tontools.git#v1.0.0

# tools list

## Animation

## Audio
Overview:
SoundEffects are scriptable object instances created in the editor. They hold raw data about a sound effect (source clip, master volume, isSpatial, etc.).

The SoundEffect class is tagged with the [GenerateEnum] attribute which auto-generates the SoundEffectId enum with a list of the asset names of all SoundEffect instances in the project.

The AudioPlayer is a ScriptableObject which holds a list of SoundEffect instances. The provided instances get paired to their associated enum value which allows any script to call a sound effect via its enum id.

The AudioPlayer also accepts SoundEffect instances directly for situations in which passing a serialized reference is preferable.

The AudioSourcePrefab gameObject/component controls the AudioSource and mediates the request to play a particular SoundEffect instance.

The AudioPlayer instantiates clones of the AudioSourcePrefab gameObject, adds them to the scene in the appropriate location, passes in the SoundEffect data for playing, and pools the clones as allowed.

### AudioPlayer.cs
### AudioSourcePrefab.cs
### SoundEffect.cs

Uses: EnumGeneration, ScriptableObjectSingleton, ObjectPooling

## CrossSceneTrackingSystem

## Documentation

## Events

## EnumGeneration
The [GenerateEnum] attribute can be added to any ScriptableObject class. This will auto-generate an enum with values representing the hashed string names of all instances of that class.

### EnumGenerator.cs
### EnumGeneratorMenu.cs
### EnumGeneratorPostProcessor.cs
### GenerateEnumAttribute.cs

## Logging
Overview:
Enhances Unity logs with calling class/method and easy switching of log levels. 

### LogService.cs
**class LogService**
**enum LogLevel**

## Networking

## PersistentData

## Popups

## Scene Management

- ISceneTransition
- - ISceneTransitionEditor
- MultiSceneManager
- - MultiSceneManagerEditor
- SceneCollection
- ScenePointer
- UniversalSceneObject

---

## Tilemap

## Utilities
