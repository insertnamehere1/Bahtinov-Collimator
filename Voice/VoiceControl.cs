using System;
using System.Collections.Generic;
using System.Speech.Synthesis;
using System.Windows.Forms;

namespace Bahtinov_Collimator.Voice
{
    /// <summary>
    /// Provides control over voice synthesis, including queuing and playing voice messages.
    /// </summary>
    public class VoiceControl
    {
        #region Fields

        private readonly SpeechSynthesizer synthesizer;
        private readonly Queue<(string Text, int Rate)> messageQueue;
        private Dictionary<int, double> errorValues;
        private bool voiceEnabled;
        private int channelPlaying = -1;
        private int newSpeechRate = 0;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="VoiceControl"/> class.
        /// Sets up the speech synthesizer, message queue, and event handlers.
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

        /// <summary>
        /// Handles focus data events to update the error values and play the focus value if necessary.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event data containing focus data.</param>
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

        /// <summary>
        /// Handles channel selection events to update the current channel and play the associated error value.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event data containing selected channels.</param>
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

        /// <summary>
        /// Loads settings and updates the voice enabled state based on configuration.
        /// </summary>
        public void LoadSettings()
        {
            // Load settings from application configuration
            voiceEnabled = Properties.Settings.Default.VoiceEnabled;

            if (voiceEnabled)
                Play("Voice Enabled", 0);
            else
                Play("Voice Disabled", 0);
        }

        /// <summary>
        /// Queues a message for playback if voice synthesis is enabled.
        /// </summary>
        /// <param name="text">The text to be spoken.</param>
        /// <param name="speechRate">The speech rate for the message.</param>
        public void Play(string text, int speechRate)
        {
            if (voiceEnabled)
            {
                if (synthesizer.State == SynthesizerState.Ready || synthesizer.State == SynthesizerState.Speaking)
                {
                    // Enqueue the message if the queue has space
                    if (messageQueue.Count < 2)
                    {
                        messageQueue.Enqueue((text, speechRate));
                    }

                    // Start the first message if the synthesizer is ready
                    if (synthesizer.State == SynthesizerState.Ready)
                    {
                        PlayNextMessage();
                    }
                }
            }
        }

        /// <summary>
        /// Plays the next message in the queue, if available.
        /// </summary>
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

        /// <summary>
        /// Handles the completion of speech synthesis to update the speech rate.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event data.</param>
        private void OnSpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            synthesizer.SpeakCompleted -= OnSpeakCompleted; // Unsubscribe to avoid multiple triggers
            synthesizer.Rate = newSpeechRate;
        }

        /// <summary>
        /// Handles the completion of speech synthesis to play the next message in the queue.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event data.</param>
        private void Synthesizer_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            // Play the next message if any
            PlayNextMessage();
        }

        /// <summary>
        /// Stops all ongoing speech and clears the message queue.
        /// </summary>
        public void Stop()
        {
            synthesizer.SpeakAsyncCancelAll();
            messageQueue.Clear();
        }

        #endregion
    }
}
