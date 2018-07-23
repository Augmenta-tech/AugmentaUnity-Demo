using System;
using System.Collections.Generic;
using UnityEngine;

namespace Augmenta
{
	public class AugmentaPerson
	{
		public int pid;
		public int oid;
		public int age;
		public Vector3 centroid;
        
		public float depth;
		public Rect boundingRect;
		public Vector3 highest;		
		public float inactiveTime;

        public Vector3 Position;
        public float VelocitySmooth;
        public List<Vector3> Velocities;

        public void Init()
        {
            Velocities = new List<Vector3>();
        }

        public void AddVelocity(Vector3 velocity)
        {
            Velocities.Add(velocity);
            if (Velocities.Count > VelocitySmooth)
                Velocities.RemoveAt(0);
        }

        public Vector3 GetSmoothedVelocity()
        {
            var meanVelocity = new Vector3();
            foreach (var rawVelocity in Velocities)
                meanVelocity += rawVelocity;

            meanVelocity /= Velocities.Count;

            return meanVelocity;
        }

        public Vector3 GetLastVelocity()
        {
            if (Velocities.Count == 0)
                return new Vector3(0f, 0f);

            return Velocities[Velocities.Count - 1];
        }
    }
}