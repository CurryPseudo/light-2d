using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

[RequireComponent(typeof(Animator))]
public class AnimationClipPlayer : MonoBehaviour {
	[Serializable]
	public struct ClipInfo {
		public AnimationClip clip;
		public float playSpeed;
		public ClipInfo(AnimationClip clip, float playSpeed) {
			this.clip = clip;
			this.playSpeed = playSpeed;
		}
	}
    private Animator animator;
	private PlayableGraph playableGraph;
	private Dictionary<string, int> clipIndexes = new Dictionary<string, int>();
    private AnimationMixerPlayable animationMixer;
	public List<ClipInfo> clipInfos;
	void Awake() {
		animator = GetComponent<Animator>();
		playableGraph = PlayableGraph.Create();
		AnimationPlayableOutput animationOutput = AnimationPlayableOutput.Create(playableGraph, "animation", animator);
		animationMixer = AnimationMixerPlayable.Create(playableGraph, clipInfos.Count);
		animationOutput.SetSourcePlayable(animationMixer);
		int clipIndex = 0;
		foreach(ClipInfo clipInfo in this.clipInfos) {
            AnimationClipPlayable clipPlayable = AnimationClipPlayable.Create(playableGraph, clipInfo.clip);
            PlayableExtensions.SetSpeed(clipPlayable, clipInfo.playSpeed);
            playableGraph.Connect(clipPlayable, 0, animationMixer, clipIndex);
            clipIndexes.Add(clipInfo.clip.name, clipIndex);
			clipIndex++;
		}
		playableGraph.Play();
	}
	public void PlayClip(string clipName) {
		int playingClipIndex = clipIndexes[clipName];
		for(int i = 0; i < animationMixer.GetInputCount(); i++) {
			if(i != playingClipIndex) {
				animationMixer.SetInputWeight(i, 0);
			}
			else {
				animationMixer.SetInputWeight(i, 1);
			}
		}
	}
	/// <summary>
	/// This function is called when the MonoBehaviour will be destroyed.
	/// </summary>
	void OnDestroy()
	{
        playableGraph.Destroy();
	}
}