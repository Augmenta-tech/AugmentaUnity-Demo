Augmenta for Unity
=======================

A Unity Augmenta library and example created by [Théoriz](http://www.theoriz.com/en/).
This is a complex version of the library with dependencies, for a minimalist version without dependencies check the V2-Minimalist branch.

Installation - Git user
-------------------------------------

 - Create a new [Unity](https://unity3d.com/fr) project and git it.
 - `$git submodule update --init --recursive` to pull everything.

 Installation - Non Git user
-------------------------------------
- Create a new [Unity](https://unity3d.com/fr) project.
- Download zip and unzip this project in `*ProjectFolder*`.
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

AugmentaCameras use the postprocessing stack v2. To install it, open the package manager (Window/Package Manager) and install the package Post-processing.

Usage
-------------------------------------

To start developping your application you need Augmenta data, for this use our [Augmenta simulator](https://github.com/Theoriz/Augmenta-simulator/releases), download and launch it.

- Open your Unity scene.

- Drop the AugmentaArea prefab in it.

- Drop the AugmentaAreaAnchor prefab in it.

- Drop the GenUI prefab in it.

- Link the AugmentaArea and AugmentaAreaAnchor by giving them the same ID (in the ID string parameters).

- Run the scene.

- Set the augmenta simulator target port as the OSCMaster local port in Genui prefab.

- Set the augmenta simulator target IP to "127.0.0.1" if you use both software on the same computer otherwise set it as the IP address of the computer running Unity.

- You should now see Augmenta points (green boxes) in the Augmenta area (red rectangle) in the scene view.

- Stop the scene.

- Create a new GameObject in the scene and add an AugmentaBasicManager script to it.

- Set its "Prefab to instantiate" field with the prefab you want to instantiate on each augmenta point.

- Run the scene and you should see your own prefab on each augmenta points in the game view.


System Explanation
-------------------------------------

The **AugmentaArea** prefab contains an AugmentaArea and its AugmentaCamera. The AugmentaArea is responsible for listening to Augmenta OSC messages, parsing them and sending the corresponding event inside Unity. It set its size according to the incoming AugmentaScene messages and update a list of AugmentaPerson according to the incoming AugmentaPerson messages.

An **AugmentaCamera** is linked to an AugmentaArea and is designed to facilitate the rendering of the augmenta data. For example it can be set to always adapt its field of view to match exactly the AugmentaArea size through different camera types (orthographic, perpective or offcenter). You can still add and use your own cameras to the scene if the AugmentaCamera does not suit your needs.

The **AugmentaAreaAnchor** is an object used to pre-visualize an AugmentaArea in your scene. You can set its size to the future size of the AugmentaArea (if you know it in advance) to better prepare the rest of your environment. You should place the AugmentaAreaAnchor where you want the AugmentaArea to be in your scene as it will always update the AugmentaArea position and rotation to match its own position and rotation. The AugmentaArea scale is dictated by the incoming AugmentaScene messages. The link between an AugmentaAreaAnchor and an AugmentaArea is done via the AugmentaAreaID and LinkedAugmentaAreaID strings (for the AugmentaArea and AugmentaAreaAnchor respectively). Each AugmentaAreaAnchor should have an ID matching the ID of an AugmentaArea, and different from every other AugmentaAreaAnchor IDs.

The **AugmentaCameraAnchor** is to the AugmentaCamera what the AugmentaAreaAnchor is to the AugmentaArea. It is linked to an AugmentaAreaAnchor and allow to render it using various ways. It can also update the corresponding AugmentaCamera (i.e. the AugmentaCamera belonging to the AugmentaArea that the AugmentaAreaAnchor is linked to) with its camera, position, rotation, post-process and Augmenta parameters.

This system of anchors is especially useful if you are working with different scenes as it allows you to have a main scene handling the incoming Augmenta data with an AugmentaArea, and several other scenes with only AugmentaAreaAnchors and AugmentaCameraAnchors. When one of those scenes is loaded along the main scene, the values of the AugmentaAreaAnchor and AugmentaCameraAnchor inside this scene will be copied to the AugmentaArea and AugmentaCamera of the main scene. This allows to design different scenes with different Augmenta setups and have them run one after the other with a single instance of Augmenta. This avoid having to close and re-open an OSC connection on every scene change in Unity.

Augmenta Behaviours
-------------------------------------

You can add special behaviours to the instantiation and handling of your Augmenta prefabs by creating a class inheriting from AugmentaBasicManager with your custom behaviour when augmenta points appear/disappear.

You can also add special behaviours to your instantiated prefab by adding an AugmentaPersonBehaviour script to your "prefab to instantiate". The provided AugmentaPersonBehaviour script implements 3 coroutines Appear/Alive/Disappear that are called when the object is created/living/destroyed respectively.
To implement your own animations you have to create a new class inheriting from AugmentaPersonBehaviour and override its intro/alive/outro coroutines. Check the AugmentaBasicPersonBehaviour script for an example of this animating a value along the object lives.

Using Several Augmenta Streams
-------------------------------------

You receive different Augmenta streams in the same Unity application as long as they are not on the same OSC port. You need to add an AugmentaArea (and AugmentaAreaAnchor) for each incoming stream. You can then set each AugmentaArea to listen to a different OSC port.

Do not forget to properly link your AugmentaAreas and AugmentaAreaAnchors together with their ID ! Each AugmentaArea/AugmentaAreaAnchor pair should have a unique ID.


Post Processing and Off Center cameras
-------------------------------------

Post processes are currently overriding projection matrices, disabling Off Center cameras (issue [here](https://github.com/Unity-Technologies/PostProcessing/issues/546)).

To avoid this, comment lines 384-388 and 604-619 in the script PostProcessLayer.cs located in the Packages folder in Post Processing/PostProcessing/Runtime/.

The lines to comment are the following :

    #if UNITY_2018_2_OR_NEWER
      if (!m_Camera.usePhysicalProperties)
    #endif
        m_Camera.ResetProjectionMatrix();
      m_Camera.nonJitteredProjectionMatrix = m_Camera.projectionMatrix;

and

    if (m_CurrentContext.IsTemporalAntialiasingActive())
    {
    #if UNITY_2018_2_OR_NEWER
    // TAA calls SetProjectionMatrix so if the camera projection mode was physical,  gets set to explicit. So we set it back to physical.
      if (m_CurrentContext.physicalCamera)
        m_Camera.usePhysicalProperties = true;
      else
    #endif
        m_Camera.ResetProjectionMatrix();

    if (m_CurrentContext.stereoActive)
    {
      if (RuntimeUtilities.isSinglePassStereoEnabled || m_Camera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right)
        m_Camera.ResetStereoProjectionMatrices();
      }
    }



Documentation
-------------

https://github.com/Theoriz/Augmenta/wiki

Version
-------------

Unity 2019.1.7f1
