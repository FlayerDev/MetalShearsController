using System;
using System.Threading.Tasks;
using System.Device.Gpio;

public class EncoderReader : IDisposable
{
    private readonly GpioController _gpio;
    public event PinChangeEventHandler OnEncoderStep;
    private readonly int _encoderPin;
    private int _steps;

    public EncoderReader(int encoderPin)
    {
        _encoderPin = encoderPin;
        _gpio = new GpioController();

        _gpio.OpenPin(_encoderPin, PinMode.InputPullUp);

        OnEncoderStep += (object sender, PinValueChangedEventArgs e) => _steps++;

        _gpio.RegisterCallbackForPinValueChangedEvent(_encoderPin, PinEventTypes.Rising, OnEncoderStep); // Driver Based!
    }

    public int Steps
    {
        get => _steps;
        private set => _steps = value;
    }

    public void ResetSteps() => _steps = 0;

    public void Dispose()
    {
        _gpio.Dispose();
        Console.WriteLine("Encoder reader disposed.");
    }
}