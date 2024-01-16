using System;
using System.Drawing;

namespace CPI.Plot3D;

/// <summary>
/// Represents a location in 3D space.
/// </summary>
public struct Point3D : IEquatable<Point3D>
{
    #region Constants

    /// <summary>
    /// The maximum distance two coordinates can be from each other
    /// for them to be considered approximately equal.
    /// </summary>
    public const float Tolerance = .000001f;

    #endregion

    #region Private Fields

    private readonly float x;
    private readonly float y;
    private readonly float z;

    #endregion

    #region Constructors

    /// <summary>
    /// Instantiates a new Point3D.
    /// </summary>
    /// <param name="x">The point's X coordinate.</param>
    /// <param name="y">The point's Y coordinate.</param>
    /// <param name="z">The point's Z coordinate.</param>
    public Point3D(float x, float y, float z)
    {
        // We don't allow Infinity or NAN as coordinates
        if (float.IsInfinity(x) || float.IsNaN(x))
            throw new ArgumentException("Argument x cannot be infinity or NAN");
        if (float.IsInfinity(y) || float.IsNaN(y))
            throw new ArgumentException("Argument y cannot be infinity or NAN");
        if (float.IsInfinity(z) || float.IsNaN(z))
            throw new ArgumentException("Argument z cannot be infinity or NAN");

        this.x = x;
        this.y = y;
        this.z = z;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The point's X coordinate.
    /// </summary>
    public float X
    {
        get
        {
            return x;
        }
    }

    /// <summary>
    /// The point's Y coordinate.
    /// </summary>
    public float Y
    {
        get
        {
            return y;
        }
    }

    /// <summary>
    /// The point's Z coordinate.
    /// </summary>
    public float Z
    {
        get
        {
            return z;
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets the location of the point projected onto the XY plane at the Z origin, from a specified
    /// camera's perspective.
    /// </summary>
    /// <param name="cameraPosition">The location to base perspective calculations on.</param>
    /// <returns>A PointF representing the projected point.</returns>
    /// <remarks>
    /// Calculations are internally performed with double precision arithmetic, then rounded
    /// down to floats at the end.
    /// </remarks>
    public PointF GetScreenPosition(Point3D cameraPosition)
    {
        PointF returnValue = new();

        returnValue.X = (float)
            (((((double)this.X - (double)cameraPosition.X)
            * (-1 * (double)cameraPosition.Z))
            / ((double)this.Z - (double)cameraPosition.Z))
            + (double)cameraPosition.X);

        returnValue.Y = (float)
            (((((double)this.Y - (double)cameraPosition.Y)
            * (-1 * (double)cameraPosition.Z))
            / ((double)this.Z - (double)cameraPosition.Z))
            + (double)cameraPosition.Y);

        if (float.IsInfinity(returnValue.X) || float.IsNaN(returnValue.X)
            || float.IsInfinity(returnValue.Y) || float.IsNaN(returnValue.Y))
        {
            throw new ArgumentOutOfRangeException("cameraPosition", string.Format("The point cannot be represented with the specified camera position because the X or Y coordinate of the projected point was either Infinity or NaN." + Environment.NewLine + "Object Point: {0}" + Environment.NewLine + "Camera Point {1}", this.ToString(), cameraPosition.ToString()));
        }

        return returnValue;
    }


    /// <summary>
    /// Determines whether this instance is very nearly equal to a specified Point3D structure.
    /// </summary>
    /// <remarks>
    /// Since floating point math is kind of fuzzy, we're taking a "close enough" approach 
    /// to equality with this method.  If the individual coordinates of two points fall within
    /// a small tolerance, we'll consider them to be approximately equal.
    ///  
    /// Remember, though, that the uncertainty here can be cumulative.  For example:
    /// if pointA.Equals(pointB) and pointB.Equals(pointC), then it's an absolute certainty
    /// that pointA.Equals(pointC).
    /// However, if pointD.ApproximatelyEquals(pointE) and pointE.ApproximatelyEquals(pointF),
    /// it is NOT certain whether pointD.ApproximatelyEquals(pointF).
    /// </remarks>
    /// <param name="other">A Point3D structure to compare to this instance.</param>
    /// <returns>True if the X,Y,Z components are approximately equal; false otherwise.</returns>
    public bool ApproximatelyEquals(Point3D other)
    {
        return (
            (Math.Abs(this.X - other.X) < Tolerance)
            && (Math.Abs(this.Y - other.Y) < Tolerance)
            && (Math.Abs(this.Z - other.Z) < Tolerance)
        );
    }

    /// <summary>
    /// Returns a string representation of the point in [X,Y,Z] format, with each coordinate 
    /// rounded to the specified number of digits.
    /// </summary>
    /// <param name="decimalPlaces">The number of decimal digits to round the output to.</param>
    /// <returns>A string representing the point's XYZ coordinates.</returns>
    public string ToString(int decimalPlaces)
    {
        return string.Format("[{0}, {1}, {2}]", ((float)Math.Round(X, decimalPlaces)).ToString(), ((float)Math.Round(Y, decimalPlaces)).ToString(), ((float)Math.Round(Z, decimalPlaces)).ToString());
    }

    #endregion

    #region Overridden Methods

    /// <summary>
    /// Returns a string representation of the point in [X,Y,Z] format.
    /// </summary>
    /// <returns>A string representing the point's XYZ coordinates.</returns>
    public override string ToString()
    {
        return string.Format("[{0}, {1}, {2}]", X.ToString(), Y.ToString(), Z.ToString());
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object. 
    /// </summary>
    /// <param name="obj">An object to compare with this instance. </param>
    /// <returns>True if the object equals this instance; false otherwise.</returns>
    public override bool Equals(object obj)
    {
        if (obj is Point3D)
            return this.Equals((Point3D)obj);
        else
            return false;
    }

    /// <summary>
    /// Returns the hash code for this instance. 
    /// </summary>
    /// <remarks>
    /// The hash code is based on the hash codes of the X, Y, and Z coordinates of the point,
    /// but we can't just XOR them all together, otherwise [3,4,5] would return the same hash 
    /// code as [5,3,4], and we wouldn't want that.  So to get a more even distribution, we 
    /// rotate hashY's bits by 8, and hashZ's bits by 16, then we XOR them all together.
    /// (It's also worth pointing out that we're casting the individual hash codes to uints before
    /// operating on them because we want our shift operations to use unsigned semantics.)
    /// </remarks>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        uint hashX = (uint)x.GetHashCode();
        uint hashY = (uint)y.GetHashCode();
        uint hashZ = (uint)z.GetHashCode();

        return (int)(hashX ^ ((hashY << 10) + (hashY >> 22)) ^ ((hashZ << 20) + (hashZ >> 12)));
    }

    #endregion

    #region Overloaded Operators

    /// <summary>
    /// Determines whether the specified Point3D instances are equal.
    /// </summary>
    /// <param name="a">The first Point3D instance to compare.</param>
    /// <param name="b">The second Point3D instance to compare.</param>
    /// <returns>True if the Point3D instances are equal; false otherwise.</returns>
    public static bool operator ==(Point3D a, Point3D b)
    {
        return a.Equals(b);
    }

    /// <summary>
    /// Determines whether the specified Point3D instances are unequal.
    /// </summary>
    /// <param name="a">The first Point3D instance to compare.</param>
    /// <param name="b">The second Point3D instance to compare.</param>
    /// <returns>True if the Point3D instances are unequal; false otherwise.</returns>
    public static bool operator !=(Point3D a, Point3D b)
    {
        return !a.Equals(b);
    }

    #endregion

    #region IEquatable<Point3D> Members

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified Point3D structure. 
    /// </summary>
    /// <param name="other">A Point3D structure to compare to this instance.</param>
    /// <returns>True if the X,Y,Z components are the same; false otherwise.</returns>
    public bool Equals(Point3D other)
    {
        return (this.x == other.x && this.y == other.y && this.z == other.z);
    }

    #endregion
}
