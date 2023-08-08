using System;
using System.Collections.Generic;
using System.Text;

namespace CPI.Plot3D
{
    /// <summary>
    /// Represents an orientation in 3D space.
    /// </summary>
    public class Orientation3D : ICloneable
    {
        # region Constants

        const double PiDividedBy180 = Math.PI / 180;

        # endregion

        # region Private Fields

        // These two unit vectors establish our orientation.
        // These will move around as our direction changes.
        Vector3D forwardVector;
        Vector3D downVector;

        // This enumeration specifies whether we interpret angles as degrees or radians
        AngleMeasurement angleMeasurement;

        # endregion

        # region Constructors

        /// <summary>
        /// Instantiates a new Orientation object.
        /// </summary>
        public Orientation3D()
        {
            forwardVector = new Vector3D(1, 0, 0);
            downVector = new Vector3D(0, 0, 1);

            angleMeasurement = AngleMeasurement.Degrees;
        }

        /// <summary>
        /// Creates a deep copy of an existing orientation object.
        /// </summary>
        /// <param name="source">The source orientation for the copy.</param>
        private Orientation3D(Orientation3D source)
        {
            forwardVector = source.ForwardVector;
            downVector = source.downVector;

            angleMeasurement = source.angleMeasurement;
        }

        # endregion

        # region Properties

        /// <summary>
        /// Gets or sets whether angles are measured in degrees or radians.
        /// </summary>
        public AngleMeasurement AngleMeasurement
        {
            get
            {
                return angleMeasurement;
            }
            set
            {
                if (value == AngleMeasurement.Degrees || value == AngleMeasurement.Radians)
                    angleMeasurement = value;
                else
                    throw new ArgumentException("AngleMeasurement must be either Degrees or Radians.");
            }
        }

        /// <summary>
        /// Gets a unit vector representing the forward direction from the object's perspective.
        /// </summary>
        public Vector3D ForwardVector
        {
            get
            {
                return this.forwardVector;
            }
        }

        /// <summary>
        /// Gets a unit vector representing the backward direction from the object's perspective.
        /// </summary>
        public Vector3D BackwardVector
        {
            get
            {
                return -this.forwardVector;
            }
        }

        /// <summary>
        /// Gets a unit vector representing the left direction from the object's perspective.
        /// </summary>
        public Vector3D LeftVector
        {
            get
            {
                return -this.RightVector;
            }
        }

        /// <summary>
        /// Gets a unit vector representing the right direction from the object's perspective.
        /// </summary>
        public Vector3D RightVector
        {
            get
            {
                return this.DownVector.CrossProduct(this.ForwardVector);
            }
        }

        /// <summary>
        /// Gets a unit vector representing the up direction from the object's perspective.
        /// </summary>
        public Vector3D UpVector
        {
            get
            {
                return -this.downVector;
            }
        }

        /// <summary>
        /// Gets a unit vector representing the down direction from the object's perspective.
        /// </summary>
        public Vector3D DownVector
        {
            get
            {
                return this.downVector;
            }
        }

        # endregion

        # region Methods

        /// <summary>
        /// Rotates right around the up/down axis.
        /// </summary>
        /// <param name="angle">
        /// The rotation angle in degrees or radians depending on the value
        /// of the AngleMeasurement property.
        /// </param>
        public void YawRight(double angle)
        {
            if (this.angleMeasurement == AngleMeasurement.Degrees)
                angle = DegreesToRadians(angle);

            forwardVector = forwardVector.Rotate(downVector, angle);
        }

        /// <summary>
        /// Rotates left around the up/down axis.
        /// </summary>
        /// <param name="angle">
        /// The rotation angle in degrees or radians depending on the value
        /// of the AngleMeasurement property.
        /// </param>
        public void YawLeft(double angle)
        {
            YawRight(-angle);
        }

        /// <summary>
        /// Rotates up around the left/right axis.
        /// </summary>
        /// <param name="angle">
        /// The rotation angle in degrees or radians depending on the value
        /// of the AngleMeasurement property.
        /// </param>
        public void PitchUp(double angle)
        {
            if (this.angleMeasurement == AngleMeasurement.Degrees)
                angle = DegreesToRadians(angle);

            Vector3D rightVectorPreRotation = this.RightVector;

            forwardVector = forwardVector.Rotate(rightVectorPreRotation, angle);
            downVector = downVector.Rotate(rightVectorPreRotation, angle);
        }

        /// <summary>
        /// Rotates down around the left/right axis.
        /// </summary>
        /// <param name="angle">
        /// The rotation angle in degrees or radians depending on the value
        /// of the AngleMeasurement property.
        /// </param>
        public void PitchDown(double angle)
        {
            PitchUp(-angle);
        }

        /// <summary>
        /// Rotates right around the forward/backward axis.
        /// </summary>
        /// <param name="angle">
        /// The rotation angle in degrees or radians depending on the value
        /// of the AngleMeasurement property.
        /// </param>
        public void RollRight(double angle)
        {
            if (this.angleMeasurement == AngleMeasurement.Degrees)
                angle = DegreesToRadians(angle);

            downVector = downVector.Rotate(forwardVector, angle);
        }

        /// <summary>
        /// Rotates left around the forward/backward axis.
        /// </summary>
        /// <param name="angle">
        /// The rotation angle in degrees or radians depending on the value
        /// of the AngleMeasurement property.
        /// </param>
        public void RollLeft(double angle)
        {
            RollRight(-angle);
        }

        # endregion

        # region Static Methods

        /// <summary>
        /// Converts from degrees to radians.
        /// </summary>
        /// <param name="degrees">An angle specified in degrees.</param>
        /// <returns>An angle specified in radians.</returns>
        public static double DegreesToRadians(double degrees)
        {
            return degrees * PiDividedBy180;
        }

        /// <summary>
        /// Converts from radians to degrees.
        /// </summary>
        /// <param name="radians">An angle specified in radians.</param>
        /// <returns>An angle specified in degrees.</returns>
        public static double RadiansToDegrees(double radians)
        {
            return radians / PiDividedBy180;
        }

        # endregion

        #region ICloneable Members

        /// <summary>
        /// Performs a deep copy of the Orientation object.
        /// </summary>
        /// <returns>A deep copy of the Orientation object.</returns>
        public Orientation3D Clone()
        {
            return new Orientation3D(this);
        }

        /// <summary>
        /// Performs a deep copy of the Orientation object.
        /// </summary>
        /// <returns>A deep copy of the Orientation object.</returns>
        object ICloneable.Clone()
        {
            return this.Clone();
        }

        #endregion
    }
}
