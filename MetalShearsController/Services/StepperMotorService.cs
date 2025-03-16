using System;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Threading;
using System.Threading.Tasks;

public class StepperMotorService : IDisposable
{
    private readonly int _stepPin;      // GPIO for PUL+ (e.g., GPIO 18 for PWM0)
    private readonly int _dirPin;       // GPIO for DIR+
    private readonly int _enablePin;    // GPIO for ENA+
    private readonly GpioController _gpio; // Controls direction and enable pins
    private readonly PwmChannel _pwm;   // Hardware PWM for step pulses

    private CancellationTokenSource _cancellationTokenSource; // For stopping the motor mid-operation

    // Constructor: Initializes GPIO and PWM
    public StepperMotorService(int stepPin, int dirPin, int enablePin)
    {
        _stepPin = stepPin;
        _dirPin = dirPin;
        _enablePin = enablePin;

        // Set up GPIO for direction and enable pins
        _gpio = new GpioController();
        _gpio.OpenPin(_dirPin, PinMode.Output);
        _gpio.OpenPin(_enablePin, PinMode.Output);

        // Initialize PWM on the step pin (e.g., Channel 0 for GPIO 18)
        int pwmChannel = GetPwmChannelForPin(_stepPin);
        _pwm = PwmChannel.Create(0, pwmChannel, frequency: 100, dutyCyclePercentage: 0.5); // Start with 100 Hz, 50% duty cycle
        _pwm.Stop(); // Ensure PWM is off initially

        _cancellationTokenSource = new CancellationTokenSource();
    }

    // Moves the stepper motor with ramps or slow speed for short distances, returns steps completed
    // Parameters:
    // - steps: Total steps to move
    // - maxSpeedHz: Peak speed during constant phase (or used to calculate slow speed)
    // - clockwise: Direction of rotation
    public async Task<int> MoveStepperAsync(int steps, int maxSpeedHz, bool clockwise)
    {
        CancellationToken cancellationToken = _cancellationTokenSource.Token;
        int stepsCompleted = 0; // Tracks steps sent (open-loop)

        // Enable the motor (active low)
        _gpio.Write(_enablePin, PinValue.Low);

        // Set direction (high = clockwise, low = counterclockwise)
        _gpio.Write(_dirPin, clockwise ? PinValue.High : PinValue.Low);

        // Ramp settings
        const int RAMP_MIN_SPEED_HZ = 10;      // Starting/ending speed for ramps
        const int RAMP_STEPS = 400;        // Steps for acceleration and deceleration phases
        const int RAMP_UPDATE_MS = 50;     // Time between frequency updates in ramp
        const int NONRAMP_SETTLE_DELAY_MS = 500;  // 0.5-second delay after short movements

        // Calculate constant speed phase steps
        int constantSteps = steps - 2 * RAMP_STEPS; // Steps at max speed
        if (constantSteps < 0) constantSteps = 0;  // No constant phase if steps < 2 * rampSteps

        try
        {
            if (constantSteps > 0) // Use ramps if constant phase is possible
            {
                // --- Acceleration Phase ---
                stepsCompleted += await RampSpeedAsync(RAMP_MIN_SPEED_HZ, maxSpeedHz, RAMP_STEPS, RAMP_UPDATE_MS, cancellationToken);

                // --- Constant Speed Phase ---
                if (!cancellationToken.IsCancellationRequested)
                {
                    _pwm.Frequency = maxSpeedHz; // Set to max speed
                    _pwm.Start(); // Start PWM if not already running
                    int constantDurationMs = (int)(constantSteps * 1000.0 / maxSpeedHz); // Duration in ms
                    await Task.Delay(constantDurationMs, cancellationToken); // Run at max speed
                    stepsCompleted += constantSteps; // Add steps from constant phase
                }

                // --- Deceleration Phase ---
                if (!cancellationToken.IsCancellationRequested)
                {
                    stepsCompleted += await RampSpeedAsync(maxSpeedHz, RAMP_MIN_SPEED_HZ, RAMP_STEPS, RAMP_UPDATE_MS, cancellationToken);
                }
            }
            else // Short distance: Ignore ramps, use 1/5 max speed, wait 2 seconds
            {
                int slowSpeedHz = maxSpeedHz / 5; // 1/5 of max speed for safety
                if (slowSpeedHz < RAMP_MIN_SPEED_HZ) slowSpeedHz = RAMP_MIN_SPEED_HZ; // Ensure at least min speed
                _pwm.Frequency = slowSpeedHz; // Set slow speed
                _pwm.Start(); // Start PWM

                int durationMs = (int)(steps * 1000.0 / slowSpeedHz); // Time to complete steps
                await Task.Delay(durationMs, cancellationToken); // Run for required time
                stepsCompleted = steps; // All steps completed at slow speed

                _pwm.Stop(); // Stop PWM early to allow settling
                if (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(NONRAMP_SETTLE_DELAY_MS, cancellationToken); // Wait 2 seconds for safety
                }
            }
        }
        catch (TaskCanceledException)
        {
            // Cancellation occurred, stepsCompleted reflects what was sent so far
        }
        finally
        {
            _pwm.Stop(); // Stop PWM pulses
            _gpio.Write(_enablePin, PinValue.High); // Disable motor
        }

        return stepsCompleted; // Return total steps sent (may differ from 'steps' if cancelled)
    }

