# README #

### What is this repository for? ###

This documentation is meant to explain how to use Augmenta in Unity with the scripts we updated in early 2018.
Here'll be describe the different scripts by order of importance in the project

## How to set it up ? 
Add it to your current git repo if you want to use it as a submodule

git submodule add /the/url/bitbucket/generate/for/you /the/Assets/folder/path

Or put the Augmenta Folder in your UnityProject

## Shortcuts 
-
## Unity Plugins 
-
## Dev information 

### AugmentaOSC

Is just here to allow your app to communicate over the OSC protocol, since the Augmenta cameras are using it, it's mandatory to have it in your project. You don't have to do anything particular about it except choosing the right port so just put it in your scene.

### AuListener

The AuListener is here to convert OSC messages received by AugmentaOSC into proper variables you'll use in your project.

We don't reccomant using AuListener directly, use AuInterface directly.

### AuInterface

You just have to get an instance of that script in your active scene, then all the properties you want to access are **static** so you won't need a direct reference to it.

The properties you can access are :

Properties      |Type                   |Description                                                       |
----------------|-----------------------|------------------------------------------------------------------|
arrayPersonCubes    |Dictionary<int, GameObject>|A Dictionary containing all the active points detected by Augmenta|
getAugmentaObjects()|Dictionary<int, GameObject>|Same as arrayPerson                                               |
int,GameObject
In addition of those properties there's also 4 events you can subscribe to.

Events       |Type  |Description                                                                         |
-------------|------|------------------------------------------------------------------------------------|
personEntered|int,GameObject|Raising when someone have just been detected by Augmenta                            |
personUpdated|int,GameObject|Raising when variables about a point have just been updated by Augmenta             |
personLeft   |int,GameObject|Raising when someone have just been lost by Augmenta                                |

Now you've got a fully functionnal Augmenta setup.

### Calibrating Augmenta
WIP

## Version 
2017.3.0f3