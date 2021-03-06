using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UniTaskExtensions = AirHockey.Utils.UniTaskExtensions;

namespace AirHockey.Match
{
    /// <summary>
    /// Board seen in the match which gives general visual announcements (e.g. score, match start and end).
    /// </summary>
    public class AnnouncementBoard : MonoBehaviour
    {
        #region Serialized fields

        [SerializeField] private CanvasGroup _canvas;
        [SerializeField] private Text _leftText;
        [SerializeField] private Text _rightText;

        #endregion

        #region Fields

        /// <summary> Duration in seconds of general fade outs used in <see cref="FadeOutAsync"/>. </summary>
        private const float FadeOutDuration = 1f;
        private const float MatchStartFadeDuration = 0.5f;
        private const float MatchEndFadeDuration = 0.5f;
        private const string MatchStartText = "MATCH STARTS IN {0}...";
        private const string ScoredText = "GOAL!!!";
        private const string OtherScoreText = "PLAYER {0} SCORED";
        private const string GetReadyText = "ON YOUR MARKS...";
        private const string GoText = "GO!";
        private const string YouWinText = "YOU WIN!!";
        private const string YouLoseText = "YOU LOSE";
        private const string TieText = "IT'' A TIE";

        #endregion

        #region Public

        /// <summary>
        /// Displays a "match is starting..." announcement asynchronously.
        /// </summary>
        /// <param name="duration">The duration of the announcement.</param>
        /// <param name="token">The token for operation cancellation.</param>
        /// <returns>The awaitable task.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="duration"/>
        /// is negative.</exception>
        public async UniTask AnnounceMatchStartAsync(int duration, CancellationToken token)
        {
            if (duration < 0)
                throw new ArgumentOutOfRangeException(nameof(duration), duration, "Duration must be positive.");
            
            SetTexts(duration);
            await FadeInAsync(MatchStartFadeDuration, token);
            _canvas.alpha = 1f;
            while (duration > 0)
            {
                SetTexts(duration);
                await UniTask.Delay(1_000, false, PlayerLoopTiming.Update, token);
                duration--;
            }
            await FadeOutAsync(MatchStartFadeDuration, token);

            void SetTexts(int s)
            {
                _leftText.text = string.Format(MatchStartText, s);
                _rightText.text = string.Format(MatchStartText, s);
            }
        }
        
        /// <summary>
        /// Announces that a goal has been scored asynchronously.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> that scored the goal.</param>
        /// <param name="duration">Teh duration of the announcement, in seconds.</param>
        /// <param name="token">The token for operation cancellation.</param>
        /// <returns>The awaitable task.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="duration"/>
        /// is negative.</exception>
        /// <exception cref="NotImplementedException">Thrown if an invalid <see cref="Player"/>
        /// was provided.</exception>
        public async UniTask AnnounceGoalAsync(Player player, int duration, CancellationToken token)
        {
            if (duration < 0)
                throw new ArgumentOutOfRangeException(nameof(duration), duration, "Duration must be positive.");
            
            switch (player)
            {
                case Player.LeftPlayer:
                    _leftText.text = ScoredText;
                    _rightText.text = string.Format(OtherScoreText, 1);
                    break;
                case Player.RightPlayer:
                    _leftText.text = string.Format(OtherScoreText, 2);
                    _rightText.text = ScoredText;
                    break;
                default:
                    throw new NotImplementedException($"Player not valid: {player}");
            }
            
            await FadeInAsync(duration * 0.1f, token);
            await UniTask.Delay((int) (duration * 1_000 * 0.8f), false, PlayerLoopTiming.Update, token);
            await FadeOutAsync(duration * 0.1f, token);
        }

        /// <summary>
        /// Displays a "get ready" announcement to the players asynchronously.
        /// </summary>
        /// <param name="duration">The duration of the announcement, in seconds.</param>
        /// <param name="token">The token for operation cancellation.</param>
        /// <returns>The awaitable task.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="duration"/>
        /// is negative.</exception>
        public async UniTask AnnounceGetReadyAsync(int duration, CancellationToken token)
        {
            if (duration < 0)
                throw new ArgumentOutOfRangeException(nameof(duration), duration, "Duration must be positive.");
            
            _leftText.text = GetReadyText;
            _rightText.text = GetReadyText;
            await FadeInAsync(duration * 0.1f, token);
            await UniTask.Delay((int) (duration * 1_000 * 0.9f), false, PlayerLoopTiming.Update, token);
            _leftText.text = GoText;
            _rightText.text = GoText;
        }

        /// <summary>
        /// Fades the board out, regardless of what's been shown.
        /// </summary>
        /// <param name="token">The token for operation cancellation.</param>
        /// <returns>The awaitable task.</returns>
        public async UniTask FadeOutAsync(CancellationToken token)
        {
            await FadeOutAsync(FadeOutDuration, token);
        }

        public void AnnounceEndOfMatch(Score.Result result, CancellationToken token)
        {
            switch (result)
            {
                case Score.Result.Tie:
                    _leftText.text = TieText;
                    _rightText.text = TieText;
                    break;
                case Score.Result.LeftPlayerWin:
                    _leftText.text = YouWinText;
                    _rightText.text = YouLoseText;
                    break;
                case Score.Result.RightPlayerWin:
                    _leftText.text = YouLoseText;
                    _rightText.text = YouWinText;
                    break;
                default:
                    throw new NotImplementedException($"Result not valid: {result}");
            }
            
            FadeInAsync(MatchEndFadeDuration, token).Forget();
        }

        #endregion

        #region Private

        private void SetAlpha(float alpha) => _canvas.alpha = alpha;
        
        /// <summary>
        /// Fades the announcement board out asynchronously. 
        /// </summary>
        /// <param name="duration">The duration of the fade, in seconds.</param>
        /// <param name="token">The token for operation cancellation.</param>
        /// <returns>The awaitable task.</returns>
        private async UniTask FadeOutAsync(float duration, CancellationToken token)
        {
            await UniTaskExtensions.ProgressAsync(SetAlpha, 1f, 0f, duration, token);
        }
        
        /// <summary>
        /// Fades the announcement board in asynchronously. 
        /// </summary>
        /// <param name="duration">The duration of the fade, in seconds.</param>
        /// <param name="token">The token for operation cancellation.</param>
        /// <returns>The awaitable task.</returns>
        private async UniTask FadeInAsync(float duration, CancellationToken token)
        {
            await UniTaskExtensions.ProgressAsync(SetAlpha, 0f, 1f, duration, token);
        }

        #endregion
    }
}