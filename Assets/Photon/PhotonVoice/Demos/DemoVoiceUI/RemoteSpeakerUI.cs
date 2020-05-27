using Photon.Voice.Unity;
using UnityEngine;
using UnityEngine.UI;


namespace Photon.Voice.DemoVoiceUI
{
    [RequireComponent(typeof(Speaker))]
    public class RemoteSpeakerUI : MonoBehaviour
    {
        [SerializeField]
        private Text nameText;
        [SerializeField]
        private Image remoteIsMuting;
        [SerializeField]
        private Image remoteIsTalking;

        private Speaker speaker;

        private void Start()
        {
            this.nameText = this.GetComponentInChildren<Text>();
            this.speaker = this.GetComponent<Speaker>();
            string nick = this.speaker.name;
            if (this.speaker.Actor != null)
            {
                nick = this.speaker.Actor.NickName;
                if (string.IsNullOrEmpty(nick))
                {
                    nick = string.Concat("user ", this.speaker.Actor.ActorNumber);
                }
            }
            this.nameText.text = nick;
        }

        void Update()
        {
            object mutedValue;
            if (this.speaker.Actor != null && this.speaker.Actor.CustomProperties.TryGetValue(DemoVoiceUI.MutePropKey, out mutedValue))
            {
                this.remoteIsMuting.enabled = (bool)mutedValue;
            }
            // TODO: It would be nice, if we could show if a user is actually talking right now (Voice Detection)
            this.remoteIsTalking.enabled = this.speaker.IsPlaying;
        }
    }
}