using System;
using System.Collections.Generic;
using UnityEngine;

namespace Augmenta
{
	public class Person
	{
		public int pid;
		public int oid;
		public int age;
		public Vector2 centroid;
		public Vector2 velocity;
		public float depth;
		public Rect boundingRect;
		public Vector3 highest;		
		public float lastUpdateTime;

		private float lastDepth;
		private Vector2 lastCentroid;
		private Vector3 lastHighest;
		private Rect lastBoundingRect;


		// Exponential smooth people's data
		public void smooth(float amount){	

			// Check if "last" values have been initialized
			if(lastDepth != 0 && lastCentroid != new Vector2() && lastHighest != new Vector3() && lastBoundingRect != new Rect()){
				
				// Apply smooth
				depth = depth*(1-amount) + lastDepth * amount;
				centroid = centroid*(1-amount) + lastCentroid * amount;
				highest = highest*(1-amount) + lastHighest * amount;
				boundingRect.position = boundingRect.position*(1-amount) + lastBoundingRect.position * amount;
				boundingRect.width = boundingRect.width*(1-amount) + lastBoundingRect.width * amount;
				boundingRect.height = boundingRect.height*(1-amount) + lastBoundingRect.height * amount;
				
				// Recalculate smoothed velocity
				velocity = centroid - lastCentroid;
				
			}
			
			// Save current values as last values for next frame
			lastDepth = depth;
			lastCentroid = centroid;
			lastHighest = highest;
			lastBoundingRect = boundingRect;
			
		}
	}
}