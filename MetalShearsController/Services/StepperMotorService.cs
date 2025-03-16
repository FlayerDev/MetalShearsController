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
    private readonly GpioController _gpio;
    private readonly PwmChannel _pwm;   // PWM channel for step pulses

    private CancellationTokenSource _cancellationTokenSource;

    public StepperMotorService(int stepPin, int dirPin, int enablePin)
    {
        _stepPin = stepPin;
        _dirPin = dirPin;
        _enablePin = enablePin;

        _gpio = new GpioController();
        _gpio.OpenPin(_dirPin, PinMode.Output);
        _gpio.OpenPin(_enablePin, PinMode.Output);

        // Map stepPin to the correct PWM channel
        int pwmChannel = GetPwmChannelForPin(_stepPin);
        _pwm = PwmChannel.Create(0, pwmChannel, frequency: 100, dutyCyclePercentage: 0.5); // Controller 0, 50% duty cycle
        _pwm.Stop(); // Ensure PWM is off initially

        _cancellationTokenSource = new CancellationTokenSource();
    }

    public async Task MoveStepperAsync(int steps, int speedHz, bool clockwise)
    {
        CancellationToken cancellationToken = _cancellationTokenSource.Token;

        _gpio.Write(_enablePin, PinValue.Low); // Enable the motor

        // Set direction
        _gpio.Write(_dirPin, clockwise ? PinValue.High : PinValue.Low);

        // Configure PWM frequency based on speedHz (steps per second)
        _pwm.Frequency = speedHz;
        _pwm.DutyCycle = 0.5; // 50% duty cycle for square wave

        // Calculate total duration in milliseconds based on steps and speed
        int totalDurationMs = (int)(steps * 1000.0 / speedHz);

        // Start PWM
        _pwm.Start();

        try
        {
            // Run for the calculated duration or until cancelled
            await Task.Delay(totalDurationMs, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            // Handle cancellation gracefully
        }
        finally
        {
            _pwm.Stop(); // Stop PWM after movement
            _gpio.Write(_enablePin, PinValue.High); // Disable motor
        }
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
        _pwm.Stop(); // Stop PWM immediately
        _gpio.Write(_enablePin, PinValue.High); // Disable motor
        _cancellationTokenSource = new CancellationTokenSource(); // Reset the token source
    }

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

    private int GetPwmChannelForPin(int pin)
    {
        // Map GPIO pin to PWM channel (based on Raspberry Pi 5 pinout)
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