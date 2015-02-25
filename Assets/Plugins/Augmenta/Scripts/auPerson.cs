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
		public int inactiveTime;
	}
}