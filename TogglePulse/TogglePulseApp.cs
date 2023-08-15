using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using Meadow.Peripherals.Leds;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace TogglePulse
{
    /// <summary>
    ///     TogglePulseApp toggles the pulse animation of the on-board LED
    ///     by monitoring an external push button via an interrupt handler.
    ///     State is achieved via the interrupt, which starts or restarts the animation (and Timer)
    ///     and the Timer.Elapsed callback that ends the animation
    /// </summary>
    public class TogglePulseApp : App<F7FeatherV2>
    {

        RgbPwmLed onboardLed;

        // animation
        Timer pulseDurationTimer;
        const int PulseDurationMillis = 10000;
        Color pulseColor = Color.Green;

        /// <summary>
        ///     Initialize the IO and the timing mechanism
        /// </summary>
        public override Task Initialize()
        {
            // "toggle" button
            IPin ToggleButtonPin = Device.Pins.D03;
            IDigitalInterruptPort buttonInput;

            buttonInput = Device.CreateDigitalInterruptPort(
                ToggleButtonPin,
                InterruptMode.EdgeRising, // on press
                ResistorMode.InternalPullDown);
            buttonInput.Changed += StartOrRestartPulse;

            onboardLed = new RgbPwmLed(
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue,
                CommonType.CommonAnode);

            pulseDurationTimer = new Timer(PulseDurationMillis)
            {
                AutoReset = false
            };
            pulseDurationTimer.Elapsed += EndPulse;

            Console.WriteLine("Initialized");

            return base.Initialize();
        }

        /// <summary>
        ///     Start the animation, stopping it first if it is already running
        /// </summary>
        private void StartOrRestartPulse(object sender, DigitalPortResult e)
        {
            if (pulseDurationTimer.Enabled) // the animation has started at least once
            {
                // there will be a Delta because pulseDurationTimer can only be enabled by the interrupt
                Console.WriteLine($"Stopping pulse to restart after {e.Delta?.Seconds}.{e.Delta?.Milliseconds} seconds");
                pulseDurationTimer.Stop();
                onboardLed.StopAnimation();
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
            onboardLed.StopAnimation();
            onboardLed.SetBrightness(0f);
        }
    }
}
