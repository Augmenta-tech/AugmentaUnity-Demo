Augmenta for Processing
=======================

A [Unity][] Augmenta helper library and example maintained by [Théoriz studio][]

Install
-------

Install [Unity][], then get the package from here

```
// Add link to package release
```

Use
---

Open your Unity project, then double click on the package

Enter the port where you are receiving Augmenta data

Example
-------

// Describe each object and workflow to use for your own project

// Insert Gifs :)

Data
----

```
    * Augmenta OSC Protocol :

        /au/personWillLeave/ args0 arg1 ... argn
        /au/personUpdated/   args0 arg1 ... argn
        /au/personEntered/   args0 arg1 ... argn

        where args are :

        
        0: pid (int)                        // Personal ID ex : 42th person to enter stage has pid=42
        1: oid (int)                        // Ordered ID ex : if 3 person on stage, 43th person has oid=2
        2: age (int)                        // Time on stage (in frame number)
        3: centroid.x (float)               // Position projected to the ground
        4: centroid.y (float)               
        5: velocity.x (float)               // Speed and direction vector
        6: velocity.y (float)
        7: depth (float)                    // Distance to sensor (in m)
        8: boundingRect.x (float)           // Bounding box on the ground (top left coordinate)
        9: boundingRect.y (float)
        10: boundingRect.width (float)
        11: boundingRect.height (float)
        12: highest.x (float)               // Highest point placement
        13: highest.y (float)
        14: highest.z (float)               // Height

        /au/scene/   args0 arg1 ... argn

        0: currentTime (int)                // Time (in frame number)
        1: percentCovered (float)           // Percent covered
        2: numPeople (int)                  // Number of person
        3: averageMotion.x (float)          // Average motion
        4: averageMotion.y (float)
        5: scene.width (int)                // Scene size
        6: scene.height (int)
```

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
