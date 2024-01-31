using System;

namespace CPI.Plot3D;

// https://www.codeproject.com/articles/14397/a-3d-plotting-library-in-c
/// <summary>
/// Represents a distance and direction in 3 dimensions.
/// </summary>
/// <see>
/// 3D Math Primer for Graphics and Game Development
/// by Fletcher Dunn and Ian Parberry
/// Chapters 4-6
/// </see>
/// <remarks>
/// This struct is the primary building block for just about every interesting thing that
/// this library can do.  If you're interested in understanding how and why this
/// library works, I suggest that you first become intimately aware of every 
/// method in this structure.  There's nothing particularly complex that happens here;
/// it's all pretty straightforward trigonometry.  But it gets used to great effect by
/// the Orientation class and the Plotter class, so you should really understand what
/// it's doing.
/// </remarks>
public struct Vector3D : IEquatable<Vector3D>
{
    #region Constants

    /// <summary>
    /// The maximum distance two coordinates can be from each other
    /// for them to be considered approximately equal.
    /// </summary>
    public const float Tolerance = Point3D.Tolerance;

    #endregion

    #region Static Fields

    /// <summary>
    /// The zero vector, which represents neither distance nor direction.
    /// </summary>
    public static readonly Vector3D Zero = new(0f, 0f, 0f);

    /// <summary>
    /// A unit vector pointing in a positive direction along the X axis.
    /// </summary>
    public static readonly Vector3D PositiveX = new(1f, 0f, 0f);

    /// <summary>
    /// A unit vector pointing in a negative direction along the X axis.
    /// </summary>
    public static readonly Vector3D NegativeX = new(-1f, 0f, 0f);

    /// <summary>
    /// A unit vector pointing in a positive direction along the Y axis.
    /// </summary>
    public static readonly Vector3D PositiveY = new(0f, 1f, 0f);

    /// <summary>
    /// A unit vector pointing in a negative direction along the Y axis.
    /// </summary>
    public static readonly Vector3D NegativeY = new(0f, -1f, 0f);

    /// <summary>
    /// A unit vector pointing in a positive direction along the Z axis.
    /// </summary>
    public static readonly Vector3D PositiveZ = new(0f, 0f, 1f);

    /// <summary>
    /// A unit vector pointing in a negative direction along the Z axis.
    /// </summary>
    public static readonly Vector3D NegativeZ = new(0f, 0f, -1f);

    #endregion

    #region Private Fields

    private readonly Point3D endPoint;

    #endregion

    #region Constructors

    /// <summary>
    /// Create an instance of Vector3D with the specified x, y, z coordinates.
    /// </summary>
    /// <param name="x">The X coordinate of the endpoint</param>
    /// <param name="y">The Y coordinate of the endpoint</param>
    /// <param name="z">The Z coordinate of the endpoint</param>
    public Vector3D(float x, float y, float z) : this(new Point3D(x, y, z)) { }

