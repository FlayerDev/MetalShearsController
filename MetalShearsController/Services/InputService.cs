using System;
using System.Device.Gpio;

public class InputService : IDisposable
{
    private readonly GpioController _controller;
    private readonly int _pin;

    public event Action OnButtonPressed;

    public InputService(int pin)
    {
        _controller = new GpioController();
        _pin = pin;
        _controller.OpenPin(_pin, PinMode.InputPullUp);
        _controller.RegisterCallbackForPinValueChangedEvent(_pin, PinEventTypes.Falling, (s, e) => OnButtonPressed?.Invoke());
    }

    public bool IsPressed() => _controller.Read(_pin) == PinValue.Low;

    public void Dispose()
    {
        _controller.UnregisterCallbackForPinValueChangedEvent(_pin, null);
        _controller.ClosePin(_pin);
        _controller.Dispose();
    }
}