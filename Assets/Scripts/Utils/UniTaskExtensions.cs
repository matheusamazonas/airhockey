using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LazySquirrelLabs.AirHockey.Utils
{
	internal static class UniTaskExtensions
	{
		#region Internal

		/// <summary>
		/// Asynchronously progresses an <paramref name="update"/> function through a range.
		/// </summary>
		/// <param name="update">The function to be invoked during progression.</param>
		/// <param name="start">The start value of the progression.</param>
		/// <param name="end">The end value of the progression.</param>
		/// <param name="duration">The duration of the progression in seconds.</param>
		/// <param name="token">The token used for cancellation.</param>
		/// <returns>The awaitable task.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="duration"/>
		/// is negative.</exception>
		internal static async UniTask ProgressAsync(Action<float> update, float start, float end, float duration,
		                                            CancellationToken token)
		{
			if (duration < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(duration), duration, "Duration must be positive.");
			}
			
			var startTime = Time.time;
			var delta = 0f;

			while (delta <= duration)
			{
				var value = Mathf.Lerp(start, end, delta / duration);
				update(value);
				await UniTask.Yield(PlayerLoopTiming.Update, token);
				token.ThrowIfCancellationRequested();
				delta = Time.time - startTime;
			}

			update(end);
		}

		#endregion
	}
}