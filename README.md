Augmenta for Unity
=======================

A Unity Augmenta library and example created by [Théoriz](http://www.theoriz.com/en/)

Installation - Git user
-------------------------------------

 - Create a new Unity [Unity](https://unity3d.com/fr) project and git it.
 - Add [Augmenta Unity](https://github.com/Theoriz/AugmentaUnity) as git submodule.
 - Add [GenUI](https://github.com/Theoriz/GenUI) as git submodule.
 - `$git submodule update --init --recursive` to pull everything.
 
 Installation - Non Git user
-------------------------------------
- Create a new Unity [Unity](https://unity3d.com/fr) project.
- Download zip and unzip [Augmenta Unity](https://github.com/Theoriz/AugmentaUnity) in `*ProjectFolder*/Assets/Plugins/Augmenta/`.
- Download zip and unzip [GenUI](https://github.com/Theoriz/GenUI) in `*ProjectFolder*/Assets/Plugins/GenUI`.
- Download zip and unzip [OCF](https://github.com/Theoriz/OCF) in `*ProjectFolder*/Assets/Plugins/GenUI/Plugins/OCF/`.
- Download zip and unzip [UnityOSC](https://github.com/Theoriz/UnityOSC) in `*ProjectFolder*/Assets/Plugins/GenUI/Plugins/OCF/Plugins/UnityOSC`.
 
GenUI
-------------------------------------

GenUI is a tool allowing you to easily add UI and OSC control to your game in build and editor, it is used here for its OSC module. An example of Augmenta UI using GenUI is done in AugmentaExampleFull scene.

You can use your own OSC library instead of the one in GenUI. In this case you will need to provide AugmentaArea script with OSC messages, the link is only done in its Start method.

PostProcess
-------------------------------------

AugmentaCameras use the postprocessing stack v2. To install it, open the package manager on Window/Package Manager

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

- Create a new GameObject in the scene and add an AugmentaBasicManager script to it.

- Set its "Prefab to instantiate" field with the prefab you want to instantiate on each augmenta point.

- Run the scene and you should see your own prefab on each augmenta points in the game view.


Advanced usage
-------------------------------------

You can add special behaviours to AugmentaBasicManager by creating an inherited class with your custom behaviour when augmenta points appear/disappear.

You can add an AugmentaPersonBehaviour script to your "prefab to instantiate" to be able to add intro/alive/outro animation to your object. 
For this you have to create a new class which will inherit from AugmentaPersonBehaviour and override its intro/alive/outro coroutines. Check the MyAugmentaPersonBehaviour script for an example of this animating the scale of the cube prefab.

Examples
-------------------------------------

This project contains two example scenes.

**AugmentaExampleMinimalist** contains only the minimum objects to get Augmenta working with an AugmentaArea, an AugmentaMainCamera and GenUI.

**AugmentaExampleAnchor** contains an example of using an AugmentaAreaAnchor to visualize and constrain the AugmentaArea position in the scene. The AugmentaAreaAnchor contains an AugmentaCamera that is used to see the anchor and whose parameters are copied to the AugmentaMainCamera on start. This system is useful if you are working with different scenes : you can have a main scene handling Augmenta with the AugmentaArea and AugmentaMainCamera, and several other scenes with only AugmentaAreaAnchors and AugmentaCameras. When one of those scenes is loaded along the main scene, the values of the AugmentaAreaAnchor and AugmentaCamera inside this scene will be copied to the AugmentaArea and AugmentaMainCamera of the main scene. This allows to design different scenes with different Augmenta setups and have them run one after the other with a single instance of Augmenta.

Documentation
-------------

https://github.com/Theoriz/Augmenta/wiki

Version
-------------

Unity 2018.1.10f1


