using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace DualSenseAPI.State
{
    public sealed class DualSenseInputStateButtonDelta
    {
        /// <summary>
        /// The change status of the square button.
        /// </summary>
        public ButtonDeltaState SquareButton { get; private set; } = ButtonDeltaState.NoChange;

        /// <summary>
        /// The change status of the cross button.
        /// </summary>
        public ButtonDeltaState CrossButton { get; private set; } = ButtonDeltaState.NoChange;

        /// <summary>
        /// The change status of the circle button.
        /// </summary>
        public ButtonDeltaState CircleButton { get; private set; } = ButtonDeltaState.NoChange;

        /// <summary>
        /// The change status of the triangle button.
        /// </summary>
        public ButtonDeltaState TriangleButton { get; private set; } = ButtonDeltaState.NoChange;

        /// <summary>
        /// The change status of the D-pad up button.
        /// </summary>
        public ButtonDeltaState DPadUpButton { get; private set; } = ButtonDeltaState.NoChange;

        /// <summary>
        /// The change status of the D-pad right button.
        /// </summary>
        public ButtonDeltaState DPadRightButton { get; private set; } = ButtonDeltaState.NoChange;

        /// <summary>
        /// The change status of the D-pad down button.
        /// </summary>
        public ButtonDeltaState DPadDownButton { get; private set; } = ButtonDeltaState.NoChange;

        /// <summary>
        /// The change status of the D-pad left button.
        /// </summary>
        public ButtonDeltaState DPadLeftButton { get; private set; } = ButtonDeltaState.NoChange;

        /// <summary>
        /// The change status of the L1 button.
        /// </summary>
        public ButtonDeltaState L1Button { get; private set; } = ButtonDeltaState.NoChange;

        /// <summary>
        /// The change status of the R1 button.
        /// </summary>
        public ButtonDeltaState R1Button { get; private set; } = ButtonDeltaState.NoChange;

        /// <summary>
        /// The change status of the L2 button.
        /// </summary>
        public ButtonDeltaState L2Button { get; private set; } = ButtonDeltaState.NoChange;

        /// <summary>
        /// The change status of the R2 button.
        /// </summary>
        public ButtonDeltaState R2Button { get; private set; } = ButtonDeltaState.NoChange;

        /// <summary>
        /// The change status of the create button.
        /// </summary>
        public ButtonDeltaState CreateButton { get; private set; } = ButtonDeltaState.NoChange;

        /// <summary>
        /// The change status of the menu button.
        /// </summary>
        public ButtonDeltaState MenuButton { get; private set; } = ButtonDeltaState.NoChange;

        /// <summary>
        /// The change status of the L3 button.
        /// </summary>
        public ButtonDeltaState L3Button { get; private set; } = ButtonDeltaState.NoChange;

        /// <summary>
        /// The change status of the R2 button.
        /// </summary>
        public ButtonDeltaState R3Button { get; private set; } = ButtonDeltaState.NoChange;

        /// <summary>
        /// The change status of the PlayStation logo button.
        /// </summary>
        public ButtonDeltaState LogoButton { get; private set; } = ButtonDeltaState.NoChange;

        /// <summary>
        /// The change status of the touchpad button.
        /// </summary>
        public ButtonDeltaState TouchpadButton { get; private set; } = ButtonDeltaState.NoChange;

        /// <summary>
        /// The change status of the mic button.
        /// </summary>
        public ButtonDeltaState MicButton { get; private set; } = ButtonDeltaState.NoChange;

        /// <summary>
        /// Whether the delta has any changes.
        /// </summary>
        public bool HasChanges { get; private set; } = false;

        private static readonly List<(PropertyInfo delta, PropertyInfo state)> propertyPairData;

        static DualSenseInputStateButtonDelta()
        {
            // we know some key things here:
            // - on the input state, all the types of button properties are boolean.
            // - on the delta, all the types of the button properties are ButtonDeltaState.
            // - all the properties of button delta are named the same as the properties on input state - it's a subset.

            // since reflection can be a bit heavy, we'll incur this burden only once at startup so we can get the necessary property info for comparison

            PropertyInfo[] deltaProperties = typeof(DualSenseInputStateButtonDelta).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            propertyPairData = deltaProperties
                .Where(x => x.PropertyType == typeof(ButtonDeltaState))
                .Select(x => (x, typeof(DualSenseInputState).GetProperty(x.Name)!)).ToList();
        }

        /// <summary>
        /// Internal constructor for a button delta. Diffs previous and next state.
        /// </summary>
        /// <param name="prevState">The previous/old input state.</param>
        /// <param name="nextState">The next/new input state.</param>
        internal DualSenseInputStateButtonDelta(DualSenseInputState prevState, DualSenseInputState nextState)
        {
            foreach (var (delta, state) in propertyPairData)
            {
                if (state.GetValue(prevState) is bool oldVal && state.GetValue(nextState) is bool newVal)
                {
                    // otherwise leave at default NoChange
                    if (oldVal != newVal)
                    {
                        delta.SetValue(this, newVal ? ButtonDeltaState.Pressed : ButtonDeltaState.Released);
                        HasChanges = true;
                    }
                }
                else
                {
                    // we should never EVER get here. and if we do, we need to know about it to fix it,
                    // as a core assumption has been violated.
                    throw new InvalidOperationException();
                }
            }
        }
    }
}
