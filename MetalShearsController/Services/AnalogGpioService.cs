using System;
using System.Device.Pwm;
using System.Device.Gpio;

public class AnalogGpioService : IDisposable
{
    private readonly PwmChannel _pwmChannel;
    private readonly int _pin;
    private readonly GpioController _controller;

    /// <summary>
    /// GPIO12	pwmchip0	channel 0.
    /// GPIO13	pwmchip0	channel 1.
    /// GPIO18	pwmchip1	channel 0.
    /// GPIO19	pwmchip1	channel 1.
    /// </summary>
    public AnalogGpioService(int chip, int channel, int pin, int frequency = 1000)
    {
        _pin = pin;
        _controller = new GpioController();
        _controller.OpenPin(_pin, PinMode.Output);

        _pwmChannel = PwmChannel.Create(chip, channel, frequency, 0.0);
        _pwmChannel.Start();
    }

    /// <summary>
    /// Analog intensity using PWM
    /// </summary>
    /// <param name="value">0: off, 255: on, 1-254: analog (pwm)</param>
    public void Write(int value)
    {
        if (value <= 0)
        {
            _controller.Write(_pin, PinValue.Low);  // Fully OFF
            _pwmChannel.DutyCycle = 0.0;
        }
        else if (value >= 255)
        {
            _controller.Write(_pin, PinValue.High); // Fully ON
            _pwmChannel.DutyCycle = 1.0;
        }
        else
        {
            _pwmChannel.DutyCycle = value / 255.0;  // Convert 0-255 to 0.0-1.0
        }
    }

    public void Dispose()
    {
        _pwmChannel.Stop();
        _pwmChannel.Dispose();
        _controller.ClosePin(_pin);
        _controller.Dispose();
    }
}