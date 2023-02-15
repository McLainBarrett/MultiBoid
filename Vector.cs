using System;

public class Vector {
	public float x = 0;
	public float y = 0;
	
	public Vector(float x = 0, float y = 0) {
		this.x = x; this.y = y;
	}

	public Vector(Vector vector) {
		x = vector.x; y = vector.y;
	}

	public Vector((float bearing, float speed) vec) {
		var d2r = Math.PI / 180;
		x = (float)(vec.speed * Math.Cos(vec.bearing * d2r));
		y = (float)(vec.speed * Math.Sin(vec.bearing * d2r));
	}


	public static Vector operator +(Vector a, Vector b) {
		return new Vector(a.x + b.x, a.y + b.y);
	}

	public static Vector operator -(Vector a, Vector b) {
		return new Vector(a.x - b.x, a.y - b.y);
	}

	public static Vector operator *(Vector a, float b) {
		return new Vector(a.x * b, a.y * b);
	}

	public static Vector operator /(Vector a, float b) {
		return new Vector(a.x / b, a.y / b);
	}


	public float SqrLength { get { return x * x + y * y;} }
	public float Length { get { return (float)Math.Sqrt(SqrLength); } }
	public float Angle { get { return (float)(Math.Atan2(y, x) * 180 / Math.PI); } }


	public Vector Normalized { get { return new Vector(x, y) / ((Length != 0) ? Length : 1); } }
	public Vector Rotate(float angle) {
		double d2r = Math.PI / 180;
		return new Vector((float)(x*Math.Cos(angle * d2r) - y*Math.Sin(angle * d2r)), 
							(float)(x*Math.Sin(angle * d2r) + y * Math.Cos(angle * d2r)));
	}


	public override string ToString() { return String.Format("({0}, {1})", x, y); }
}