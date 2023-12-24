using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CPI.Plot3D
{
    /// <summary>
    /// Plots points and draws lines in 3D space.
    /// </summary>
    public class Plotter3D : IDisposable
    {
        # region Private Fields

        // Represents the orientation of the cursor in 3D space.
        Orientation3D orientation;

        // Indicates the pen's current location in 3d coordinates.
        Point3D location = new(0, 0, 0);

        // The pen we use to draw on the canvas.
        private Pen pen;

        // The location of the "camera" that we use to determine perspective.
        // All points are projected onto a "screen" that is the XY plane at the Z origin,
        // and moving the camera around messes with the perspective.
        private Point3D cameraLocation = new(60, 0, -600);

        // If the pen is down, we draw lines when we move forward.  If not, we just change our
        // location without drawing any lines.
        private bool isPenDown = true;

        // The graphics object that we're drawing on.  This can be any graphics object, be it from a
        // windows Form, a Bitmap, or a Metafile.
        Graphics canvas;

        // A rectangle that represents the bounds of the stuff that we've drawn.
        Rectangle boundingBox;

        # endregion

        # region Constructors

        /// <summary>
        /// Instantiates a new Plotter.
        /// </summary>
        /// <param name="canvas">The Graphics object that we want to draw on.</param>
        public Plotter3D(Graphics canvas) : this(canvas, new Pen(Color.Black)) {}

        /// <summary>
        /// Instantiates a new Plotter.
        /// </summary>
        /// <param name="canvas">The Graphics object that we want to draw on.</param>
        /// <param name="pen">The pen we want to use to draw on the canvas.</param>
        public Plotter3D(Graphics canvas, Pen pen) : this(canvas, pen, new Point3D(-30, 0, -600)) {}

        /// <summary>
        /// Instantiates a new Plotter.
        /// </summary>
        /// <param name="canvas">The Graphics object that we want to draw on.</param>
        /// <param name="cameraLocation">The location of the camera that we use to calculate perspective.</param>
        public Plotter3D(Graphics canvas, Point3D cameraLocation) : this(canvas, new Pen(Color.Black), cameraLocation) {}

        /// <summary>
        /// Instantiates a new Plotter.
        /// </summary>
        /// <param name="canvas">The Graphics object that we want to draw on.</param>
        /// <param name="pen">The pen we want to use to draw on the canvas.</param>
        /// <param name="cameraLocation">The location of the camera that we use to calculate perspective.</param>
        public Plotter3D(Graphics canvas, Pen pen, Point3D cameraLocation)
        {
            this.canvas = canvas;
            this.pen = pen;
            this.cameraLocation = cameraLocation;

            this.orientation = new Orientation3D();
        }

        # endregion

        # region Properties

        /// <summary>
        /// Gets or sets the orientation of the cursor.
        /// </summary>
        public Orientation3D Orientation
        {
            get
            {
                return orientation;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value", "Orientation cannot be null.");

                orientation = value;
            }
        }

        /// <summary>
        /// Gets or sets the pen used to draw on the canvas.
        /// </summary>
        public Pen Pen
        {
            get
            {
                return pen;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value", "Pen cannot be null.");

                try
                {
                    pen.Dispose();
                }
                catch (ArgumentException)
                {
                    // If the pen is immutable, like one from the System.Drawing.Pens collection,
                    // it'll throw an exception when we try to dispose it.  I don't think there's
                    // any reasonable way to find out if a pen is immutable other than to try and 
                    // dispose it, though.  Which just seems silly.  Anyway, this exception may 
                    // happen in the normal course of things, in which case we just silently ignore it.
                }

                pen = value;
            }
        }

        /// <summary>
        /// Gets or sets the color of the pen used to draw on the canvas.
        /// </summary>
        public Color PenColor
        {
            get
            {
                return pen.Color;
            }
            set
            {
                Pen newPen = (Pen)pen.Clone();
                newPen.Color = value;

                try
                {
                    pen.Dispose();
                }
                catch (ArgumentException)
                {
                    // If the pen is immutable, like one from the System.Drawing.Pens collection,
                    // it'll throw an exception when we try to dispose it.  I don't think there's
                    // any reasonable way to find out if a pen is immutable other than to try and 
                    // dispose it, though.  Which just seems silly.  Anyway, this exception may 
                    // happen in the normal course of things, in which case we just silently ignore it.
                }

                pen = newPen;
            }
        }

        /// <summary>
        /// Gets or sets the width of the pen used to draw on the canvas.
        /// </summary>
        public float PenWidth
        {
            get
            {
                return pen.Width;
            }
            set
            {
                Pen newPen = (Pen)pen.Clone();
                newPen.Width = value;

                try
                {
                    pen.Dispose();
                }
                catch (ArgumentException)
                {
                    // If the pen is immutable, like one from the System.Drawing.Pens collection,
                    // it'll throw an exception when we try to dispose it.  I don't think there's
                    // any reasonable way to find out if a pen is immutable other than to try and 
                    // dispose it, though.  Which just seems silly.  Anyway, this exception may 
                    // happen in the normal course of things, in which case we just silently ignore it.
                }

                pen = newPen;
            }
        }

        /// <summary>
        /// Gets or sets the drawing mode of the plotter.  If IsPenDown == true, moving the pen
        /// around will draw on the canvas.  If IsPenDown == false, moving the pen around will 
        /// change the position of the pen, but won't draw anything.
        /// </summary>
        public bool IsPenDown
        {
            get
            {
                return isPenDown;
            }
            set
            {
                isPenDown = value;
            }
        }

        /// <summary>
        /// Gets or sets the cursor's location in 3D space.
        /// </summary>
        public Point3D Location
        {
            get
            {
                return location;
            }
            set
            {
                location = value;
            }
        }

        /// <summary>
        /// Gets the location of the camera used to determine perspective
        /// </summary>
        public Point3D CameraLocation
        {
            get
            {
                return this.cameraLocation;
            }
        }

        /// <summary>
        /// Gets the graphics object to draw on.
        /// </summary>
        public Graphics Canvas
        {
            get
            {
                return this.canvas;
            }
        }

        /// <summary>
        /// Gets or sets whether angles are measured in degrees or radians.
        /// </summary>
        public AngleMeasurement AngleMeasurement
        {
            get
            {
                return orientation.AngleMeasurement;
            }
            set
            {
                orientation.AngleMeasurement = value;
            }
        }

        /// <summary>
        /// Gets a rectangle that contains everything that's been drawn so far.
        /// </summary>
        /// <remarks>
        /// The math here is a little imprecise (especially when you're dealing with
        /// a PenWidth > 1) but the box should contain AT LEAST the bounds of the drawing,
        /// possibly with a couple of pixels of padding on each side.
        /// </remarks>
        public Rectangle BoundingBox
        {
            get
            {
                return boundingBox;
            }
        }

        # endregion

        # region Methods

        /// <summary>
        /// Moves the cursor forward, and draws a line from the start point to the end point
        /// if IsPenDown == true.
        /// </summary>
        /// <param name="distance">The distance to move forward.</param>
        public void Forward(double distance)
        {
            Point3D oldLocation = location;

            this.Location += (Orientation.ForwardVector * distance);

            if (IsPenDown)
            {
                canvas.DrawLine(pen, oldLocation.GetScreenPosition(cameraLocation), this.Location.GetScreenPosition(cameraLocation));

                ExpandBoundingBox(oldLocation.GetScreenPosition(this.CameraLocation));
                ExpandBoundingBox(this.Location.GetScreenPosition(this.CameraLocation));
            }
        }

        /// <summary>
        /// Moves the cursor to the specified location, and draws a line from teh start point 
        /// to the end point if IsPenDown == true.
        /// </summary>
        /// <param name="newLocation">The location to move the cursor to.</param>
        /// <remarks>
        /// This method allows you to draw a line to an absolute point, which runs counter
        /// to the spirit of this library.  This library uses relative positioning, which 
        /// allows you to define an object relatively, then move or rotate it however you like,
        /// and that all falls apart as soon as you start using absolute positioning.  Nevertheless,
        /// there are some times when it's useful to be able to draw a line to an absolute position,
        /// so there are times when this method is handy.  But use it sparingly.
        /// </remarks>
        public void MoveTo(Point3D newLocation)
        {
            Point3D oldLocation = location;

            this.Location = newLocation;

            if (IsPenDown)
            {
                canvas.DrawLine(pen, oldLocation.GetScreenPosition(cameraLocation), this.Location.GetScreenPosition(cameraLocation));

                ExpandBoundingBox(oldLocation.GetScreenPosition(this.CameraLocation));
                ExpandBoundingBox(this.Location.GetScreenPosition(this.CameraLocation));
            }
        }

        /// <summary>
        /// Sets the IsPenDown property to true.
        /// </summary>
        /// <remarks>
        /// This method has been included because I think that 
        ///     p.PenDown();
        /// is more readable than
        ///     p.IsPenDown = true;
        /// </remarks>
        public void PenDown()
        {
            this.IsPenDown = true;
        }

        /// <summary>
        /// Sets the IsPenDown property to false.
        /// </summary>
        /// <remarks>
        /// This method has been included because I think that
        ///     p.PenUp();
        /// is more readable than
        ///     p.IsPenDown = false;
        /// </remarks>
        public void PenUp()
        {
            this.IsPenDown = false;
        }

        /// <summary>
        /// Rotates the cursor right, relative to its current orientation.
        /// </summary>
        /// <param name="angle">
        /// The rotation angle in degrees or radians depending on the value
        /// of the AngleMeasurement property.
        /// </param>
        public void TurnRight(double angle)
        {
            Orientation.YawRight(angle);
        }

        /// <summary>
        /// Rotates the cursor left, relative to its current orientation.
        /// </summary>
        /// <param name="angle">
        /// The rotation angle in degrees or radians depending on the value
        /// of the AngleMeasurement property.
        /// </param>
        public void TurnLeft(double angle)
        {
            TurnRight(-angle);
        }

        /// <summary>
        /// Rotates the cursor up, relative to its current orientation.
        /// </summary>
        /// <param name="angle">
        /// The rotation angle in degrees or radians depending on the value
        /// of the AngleMeasurement property.
        /// </param>
        public void TurnUp(double angle)
        {
            Orientation.PitchUp(angle);
        }

        /// <summary>
        /// Rotates the cursor down, relative to its current orientation.
        /// </summary>
        /// <param name="angle">
        /// The rotation angle in degrees or radians depending on the value
        /// of the AngleMeasurement property.
        /// </param>
        public void TurnDown(double angle)
        {
            TurnUp(-angle);
        }

        /// <summary>
        /// Expands the bounding box as you draw so that the box always contains the
        /// drawing entirely.
        /// </summary>
        /// <param name="point">A new point being drawn.</param>
        private void ExpandBoundingBox(PointF point)
        {
            int currentLeft;
            int currentRight;
            int currentTop;
            int currentBottom;

            if (boundingBox == Rectangle.Empty)
            {
                currentLeft = int.MaxValue;
                currentRight = int.MinValue;
                currentTop = int.MaxValue;
                currentBottom = int.MinValue;
            }
            else
            {
                currentLeft = boundingBox.Left;
                currentRight = boundingBox.Right;
                currentTop = boundingBox.Top;
                currentBottom = boundingBox.Bottom;
            }

            int halfPenSize = (int)(this.Pen.Width / 2);

            int newLeft = (int)Math.Floor(Math.Min(point.X - halfPenSize - 2, currentLeft));
            int newRight = (int)Math.Ceiling(Math.Max(point.X + halfPenSize + 1, currentRight));
            int newTop = (int)Math.Floor(Math.Min(point.Y - halfPenSize - 2, currentTop));
            int newBottom = (int)Math.Ceiling(Math.Max(point.Y + halfPenSize + 1, currentBottom));

            boundingBox = Rectangle.FromLTRB(newLeft, newTop, newRight, newBottom);
        }

        # endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes of the object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Disposes of the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    pen.Dispose();
                }
                catch (ArgumentException)
                {
                    // If the pen is immutable, like one from the System.Drawing.Pens collection,
                    // it'll throw an exception when we try to dispose it.  I don't think there's
                    // any reasonable way to find out if a pen is immutable other than to try and 
                    // dispose it, though.  Which just seems silly.  Anyway, this exception may 
                    // happen in the normal course of things, in which case we just silently ignore it.
                }
            }
        }

        #endregion
    }

}
