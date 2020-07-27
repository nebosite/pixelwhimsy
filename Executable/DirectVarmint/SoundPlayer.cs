using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
//using Microsoft.DirectX.DirectSound;
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.DirectSound;


namespace DirectVarmint
{
    public class SoundPlayer : IDisposable
    {
        const int sampleRate = 44100;
        const int bytesPerSample = 2;
        const int channels = 2;
        const int bufferSize = 200000;
		int maxActiveSounds = 1000;
        int maxPlayableSounds = 48;

        int mixBufferSize = sampleRate * 4 / 4;

        // TODO: Use a SHarpDX reference to get this sound code to work
        //Device device;
        DirectSound directSound = new DirectSound();
        private SecondarySoundBuffer playBuffer = null;
        byte[] rawBuffer = null;
        int[] mixingBuffer = null;
        MemoryStream bufferStream = null;
        Random rand = new Random();
        public int lastWritePosition = 0;
        HiPerfTimer timer = new HiPerfTimer();
        public double mixTime;
        public double MasterVolume = 1.0;
        bool disposed = false;
        List<SoundInstance> soundQueue = new List<SoundInstance>();

        #region SOUNDEFFECT

        /// --------------------------------------------------------------------------
        /// <summary>
        /// This class holds onto raw sound data
        /// </summary>
        /// --------------------------------------------------------------------------
        public class SoundEffect
        {
            internal short[] soundData;
            internal int numSamples;

            public short[] SoundData { get { return soundData; } }
            public int NumSamples { get { return numSamples; } set { this.numSamples = value; } }


            /// --------------------------------------------------------------------------
            /// <summary>
            /// Create a blank sound effect
            /// </summary>
            /// <param name="numberOfSamples"></param>
            /// --------------------------------------------------------------------------
            public SoundEffect(int numberOfSamples)
            {
                this.numSamples = numberOfSamples;
                soundData = new short[numberOfSamples * channels];
            }

            // TODO:  Check out this for some hints: https://github.com/madeinouweland/play-wav-files-in-windows-10-app-with-sharpdx/blob/master/WaveDemo.Audio/WavePlayer.cs

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Constructor.  THis will convert the wav resource into the format 
            /// specified
            /// </summary>
            /// <param name="effectName"></param>
            /// <param name="device"></param>
            /// --------------------------------------------------------------------------
            public SoundEffect(Stream wavSource, DirectSound directSound, int targetSampleRate)
            {
                WaveFormat waveFormat;
                using (var reader = new BinaryReader(wavSource))
                {
                    waveFormat = new WaveFormat(reader);
                }
                var bufferDescription = new SoundBufferDescription();
                bufferDescription.BufferBytes = waveFormat.ConvertLatencyToByteSize(60000);
                bufferDescription.Format = waveFormat;
                bufferDescription.Flags = BufferFlags.GetCurrentPosition2 | BufferFlags.ControlPositionNotify | BufferFlags.GlobalFocus |
                                            BufferFlags.ControlVolume | BufferFlags.StickyFocus;
                bufferDescription.AlgorithmFor3D = Guid.Empty;
                //var soundBuffer = new SecondarySoundBuffer(wavSource, bufferDescription, device);
                var soundBuffer = new SecondarySoundBuffer(directSound, bufferDescription);

                var capabilities = soundBuffer.Capabilities;
                //int bytesPerSample = soundBuffer.GetFormat().BitsPerSample / 8;
                //int sourceChannels = soundBuffer.Format.Channels;
                //numSamples = soundBuffer.Caps.BufferBytes / bytesPerSample / soundBuffer.Format.Channels;
                numSamples = capabilities.BufferBytes / waveFormat.BlockAlign;

                short[] tempData = new short[numSamples * 2];

                Array data = null;
                if (bytesPerSample == 1)
                {
                    data = soundBuffer.Read(0, typeof(byte), LockFlag.None, numSamples * sourceChannels);
                }
                else if (bytesPerSample == 2)
                {
                    data = soundBuffer.Read(0, typeof(short), LockFlag.None, numSamples * sourceChannels);
                }
                else throw new ApplicationException("I only understand 8bit and 16bit audio formats");

                int channelOffset = sourceChannels - 1;
                int lastZero = 0;

                // convert data to 16bit stereo
                for (int i = 0; i < numSamples; i++)
                {
                    short leftSample = 0;
                    short rightSample = 0;

                    if (bytesPerSample == 1)
                    {
                        byte leftByte = (byte)data.GetValue(i * sourceChannels);
                        byte rightByte = (byte)data.GetValue(i * sourceChannels + channelOffset);

                        // Some compressed PCM data leaves junk at the end, this code 
                        // tracks the junk so we can clear it later.
                        if (leftByte == 0)
                        {
                            if (lastZero == 0) lastZero = i;
                        }
                        else lastZero = 0;

                        leftSample = (short)((leftByte - 128) * 256);
                        rightSample = (short)((rightByte - 128) * 256);
                    }
                    else if (bytesPerSample == 2)
                    {
                        leftSample = (short)data.GetValue(i * sourceChannels);
                        rightSample = (short)data.GetValue(i * sourceChannels + channelOffset);
                    }

                    tempData[i * 2] = leftSample;
                    tempData[i * 2 + 1] = rightSample;
                }

                // Clean up the 'junk' left at the end of some compressed PCM files
                if (lastZero > 0)
                {
                    for (int i = lastZero; i < numSamples; i++)
                    {
                        tempData[i * 2] = 0;
                        tempData[i * 2 + 1] = 0;
                    }
                }

                // Convert to the target sample rate
                int realSamples = (int)(((long)numSamples * targetSampleRate) / soundBuffer.Format.SamplesPerSecond);

                soundData = new short[realSamples * 2];
                for (int i = 0; i < realSamples; i++)
                {
                    double sourceSample = ((double)i / realSamples * numSamples);
                    int source1 = (int)sourceSample;
                    int source2 = source1 + 1;
                    if (source2 >= numSamples)
                    {
                        sourceSample = source2 = source1;
                    }

                    short leftChannel1 = tempData[source1 * 2];
                    short leftChannel2 = tempData[source2 * 2];
                    short rightChannel1 = tempData[source1 * 2 + 1];
                    short rightChannel2 = tempData[source2 * 2 + 1];
                    double contribution2 = sourceSample - source1;
                    double contribution1 = 1.0 - contribution2;

                    double adjustedLeft = contribution1 * leftChannel1 + contribution2 * leftChannel2;
                    double adjustedRight = contribution1 * rightChannel1 + contribution2 * rightChannel2;

                    soundData[i * 2] = (short)(adjustedLeft);
                    soundData[i * 2 + 1] = (short)(adjustedRight);
                }

                numSamples = realSamples;

            }
        }
        #endregion

