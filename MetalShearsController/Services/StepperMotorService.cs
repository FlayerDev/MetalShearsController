using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

public class StepperMotorService : IDisposable
{
    private readonly int _stepPin;      // GPIO for PUL+
    private readonly int _dirPin;       // GPIO for DIR+
    private readonly int _enablePin;    // GPIO for ENA+
    private readonly GpioController _gpio;

    private CancellationTokenSource _cancellationTokenSource;

    public StepperMotorService(int stepPin, int dirPin, int enablePin)
    {
        _stepPin = stepPin;
        _dirPin = dirPin;
        _enablePin = enablePin;

        _gpio = new GpioController();
        _gpio.OpenPin(_stepPin, PinMode.Output);
        _gpio.OpenPin(_dirPin, PinMode.Output);
        _gpio.OpenPin(_enablePin, PinMode.Output);

        _cancellationTokenSource = new CancellationTokenSource();
    }

    public async Task MoveStepperAsync(int steps, int speedHz, bool clockwise)
    {
        CancellationToken cancellationToken = _cancellationTokenSource.Token;
        
        _gpio.Write(_enablePin, PinValue.Low);

        int pulseDelayMilliseconds = 1000 / speedHz; // Convert Hz to milliseconds
        _gpio.Write(_dirPin, clockwise ? PinValue.High : PinValue.Low); // Set direction

        for (int i = 0; i < steps; i++)
        {
            if (cancellationToken.IsCancellationRequested)
                break; // Stop moving if cancellation is requested

            _gpio.Write(_stepPin, PinValue.High);
            await Task.Delay(pulseDelayMilliseconds / 2, cancellationToken);
            _gpio.Write(_stepPin, PinValue.Low);
            await Task.Delay(pulseDelayMilliseconds / 2, cancellationToken);

        }
        _gpio.Write(_enablePin, PinValue.High);
    }
    public void Stop()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = new CancellationTokenSource(); // Reset the token source
    }

    public void Dispose()
    {
        _gpio.Write(_enablePin, PinValue.High); // Disable motor
        _gpio.ClosePin(_stepPin);
        _gpio.ClosePin(_dirPin);
        _gpio.ClosePin(_enablePin);
        _gpio.Dispose();
        _cancellationTokenSource.Dispose();
    }
}