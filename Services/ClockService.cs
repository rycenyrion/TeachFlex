using System;
using System.Windows.Threading;

namespace TeachFlex.Services
{
    public interface IClockService
    {
        event EventHandler?
            TimeChanged;

        DateTime CurrentDateTime
        {
            get;
        }

        void Start();

        void Stop();
    }

    public class ClockService :
        IClockService
    {
        private readonly DispatcherTimer
            _timer;

        public ClockService()
        {
            CurrentDateTime =
                DateTime.Now;

            _timer =
                new DispatcherTimer
                {
                    Interval =
                        TimeSpan.FromSeconds(
                            1)
                };

            _timer.Tick +=
                Timer_Tick;
        }

        public event EventHandler?
            TimeChanged;

        public DateTime CurrentDateTime
        {
            get;
            private set;
        }

        public void Start()
        {
            CurrentDateTime =
                DateTime.Now;

            TimeChanged?.Invoke(
                this,
                EventArgs.Empty);

            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private void Timer_Tick(
            object? sender,
            EventArgs e)
        {
            CurrentDateTime =
                DateTime.Now;

            TimeChanged?.Invoke(
                this,
                EventArgs.Empty);
        }
    }
}