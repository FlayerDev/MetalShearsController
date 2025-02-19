using System;
using System.Device.Gpio;

public class InputService : IDisposable
{
    private readonly GpioController _controller;
    private readonly int _pin;
    private readonly PinChangeEventHandler _pinChangeHandler;

    public event Action OnButtonPressed = delegate { };

    public InputService(int pin)
    {
        _controller = new GpioController();
        _pin = pin;
        _controller.OpenPin(_pin, PinMode.InputPullUp);
        
        // Store delegate reference to properly unregister later
        _pinChangeHandler = (s, e) => OnButtonPressed?.Invoke();
        _controller.RegisterCallbackForPinValueChangedEvent(_pin, PinEventTypes.Falling, _pinChangeHandler);
    }

    public bool IsPressed() => _controller.Read(_pin) == PinValue.Low;

    public void Dispose()
    {
        _controller.UnregisterCallbackForPinValueChangedEvent(_pin, _pinChangeHandler);
        _controller.ClosePin(_pin);
        _controller.Dispose();
    }
}
