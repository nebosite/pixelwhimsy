using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace PixelWhimsy
{
    public partial class Slate
    {
        Dictionary<int, double> frequencies = null;
        const double ChromaticRatio = 1.059463094;

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Fill up the frequencies dictioanry
        /// </summary>
        /// --------------------------------------------------------------------------
        void FillFrequencies()
        {
            double[] fvalues = new double[13];
            double f = 1.0;
            for (int i = 6; i <= 12; i++)
            {
                fvalues[i] = f;
                f *= ChromaticRatio;
            }
            f = 1.0;
            for (int i = 6; i >= 0; i--)
            {
                fvalues[i] = f;
                f /= ChromaticRatio;
            }

            frequencies = new Dictionary<int, double>();
            frequencies.Add(0, fvalues[0]);
            frequencies.Add(1, fvalues[1]);
            frequencies.Add(2, fvalues[2]);
            frequencies.Add(3, fvalues[3]);
            frequencies.Add(4, fvalues[4]);
            frequencies.Add(5, fvalues[5]);
            frequencies.Add(6, fvalues[6]);
            frequencies.Add(7, fvalues[7]);
            frequencies.Add(8, fvalues[8]);
            frequencies.Add(9, fvalues[9]);
            frequencies.Add(10, fvalues[10]);
            frequencies.Add(11, fvalues[11]);
            frequencies.Add(12, fvalues[12]);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Play a musical note
        /// </summary>
        /// <param name="keyCode"></param>
        /// --------------------------------------------------------------------------
        void PlayNote(int note)
        {
            modulator = note % 10;
            RenderModulatorToolbar();

            if (frequencies == null) FillFrequencies();

            double relativeFrequency = 1.0;
            if (frequencies.ContainsKey(note)) relativeFrequency = frequencies[note];

            if (KeyIsPressed(Keys.LShiftKey)) relativeFrequency /= 2;
            if (KeyIsPressed(Keys.RShiftKey)) relativeFrequency *= 2;
            if (KeyIsPressed(Keys.LControlKey)) relativeFrequency /= ChromaticRatio;
            if (KeyIsPressed(Keys.RControlKey)) relativeFrequency *= ChromaticRatio;

            MediaBag.Play(GlobalState.CurrentNoteSound, relativeFrequency);
        }
    }
}
