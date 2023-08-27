using System;
using System.Threading;
using AirHockey.Match.Scoring;
using Cysharp.Threading.Tasks;

namespace AirHockey.Match.Referees
{
    /// <summary>
    /// <see cref="Referee"/> which never ends the match and lets the players play endlessly.
    /// </summary>
    internal class EndlessReferee : Referee
    {
        #region Setup

        /// <summary>
        /// <see cref="EndlessReferee"/>'s constructor.
        /// </summary>
        /// <param name="pause">How to pause the match when a player scores.</param>
        /// <param name="end">How to end the match. Although this <see cref="Referee"/> never ends the match
        /// automatically, it can be done via player input. </param>
        internal EndlessReferee(AsyncPauser pause, Action end) : base(pause, end)
        {
        }

        #endregion

        #region Internal

        internal override async UniTask ProcessScoreAsync(Player player, Score score, CancellationToken token)
        {
            await PauseAsync(player, token);
        }

        #endregion
    }
}