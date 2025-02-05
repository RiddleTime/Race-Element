using System;
using System.Collections.Generic;
using System.Text;

namespace DualSenseAPI.State
{

    /// <summary>
    /// A handler for a state polling IO event. The sender has the <see cref="DualSenseInputState"/>
    /// from the most recent poll, and can be used to update the next
    /// <see cref="DualSenseOutputState"/>.
    /// </summary>
    /// <param name="sender">The <see cref="DualSense"/> instance that was just polled.</param>
    public delegate void StatePolledHandler(DualSense sender);

    /// <summary>
    /// A handler for a button state changed IO event. The sender has the <see cref="DualSenseInputState"/>
    /// from the most recent poll, and can be used to update the next <see cref="DualSenseInputState"/>.
    /// </summary>
    /// <param name="sender">The <see cref="DualSense"/> instance that was just polled.</param>
    /// <param name="changes">The change status of each button.</param>
    public delegate void ButtonStateChangedHandler(DualSense sender, DualSenseInputStateButtonDelta changes);
}
