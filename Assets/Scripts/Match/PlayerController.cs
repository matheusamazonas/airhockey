using System;
using System.Threading;
using AirHockey.Movement;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AirHockey.Match
{
    /// <summary>
    /// Player in a <see cref="Match"/>
    /// </summary>
    public enum Player
    {
        LeftPlayer,
        RightPlayer
    }
    
    /// <summary>
    /// A <see cref="Player"/> entity in the <see cref="Match"/>. Controls movement and its scene elements. 
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        #region Serialized fields

        [SerializeField] private InputMovementController _movementController;

        #endregion

        #region Fields

        private Transform _transform;

        #endregion

        #region Setup

        private void Awake()
        {
            _transform = transform;
        }

        #endregion

        #region Public

        /// <summary>
        /// Enables user input to control the player.
        /// </summary>
        public void StartMoving()
        {
            _movementController.CanMove = true;
        }

        /// <summary>
        /// Stops the player, ignoring any user input.
        /// </summary>
        public void StopMoving()
        {
            _movementController.CanMove = false;
        }

        /// <summary>
        /// Moves the <see cref="Player"/> instantly.
        /// </summary>
        /// <param name="position">The position to move to.</param>
        public void MoveTo(Vector3 position)
        {
            _transform.position = position;
        }
        
        /// <summary>
        /// Moves the <see cref="Player"/> asynchronously.
        /// </summary>
        /// <param name="position">The position to move to.</param>
        /// <param name="duration">The duration of the movement, in seconds.</param>
        /// <param name="token">The token for operation cancellation.</param>
        /// <returns>The awaitable task.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="duration"/>
        /// is negative.</exception>
        public async UniTask MoveToAsync(Vector3 position, float duration, CancellationToken token)
        {
            if (duration < 0)
                throw new ArgumentOutOfRangeException(nameof(duration), duration, "Duration must be positive.");
            
            var totalTime = 0f;
            var initialPosition = _transform.position;
            while (totalTime <= duration)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);
                _transform.position = Vector3.Lerp(initialPosition, position, totalTime/duration);
                totalTime += Time.deltaTime;
            }
        }

        #endregion
    }
}