        #region SOUND INSTANCE
        /// --------------------------------------------------------------------------
        /// <summary>
        /// Class for controlling a playing sound effect
        /// </summary>
        /// --------------------------------------------------------------------------
        public class SoundInstance
        {
            public SoundEffect effect;
            long playPosition = 0;
            public bool finished = false;
            public bool Finished
            {
                get 
                {
                    if (xnaInstance != null) return xnaInstance.State == Microsoft.Xna.Framework.Audio.SoundState.Stopped;
                    return finished; 
                }
                set
                {
                    finished = value;
                    if (finished && xnaInstance != null)
                    {
                        xnaInstance.Stop();
                    }
                }
            }
                    
            long playSpeed = 0x10000;
            int volume = 0x10000;
            bool looping = false;
            bool dontKill = false;

            public bool Looping { get { return looping; } set { looping = value; } }
            public bool DontKill { get { return dontKill; } set { dontKill = value; } }
            public int PlayPointer { get { return (int)(playPosition >> 16); } }

            /// <summary>
            /// Set the volume for an effect. 1.0 = natural volume
            /// </summary>
            public double Volume
            {
                get
                {
                    return (double)volume / 0x10000;
                }

                set
                {
                    if (value < 0) value = 0;
                    if (value > 15000) value = 15000;
                    volume = (int)(value * 0x10000);
                }
            }

            /// <summary>
            /// The the relative frequency.  1.0 = natural frequency
            /// </summary>
            public double RelativeFrequency
            {
                get
                {
                    return (double)playSpeed / 0x10000;
                }
                set
                {
                    if (value < 0) value = 0;
                    playSpeed = (long)(value * 0x10000);
                }
            }

