using System;
using NAudio.Wave;

namespace HandwritingFeedback.Util
{
    /// <summary>
    /// A sine wave generator that uses portamento to smoothly transition to other frequencies. <br />
    /// Provided by Mark Heath.<br />
    /// Title: NAudio Sine Generator with Portamento<br />
    /// Author: Mark Heath<br />
    /// Date: 2016<br />
    /// Availability: https://markheath.net/post/naudio-sine-portamento<br />
    /// Availability (2): https://github.com/naudio/sinegenerator-sample
    /// Copyright 2020 Mark Heath
    /// </summary>
    public class SineWaveProvider : ISampleProvider
    {
        private readonly float[] _waveTable;
        private double _phase;
        private double _currentPhaseStep;
        private double _targetPhaseStep;
        private double _frequency;
        private double _phaseStepDelta;
        private bool _seekFreq;

        /// Copyright 2020 Mark Heath. See class-level documentation.
        public SineWaveProvider(int sampleRate = 44100)
        {
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);
            _waveTable = new float[sampleRate];
            for (int index = 0; index < sampleRate; ++index)
                _waveTable[index] = (float)Math.Sin(2 * Math.PI * (double)index / sampleRate);
            Frequency = 300f;
            Volume = 0.25f;
        }

        public double PortamentoTime;

        /// <summary>
        /// The frequency of the sine wave.
        /// </summary>
        public double Frequency
        {
            get => _frequency;
            set
            {
                _frequency = value;
                _seekFreq = true;
            }
        }

        /// <summary>
        /// The volume of the sine wave.
        /// </summary>
        public float Volume { get; set; }

        public WaveFormat WaveFormat { get; }

        /// Copyright 2020 Mark Heath. See class-level documentation.
        public int Read(float[] buffer, int offset, int count)
        {
            if (_seekFreq) // process frequency change only once per call to Read
            {
                _targetPhaseStep = _waveTable.Length * (_frequency / WaveFormat.SampleRate);

                _phaseStepDelta = (_targetPhaseStep - _currentPhaseStep) / (WaveFormat.SampleRate * PortamentoTime);
                _seekFreq = false;
            }
            var vol = Volume; // process volume change only once per call to Read
            for (int n = 0; n < count; ++n)
            {
                int waveTableIndex = (int)_phase % _waveTable.Length;
                buffer[n + offset] = _waveTable[waveTableIndex] * vol;
                _phase += _currentPhaseStep;
                if (_phase > _waveTable.Length)
                    _phase -= _waveTable.Length;
                if (_currentPhaseStep != _targetPhaseStep)
                {
                    _currentPhaseStep += _phaseStepDelta;
                    if (_phaseStepDelta > 0.0 && _currentPhaseStep > _targetPhaseStep)
                        _currentPhaseStep = _targetPhaseStep;
                    else if (_phaseStepDelta < 0.0 && _currentPhaseStep < _targetPhaseStep)
                        _currentPhaseStep = _targetPhaseStep;
                }
            }
            return count;
        }
    }
}