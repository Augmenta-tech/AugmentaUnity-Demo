using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Augmenta
{
    /// <summary>
    /// Interface to implement custom spawn and destroy behaviour for Augmenta objects
    /// </summary>
    public interface IAugmentaObjectBehaviour
    {
        /// <summary>
        /// Called when the object is instantiated
        /// </summary>
        void Spawn();

        /// <summary>
        /// Called when the object should be destroyed
        /// </summary>
        void Destroy();
    }
}