            Microsoft.Xna.Framework.Audio.SoundEffectInstance xnaInstance;

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Xna version of this
            /// </summary>
            /// <param name="numberOfSamples"></param>
            /// --------------------------------------------------------------------------
            public SoundInstance(Microsoft.Xna.Framework.Audio.SoundEffectInstance xnaInstance)
            {
                this.xnaInstance = xnaInstance;
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="effect"></param>
            /// --------------------------------------------------------------------------
            internal SoundInstance(SoundEffect effect)
            {
                this.effect = effect;
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Helper method for testing audio Hardware
            /// </summary>
            /// <param name="samplesToWrite"></param>
            /// --------------------------------------------------------------------------
            internal void Mix(int[] buffer, int samplesToWrite, double masterVolume, bool actuallyPlay)
            {
                int playSpot = (int)(playPosition >> 16);
                int adjustedVolume = (int)(volume * masterVolume);

                // Mix into the raw buffer starting at index 0
                for (int i = 0; i < samplesToWrite * 2; i += 2)
                {
                    if (actuallyPlay)
                    {
                        int fraction = (int)(playPosition & 0xffff);
                        int leftSample1 = effect.soundData[playSpot * 2];
                        int leftSample2 = effect.soundData[(playSpot + 1) * 2];
                        int rightSample1 = effect.soundData[playSpot * 2 + 1];
                        int rightSample2 = effect.soundData[(playSpot + 1) * 2 + 1];
                        int contribution2 = fraction;
                        int contribution1 = 0xffff - contribution2;

                        int actualLeft = (leftSample1 * contribution1) / 0xffff + (leftSample2 * contribution2) / 0xffff;
                        int actualRight = (rightSample1 * contribution1) / 0xffff + (rightSample2 * contribution2) / 0xffff;

                        buffer[i] += (int)(((long)actualLeft * adjustedVolume) >> 16);
                        buffer[i + 1] += (int)(((long)actualRight * adjustedVolume) >> 16);
                    }

                    playPosition += playSpeed;
                    playSpot = (int)(playPosition >> 16);
                    if (playSpot >= effect.numSamples-1)
                    {
                        if (looping)
                        {
                            playPosition -= (playSpot * 0x10000L);
                            playSpot = 0;
                        }
                        else
                        {
                            this.Finished = true;
                            break;
                        }
                    }

                }
            }
        }
        #endregion

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Load a sound Effect from a file or resource
        /// </summary>
        /// --------------------------------------------------------------------------
        public SoundEffect GetSoundEffect(string effectName)
        {
           // return new SoundEffect(DVTools.GetStream(effectName), device, sampleRate);
            return new SoundEffect(DVTools.GetStream(effectName), null, sampleRate);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Add a sound to the sound queue
        /// </summary>
        /// --------------------------------------------------------------------------
        public SoundInstance Play(SoundEffect effect)
        {
            SoundInstance instance = new SoundInstance(effect);

            return Play(instance);

        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Add a sound to the sound queue
        /// </summary>
        /// --------------------------------------------------------------------------
        public SoundInstance Play(SoundInstance instance)
        {
            lock (this)
            {
                soundQueue.Insert(0, instance);
                RemoveExtraSounds();
            }


            return instance;

        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// If we have too many sounds, remove the ones we don't need
        /// </summary>
        /// --------------------------------------------------------------------------
        void RemoveExtraSounds()
        {
            lock (this)
            {

                int soundsToKill = soundQueue.Count - maxActiveSounds;

                // Remove finished sounds first
                for (int i = 0; i < soundQueue.Count && soundsToKill > 0; )
                {
                    if (soundQueue[i].Finished)
                    {
                        soundsToKill--;
                        soundQueue.RemoveAt(i);
                        continue;
                    }

                    i++;
                }

                // Remove oldest killable non-looping sounds
                for (int i = 0; i < soundQueue.Count && soundsToKill > 0; )
                {
                    if (!soundQueue[i].Looping && !soundQueue[i].DontKill)
                    {
                        soundsToKill--;
                        soundQueue.RemoveAt(i);
                        continue;
                    }

                    i++;
                }

                // Remove oldest killable sounds
                for (int i = 0; i < soundQueue.Count && soundsToKill > 0; )
                {
                    if (!soundQueue[i].DontKill)
                    {
                        soundsToKill--;
                        soundQueue.RemoveAt(i);
                        continue;
                    }

                    i++;
                }
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        /// --------------------------------------------------------------------------
        public SoundPlayer(Control owner)
        {
            //device = new Device();
            //device.SetCooperativeLevel(owner, CooperativeLevel.Normal);

            CreateMixingBuffer();
            ThreadPool.QueueUserWorkItem(new WaitCallback(MixWorker), (object)100);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Use this to turn off the mixing thread
        /// </summary>
        /// --------------------------------------------------------------------------
        public void Dispose()
        {
            disposed = true;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Put this in it's own thread 
        /// </summary>
        /// <param name="milliseconds"></param>
        /// --------------------------------------------------------------------------
        public void MixWorker(object milliseconds)
        {
            while (!disposed)
            {
                MixAhead((int)milliseconds);
                Thread.Sleep((int)milliseconds/4);
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Create the mixing buffer
        /// </summary>
        /// --------------------------------------------------------------------------
        private void CreateMixingBuffer()
        {
            rawBuffer = new byte[bufferSize];
            mixingBuffer = new int[bufferSize / (bytesPerSample)];

            bufferStream = new MemoryStream(rawBuffer);

            WaveFormat wf = new WaveFormat();
            wf.FormatTag = WaveFormatTag.Pcm;
            wf.SamplesPerSecond = sampleRate;
            wf.BitsPerSample = 16;
            wf.Channels = 2;
            wf.BlockAlign = bytesPerSample * channels;
            wf.AverageBytesPerSecond = sampleRate * bytesPerSample * channels;
            BufferDescription desc = new BufferDescription(wf);

            desc.BufferBytes = mixBufferSize;
            desc.ControlVolume = true;
            desc.GlobalFocus = true;
            desc.LocateInSoftware = true;

            playBuffer = new SecondaryBuffer(desc, device);
            mixBufferSize = playBuffer.Caps.BufferBytes;

            playBuffer.Play(0, BufferPlayFlags.Looping);
        }

        public int virtualPosition = 0;
        public double frequency;
        List<SoundInstance> soundsToMix = new List<SoundInstance>();

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Play a sound
        /// </summary>
        /// --------------------------------------------------------------------------
        public void MixAhead(int milliseconds)
        {
            timer.Start();
            // Setup up some memory pointers

            // Where is the mix buffer player right now?
            int playPosition = 0;// playBuffer.PlayPosition;

            // How many samples do we need to write to stay ahead?
            int samples = (sampleRate * milliseconds) / 1000;
            int writeTo = playPosition + samples * bytesPerSample * channels;
            int bytesToWrite = writeTo - lastWritePosition;

            if (lastWritePosition < playPosition &&
                (mixBufferSize + lastWritePosition) < writeTo)
            {
                bytesToWrite = writeTo - mixBufferSize - lastWritePosition;
            }

            // In case the player gets way out of whack, this is a sanity
            // check to put it back on track
            if (bytesToWrite < 0)
            {
                bytesToWrite = samples;
                lastWritePosition = playPosition;
            }

            int samplesToWrite = bytesToWrite / (bytesPerSample * channels);

            // Clear the mixing buffer
            for (int i = 0; i < samplesToWrite * channels; i++)
            {
                mixingBuffer[i] = 0;
            }

            // Mix in all sound effects into a mixing buffer
            soundsToMix.Clear();
            lock (this)
            {
                for (int i = 0; i < soundQueue.Count; i++)
                {
                    if (soundQueue[i] == null || soundQueue[i].finished)
                    {
                        continue;
                    }
                    soundsToMix.Add(soundQueue[i]);
                }
            }

            int numSoundsMixed = 0;
            foreach (SoundInstance instance in soundsToMix)
            {
                instance.Mix(mixingBuffer, samplesToWrite, MasterVolume, numSoundsMixed < maxPlayableSounds);
                numSoundsMixed++;
            }

            //MixTestSound(samplesToWrite);

            // Copy mixing buffer to rawBuffer for writing, clipping as we go
            for (int i = 0; i < samplesToWrite * channels; i++)
            {
                int data = mixingBuffer[i];

                // Clip
                if (data < -32767)
                    data = -32767;
                else if (data > 32767)
                    data = 32767;

                rawBuffer[i * 2] = (byte)(data & 0xff);
                rawBuffer[i * 2 + 1] = (byte)(data >> 8);
            }


            // Write the raw buffer into the Secondary buffer at the correct location
            bufferStream.Seek(0, SeekOrigin.Begin);

            //if (bytesToWrite > 0)
            //    playBuffer.Write(lastWritePosition, bufferStream, bytesToWrite, LockFlag.None);

            lastWritePosition += bytesToWrite;
            if (lastWritePosition >= mixBufferSize)
                lastWritePosition -= mixBufferSize;

            mixTime = timer.ElapsedSeconds;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Helper method for testing audio Hardware
        /// </summary>
        /// <param name="samplesToWrite"></param>
        /// --------------------------------------------------------------------------
        private void MixTestSound(int samplesToWrite)
        {
            // Mix into the raw buffer starting at index 0
            for (int i = 0; i < samplesToWrite; i++)
            {
                virtualPosition++;
                int spot = i * bytesPerSample * channels;
                frequency = 400 + 20 * Math.Sin((double)(virtualPosition * 4) / sampleRate);
                //frequency = 400;
                double theta = ((double)virtualPosition / sampleRate) * 2.0 * Math.PI * frequency;
                short leftChannel = (short)(30000.0 * Math.Sin(theta));
                ushort rightChannel = (ushort)rand.Next(ushort.MaxValue);
                //leftChannel = 0;

                rawBuffer[spot] = (byte)(leftChannel & 0xff);
                rawBuffer[spot + 1] = (byte)(leftChannel >> 8);
                rawBuffer[spot + 2] = (byte)(rightChannel & 0xff);
                rawBuffer[spot + 3] = (byte)(rightChannel >> 8);
            }
        }
    }
}
