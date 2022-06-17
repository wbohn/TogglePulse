using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using System;
using System.Timers;

namespace TogglePulse
{
    /// <summary>
    ///     TogglePulseApp toggles the pulse animation of the on-board LED
    ///     by monitoring an external push button via an interrupt handler.
    ///     State is achieved via the interrupt, which starts or restarts the animation (and Timer)
    ///     and the Timer.Elapsed callback that ends the animation
    /// </summary>
    public class TogglePulseApp : App<F7FeatherV2, TogglePulseApp>
    {
        RgbPwmLed onboardLed;
        IDigitalInputPort buttonInput; // "toggle" button

        Timer pulseDurationTimer;
        const int PulseDurationMillis = 3000;
        Color pulseColor = Color.Green;

        /// <summary>
        ///     Initialize the IO and the timing mechanism
        /// </summary>
        public TogglePulseApp()
        {
            onboardLed = new RgbPwmLed(device: Device,
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue,
                Meadow.Peripherals.Leds.IRgbLed.CommonType.CommonAnode);

            buttonInput = Device.CreateDigitalInputPort(
                Device.Pins.D03,
                InterruptMode.EdgeRising); // on press
            buttonInput.Changed += StartOrRestartPulse;

            pulseDurationTimer = new Timer(PulseDurationMillis)
            {
                AutoReset = false
            };
            pulseDurationTimer.Elapsed += EndPulse;
            Console.WriteLine("Initialized");
        }

        /// <summary>
        ///     Start the animation, stopping it first if it is already running
        /// </summary>
        private void StartOrRestartPulse(object sender, DigitalPortResult e)
        {
            if (pulseDurationTimer.Enabled) // the animation has started at least once
            {
                // there will be a Delta because pulseDurationTimer can only be enabled by the interrupt
                Console.WriteLine($"Stopping pulse to restart after {e.Delta?.Milliseconds}");
                pulseDurationTimer.Stop();
                onboardLed.Stop();
            }

            // start the animation for at least the first time
            Console.WriteLine($"Starting pulse for {PulseDurationMillis}");
            pulseDurationTimer.Start(); // pulseDurationTimer.Enabled = true
            onboardLed.StartPulse(pulseColor, TimeSpan.FromMilliseconds(PulseDurationMillis));
        }

        /// <summary>
        ///     Stop the animation because it ran for {PulseDurationMillis}.
        /// </summary>
        private void EndPulse(object sender, ElapsedEventArgs e)
        {
            // pulseDurationTimer.Enabled = false because it elapsed
            Console.WriteLine($"Stopping pulse after {PulseDurationMillis}");
            onboardLed.Stop();
        }
    }
}
