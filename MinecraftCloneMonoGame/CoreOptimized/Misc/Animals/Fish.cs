using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftCloneMonoGame.CoreOptimized.Misc.Animals
{
    public class Fish : ArtificallyInteligence
    {
        public Definition.Typus AITypus { get; set; }
        public Stack<Definition.Intention> AIIntentions { get; set; }


        public int HitPoints { get; private set; }
        public bool IsAlive { get; private set; }

        public void ThinkAI()
        {
            switch (MinecraftClone.CoreII.Global.GlobalShares.GlobalRandom.Next(0, 5))
            {
                case 0:
                    AIIntentions.Push(Definition.Intention.Eat);
                    break;
                default :
                    AIIntentions.Push(Definition.Intention.Idle);
                    break;
            }
        }

        public void ProcessAI()
        {
            while (IsAlive)
            {
                // THINK NEXT INTENTION
                ThinkAI();
            }
        }

        public void Hit(int damage)
        {
            IsAlive = !(HitPoints - damage <= 0);
        }
    }
}
