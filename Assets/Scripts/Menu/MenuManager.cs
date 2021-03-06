using System;
using AirHockey.Match;
using AirHockey.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Screen = AirHockey.UI.Screen;

namespace AirHockey.Menu 
{
    /// <summary>
    /// Manages the game's main menu.
    /// </summary>
    public class MenuManager : MonoBehaviour
    {
        #region Events

        /// <summary>
        /// Invoked whenever a new match starts.
        /// </summary>
        public event Action<MatchSettings> OnStartMatch;

        #endregion
        
        #region Serialized fields

        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _creditsButton;
        [SerializeField] private PlayScreen _playScreen;
        [SerializeField] private SettingsScreen _settingsScreen;
        [SerializeField] private Screen _creditsScreen;
        [SerializeField] private CanvasFader _transition;
        [SerializeField, Range(0, 10)] private float _transitionDuration;

        #endregion

        #region Fields

        private Screen _currentScreen;

        #endregion

        #region Setup

        private void Awake()
        {
            UnityEngine.Screen.orientation = ScreenOrientation.Portrait;
            _playButton.onClick.AddListener(HandleSelectNewMatch);
            _settingsButton.onClick.AddListener(HandleSelectSettings);
            _creditsButton.onClick.AddListener(HandleSelectCredits);
            _playScreen.OnGoBack += HandleReturn;
            _settingsScreen.OnGoBack += HandleReturn;
            _creditsScreen.OnGoBack += HandleReturn;
            _playScreen.OnStartMatch += StartMatch;
            _settingsScreen.LoadAudioLevels();
        }

        private void OnDestroy()
        {
            _playButton.onClick.RemoveListener(HandleSelectNewMatch);
            _settingsButton.onClick.RemoveListener(HandleSelectSettings);
            _creditsButton.onClick.RemoveListener(HandleSelectCredits);
            _playScreen.OnGoBack -= HandleReturn;
            _settingsScreen.OnGoBack -= HandleReturn;
            _creditsScreen.OnGoBack -= HandleReturn;
            _playScreen.OnStartMatch -= StartMatch;
        }

        #endregion

        #region Event handlers

        private async void HandleSelectNewMatch()
        {
            await TransitionToAsync(_playScreen);
        }
        
        private async void HandleSelectSettings()
        {
            await TransitionToAsync(_settingsScreen);
        }
        
        private async void HandleSelectCredits()
        {
            await TransitionToAsync(_creditsScreen);
        }
        
        private void StartMatch(MatchSettings settings)
        {
            OnStartMatch?.Invoke(settings);
            _currentScreen = null;
        }

        private async void HandleReturn()
        {
            await ReturnAsync();
        }

        #endregion

        #region Public
        
        public async UniTask ReturnAsync()
        {
            await _transition.FadeInAsync(_transitionDuration / 2f);
           
            if (_currentScreen != null)
                _currentScreen.Hide();

            _currentScreen = null;
            
            await _transition.FadeOutAsync(_transitionDuration / 2f);
        }

        #endregion

        #region Private

        /// <summary>
        /// Transitions to the given <paramref name="screen"/> asynchronously.
        /// </summary>
        /// <param name="screen">The screen to transition to.</param>
        /// <returns>A task to be awaited representing the fade transition.</returns>
        private async UniTask TransitionToAsync(Screen screen)
        {
            await _transition.FadeInAsync(_transitionDuration / 2f);
           
            if (_currentScreen != null)
                _currentScreen.Hide();
           
            screen.Show();
            _currentScreen = screen;
            
            await _transition.FadeOutAsync(_transitionDuration / 2f);
        }

        #endregion
    }
}