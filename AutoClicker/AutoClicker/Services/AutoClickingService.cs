using System;
using System.Threading;
using System.Threading.Tasks;
using AutoClicker.Input.Services;

namespace AutoClicker.Services
{
    public interface IAutoClickingService
    {
        bool IsActive { get; }
        int ClickInterval { get; set; }
        int IntervalRandomness { get; set; }
        void Toggle();
        
        public Func<bool>? Filter { get; set; }

        event Action<bool> Toggled;
    }

    public class AutoClickingService : IAutoClickingService
    {
        private readonly IWindowsApiService _windowsApiService;
        private Thread _clickThread = null!;
        private CancellationTokenSource _cancellationTokenSource = null!;
        
        
        public event Action<bool>? Toggled;
        
        public bool IsActive { get; private set; } = false;
        public int ClickInterval { get; set; }
        public int IntervalRandomness { get; set; }
        public Func<bool>? Filter { get; set; }
        
        public AutoClickingService(IWindowsApiService windowsApiService)
        {
            _windowsApiService = windowsApiService;
            ClickInterval = 1000; // default click interval of 1 second
        }

        public void Toggle()
        {
            if (IsActive)
            {
                Deactivate();
            }
            else
            {
                Activate();
            }
        }


        private void Deactivate()
        {
            IsActive = false;
            _cancellationTokenSource?.Cancel();
            _clickThread?.Join(); // Ensure the thread is stopped before continuing
            Toggled?.Invoke(false);
        }

        private void Activate()
        {
            if (IsActive)
            {
                return; // Already active, don't start another thread
            }

            IsActive = true;
            _cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = _cancellationTokenSource.Token;

            _clickThread = new Thread(() => AutoClick(token))
            {
                IsBackground = true
            };
            _clickThread.Start();
            Toggled?.Invoke(true);
        }

        // Simulate mouse clicks at regular intervals
        private async Task AutoClick(CancellationToken cancellationToken)
        {
            try
            {
                var interval = ClickInterval;
                
                if(IntervalRandomness > 0)
                {
                    var intervalRandomness = new Random();
                    interval += intervalRandomness.Next(-IntervalRandomness, IntervalRandomness);
                }

                Console.WriteLine($"Clicking every {interval}ms");
                
                while (IsActive && !cancellationToken.IsCancellationRequested)
                {
                    if (Filter != null && !Filter())
                    {
                        await Task.Delay(interval, cancellationToken); // Sleep for the click interval
                        continue;
                    }
                    
                    _windowsApiService.SimulateMouseClick();
                    await Task.Delay(interval, cancellationToken); // Sleep for the click interval
                }
            }
            catch (ThreadInterruptedException)
            {
                // Thread was interrupted, likely by a cancellation request
            }
        }
    }
}
