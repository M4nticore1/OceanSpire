using UnityEngine;
using UnityEngine.Audio;

public static class AudioUtils
{
    public static void PlaySFX(AudioClip clip, AudioMixerGroup group)
    {
        if (!clip) return;

        var src = CreateAudioSource(group);
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
        if (!clip) return;

        var src = CreateAudioSource(group);
        src.gameObject.transform.position = pos;

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
        if (clips == null || clips.Length == 0) return null;

        int index = Random.Range(0, clips.Length);

        return clips[index];
    }

    private static AudioSource CreateAudioSource(AudioMixerGroup group)
    {
        var go = new GameObject("SFX");
        var source = go.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = group;
        return source;
    }
}