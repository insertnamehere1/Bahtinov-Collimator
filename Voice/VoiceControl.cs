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
        private readonly Dictionary<int, double> errorValues;
        private bool voiceEnabled;
        private int channelPlaying = -1;
        private int newSpeechRate = 0;
        private readonly Dictionary<int, double> lastPlayedValues = new Dictionary<int, double>();

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
        /// Handles the focus data event by updating the error values for the specified group. 
        /// If the value for the currently playing channel has changed since the last call, 
        /// the new value is played and announced. 
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="FocusDataEventArgs"/> object that contains the event data, 
        /// including the focus value (Bahtinov offset) and group identifier.</param>
        public void FocusDataEvent(object sender, FocusDataEventArgs e)
        {
            double focusValue = e.FocusData.BahtinovOffset;
            int group = e.FocusData.Id;

            if (group == -1)
            {
                errorValues.Clear();
                lastPlayedValues.Clear(); // Clear the last played values when resetting
            }
            else
            {
                errorValues[group] = focusValue;

                // If the current channel is the one that was updated, check if the value has changed
                if (channelPlaying != -1 && errorValues.ContainsKey(channelPlaying))
                {
                    double currentValue = errorValues[channelPlaying];

                    // Check if the value has changed since the last time it was played
                    if (!lastPlayedValues.ContainsKey(channelPlaying) || lastPlayedValues[channelPlaying] != currentValue)
                    {
                        Play(RemoveLeading0(currentValue), 2);
                        Console.WriteLine(currentValue.ToString("F1"));

                        // Update the last played value for the channel
                        lastPlayedValues[channelPlaying] = currentValue;
                    }
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
                    Play( RemoveLeading0(errorValues[i]) , 2);
                    break;
                }
            }
        }

        /// <summary>
        /// Converts a double value to a string and removes the leading zero 
        /// if the value is between -1 and 1 (exclusive).
        /// </summary>
        /// <param name="value">The double value to be converted to a string.</param>
        /// <returns>A string representation of the double value without a leading zero 
        /// if the value is between -1 and 1 (exclusive), otherwise the standard formatted string.</returns>
        private string RemoveLeading0(double value)
        {
            // Format the number to one decimal place
            string result = value.ToString("F1");

            // If the value is between -1 and 1 and has a leading zero, remove it
            if (value > -1 && value < 1)
            {
                if (result.StartsWith("0."))
                    result = result.Substring(1);  // Remove the leading '0' for positive numbers
                else if (result.StartsWith("-0."))
                    result = "-" + result.Substring(2); // Remove the leading '0' after the negative sign
            }

            return result;
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
                    if (messageQueue.Count == 0)
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
