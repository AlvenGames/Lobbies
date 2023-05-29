using System;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiplayerGame.Lobbies
{
    // Manages the Amount of times you can hit a service call.
    // Adds a buffer to account for ping times.
    // Will Queue the latest overflow task for when the cooldown ends.
    // Created to mimic the way rate limits are implemented Here:  https://docs.unity.com/lobby/rate-limits.html
    public class ServiceRateLimiter
    {
        public event Action<bool> OnCooldownChange;
        public readonly int CooldownMS;
        public bool TaskAvailable => _tasksAvailable > 0;

        private readonly uint _serviceCallTimes;
        private bool _coolingDown;
        private uint _tasksAvailable;

        public bool IsCoolingDown
        {
            get => _coolingDown;
            private set
            {
                if (_coolingDown == value) return;

                _coolingDown = value;
                OnCooldownChange?.Invoke(_coolingDown);
            }
        }

        // (If you're still getting rate limit errors, try increasing the pingBuffer)
        public ServiceRateLimiter(uint callTimes, float cooldown, int pingBuffer = 0)
        {
            _serviceCallTimes = callTimes;
            _tasksAvailable = _serviceCallTimes;
            CooldownMS = Mathf.CeilToInt(cooldown * 1000) + pingBuffer;
        }

        public async Task RunUntilCooldown(Task task)
        {
            if (_tasksAvailable <= 0)
            {
                while (_coolingDown) await Task.Delay(10);
            }

            _tasksAvailable--;
            try
            {
                await task;
            }
            finally
            {
                if (!IsCoolingDown)
                {
                    ParallelCooldownAsync();
                }
            }
        }

        public async Task<T> RunUntilCooldownT<T>(Task<T> task)
        {
            await RunUntilCooldown(task);
            return task.Result;
        }
        
        // public async Task QueueUntilCooldown()
        // {
        //     if (!IsCoolingDown)
        //     {
        //         ParallelCooldownAsync();
        //     }
        //
        //     if (--_tasksAvailable > 0)
        //     {
        //         return;
        //     }
        //
        //     while (_coolingDown)
        //     {
        //         await Task.Delay(10);
        //     }
        // }

        private async void ParallelCooldownAsync()
        {
            IsCoolingDown = true;
            await Task.Delay(CooldownMS);
            IsCoolingDown = false;
            _tasksAvailable = _serviceCallTimes;
        }
    }
}