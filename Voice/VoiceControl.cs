using System;
using System.Collections.Generic;
using System.Speech.Synthesis;
using System.Threading;
using System.Windows.Forms;

namespace Bahtinov_Collimator.Voice
{
    public class VoiceControl
    {
        #region Fields

        private readonly SpeechSynthesizer synthesizer;
        private readonly Queue<(string, int)> messageQueue;
        private Dictionary<int, double> errorValues;
        private bool voiceEnabled;
        private int channelPlaying = -1;
        private int newSpeechRate = 0; 

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="VoiceControl"/> class.
        /// </summary>
        public VoiceControl()
        {
            errorValues = new Dictionary<int, double>();

            synthesizer = new SpeechSynthesizer();
            messageQueue = new Queue<(string, int)>();
            synthesizer.SpeakCompleted += Synthesizer_SpeakCompleted;
            synthesizer.SelectVoice("Microsoft Zira Desktop");

            BahtinovProcessing.FocusDataEvent += FocusDataEvent;
            FocusChannelComponent.ChannelSelectDataEvent += ChannelSelected;

            LoadSettings();
        }

        #endregion

        #region Methods

        public void FocusDataEvent(object sender, FocusDataEventArgs e)
        {
            double focusValue = e.FocusData.BahtinovOffset;
            int group = e.FocusData.Id;

            if (group == -1)
            {
                errorValues.Clear();
            }
            else
            {
                errorValues[group] = focusValue;

                // If the current channel is the one that was updated, announce the new value
                if (channelPlaying != -1 && errorValues.ContainsKey(channelPlaying))
                {
                    Play(errorValues[channelPlaying].ToString("F1"), 2);
                }
            }
        }

        private void ChannelSelected(object sender, ChannelSelectEventArgs e)
        {
            channelPlaying = -1;
            Stop();

            for (int i = 0; i < e.ChannelSelected.Length; i++)
            {
                if (e.ChannelSelected[i] && i < errorValues.Count)
                {
                    channelPlaying = i;
                    Play("Channel " + (i + 1), 2);
                    Play(errorValues[i].ToString("F1"), 2);
                    break;                   
                }
            }
        }

        public void LoadSettings()
        {
            // load settings
            voiceEnabled = Properties.Settings.Default.VoiceEnabled;

            if (voiceEnabled)
                Play("Voice Enabled", 0);
            else
                Play("Voice Disabled", 0);
        }

        public void Play(string text, int speechRate)
        {
            if (voiceEnabled)
            {
                if (synthesizer.State == SynthesizerState.Ready || synthesizer.State == SynthesizerState.Speaking)
                {
                    // If currently speaking or ready, enqueue the message
                    if (messageQueue.Count < 2)
                    {
                        messageQueue.Enqueue((text, speechRate));
                    }

                    // If the synthesizer is not currently speaking, start the first message
                    if (synthesizer.State == SynthesizerState.Ready)
                    {
                        PlayNextMessage();
                    }
                }
            }
        }

        private void PlayNextMessage()
        {
            if (messageQueue.Count > 0)
            {
                var (nextMessage, rate) = messageQueue.Dequeue();
                newSpeechRate = rate;
                synthesizer.SpeakCompleted += OnSpeakCompleted;
                synthesizer.SpeakAsync(nextMessage);
            }
        }

        private void OnSpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            synthesizer.SpeakCompleted -= OnSpeakCompleted; // Unsubscribe to avoid multiple triggers
            synthesizer.Rate = newSpeechRate;
        }

        private void Synthesizer_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            // When the current message finishes, play the next one if any
            PlayNextMessage();
        }

        public void Stop()
        {
            synthesizer.SpeakAsyncCancelAll();
            messageQueue.Clear();
        }

        #endregion
    }
}
