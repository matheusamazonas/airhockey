using UnityEngine;
using UnityEngine.UI;

namespace AirHockey.UI.Popups
{
    /// <summary>
    /// Error popup with a message.
    /// </summary>
    internal class ErrorPopup : Popup
    {
        #region Serialized fields

        [SerializeField] private Button _acknowledgeButton;
        [SerializeField] private Text _text;

        #endregion

        #region Properties

        /// <summary>
        /// Message to be displayed in the popup.
        /// </summary>
        internal string Message
        {
            set => _text.text = value;
        }

        #endregion

        #region Setup

        private void Awake()
        {
	        _acknowledgeButton.onClick.AddListener(Hide);
        }

        private void OnDestroy()
        {
	        _acknowledgeButton.onClick.RemoveListener(Hide);
        }

        #endregion
    }
}