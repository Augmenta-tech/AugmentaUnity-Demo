Augmenta for Unity
=======================

A Unity Augmenta library and example created by [Théoriz](http://www.theoriz.com/en/).

This is the minimalist version of the library. For a more complex version with additionnal dependencies, check the master branch.

Installation - Git User
-------------------------------------

 - Create a new [Unity](https://unity3d.com/fr) project and git it.
 - `$git submodule update --init --recursive` to pull everything.

 Installation - Non Git User
-------------------------------------
- Create a new [Unity](https://unity3d.com/fr) project.
- Download zip and unzip this project in `*ProjectFolder*`.
- Download zip and unzip [Augmenta Unity](https://github.com/Theoriz/AugmentaUnity) in `*ProjectFolder*/Assets/Plugins/Augmenta/`.

How to Use
-------------------------------------

To start developping your application you probably need Augmenta data. If you do not have an Augmenta node ready, you can use our [Augmenta simulator](https://github.com/Theoriz/Augmenta-simulator/releases).

- Open your Unity scene.

- Drop the Augmenta prefab (from Assets/Plugins/Augmenta/Prefabs) in it.

- Set the input port in the AugmentaManager script of the Augmenta prefab to your protocol port.

- Run the scene.

- You should see gizmos of your scene and persons in the scene view. You can add/remove debug objects visible in gameview with the Show Debug option

Using Custom Object Prefabs
-------------------------------------

To instantiate your own prefab on each Augmenta object, add your prefab to the Custom Object Prefab parameter of the Augmenta Manager.

You can change this prefab at runtime by calling the function `ChangeCustomObjectPrefab(GameObject newPrefab)` of the Augmenta Manager.

Using Several Augmenta Streams
-------------------------------------

You can receive different Augmenta streams in the same Unity application as long as they are not on the same OSC port. You need to add an Augmenta prefab (i.e. AugmentaManager) for each incoming stream, then set each AugmentaManager ID and input port to listen to each protocol.


Documentation
-------------

https://github.com/Theoriz/Augmenta/wiki

Version
-------------

Unity 2019.4.3f1
