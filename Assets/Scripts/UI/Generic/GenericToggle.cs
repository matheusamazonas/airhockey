using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LazySquirrelLabs.AirHockey.UI.Generic
{
	/// <summary>
	/// A generic toggle that stores a value.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal abstract class GenericToggle<T> : MonoBehaviour, IPointerClickHandler
	{
		#region Events

		/// <summary>
		/// Event invoked whenever the toggle is selected.
		/// </summary>
		internal event Action<GenericToggle<T>> OnSelect;

		#endregion

		#region Serialized Fields

		[SerializeField] private Text _text;
		[SerializeField] private Color _selectedColor;
		[SerializeField] private Color _unselectedColor;
		[SerializeField] private T _value;

		#endregion

		#region Fields

		private bool _selected;

		#endregion

		#region Properties

		/// <summary>
		/// The value that this toggle represents.
		/// </summary>
		internal T Value => _value;

		#endregion

		#region Event handlers

		public void OnPointerClick(PointerEventData eventData)
		{
			if (_selected)
			{
				return;
			}

			Select();
		}

		#endregion

		#region Internal

		/// <summary>
		/// Forces toggle selection. Invokes the <see cref="OnSelect"/> event.
		/// </summary>
		internal void ForceSelection()
		{
			Select();
		}

		/// <summary>
		/// Forces toggle deselection.
		/// </summary>
		internal void ForceDeselection()
		{
			_selected = false;
			_text.color = _unselectedColor;
		}

		#endregion

		#region Private

		/// <summary>
		/// Selects this toggle. Updates the visuals and invokes the <see cref="OnSelect"/> event.
		/// </summary>
		private void Select()
		{
			_selected = true;
			_text.color = _selectedColor;
			OnSelect?.Invoke(this);
		}

		#endregion
	}
}