using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Input;
using Stride.Engine;
using Stride.Audio;
using Vortice.Vulkan;

namespace IronNations.Battle.Core
{
    public class BattleAudioManager : SyncScript
    {
        public Sound drumsSound , trumpetSound;
        public float volume = 1;
        SoundInstance trumpetInstance , drumsInstance;

        public override void Start()
        {
            trumpetInstance = trumpetSound.CreateInstance();
            drumsInstance = drumsSound.CreateInstance();
        }

        public async void PlayTrumpet ()
        {
            if (trumpetSound != null)
            {
                trumpetInstance.Volume = volume;
                await trumpetInstance.ReadyToPlay();
                trumpetInstance.Play();
            }
        }

        public async void PlayDrums()
        {
            if (drumsSound != null)
            {
                drumsInstance.Volume = volume;
                await drumsInstance.ReadyToPlay();
                drumsInstance.Play();
            }
        }

        public override void Update()
        {
            // Do stuff every new frame
        }
    }
}