    // Helper method: Handles speed ramping (acceleration or deceleration)
    // - startHz: Starting frequency
    // - endHz: Ending frequency
    // - rampSteps: Number of steps in the ramp
    // - updateMs: Time between frequency updates
    // Returns steps completed during the ramp
    private async Task<int> RampSpeedAsync(int startHz, int endHz, int rampSteps, int updateMs, CancellationToken cancellationToken)
    {
        int stepsCompleted = 0;
        int stepsPerUpdate = rampSteps / (1000 / updateMs); // Steps per update (e.g., 5 steps every 10ms over 1sec)
        if (stepsPerUpdate < 1) stepsPerUpdate = 1; // Minimum 1 step per update

        int updates = rampSteps / stepsPerUpdate; // Number of frequency updates
        float freqStep = (endHz - startHz) / (float)updates; // Frequency increment per update

        _pwm.Frequency = startHz; // Set initial frequency
        _pwm.Start(); // Start PWM

        for (int i = 0; i < updates && !cancellationToken.IsCancellationRequested; i++)
        {
            int currentFreq = (int)(startHz + freqStep * i); // Calculate current frequency
            _pwm.Frequency = currentFreq; // Update PWM frequency
            await Task.Delay(updateMs, cancellationToken); // Wait for this frequency to run
            stepsCompleted += (int)(currentFreq * (updateMs / 1000.0)); // Steps = freq * time (in seconds)
        }

        // Adjust for any remaining steps to hit rampSteps exactly
        if (stepsCompleted < rampSteps && !cancellationToken.IsCancellationRequested)
        {
            int remainingSteps = rampSteps - stepsCompleted;
            int finalFreq = (int)(startHz + freqStep * updates); // Final frequency (should be close to endHz)
            _pwm.Frequency = finalFreq;
            int remainingMs = (int)(remainingSteps * 1000.0 / finalFreq);
            await Task.Delay(remainingMs, cancellationToken);
            stepsCompleted += remainingSteps;
        }

        return stepsCompleted;
    }

    // Stops the motor immediately
    public void Stop()
    {
        _cancellationTokenSource.Cancel(); // Signal cancellation
        _pwm.Stop(); // Stop PWM pulses
        _gpio.Write(_enablePin, PinValue.High); // Disable motor
        _cancellationTokenSource = new CancellationTokenSource(); // Reset for next use
    }

    // Cleans up resources
    public void Dispose()
    {
        _pwm.Stop(); // Stop PWM
        _gpio.Write(_enablePin, PinValue.High); // Disable motor
        _gpio.ClosePin(_dirPin);
        _gpio.ClosePin(_enablePin);
        _pwm.Dispose();
        _gpio.Dispose();
        _cancellationTokenSource.Dispose();
    }

    // Maps GPIO pin to PWM channel (Raspberry Pi 5 compatible)
    private int GetPwmChannelForPin(int pin)
    {
        switch (pin)
        {
            case 12: // PWM0
            case 18: // PWM0
                return 0;
            case 13: // PWM1
            case 19: // PWM1
                return 1;
            default:
                throw new ArgumentException($"Pin {pin} is not a valid PWM pin.");
        }
    }
}