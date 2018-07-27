Augmenta for Unity
=======================

A Unity Augmenta library and example created by [Théoriz](http://www.theoriz.com/en/)

Installation
-------------------------------------

 - Install [Unity](https://unity3d.com/fr).
 - Add [Augmenta Unity repository](https://github.com/Theoriz/AugmentaUnity) as submodule or unzip it in your project.
 - Add [GenUI](https://github.com/Theoriz/GenUI) as submodule or unzip it in your project.

GenUI
-------------------------------------

GenUI is a tool allowing you to easily add UI and OSC control to your game in build and editor, it is used here for its OSC module. An example of Augmenta UI using GenUI is done in AugmentaExampleFull scene.

You can use your own OSC library instead of the one in GenUI. In this case you will need to provide AugmentaArea script with OSC messages, the link is only done in its Start method.

Usage
-------------------------------------

To start developping your application you need Augmenta data, for this use our [Augmenta simulator](https://github.com/Theoriz/Augmenta-simulator/releases), download and launch it.

- Open your Unity scene.

- Drop the AugmentaArea prefab in it.

- Drop the GenUI prefab in it.

- Run the scene.

- Set the augmenta simulator target port as the OSCMaster local port in Genui prefab. 

- Set the augmenta simulator target IP to "127.0.0.1" if you use both software on the same computer otherwise set it as the IP address of the computer running Unity.

- You should now see Augmenta points (green boxes) in the Augmenta area (red rectangle) in the scene view.

- Stop the scene.


    Explanation
    -------------------------------------

    In scene view you should see a red rectangle which corresponds to the Augmenta Area, it represents the area where people will move and be detected. The size is determined by the augmenta scene width and height set in augmenta simulator.  To link it to Unity unit system (meter) we use the PixelPerMeter coefficient, it determines the area scale while the width and height determines its ratio.

    -------------------------------------

- Add a new GameObject to the scene and add it an AugmentaBasicManager script.

- Set its "Prefab to instantiate" field with the prefab you want to instantiate on each augmenta point.

- Run the scene and you should see your own prefab on each augmenta points in the game view.


Advanced usage
-------------------------------------

You can add special behaviours to AugmentaBasicManager by creating an inherited class with your custom behaviour when augmenta points appear/disappear.

You can add an Augmenta behaviour script to your "prefab to instantiate" to be able to add intro/alive/outro animation to your object. 
For this you have to create a new class which will inherit from AugmentaBehaviour and use its "AbstractValue", see the AugmentaExample scene for more information.

Documentation
-------------

https://github.com/Theoriz/Augmenta/wiki

