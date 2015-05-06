Augmenta for Unity
=======================

A [Unity][] Augmenta helper library and example maintained by [Théoriz studio][]

Install
-------

Install [Unity][], then get the package from here

```
// Add link to package release
```

How to connect your app with Augmenta
-------------------------------------

#### IMPORT

- Open Unity, then double-click on the package or or load it via "Assets -> Import package -> Custom package"
- Click "Import"
- The Augmenta library will be added in your plugins folder

#### SETUP

- If you haven't started developing your own app yet, you can copy the "AugmentaExample2D" or "AugmentaExample3D" scene and start working from here
- If you're integrating Augmenta into an existing project, you have to :
    > Drag the "OscReceiver" prefab into your scene (you can also use it to receive your own Osc messages)
    > Drag the "AugmentaReceiver" prefab into your scene

#### CLASSES

- Inside the "AugmentaReceiver" object is the "InteractiveArea" object : it is the virtual equivalent of the area where your people/objects are moving in real life. You can move the "InteractiveArea" wherever you like in your scene and virtual people will appear on it.
- Attached to the "AugmentaReceiver" is a script called "auInterface", which takes care of instanciating and updating cubes representing the people/objects in the real world scene. The easiest way to access information is through this interface. You may want to hide the debug cubes by turning the "debug" variable to "false".

     
4a --- USING AUGMENTA BY GETTING ALL THE OBJECTS IN THE SCENE ---

- Get the dictionary of the Augmenta objects in the scene by doing :
```
    Dictionary<int,GameObject> myAugmentaObjects = auInterface.getAugmentaObjects();
```

- Loop through it to read the informations :
```
    foreach(KeyValuePair<int, GameObject> pair in myAugmentaObjects) {
        Debug.Log("The point with id ["+pair.Key+"] is located at : x="+pair.Value.GetComponent<PersonObject>().getCentroid().x+" and y="+pair.Value.GetComponent<PersonObject>().getCentroid().z);
        Debug.Log ("He's "+pair.Value.GetComponent<PersonObject>().getAge()+" frames old");
    }
```

4b --- USING AUGMENTA BY LISTENING TO THE MESSAGES ---

- Add the new functions : "ObjectEntered(GameObject o)", "ObjectUpdated(GameObject o)", and "ObjectWillLeave(int id)" to your code
- Each function will be called by the auInterface when objects are added/moved/removed, which can be useful especially when you have to instanciate one object per person in the scene
- As you may have noticed, when the object is about to be removed, you'll have to rely on its ID only.


4c --- USING AUGMENTA BY DOING YOUR CUSTOM CODE ---

- If you want to bypass the auInterface and the InteractiveArea, we're okay with that : you just have to know that the position informations will be given between 0 an 1
- You can listen to the messages broadcasted by the auListener : PersonEntered(Person p) / PersonUpdated(Person p) / PersonWillLeave(Person p)
- Or access the Persons array like this :
```
     foreach(KeyValuePair<int, Person> pair in auListener.getPeopleArray()) {
        Debug.Log("The point with id ["+pair.Key+"] has raw values : x="+pair.Value.centroid.x+" and y="+pair.Value.centroid.y);
     }
```

Documentation
-------------

https://github.com/Theoriz/Augmenta/wiki

Contribute
----------

Check [TODO](TODO.md) and [TOFIX](TOFIX.md), then

```
git clone https://github.com/Lyptik/augmentaUnity.git
```

Thanks
------

Osc implementation based on OSCuMote (which is based on some TUIO C# code) :
http://itu.dk/people/jzso/OSCuMote/oscumote.html

Thanks to the guys at [OpenTSPS][], this library is heavily inspired from it.

Thanks to the devs and beta testers whose contribution are vitals to the project
 Tom Duchêne / David-Alexandre Chanel / Jonathan Richer / you !

[Unity]: http://http://unity3d.com/
[Théoriz studio]: http://www.theoriz.com/
[OpenTSPS]: https://github.com/labatrockwell/openTSPS/
