Augmenta for Unity
=======================

Unity examples using the [Augmenta-Unity](https://github.com/theoriz/augmentaunity) library created by [Théoriz](http://www.theoriz.com/en/).

Installation
-------------------------------------

### Git User

 - Create a new [Unity](https://unity3d.com/fr) project and git it.
 - `$git submodule update --init --recursive` to pull everything.

### Non Git User

- Create a new [Unity](https://unity3d.com/fr) project.
- Download zip and unzip this project in `*ProjectFolder*`.
- Download zip and unzip [Augmenta Unity](https://github.com/Theoriz/AugmentaUnity) in `*ProjectFolder*/Assets/Plugins/Augmenta/`.
- Download zip and unzip [Shared-Texture-Unity](https://github.com/Theoriz/Shared-Texture-Unity) in `*ProjectFolder*/Assets/Plugins/SharedTextureUnity/`.


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

**Using Custom Behaviours**

You can implement custom spawn and destroy behaviours for your custom Augmenta objects by implementing the IAugmentaObjectBehaviour interface in a script of your object. If you do, its Spawn function will be called when the object is instantiated, and its Destroy function will be called when the object should be destroyed (i.e. when the corresponding AugmentaObject is destroyed).

Note that if you implement the IAugmentaObjectBehaviour interface, the AugmentaObject will *NOT* destroy your object when it destroys itself, instead it will call the Destroy function of the interface. You should handle the destruction of the custom object yourself in the Destroy() function of the interface.

An example use of the custom behaviours is shown in scene 10 - AugmentaObjectBehaviour.

### Using Several Augmenta Streams

You can receive different Augmenta streams in the same Unity application as long as they are not on the same OSC port. You need to add an Augmenta prefab (i.e. AugmentaManager) for each incoming stream, then set each AugmentaManager ID and input port to listen to each protocol.

Example Scenes 
-------------

### 0 - Minimalist

The simplest example using only the AugmentaManager to parse incoming Augmenta data and expose them to Unity.

![](https://media.giphy.com/media/Y1MjARAF8cMu2OeXFn/giphy.gif)

### 1 - AugmentaSceneToSpout

In this example, an Augmenta Scene Camera is used to always render exactly the Augmenta Scene. The resulting texture is sent via Spout to be used in an external software.

![](https://media.giphy.com/media/iG9m3kPTu5NwZgpq5T/giphy.gif)

### 2 - AugmentaMPSCounter

This example analyzes the incoming Augmenta messages rate to compute an estimation of the number of messages received per second.

![](https://media.giphy.com/media/Ylf4cVXPw6uEwOi5dm/giphy.gif)

### 3 - AugmentaVideoOutput

This example shows how to use an Augmenta Video Output along with an Augmenta Video Output Camera to always render exactly the video texture area sent by [Fusion](https://augmenta-tech.com/download/#fusion). The resulting texture is sent via Spout and shown on a debug quad in the scene.

In this workflow, the field of view of the camera is computed to always match exactly the Video Output area.

![](https://media.giphy.com/media/lS6zCOw9Fp99V4lb8O/giphy.gif)

### 4 - AugmentaVideoOutputFromExternalCamera

This example shows how to use an Augmenta Video Output along with any camera in the scene to render the intersection of the video texture area sent by [Fusion](https://augmenta-tech.com/download/#fusion) and the camera's field of view. The resulting texture is sent via Spout and shown on a debug quad in the scene.

In this workflow, the field of view of the camera is fixed and color padding is added to the output video texture to match the desired texture resolution.

![](https://media.giphy.com/media/ek4c3lDIjIUbqX5OBf/giphy.gif)

### 5 - SeveralAugmentaScenes

In this example, two different Augmenta streams are received on two different ports and placed in the scene to simulate the usecase of an interactive floor and an interactive wall used together in the same scene.

![](https://media.giphy.com/media/hoyZw8ZM5KVLGT7sm6/giphy.gif)

### 6 - AugmentaToGameObject

In this example, a custom object prefab is used to make a simple scene with squirrels react to Augmenta persons.

![](https://media.giphy.com/media/Ply1TCKv8stLIGNeCt/giphy.gif)

### 7 - AugmentaToShader

In this example, the Augmenta person data is send to a ripple shader in order to have the shader creates ripples under the Augmenta persons.

![](https://media.giphy.com/media/iKGxo1w593GKKVREXk/giphy.gif)

### 8 - AugmentaToVFXGraph

In this example, the Augmenta person data is send to a VFXGraph in order to make the sand particle react to the oldest 3 persons in the scene.

![](https://media.giphy.com/media/kc71FmUgEIg7elRTd7/giphy.gif)

### 9 - FusionSpout

In this example, the FusionSpout prefab is used to display a Spout coming from Augmenta Fusion on a quad fitted to an AugmentaVideoOutput.

![](https://media.giphy.com/media/2e6Wkvgc284Bxh94ZY/giphy.gif)

### 10 - AugmentaObjectBehaviour

In this example, the IAugmentaObjectBehaviour interface is used in the custom object prefab to fade in and out a sphere rotating around each AugmentaObject.

![](https://media.giphy.com/media/z5JYu475MKpQ0YFmVC/giphy.gif)

Augmenta Documentation
-------------

https://github.com/Theoriz/Augmenta/wiki

Version
-------------

Unity 2020.2.7f1
