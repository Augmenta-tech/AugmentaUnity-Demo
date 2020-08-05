Augmenta for Unity
=======================

Unity examples using the Augmenta-Unity library created by [Théoriz](http://www.theoriz.com/en/).

Installation
-------------------------------------

### Git User

 - Create a new [Unity](https://unity3d.com/fr) project and git it.
 - `$git submodule update --init --recursive` to pull everything.

### Non Git User

- Create a new [Unity](https://unity3d.com/fr) project.
- Download zip and unzip this project in `*ProjectFolder*`.
- Download zip and unzip [Augmenta Unity](https://github.com/Theoriz/AugmentaUnity) in `*ProjectFolder*/Assets/Plugins/Augmenta/`.

How to Use
-------------------------------------

### Setup

To start developping your application you probably need Augmenta data. If you do not have an Augmenta node ready, you can use our [Augmenta simulator](https://github.com/Theoriz/Augmenta-simulator/releases).

- Open your Unity scene.

- Drop the Augmenta prefab (from Assets/Plugins/Augmenta/Prefabs) in it.

- Set the input port in the AugmentaManager script of the Augmenta prefab to your protocol port.

- Run the scene.

- You should see gizmos of your scene and persons in the scene view. You can add/remove debug objects visible in gameview with the Show Debug option

### Using Custom Object Prefabs

To instantiate your own prefab on each Augmenta object, add your prefab to the Custom Object Prefab parameter of the Augmenta Manager.

You can change this prefab at runtime by calling the function `ChangeCustomObjectPrefab(GameObject newPrefab)` of the Augmenta Manager.

### Using Several Augmenta Streams

You can receive different Augmenta streams in the same Unity application as long as they are not on the same OSC port. You need to add an Augmenta prefab (i.e. AugmentaManager) for each incoming stream, then set each AugmentaManager ID and input port to listen to each protocol.

Example Scenes 
-------------

### 0 - Minimalist

The simplest example using only the AugmentaManager to parse incoming Augmenta data and expose them to Unity.

### 1 - AugmentaSceneToSpout

In this example, an Augmenta Scene Camera is used to always render exactly the Augmenta Scene. The resulting texture is sent via Spout to be used in an external software.

### 2 - AugmentaMPSCounter

This example analyzes the incoming Augmenta messages rate to compute an estimation of the number of messages received per second.

### 3 - AugmentaVideoOutput

This example shows how to use an Augmenta Video Output along with an Augmenta Video Output Camera to always render exactly the video texture area sent by [Fusion](https://augmenta-tech.com/download/#fusion). The resulting texture is sent via Spout and shown on a debug quad in the scene.

In this workflow, the field of view of the camera is computed to always match exactly the Video Output area.

### 4 - AugmentaVideoOutputFromExternalCamera

This example shows how to use an Augmenta Video Output along with any camera in the scene to render the intersection of the video texture area sent by [Fusion](https://augmenta-tech.com/download/#fusion) and the camera's field of view. The resulting texture is sent via Spout and shown on a debug quad in the scene.

In this workflow, the field of view of the camera is fixed and color padding is added to the output video texture to match the desired texture resolution.

### 5 - SeveralAugmentaScenes

In this example, two different Augmenta streams are received on two different ports and placed in the scene to simulate the usecase of an interactive floor and an interactive wall used together in the same scene.

### 6 - AugmentaToGameObject

In this example, a custom object prefab is used to make a simple scene with squirrels react to Augmenta persons.

### 7 - AugmentaToShader

In this example, the Augmenta person data is send to a ripple shader in order to have the shader creates ripples under the Augmenta persons.


Augmenta Documentation
-------------

https://github.com/Theoriz/Augmenta/wiki

Version
-------------

Unity 2019.4.6f1
