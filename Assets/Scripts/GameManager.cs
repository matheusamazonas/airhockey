using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LazySquirrelLabs.AirHockey.Match;
using LazySquirrelLabs.AirHockey.Match.Managers;
using LazySquirrelLabs.AirHockey.Menu;
using LazySquirrelLabs.AirHockey.SceneManagement;
using LazySquirrelLabs.AirHockey.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using InputManager = LazySquirrelLabs.AirHockey.Input.InputManager;

namespace LazySquirrelLabs.AirHockey
{
	/// <summary>
	/// The top-most manager int he entire application.
	/// </summary>
	internal class GameManager : MonoBehaviour
	{
		#region Entities

		/// <summary>
		/// Application state.
		/// </summary>
		private enum GamePart
		{
			/// <summary>
			/// Whenever the application is loading for the first time.
			/// </summary>
			None,
			Menu,
			Match
		}

		#endregion

		#region Serialized fields

		[SerializeField] private SceneReference _menuScene;
		[SerializeField] private SceneReference _matchScene;
		[SerializeField] private CanvasFader _transition;
		[SerializeField] private InputManager _inputManager;

		/// <summary>
		/// The duration of &lt;see cref="UI.Screen"/&gt; transitions in the UI.
		/// </summary>
		[SerializeField, Range(0.1f, 5f)] private float _transitionDuration;

		#endregion

		#region Fields

		private readonly CancellationTokenSource _cancellationTokenSource = new();

		private MenuManager _menuManager;
		private MatchManager _matchManager;

		/// <summary>
		/// The currently loaded scene.
		/// </summary>
		private Scene? _scene;

		/// <summary>
		/// The settings of the current match.
		/// </summary>
		private MatchSettings _matchSettings;

		/// <summary>
		/// Current state of the application.
		/// </summary>
		private GamePart _part;

		/// <summary>
		/// Whether a scene is being loaded.
		/// </summary>
		private bool _loading;

		#endregion

		#region Setup

		private async void Start()
		{
			try
			{
				await LoadMenuAsync(false);
				_inputManager.OnBack += HandleBackNavigation;
			}
			catch (OperationCanceledException)
			{
				Debug.Log("Game manager start stopped because the operation was cancelled.");
			}
		}

		private void OnDestroy()
		{
			_cancellationTokenSource.Cancel();
			_cancellationTokenSource.Dispose();
			_inputManager.OnBack -= HandleBackNavigation;

			if (_menuManager)
			{
				_menuManager.OnQuit -= Application.Quit;
			}
		}

		#endregion

		#region Event handlers

		/// <summary>
		/// Handles the event of a return to the main menu.
		/// </summary>
		/// <exception cref="NotImplementedException">Thrown if the current <see cref="GamePart"/>
		/// is invalid.</exception>
		private async void HandleBackNavigation()
		{
			// Ignore the return if it's already loading something.
			if (_loading)
			{
				return;
			}

			try
			{
				var token = _cancellationTokenSource.Token;

				switch (_part)
				{
					case GamePart.None:
						Debug.Log("Can't return when the application is loading.");
						break;
					case GamePart.Menu when _menuManager.IsOnMainMenu:
						_menuManager.ShowQuitPopup();
						break;
					case GamePart.Menu:
						await _menuManager.ReturnAsync(token);
						break;
					case GamePart.Match:
						var matchEnd = _matchManager.StopMatchAsync(_transitionDuration * 0.9f, token);
						var loadMenu = LoadMenuAsync(true);
						await UniTask.WhenAll(matchEnd, loadMenu);
						break;
					default:
						throw new NotImplementedException($"Game part not implemented: {_part}");
				}
			}
			catch (OperationCanceledException)
			{
				Debug.Log("Return handling stopped because the operation was cancelled.");
			}
		}

		/// <summary>
		/// Handles the event of a start match from the main menu.
		/// </summary>
		/// <param name="settings">The settings of the match to be started.</param>
		private async void HandleStartMatch(MatchSettings settings)
		{
			_menuManager.OnStartMatch -= HandleStartMatch;
			_matchSettings = settings;

			try
			{
				await StartMatch(settings);
			}
			catch (OperationCanceledException)
			{
				Debug.Log("Match start was cancelled.");
			}
		}

		/// <summary>
		/// Handles the event of a restart match request.
		/// </summary>
		private async void HandleRestartMatch()
		{
			try
			{
				_matchManager.OnLeaveRequest -= HandleBackNavigation;
				_matchManager.OnRestartRequest -= HandleRestartMatch;
				await StartMatch(_matchSettings);
			}
			catch (OperationCanceledException)
			{
				Debug.Log("Match restart was cancelled.");
			}
		}

		#endregion

		#region Private

		/// <summary>
		/// Loads the menu scene.
		/// </summary>
		/// <param name="transition">Whether the transition animation should be played when loading the menu. </param>
		/// <returns>A task to be awaited which represents the loading.</returns>
		private async UniTask LoadMenuAsync(bool transition)
		{
			if (transition)
			{
				await StartTransitionAsync();
			}

			_menuManager = await LoadManagedSceneAsync<MenuManager>(_menuScene);
			_menuManager.OnStartMatch += HandleStartMatch;
			_menuManager.OnQuit += Application.Quit;
			_part = GamePart.Menu;

			if (transition)
			{
				await EndTransitionAsync();
			}
		}

		/// <summary>
		/// Loads the match <see cref="Scene"/> and starts a new <see cref="Match"/> asynchronously.
		/// </summary>
		/// <param name="settings">The settings of the match to be started.</param>
		private async UniTask StartMatch(MatchSettings settings)
		{
			await StartTransitionAsync();
			_matchManager = await LoadManagedSceneAsync<MatchManager>(_matchScene);
			_matchManager.OnLeaveRequest += HandleBackNavigation;
			_matchManager.OnRestartRequest += HandleRestartMatch;
			_part = GamePart.Match;
			await EndTransitionAsync();
			await _matchManager.StartMatchAsync(settings, _cancellationTokenSource.Token);
		}

		/// <summary>
		/// Starts a scene transition.
		/// </summary>
		private async UniTask StartTransitionAsync()
		{
			_loading = true;
			await _transition.FadeInAsync(_transitionDuration, _cancellationTokenSource.Token);
		}

		/// <summary>
		/// End a scene transition.
		/// </summary>
		private async UniTask EndTransitionAsync()
		{
			await _transition.FadeOutAsync(_transitionDuration, _cancellationTokenSource.Token);
			_loading = false;
		}

		/// <summary>
		/// Loads a scene that contains a manager asynchronously.
		/// </summary>
		/// <param name="scene">The scene to be loaded.</param>
		/// <typeparam name="T">The type of the manager to be fetched in the scene.</typeparam>
		/// <returns>A task to be awaited which represents the loading. Its value is the scene's manager. </returns>
		/// <exception cref="Exception">Thrown if the given <paramref name="scene"/> wasn't loaded.
		/// <typeparamref name="T"/>The type of the manager in the scene.</exception>
		private async UniTask<T> LoadManagedSceneAsync<T>(SceneReference scene) where T : MonoBehaviour
		{
			if (_scene != null) // In some cases (e.g. leading menu), there is nothing to unload.
			{
				await SceneManager.UnloadSceneAsync(_scene.Value);
			}

			await SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
			_scene = SceneManager.GetSceneByPath(scene);

			if (_scene == null)
			{
				throw new Exception($"Managed scene wasn't loaded ({typeof(T)}).");
			}

			SceneManager.SetActiveScene(_scene.Value);
			return FindAnyObjectByType<T>();
		}

		#endregion
	}
}