    /// <summary>
    /// Create an instance of Vector3D with the specified endpoint.
    /// </summary>
    /// <param name="endPoint">The endpoint of the vector.</param>
    public Vector3D(Point3D endPoint)
    {
        this.endPoint = endPoint;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The X coordinate of the vector.
    /// </summary>
    public float X
    {
        get
        {
            return endPoint.X;
        }
    }

    /// <summary>
    /// The Y coordinate of the vector.
    /// </summary>
    public float Y
    {
        get
        {
            return endPoint.Y;
        }
    }

    /// <summary>
    /// The Z coordinate of the vector.
    /// </summary>
    public float Z
    {
        get
        {
            return endPoint.Z;
        }
    }

    /// <summary>
    /// The length of the vector.
    /// </summary>
    public double Length
    {
        get
        {
            double doubleX = (double)endPoint.X;
            double doubleY = (double)endPoint.Y;
            double doubleZ = (double)endPoint.Z;

            return Math.Sqrt(doubleX * doubleX + doubleY * doubleY + doubleZ * doubleZ);
        }
    }

    /// <summary>
    /// The end point of the vector.
    /// </summary>
    public Point3D EndPoint
    {
        get
        {
            return this.endPoint;
        }
    }

    #endregion

    #region Overridden Methods

    /// <summary>
    /// Returns a string representation of the vector's endpoint in [X,Y,Z] format.
    /// </summary>
    /// <returns>A string representing the endpoint's XYZ coordinates.</returns>
    public override string ToString()
    {
        return EndPoint.ToString();
    }
    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object. 
    /// </summary>
    /// <param name="obj">An object to compare to this instance.</param>
    /// <returns>True if the object is a Vector3D and the vectors are the same; false otherwise.</returns>
    public override bool Equals(object obj)
    {
        if (obj is Vector3D)
            return this.Equals((Vector3D)obj);
        else
            return false;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    /// <remarks>
    /// Since the Vector3D's equality methods are based on its underlying Point3D endPoint, it stands
    /// to reason that its hash code also needs to be based on the the endPoint.  But we don't just
    /// want to return the same number for the two different classes.  So instead, we get the hash 
    /// code for the endPoint and flip all its bits, which gives us a very different number, but 
    /// very closely related to our source.
    /// That means that for any Vector3D v, v.GetHashCode() ^ v.endPoint.GetHashCode() == -1,
    /// in case you're interested.
    /// </remarks>
    public override int GetHashCode()
    {
        return ~endPoint.GetHashCode();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Determines whether this instance is very nearly equal to a specified Vector3D structure.
    /// </summary>
    /// <remarks>
    /// This is implemented by performaning an ApproximatelyEquals call on the Point3D endPoints
    /// of the two vectors.
    /// </remarks>
    /// <param name="other">A Vector3D structure to compare to this instance.</param>
    /// <returns>True if the vectors are approximately equal; false otherwise.</returns>
    public bool ApproximatelyEquals(Vector3D other)
    {
        return this.endPoint.ApproximatelyEquals(other.endPoint);
    }

    /// <summary>
    /// Returns a string representation of the vector's endpoint in [X,Y,Z] format, 
    /// with each coordinate rounded to the specified number of digits.
    /// </summary>
    /// <param name="decimalPlaces">The number of decimal digits to round the output to.</param>
    /// <returns>A string representing the endpoint's XYZ coordinates.</returns>
    public string ToString(int decimalPlaces)
    {
        return EndPoint.ToString(decimalPlaces);
    }


    /// <summary>
    /// Returns a Vector3D with the same direction as this vector, but with a length of 1.
    /// (Otherwise known as a unit vector.)
    /// </summary>
    /// <returns>A unit vector with the same direction as the current vector.</returns>
    /// <see>http://en.wikipedia.org/wiki/Unit_vector</see>
    /// <see>
    /// 3D Math Primer for Graphics and Game Development
    /// by Fletcher Dunn and Ian Parberry
    /// pp. 53-4
    /// </see>
    public Vector3D Normalize()
    {
        double divisor = this.Length;

        if (divisor == 0)
        {
            throw new InvalidOperationException("Can't normalize a zero vector.");
        }

        return this / divisor;
    }

    /// <summary>
    /// Returns the cross product of two vectors, which is the vector which is perpendicular
    /// to the two vectors with a magnitude equal to the area of the parallelogram they span.
    /// </summary>
    /// <param name="a">A vector object.</param>
    /// <returns>The cross product of the two vectors.</returns>
    /// <see>http://en.wikipedia.org/wiki/Cross_product</see>
    /// <see>
    /// 3D Math Primer for Graphics and Game Development
    /// by Fletcher Dunn and Ian Parberry
    /// pp. 62-5
    /// </see>
    public Vector3D CrossProduct(Vector3D a)
    {
        return new Vector3D(
            (float)((double)this.Y * (double)a.Z - (double)this.Z * (double)a.Y),
            (float)((double)this.Z * (double)a.X - (double)this.X * (double)a.Z),
            (float)((double)this.X * (double)a.Y - (double)this.Y * a.X)
            );
    }

    /// <summary>
    /// Rotates the vector around an arbitrary axis.
    /// </summary>
    /// <param name="rotationAxis">A Vector3D representing the axis to rotate around.</param>
    /// <param name="rotationAngle">The angle, in radians, to rotate the vector.</param>
    /// <returns>A rotated vector.</returns>
    /// <remarks>
    /// If you have the book "3D Math Primer for Graphics and Game Development", it might be useful
    /// to look at figure 8.12 on page 110 for a visual of what we're actually doing in this method.  
    /// Here's a translation between the variable names in this method and the variable names 
    /// in the diagram:
    /// 
    /// this = v
    /// rotationAxis = n
    /// parallelVector = v[parallel]
    /// perpendicularVector = v[perpendicular]
    /// mutuallyPerpendicularVector = w
    /// rotatedPerpendicularVector = v'[perpendicular]
    /// rotatedVector = v'
    /// 
    /// </remarks>
    /// <see>
    /// 3D Math Primer for Graphics and Game Development
    /// by Fletcher Dunn and Ian Parberry
    /// pp. 109-111
    /// </see>
    public Vector3D Rotate(Vector3D rotationAxis, double rotationAngle)
    {
        if (double.IsInfinity(rotationAngle) || double.IsNaN(rotationAngle))
            throw new ArgumentException("Rotation angle cannot be Infinity or NaN.", "rotationAngle.");

        if (rotationAxis == Vector3D.Zero)
            throw new ArgumentException("You can't rotate around a zero vector.", "rotationAxis");

        // Make sure our rotation axis is a unit vector.
        rotationAxis = rotationAxis.Normalize();

        // Project this vector onto the rotation axis using the dot product 
        // of this vector and the rotation axis.
        Vector3D parallelVector = (this * rotationAxis) * rotationAxis;

        // Project this vector onto the plane perpendicular to the rotation axis by subtracting the
        // parallel vector from the original vector.
        Vector3D perpendicularVector = this - parallelVector;

        // Calculate a vector that is mutually perpendicular to parallelVector and perpendicularVector.
        // We can do this with a cross product.
        Vector3D mutuallyPerpendicularVector = rotationAxis.CrossProduct(perpendicularVector);

        // Calculate the vector that represents the portion of our rotated vector that is perpendicular
        // to our rotation vector.
        Vector3D rotatedPerpendicularVector =
            (Math.Cos(rotationAngle) * perpendicularVector)
            + (Math.Sin(rotationAngle) * mutuallyPerpendicularVector);

        // Now we finish up by calculating the completely rotated vector.
        Vector3D rotatedVector = rotatedPerpendicularVector + parallelVector;

        return rotatedVector;
    }

    #endregion

    #region Overloaded Operators

    /// <summary>
    /// Determines whether the specified vectors are equal.
    /// </summary>
    /// <param name="a">The first Vector3D instance to compare.</param>
    /// <param name="b">The second Vector3D instance to compare.</param>
    /// <returns>True if the Vector3D instances are equal; false otherwise.</returns>
    public static bool operator ==(Vector3D a, Vector3D b)
    {
        return a.Equals(b);
    }

    /// <summary>
    /// Determines whether the specified vectors are unequal.
    /// </summary>
    /// <param name="a">The first Vector3D instance to compare.</param>
    /// <param name="b">The second Vector3D instance to compare.</param>
    /// <returns>True if the Vector3D instances are unequal; false otherwise.</returns>
    public static bool operator !=(Vector3D a, Vector3D b)
    {
        return !(a == b);
    }

    /// <summary>
    /// Moves a point the length of a specified vector.
    /// </summary>
    /// <param name="a">A Point3D object.</param>
    /// <param name="b">A Vector3D object.</param>
    /// <returns>A Point3D object moved the distance and direction specified by the vector.</returns>
    public static Point3D operator +(Point3D a, Vector3D b)
    {
        float newX = a.X + b.X;
        float newY = a.Y + b.Y;
        float newZ = a.Z + b.Z;

        if (float.IsInfinity(newX) || float.IsNaN(newX)
            || float.IsInfinity(newY) || float.IsNaN(newY)
            || float.IsInfinity(newZ) || float.IsNaN(newZ))
        {
            throw new OverflowException(string.Format("{0} + {1} produced an arithmetic overflow.", a.ToString(), b.ToString()));
        }

        return new Point3D(newX, newY, newZ);
    }

    /// <summary>
    /// Moves a point the length of the inverse of a specifed vector.
    /// </summary>
    /// <param name="a">A Point3D object.</param>
    /// <param name="b">A Vector3D object.</param>
    /// <returns>A Point3D object moved the distance and the inverse of the direction specifed by the vector.</returns>
    public static Point3D operator -(Point3D a, Vector3D b)
    {
        float newX = a.X - b.X;
        float newY = a.Y - b.Y;
        float newZ = a.Z - b.Z;

        if (float.IsInfinity(newX) || float.IsNaN(newX)
            || float.IsInfinity(newY) || float.IsNaN(newY)
            || float.IsInfinity(newZ) || float.IsNaN(newZ))
        {
            throw new OverflowException(string.Format("{0} - {1} produced an arithmetic overflow.", a.ToString(), b.ToString()));
        }

        return new Point3D(newX, newY, newZ);
    }

    /// <summary>
    /// Multiplying a vector by a scalar results in a vector with the same (or exactly opposite)
    /// direction as the original, but with a different length.
    /// </summary>
    /// <param name="a">A Vector3D object.</param>
    /// <param name="b">A double.</param>
    /// <returns>
    /// A Vector3D instance with the same (or exactly opposite) direction as the original, 
    /// but with a length multiplied by the specified number.
    /// </returns>
    /// <see>
    /// 3D Math Primer for Graphics and Game Development
    /// by Fletcher Dunn and Ian Parberry
    /// pp. 51-2
    /// </see>
    public static Vector3D operator *(Vector3D a, double b)
    {
        if (double.IsInfinity(b) || double.IsNaN(b))
            throw new ArgumentException("Multiplier cannot be Infinity or NaN.", "b");

        return new Vector3D((float)((double)a.X * b), (float)((double)a.Y * b), (float)((double)a.Z * b));
    }

    /// <summary>
    /// Multiplying a vector by a scalar results in a vector with the same (or exactly opposite)
    /// direction as the original, but with a different length.
    /// </summary>
    /// <param name="a">A double.</param>
    /// <param name="b">A Vector3D object.</param>
    /// <returns>
    /// A Vector3D instance with the same (or exactly opposite) direction as the original, 
    /// but with a length multiplied by the specified number.
    /// </returns>
    /// <see>
    /// 3D Math Primer for Graphics and Game Development
    /// by Fletcher Dunn and Ian Parberry
    /// pp. 51-2
    /// </see>
    public static Vector3D operator *(double a, Vector3D b)
    {
        return b * a;
    }

    /// <summary>
    /// Taking the inverse of a vector maintains its length, but reverses its direction.  
    /// (It's the same as multiplying the vector by -1.)
    /// </summary>
    /// <param name="a">A Vector3D object.</param>
    /// <returns>The inverse of the specified vector.</returns>
    public static Vector3D operator -(Vector3D a)
    {
        return new Vector3D(-a.X, -a.Y, -a.Z);
    }

    /// <summary>
    /// Adds two vectors.
    /// </summary>
    /// <param name="a">A Vector3D object.</param>
    /// <param name="b">A Vector3D object.</param>
    /// <returns>The sum of the two vectors.</returns>
    /// <see>
    /// 3D Math Primer for Graphics and Game Development
    /// by Fletcher Dunn and Ian Parberry
    /// pp. 54-7
    /// </see>
    public static Vector3D operator +(Vector3D a, Vector3D b)
    {
        return new Vector3D(a.endPoint + b);
    }

    /// <summary>
    /// Subtracts a vector from another.
    /// </summary>
    /// <param name="a">A Vector3D object.</param>
    /// <param name="b">A Vector3D object.</param>
    /// <returns>The results of subtracting the second vector from the first.</returns>
    /// <see>
    /// 3D Math Primer for Graphics and Game Development
    /// by Fletcher Dunn and Ian Parberry
    /// pp. 54-7
    /// </see>
    public static Vector3D operator -(Vector3D a, Vector3D b)
    {
        return new Vector3D(a.EndPoint - b);
    }

    /// <summary>
    /// Calculates the dot product of two vectors.
    /// </summary>
    /// <param name="a">A Vector3D object.</param>
    /// <param name="b">A Vector3D object.</param>
    /// <returns>A double representing the dot product of the two vectors.</returns>
    /// <remarks>
    /// The dot product comes in very handy when projecting one vector onto another, and it's
    /// worth taking the time to understand its geometric interpretation.
    /// </remarks>
    /// <see>
    /// 3D Math Primer for Graphics and Game Development
    /// by Fletcher Dunn and Ian Parberry
    /// pp. 58-62
    /// </see>
    public static double operator *(Vector3D a, Vector3D b)
    {
        double axDouble = (double)a.X;
        double bxDouble = (double)b.X;
        double ayDouble = (double)a.Y;
        double byDouble = (double)b.Y;
        double azDouble = (double)a.Z;
        double bzDouble = (double)b.Z;

        // Dot product
        return axDouble * bxDouble + ayDouble * byDouble + azDouble * bzDouble;
    }

    /// <summary>
    /// Dividing a vector by a scalar results in a vector with the same (or exactly opposite)
    /// direction as the original, but with a different length.
    /// </summary>
    /// <param name="a">A Vector3D object.</param>
    /// <param name="b">A double.</param>
    /// <returns>
    /// A Vector3D instance with the same (or exactly opposite) direction as the original, 
    /// but with a length divided by the specified number.
    /// </returns>
    /// <remarks>
    /// Dividing a vector by a scalar is the same as multiplying the vector by 1 divided by the scalar.
    /// </remarks>
    /// <see>
    /// 3D Math Primer for Graphics and Game Development
    /// by Fletcher Dunn and Ian Parberry
    /// pp. 51-2
    /// </see>
    public static Vector3D operator /(Vector3D a, double b)
    {
        double oneOverB = 1.0 / b;

        if (double.IsInfinity(oneOverB))
            throw new DivideByZeroException("Can't divide a vector by zero.");

        return a * oneOverB;
    }


    #endregion

    #region IEquatable<Vector3D> Members

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified Vector3D structure. 
    /// </summary>
    /// <param name="other">A Vector3D structure to compare to this instance.</param>
    /// <returns>True if the vectors are the same; false otherwise.</returns>
    /// <remarks>
    /// This is implemented by simply calling the Equals method 
    /// on the Point3D endpoints of the two vectors.
    /// </remarks>
    public bool Equals(Vector3D other)
    {
        return this.endPoint.Equals(other.endPoint);
    }

    #endregion
}
