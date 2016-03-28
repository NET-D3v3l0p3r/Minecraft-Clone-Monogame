using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinecraftClone.Core.MapGenerator
{
    public class Possiblity
    {
        public static int LastBound = -1;

        public int Maximum { get; private set; }
        public int Minimum { get; private set; }
        public string Resource { get; private set; }

        public Possiblity(string resource, float possiblity)
        {
            Resource = resource;
            Minimum = LastBound + 1;
            Maximum = (int)(possiblity * (25 * 40)) + Minimum;//215
            LastBound = Maximum;
        }

        public void SetMaximum(int maximum)
        {
            Maximum = maximum;
        }
        public void SetMinimum(int minimum)
        {
            Minimum = minimum;
        }
    }
}
