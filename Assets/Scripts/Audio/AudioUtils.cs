using UnityEngine;
using UnityEngine.Audio;

public static class AudioUtils
{
    private static bool isQuitting = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        isQuitting = false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        Application.quitting += OnApplicationQuitting;
    }

    private static void OnApplicationQuitting()
    {
        isQuitting = true;
    }

    public static void PlaySFX(AudioClip clip, AudioMixerGroup group)
    {
        if (clip == null) return;
        if (isQuitting) return;

        var src = CreateAudioSource(group);
        if (src == null) return;

        src.clip = clip;
        src.spatialBlend = 0f;

        src.Play();
        Object.Destroy(src.gameObject, clip.length);
    }

    public static void PlaySFX(AudioClip[] clips, AudioMixerGroup group)
    {
        PlaySFX(GetRandomAudioClip(clips), group);
    }

    public static void PlaySFXAtPosition(AudioClip clip, Vector3 pos, float minDist, float maxDist, AudioMixerGroup group)
    {
        if (clip == null) return;

        var src = CreateAudioSource(group);
        if (src == null) return;

        src.transform.position = pos;

        src.clip = clip;
        src.minDistance = minDist;
        src.maxDistance = maxDist;
        src.spatialBlend = 1f;
        src.dopplerLevel = 0f;

        src.Play();
        GameObject.Destroy(src.gameObject, clip.length);
    }

    public static void PlaySFXAtPosition(AudioClip[] clips, Vector3 pos, float minDist, float maxDist, AudioMixerGroup group)
    {
        PlaySFXAtPosition(GetRandomAudioClip(clips), pos, minDist, maxDist, group);
    }

    private static AudioClip GetRandomAudioClip(AudioClip[] clips)
    {
        if (clips == null) return null;
        if (clips.Length == 0) return null;

        var index = Random.Range(0, clips.Length);
        return clips[index];
    }

    private static AudioSource CreateAudioSource(AudioMixerGroup group)
    {
        if (isQuitting) return null;

        var go = new GameObject("SFX");
        var source = go.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = group;
        return source;
    }
}