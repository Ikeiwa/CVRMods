using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using DarkRift;
using Dissonance.Integrations.DarkRift2;
using Dissonance;
using UnityEngine;

[assembly: MelonInfo(typeof(BetterPlayerAudio.Main), "Better Player Audio", "1.0.0", "Ikeiwa")]
[assembly: MelonGame("Alpha Blend Interactive", "ChilloutVR")]
namespace BetterPlayerAudio
{
    public class Main : MelonMod
    {
        private DissonanceComms dissonanceComms;
        private AudioSource prefabAudioSource;

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (buildIndex == 3)
            {
                dissonanceComms = Object.FindObjectOfType<DissonanceComms>();
                if (dissonanceComms)
                {
                    dissonanceComms.OnPlayerEnteredRoom += DissonanceCommsOnOnPlayerEnteredRoom;
                    prefabAudioSource = dissonanceComms.PlaybackPrefab.GetComponent<AudioSource>();
                    if (prefabAudioSource)
                        ApplyAudioSettings(prefabAudioSource);

                }
            }
        }

        private void ApplyAudioSettings(AudioSource source)
        {
            source.dopplerLevel = 0;
            source.maxDistance = 8;
            source.minDistance = 0.2f;
            source.rolloffMode = AudioRolloffMode.Custom;
            source.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
            source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, AnimationCurve.Linear(0,1,1,0));

            AnimationCurve spatialBlendCurve;
            spatialBlendCurve = new AnimationCurve(new Keyframe[]
            {
                new Keyframe(0,0,0,0),
                new Keyframe(source.minDistance/source.maxDistance,1,0,0),
                new Keyframe(1,1,0,0)
            });

            source.SetCustomCurve(AudioSourceCurveType.SpatialBlend,spatialBlendCurve);

            AnimationCurve spreadCurve;
            spreadCurve = new AnimationCurve(new Keyframe[]
            {
                new Keyframe(0,0.5f,0,0),
                new Keyframe(source.minDistance/source.maxDistance,0,0,0),
                new Keyframe(1,0,0,0)
            });

            source.SetCustomCurve(AudioSourceCurveType.Spread,spreadCurve);
        }

        private void DissonanceCommsOnOnPlayerEnteredRoom(VoicePlayerState voicePlayerState, string userId)
        {

        }
    }
}
